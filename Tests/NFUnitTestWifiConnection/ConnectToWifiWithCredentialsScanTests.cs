using nanoFramework.Networking;
using nanoFramework.TestFramework;
using System;
using System.Diagnostics;
using System.Threading;

namespace NFUnitTestWifiConnection
{
    [TestClass]
    public class ConnectToWifiWithCredentialsScanTests
    {
        private const string Ssid = "YOU_SSID";
        private const string Password = "YOUR_PASSWORD";

        [Setup]
        public void SetupConnectToWifiWithCredentialsScanTests()
        {
            // Comment next line to run the tests on a real hardware
            // Adjust your SSID and password in the constants
            Assert.SkipTest("Skipping tests using nanoCLR Win32 in a pipeline");
        }

        [TestMethod]
        public void TestNormalConnectionScanAndConnect()
        {
            // Give 10 seconds to the wifi join to happen
            CancellationTokenSource cs = new(10000);
            var success = NetworkHelper.ScanAndConnectWifiDhcp(Ssid, Password, setDateTime: true, token: cs.Token);
            ConnectToWifiWithCredentialsTests.DisplayLastError(success);
            Assert.True(success);
            Assert.Null(NetworkHelper.ConnectionError.Exception);
        }
    }
}
