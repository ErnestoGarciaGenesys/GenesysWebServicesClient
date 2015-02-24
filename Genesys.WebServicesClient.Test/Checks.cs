using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Threading;

namespace Genesys.WebServicesClient.Test
{
    [TestClass]
    public class Checks
    {
        class SerializationResult
        {
#pragma warning disable 0649 // CS0649: Field 'field' is never assigned to, and will always have its default value
            public string key;
            public IList<string> list;
            public string readonlyProp { get; internal set; }
#pragma warning restore 0649
        }

        [TestMethod]
        public void JavaScriptSerializer_DeserializeObject_result()
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            object result = serializer.DeserializeObject(@"{ ""key"" : ""value"" }");
            object value = ((IDictionary<string, object>)result)["key"];
            var typedResult = serializer.ConvertToType<SerializationResult>(result);
            Console.WriteLine(result);
        }

        [TestMethod]
        public void type_of_deserializing_JSON_array_of_strings()
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            object result = serializer.DeserializeObject(@"[""value1"", ""value2""]");
            Assert.IsInstanceOfType(result, typeof(object[]));
            var typedResult = serializer.ConvertToType<IReadOnlyList<string>>(result);
            Assert.IsInstanceOfType(typedResult, typeof(IReadOnlyList<string>));
        }

        [TestMethod]
        public void type_of_deserializing_JSON_field_with_array_of_strings()
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            var result = serializer.Deserialize<SerializationResult>(@"{ ""list"" : [""value1"", ""value2""] }");
            Assert.IsInstanceOfType(result.list, typeof(IList<string>));
        }

        [TestMethod]
        public void serializing_to_internal_property()
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            var result = serializer.Deserialize<SerializationResult>(@"{ ""readonlyProp"" : ""value"" }");
            Assert.IsNull(result.readonlyProp);
        }

        class FirstMediaEmail { public void Ready() {} }
        class _agent { public static FirstMediaEmail FirstMediaEmail; }

        [TestMethod]
        public void use_ThreadPool()
        {
            ThreadPool.QueueUserWorkItem(o => _agent.FirstMediaEmail.Ready());
        }
    }
}
