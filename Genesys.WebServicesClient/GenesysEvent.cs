using System.Collections.Generic;
using Cometd.Bayeux;
using System.Web.Script.Serialization;

namespace Genesys.WebServicesClient
{
    public class GenesysEvent
    {
        static readonly JavaScriptSerializer JsonParser = new JavaScriptSerializer();
        
        readonly IMessage message;

        public GenesysEvent(IMessage message)
        {
            this.message = message;
        }

        public string Channel
        {
            get { return message.Channel; }
        }

        public IDictionary<string, object> Data
        {
            get { return message.DataAsDictionary; }
        }

        public string MessageType
        {
            get { return Data["messageType"] as string; }
        }

        public string NotificationType
        {
            get { return Data["notificationType"] as string; }
        }

        public T GetResourceAsType<T>(string resourceKey)
        {
            object resource = Data[resourceKey];
            return JsonParser.ConvertToType<T>(resource);
        }

        public T GetResourceAsTypeOrNull<T>(string resourceKey)
            where T : class
        {
            object resource;
            if (Data.TryGetValue(resourceKey, out resource))
                return JsonParser.ConvertToType<T>(resource);
            else
                return null;
        }
    }
}
