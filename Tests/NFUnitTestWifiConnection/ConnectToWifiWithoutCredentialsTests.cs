using nanoFramework.Networking;
using nanoFramework.TestFramework;
using System;
using System.Threading;

namespace NFUnitTestWifiConnection
{
    [TestClass]
    public class ConnectToWifiWithoutCredentialsTests
    {
        [Setup]
        public void SetupConnectToWifiWithoutCredentialsTests()
        {
            // Comment next line to run the tests on a real hardware
            // Make sure the device has already the SSID and password stored
            Assert.SkipTest("Skipping tests using nanoCLR Win32 in a pipeline");
        }

        [TestMethod]
        public void TestReconnection()
        {
            // Give 10 seconds to the wifi join to happen
            CancellationTokenSource cs = new(10000);
            var success = NetworkHelper.ReconnectWifi(token: cs.Token);
            ConnectToWifiWithCredentialsTests.DisplayLastError(success);
            Assert.True(success);
            Assert.Null(NetworkHelper.ConnectionError.Exception);
        }
    }
}
