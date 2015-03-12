using Genesys.WebServicesClient.Resources;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genesys.WebServicesClient.Components
{
    public class GenesysUser : ActiveGenesysComponent
    {
        // Needs disposal
        IEventSubscription eventSubscription;

        /// <summary>
        /// Available means that this object has been correctly initialized and all its
        /// resource properties and methods are available to use.
        /// </summary>
        public bool Available { get { return InternalActivationStage == ActivationStage.Started; } }

        public event EventHandler AvailableChanged;

        #region Initialization Properties

        [Category("Initialization")]
        public GenesysConnection Connection
        {
            get { return (GenesysConnection)Parent; }
            set
            {
                if (InternalActivationStage != ActivationStage.Idle)
                    throw new InvalidOperationException("Property must be set while component is not started");
                
                Parent = value;
            }
        }

        #endregion Initialization Properties

        protected override Exception CanStart()
        {
            if (Connection == null)
                return new InvalidOperationException("Connection property must be set");

            if (Connection.ConnectionState != ConnectionState.Open)
                return new ActivationException("Connection is not open");

            return null;
        }

        protected override async Task StartImplAsync(CancellationToken cancellationToken)
        {
            eventSubscription = Connection.InternalEventReceiver.SubscribeAll(HandleEvent);

            // Documentation about recovering existing state:
            // http://docs.genesys.com/Documentation/HTCC/8.5.2/API/RecoveringExistingState#Reading_device_state_and_active_calls_together

            var response =
                await Connection.InternalClient.CreateRequest("GET", "/api/v2/me?subresources=*")
                    .SendAsync<UserResourceResponse>(cancellationToken);

            StartHierarchyUpdate(doLast =>
                UpdateResource(doLast,
                    (IDictionary<string, object>)response.AsDictionary["user"],
                    response.AsType.user));
        }

        protected override void StopImpl()
        {
            if (eventSubscription != null)
            {
                try
                {
                    eventSubscription.Dispose();
                }
                catch (Exception e)
                {
                    Trace.TraceError("Event unsubscription failed: " + e);
                }

                eventSubscription = null;
            }
        }

        protected override void OnParentUpdated(InternalUpdatedEventArgs e)
        {
            if (Connection.ConnectionState == ConnectionState.Open && AutoRecover)
                Start();

            if (Connection.ConnectionState == ConnectionState.Close)
                Stop();
        }

        void HandleEvent(object sender, GenesysEvent genesysEvent)
        {
            if (genesysEvent.Data.ContainsKey("user"))
            {
                StartHierarchyUpdate(genesysEvent, doLast =>
                    UpdateResource(doLast,
                        genesysEvent.GetResourceAsType<IDictionary<string, object>>("user"),
                        genesysEvent.GetResourceAsType<UserResource>("user")));
            }
            else
            {
                StartHierarchyUpdate(genesysEvent, doLast => RaiseUpdated(doLast));
            }
        }

        void UpdateResource(IDelayedEvents doLast, IDictionary<string, object> untypedResource, UserResource typedResource)
        {
            var oldResource = UserResourceData;

            foreach (var attrib in untypedResource)
            {
                if (AttributeChanged(attrib.Key, attrib.Value, oldResource))
                    RaisePropertyChanged(doLast, FirstToUpper(attrib.Key));
            }

            RaiseUpdated(doLast);

            UserResource = typedResource;
            UserResourceData = untypedResource;

            var untypedSettings = (IDictionary<string, object>)untypedResource["settings"];

            // Concretizing dictionary type to a dictionary of dictionaries,
            // because Settings contains sections, which contain key-value pairs.
            Settings = untypedSettings.ToDictionary(kvp => kvp.Key, kvp => (IDictionary<string, object>)kvp.Value);
        }

        static bool AttributeChanged(string attribName, object newVal, IDictionary<string, object> oldRes)
        {
            if (oldRes == null)
                return true;

            object oldVal;
            if (oldRes.TryGetValue(attribName, out oldVal))
            {
                if (newVal is string && oldVal is string)
                {
                    // compare for strings
                    return (string)newVal != (string)oldVal;
                }
                else
                {
                    // assume changed for any other types not considered for comparison
                    return true;
                }
            }
            else
            {
                // there was no previous value
                return true;
            }
        }

        #region Internal

        public UserResource UserResource { get; private set; }

        #endregion Internal

        public event EventHandler Updated;

        void RaiseUpdated(IDelayedEvents postEvents)
        {
            postEvents.Add(() =>
            {
                if (Updated != null)
                    Updated(this, EventArgs.Empty);
            });
        }

        public IDictionary<string, IDictionary<string, object>> Settings { get; private set; }

        #region Operations

        public Task ChangeState(string value)
        {
            return Connection.InternalClient.CreateRequest("POST", "/api/v2/me", new { operationName = value }).SendAsync();
        }

        #endregion Operations

        #region Attributes

        public IDictionary<string, object> UserResourceData { get; private set; }


        object GetAttributeByName(string attributeName)
        {
            if (UserResourceData == null)
                return null;

            object valueObj;
            UserResourceData.TryGetValue(attributeName, out valueObj);
            return valueObj;
        }

        object GetAttribute([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            return GetAttributeByName(FirstToLower(propertyName));
        }

        //{
        //    "userName": "a",
        //    "id": "16e299d4f1844ca09b30f96ccd921c53",
        //    "lastName": "a",
        //    "firstName": "a",
        //    "roles": [
        //        "ROLE_AGENT"
        //    ],
        //    "devices": [],
        //    "skills": [],
        //    "interactions": [],
        //    "channelStates": []
        //}

        public string UserName { get { return GetAttribute() as string; } }

        public string Id { get { return GetAttribute() as string; } }

        public string LastName { get { return GetAttribute() as string; } }

        public string FirstName { get { return GetAttribute() as string; } }

        public IEnumerable<string> Roles { get { return GetAttribute() as IEnumerable<string>; } }

        #endregion Attributes

        static string FirstToLower(string s)
        {
            return Char.ToLowerInvariant(s[0]) + s.Substring(1);
        }

        static string FirstToUpper(string s)
        {
            return Char.ToUpperInvariant(s[0]) + s.Substring(1);
        }
    }
}
