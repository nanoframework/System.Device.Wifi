using nanoFramework.Networking;
using nanoFramework.TestFramework;
using System;
using System.Diagnostics;
using System.Threading;

namespace NFUnitTestWifiConnection
{
    [TestClass]
    public class ConnectToWifiFixIPAddressTests
    {
        private const string Ssid = "YOU_SSID";
        private const string Password = "YOUR_PASSWORD";

        [Setup]
        public void SetupConnectToWifiFixIPAddressTests()
        {
            // Comment next line to run the tests on a real hardware
            // Adjust your SSID and password in the constants
            Assert.SkipTest("Skipping tests using nanoCLR Win32 in a pipeline");
        }

        [TestMethod]
        public void TestFixIPAddress()
        {
            // Give 10 seconds to the wifi join to happen
            CancellationTokenSource cs = new(10000);
            var success = NetworkHelper.ConnectWifiFixAddress(Ssid, Password, new IPConfiguration("192.168.1.7", "255.255.255.0", "192.168.1.1", new[] { "192.168.1.1" }), setDateTime: true, token: cs.Token);
            ConnectToWifiWithCredentialsTests.DisplayLastError(success);
            Assert.True(success);
            Assert.Null(NetworkHelper.ConnectionError.Exception);
        }
    }
}
