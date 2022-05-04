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

            Assert.True(success);
            Assert.Null(WifiNetworkHelper.HelperException);

            // need to reset this internal flag to allow calling the NetworkHelper again
            WifiNetworkHelper.ResetInstance();
        }
    }
}
