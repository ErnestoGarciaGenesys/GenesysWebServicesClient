using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genesys.WebServicesClient
{
    static class GTrace
    {
        static readonly TraceSource Log = new TraceSource(typeof(GTrace).Namespace);

        public enum TraceType
        {
            Request,
            Event,
            Bayeux,
            BayeuxError,
        }

        public static void Trace(TraceType type, string format, params object[] args)
        {
            var traceEventType =
                type == TraceType.BayeuxError ?
                TraceEventType.Error :
                TraceEventType.Verbose;

            Log.TraceEvent(traceEventType, (int)type, format, args);
        }

        public static string PrettifyJson(string json)
        {
            object obj = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }

    }
}
