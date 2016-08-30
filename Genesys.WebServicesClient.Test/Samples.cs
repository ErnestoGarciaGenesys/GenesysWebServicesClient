using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Diagnostics;
using System.Web.Script.Serialization;
using System.Text;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Genesys.WebServicesClient;

namespace Genesys.WebServicesClient.Test
{
    [TestClass]
    public class Samples
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void Receive_events()
        {
            var client = new GenesysClient.Setup()
            {
                ServerUri = TestParams.ServerUri,
                UserName = TestParams.UserName,
                Password = TestParams.Password,
                AsyncTaskScheduler = TaskScheduler.Default,
            }
                .Create();

            using (client)
            {
                var versionResponse = client.CreateRequest("GET", "/api/v2/diagnostics/version").SendAsync().Result;
                TestContext.WriteLine("Received: {0}", versionResponse);

                using (var eventReceiver = client.CreateEventReceiver(new GenesysEventReceiver.Setup()))
                {
                    var subscription = eventReceiver.SubscribeAll((s, e) =>
                    {
                        TestContext.WriteLine("Comet message received: {0}", e);
                    });

                    eventReceiver.Open(5000);

                    var postResponse = client.CreateRequest("POST", "/api/v2/me", new { operationName = "Ready" }).SendAsync().Result;
                    TestContext.WriteLine("POST response: {0}", postResponse);

                    Thread.Sleep(1000);

                    var notReadyPostResponse = client.CreateRequest("POST", "/api/v2/me", new { operationName = "NotReady" }).SendAsync().Result;
                    TestContext.WriteLine("POST response: {0}", notReadyPostResponse);

                    Thread.Sleep(1000);

                    subscription.Dispose();

                    Thread.Sleep(1000);
                }
            }
        }

    }
}
