//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using nanoFramework.Networking;
using nanoFramework.TestFramework;
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
        public void TestFixIPAddress_01()
        {
            // Give 10 seconds to the WiFi join to happen
            CancellationTokenSource cs = new(10000);

            var success = WiFiNetworkHelper.ConnectFixAddress(
                Ssid,
                Password,
                new IPConfiguration(
                    "192.168.1.7",
                    "255.255.255.0",
                    "192.168.1.1",
                    new[] { "192.168.1.1" }),
                requiresDateTime: true,
                token: cs.Token);

            ConnectToWifiWithCredentialsTests.DisplayLastError(success);

            Assert.True(success);
            Assert.Null(WiFiNetworkHelper.HelperException);

            // need to reset this internal flag to allow calling the NetworkHelper again
            WiFiNetworkHelper.ResetInstance();
        }

        [TestMethod]
        public void TestFixedIPAddress_02()
        {
            WiFiNetworkHelper.SetupNetworkHelper(new IPConfiguration(
                "192.168.1.111",
                "255.255.255.0",
                "192.168.1.1",
                new[] { "192.168.1.1" }), true);

            // wait 10 seconds to connect to the network
            Assert.True(WiFiNetworkHelper.NetworkReady.WaitOne(10000, true));

            // need to reset this internal flag to allow calling the NetworkHelper again
            WiFiNetworkHelper.ResetInstance();
        }
    }
}
