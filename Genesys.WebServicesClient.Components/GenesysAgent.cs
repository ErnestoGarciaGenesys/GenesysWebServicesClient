using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genesys.WebServicesClient.Components
{
    public class GenesysAgent : IComponent, INotifyPropertyChanged
    {
        public GenesysConnection Connection { get; set; }
        internal GenesysEventReceiver EventReceiver { get; private set; }

        string state;

        bool initialized = false;

        public void Initialize()
        {
            Connection.Initialize();
            initialized = true;
            EventReceiver = Connection.Client.CreateEventReceiver();
            EventReceiver.Open();
            EventReceiver.SubscribeAll(HandleEvent);
            RefreshAgent();
        }

        public string State
        {
            get { return state; }
            private set
            {
                state = value;
                RaisePropertyChanged("State");
            }
        }

        public async Task ChangeState(string value)
        {
            await Connection.Client.CreateRequest("POST", "/api/v2/me", new { operationName = value }).SendAsync();
        }

        public Task MakeReady()
        {
            return ChangeState("Ready");
        }

        public Task MakeNotReady()
        {
            return ChangeState("NotReady");
        }

        void HandleEvent(object sender, GenesysEvent e)
        {
            var messageType = e.Data["messageType"] as string;

            if (messageType == "DeviceStateChangeMessage")
            {
                var devices = (object[])e.Data["devices"];
                var device = (IDictionary<string, object>)devices[0];
                var userState = (IDictionary<string, object>)device["userState"];
                State = (string)userState["state"];
            }
        }

        async void RefreshAgent()
        {
            // doc: http://docs.genesys.com/Documentation/HTCC/8.5.2/API/RecoveringExistingState#Reading_device_state_and_active_calls_together
            string response = await Connection.Client.CreateRequest("GET", "api/v2/me?subresources=*").SendAsync();

        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion INotifyPropertyChanged

        #region IComponent

        public event EventHandler Disposed;

        public ISite Site { get; set; }

        public void Dispose() { }

        #endregion IComponent

        // Saved for future reference when having to implement more complex data binding objects.
        //#region INotifyDataErrorInfo

        //Dictionary<string, IList<string>> errors = new Dictionary<string, IList<string>>();
        //public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        //public IEnumerable GetErrors(string propertyName)
        //{
        //    return errors.ContainsKey(propertyName) ?
        //        errors[propertyName] :
        //        Enumerable.Empty<string>();
        //}

        //public bool HasErrors
        //{
        //    get { return errors.Count > 0; }
        //}

        //void RaiseError(string propertyName, string error)
        //{
        //    errors[propertyName] = new string[] { error };

        //    if (ErrorsChanged != null)
        //        ErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));

        //    RaisePropertyChanged(propertyName);
        //}

        //void RemoveError(string propertyName)
        //{
        //    errors.Remove(propertyName);
        //}

        //#endregion INotifyDataErrorInfo

        //#region IDataErrorInfo

        //string IDataErrorInfo.Error
        //{
        //    get
        //    {
        //        return HasErrors ? "Error" : "";
        //    }
        //}

        //string IDataErrorInfo.this[string columnName]
        //{
        //    get
        //    {
        //        return GetErrors(columnName).Cast<string>().FirstOrDefault() ?? "";                    
        //    }
        //}

        //#endregion IDataErrorInfo
    }
}
