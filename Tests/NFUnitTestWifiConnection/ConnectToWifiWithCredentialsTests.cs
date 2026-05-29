//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using nanoFramework.Networking;
using nanoFramework.TestFramework;
using System.Diagnostics;
using System.Threading;

namespace NFUnitTestWifiConnection
{
    [TestClass]
    public class ConnectToWifiWithCredentialsTests
    {
        private const string Ssid = "YOU_SSID";
        private const string Password = "YOUR_PASSWORD";

        [Setup]
        public void SetupConnectToWifiWithCredentialsTests()
        {
            // Comment next line to run the tests on a real hardware
            // Adjust your SSID and password in the constants
            Assert.SkipTest("Skipping tests using nanoCLR Win32 in a pipeline");
        }

        public static void DisplayLastError(bool success)
        {
            if (success)
            {
                Debug.WriteLine($"Connection to network is a success");
            }
            else
            {
                Debug.WriteLine($"Can't connect to the network, error: {WifiNetworkHelper.Status}");
                if (WifiNetworkHelper.HelperException != null)
                {
                    Debug.WriteLine($"ex: { WifiNetworkHelper.HelperException}");
                }
            }
        }

        [TestMethod]
        public void TestNormalConnection()
        {
            // Give 10 seconds to the WiFi join to happen
            CancellationTokenSource cs = new(10000);

            var success = WifiNetworkHelper.ConnectDhcp(
                Ssid,
                Password,
                requiresDateTime: true,
                token: cs.Token);

            DisplayLastError(success);

            Assert.IsTrue(success);
            Assert.IsNull(WifiNetworkHelper.HelperException);

            WifiNetworkHelper.Reset();
        }

        [TestMethod]
        public void TestRetryAfterTimeout()
        {
            // First attempt: very short timeout so it expires
            CancellationTokenSource cs1 = new(1000);
            var firstResult = WifiNetworkHelper.ConnectDhcp(
                Ssid,
                Password,
                token: cs1.Token);

            Assert.IsFalse(firstResult, "First call should have timed out");

            // Second attempt with a longer timeout — must not throw InvalidOperationException
            CancellationTokenSource cs2 = new(15000);
            var secondResult = WifiNetworkHelper.ConnectDhcp(
                Ssid,
                Password,
                requiresDateTime: true,
                token: cs2.Token);

            DisplayLastError(secondResult);
            Assert.IsTrue(secondResult, "Second attempt should succeed after retry");

            WifiNetworkHelper.Reset();
        }

        [TestMethod]
        public void TestResetAllowsEventBasedRestart()
        {
            // Use event-based helper once
            WifiNetworkHelper.SetupNetworkHelper(Ssid, Password);

            bool connected = WifiNetworkHelper.NetworkReady.WaitOne(15000, true);
            Assert.IsTrue(connected, "Expected to connect on first event-based attempt");

            // Reset and restart with same credentials
            WifiNetworkHelper.Reset();
            WifiNetworkHelper.SetupNetworkHelper(Ssid, Password);

            connected = WifiNetworkHelper.NetworkReady.WaitOne(15000, true);
            Assert.IsTrue(connected, "Expected to connect after Reset + SetupNetworkHelper restart");

            WifiNetworkHelper.Reset();
        }

        [TestMethod]
        public void TestSingleUsageEventBased()
        {
            Assert.ThrowsException(typeof(System.InvalidOperationException), () =>
            {
                // First call is OK
                WifiNetworkHelper.SetupNetworkHelper(Ssid, Password);

                // Second call without Reset must throw
                WifiNetworkHelper.SetupNetworkHelper(Ssid, Password);
            });

            WifiNetworkHelper.Reset();
        }
    }
}
