//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using nanoFramework.Networking;
using nanoFramework.TestFramework;
using System;
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
            // Give 10 seconds to the WiFi join to happen
            CancellationTokenSource cs = new(10000);

            var success = WifiNetworkHelper.ScanAndConnectDhcp(
                Ssid,
                Password,
                requiresDateTime: true,
                token: cs.Token);

            ConnectToWifiWithCredentialsTests.DisplayLastError(success);

            Assert.True(success);
            Assert.Null(WifiNetworkHelper.HelperException);

            // need to reset this internal flag to allow calling the NetworkHelper again
            WifiNetworkHelper.ResetInstance();
        }

        [TestMethod]
        public void TestDhcp_01()
        {
            WifiNetworkHelper.SetupNetworkHelper(
                Ssid,
                Password,
                requiresDateTime: true);

            // wait 10 seconds to connect to the network and get an IP address
            Assert.True(WifiNetworkHelper.NetworkReady.WaitOne(10000, true));
            Assert.Null(WifiNetworkHelper.HelperException);

            // need to reset this internal flag to allow calling the NetworkHelper again
            WifiNetworkHelper.ResetInstance();
        }

        [TestMethod]
        public void TestDhcp_02()
        {
            WifiNetworkHelper.SetupNetworkHelper(true);

            // wait 10 seconds to connect to the network and get an IP address
            Assert.True(WifiNetworkHelper.NetworkReady.WaitOne(10000, true));

            // need to reset this internal flag to allow calling the NetworkHelper again
            WifiNetworkHelper.ResetInstance();
        }

        [TestMethod]
        public void TestSingleUsage()
        {
            Assert.Throws(typeof(InvalidOperationException), () =>
            {
                // call once, it's OK
                WifiNetworkHelper.SetupNetworkHelper();

                // call twice, it's a NO NO and should throw an exception
                WifiNetworkHelper.SetupNetworkHelper();
            });
        }
    }
}
