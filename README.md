[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=nanoframework_System.Device.WiFi&metric=alert_status)](https://sonarcloud.io/dashboard?id=nanoframework_System.Device.WiFi) [![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=nanoframework_System.Device.WiFi&metric=reliability_rating)](https://sonarcloud.io/dashboard?id=nanoframework_System.Device.WiFi) [![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE) [![NuGet](https://img.shields.io/nuget/dt/nanoFramework.System.Device.WiFi.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.System.Device.WiFi/) [![#yourfirstpr](https://img.shields.io/badge/first--timers--only-friendly-blue.svg)](https://github.com/nanoframework/Home/blob/main/CONTRIBUTING.md) [![Discord](https://img.shields.io/discord/478725473862549535.svg?logo=discord&logoColor=white&label=Discord&color=7289DA)](https://discord.gg/gCyBu8T)

![nanoFramework logo](https://raw.githubusercontent.com/nanoframework/Home/main/resources/logo/nanoFramework-repo-logo.png)

-----
Document Language: [English](README.md) | [简体中文](README.zh-cn.md)

### Welcome to the .NET **nanoFramework** System.Device.WiFi Library repository

This repository contains the nanoFramework System.Device.WiFi class library.

## Build status

| Component | Build Status | NuGet Package |
|:-|---|---|
| System.Device.WiFi | [![Build Status](https://dev.azure.com/nanoframework/System.Device.WiFi/_apis/build/status/System.Device.WiFi?repoName=nanoframework%2FSystem.Device.WiFi&branchName=main)](https://dev.azure.com/nanoframework/System.Device.WiFi/_build/latest?definitionId=13&repoName=nanoframework%2FSystem.Device.WiFi&branchName=main) | [![NuGet](https://img.shields.io/nuget/v/nanoFramework.System.Device.WiFi.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.System.Device.WiFi/) |
| System.Device.WiFi (preview) | [![Build Status](https://dev.azure.com/nanoframework/System.Device.WiFi/_apis/build/status/System.Device.WiFi?repoName=nanoframework%2FSystem.Device.WiFi&branchName=develop)](https://dev.azure.com/nanoframework/System.Device.WiFi/_build/latest?definitionId=13&repoName=nanoframework%2FSystem.Device.WiFi&branchName=develop) | [![NuGet](https://img.shields.io/nuget/vpre/nanoFramework.System.Device.WiFi.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.System.Device.WiFi/) |

## WiFiNetworkHelper usage

The WiFiNetworkHelper class is mainly dedicated to help you connect automatically to WiFi networks. That said, it can be used as well to check if you have a valid IP address and a valid date on any interface including on ethernet.

### Preferred usage

The following code will allow you to connect automatically, wait for a valid IP address and a valid date with specific credentials:

```csharp
const string Ssid = "YourSSID";
const string Password = "YourWifiPassword";
// Give 60 seconds to the wifi join to happen
CancellationTokenSource cs = new(60000);
var success = WiFiNetworkHelper.ConnectDhcp(Ssid, Password, requiresDateTime: true, token: cs.Token);
if (!success)
{
    // Something went wrong, you can get details with the ConnectionError property:
    Debug.WriteLine($"Can't connect to the network, error: {WiFiNetworkHelper.Status}");
    if (WiFiNetworkHelper.HelperException != null)
    {
        Debug.WriteLine($"ex: {WiFiNetworkHelper.HelperException}");
    }
}
// Otherwise, you are connected and have a valid IP and date
```

Note that this function will store the network credentials on the device.

### Using stored credentials

You can as well connect to a network with pre stored credentials on the device depending on the type of device you have. Please check for proper support with your device.

```csharp
if (!WiFiNetworkHelper.IsConfigurationStored())
{
    Debug.WriteLine("No configuration stored in the device");
}
else
{
    // The wifi credentials are already stored on the device
    // Give 60 seconds to the wifi join to happen
    CancellationTokenSource cs = new(60000);
    var success = WiFiNetworkHelper.Reconnect(requiresDateTime: true, token: cs.Token);
    if (!success)
    {
        // Something went wrong, you can get details with the ConnectionError property:
        Debug.WriteLine($"Can't connect to the network, error: {WiFiNetworkHelper.Status}");
        if (WiFiNetworkHelper.HelperException != null)
        {
            Debug.WriteLine($"ex: {WiFiNetworkHelper.HelperException}");
        }
    }
    // Otherwise, you are connected and have a valid IP and date
}
```

### Scan and join

You can force to scan the network and then join the network. This case can be useful in specific conditions. Be aware, that you may have to use this method with one of the previous method depending the support of rescanning while you are already connected.

```csharp
const string Ssid = "YourSSID";
const string Password = "YourWifiPassword";
// Give 60 seconds to the wifi join to happen
CancellationTokenSource cs = new(60000);
var success = WiFiNetworkHelper.ScanAndConnectDhcp(Ssid, Password, requiresDateTime: true, token: cs.Token);
if (!success)
{
    // Something went wrong, you can get details with the ConnectionError property:
    Debug.WriteLine($"Can't connect to the network, error: {WiFiNetworkHelper.Status}");
    if (WiFiNetworkHelper.HelperException != null)
    {
        Debug.WriteLine($"ex: {WiFiNetworkHelper.HelperException}");
    }
}
// Otherwise, you are connected and have a valid IP and date
```

Note that this function will store the network credentials on the device.

### Joining with a static IP address

You join a WiFi network with a static IP address:

```csharp
const string Ssid = "YourSSID";
const string Password = "YourWifiPassword";
// Give 60 seconds to the wifi join to happen
CancellationTokenSource cs = new(60000);
var success = WiFiNetworkHelper.ConnectFixAddress(Ssid, Password, new IPConfiguration("192.168.1.7", "255.255.255.0", "192.168.1.1"), requiresDateTime: true, token: cs.Token);
```

### Checking valid IP address and date

The WiFiNetworkHelper offers couple of functions to check validity of your IP address, the DateTime and helping setting them up:

```csharp
var success = IsValidDateTime();
// if success is true, you have a valid DateTime
success = IsValidIpAddress(NetworkInterfaceType.Wireless80211);
// if success is true, you have a valid IP address on the Wireless adapter
// Be aware that a local address like 127.0.0.1 is considered as a valid address for ethernet
```

You can as well wait for a valid IP address and date time:

```csharp
// Give 60 seconds to check if we have a valid IP and a valid DateTime
CancellationTokenSource cs = new(60000);
var success = WaitForValidIPAndDate(true, NetworkInterfaceType.Ethernet, cs.Token);
// if success is true then you are connected
```

## WiFiNetworkHelper usage

The WiFiNetworkHelper is mainly dedicated to help you connect automatically to WiFi networks. That said, it can be used as well to check if you have a valid IP address and a valid date on any interface including on ethernet.

### Preferred usage

The following code will allow you to connect automatically, wait for a valid IP address and a valid date with specific credentials:

```csharp
const string Ssid = "YourSSID";
const string Password = "YourWifiPassword";
// Give 60 seconds to the wifi join to happen
CancellationTokenSource cs = new(60000);
var success = WiFiNetworkHelper.ConnectDhcp(Ssid, Password, requiresDateTime: true, token: cs.Token);
if (!success)
{
    // Something went wrong, you can get details with the ConnectionError property:
    Debug.WriteLine($"Can't connect to the network, error: {WiFiNetworkHelper.Status}");
    if (WiFiNetworkHelper.HelperException != null)
    {
        Debug.WriteLine($"ex: {WiFiNetworkHelper.HelperException}");
    }
}
// Otherwise, you are connected and have a valid IP and date
```

Note that this function will store the network credentials on the device.

### Using stored credentials

You can as well connect to a network with pre stored credentials on the device depending on the type of device you have. Please check for proper support with your device.

```csharp
// The wifi credentials are already stored on the device
 // Give 60 seconds to the wifi join to happen
CancellationTokenSource cs = new(60000);
var success = WiFiNetworkHelper.Reconnect(requiresDateTime: true, token: cs.Token);
if (!success)
{
    // Something went wrong, you can get details with the ConnectionError property:
    Debug.WriteLine($"Can't connect to the network, error: {WiFiNetworkHelper.Status}");
    if (WiFiNetworkHelper.HelperException != null)
    {
        Debug.WriteLine($"ex: {WiFiNetworkHelper.HelperException}");
    }
}
// Otherwise, you are connected and have a valid IP and date
```

### Scan and join

You can force to scan the network and then join the network. This case can be useful in specific conditions. Be aware, that you may have to use this method with one of the previous method depending the support of rescanning while you are already connected.

```csharp
const string Ssid = "YourSSID";
const string Password = "YourWifiPassword";
// Give 60 seconds to the wifi join to happen
CancellationTokenSource cs = new(60000);
var success = WiFiNetworkHelper.ScanAndConnectDhcp(Ssid, Password, requiresDateTime: true, token: cs.Token);
if (!success)
{
    // Something went wrong, you can get details with the ConnectionError property:
    Debug.WriteLine($"Can't connect to the network, error: {WiFiNetworkHelper.Status}");
    if (WiFiNetworkHelper.HelperException != null)
    {
        Debug.WriteLine($"ex: {WiFiNetworkHelper.HelperException}");
    }
}
// Otherwise, you are connected and have a valid IP and date
```

Note that this function will store the network credentials on the device.

### Joining with a static IP address

You join a WiFi network with a static IP address:

```csharp
const string Ssid = "YourSSID";
const string Password = "YourWifiPassword";
// Give 60 seconds to the wifi join to happen
CancellationTokenSource cs = new(60000);
var success = WiFiNetworkHelper.ConnectFixAddress(Ssid, Password, new IPConfiguration("192.168.1.7", "255.255.255.0", "192.168.1.1"), requiresDateTime: true, token: cs.Token);
```

### Checking valid IP address and date

The WiFiNetworkHelper offers couple of functions to check validity of your IP address, the DateTime and helping setting them up:

```csharp
var success = IsValidDateTime();
// if success is true, you have a valid DateTime
success = IsValidIpAddress(NetworkInterfaceType.Wireless80211);
// if success is true, you have a valid IP address on the Wireless adapter
// Be aware that a local address like 127.0.0.1 is considered as a valid address for ethernet
```

You can as well wait for a valid IP address and date time:

```csharp
// Give 60 seconds to check if we have a valid IP and a valid DateTime
CancellationTokenSource cs = new(60000);
var success = WaitForValidIPAndDate(true, NetworkInterfaceType.Ethernet, cs.Token);
// if success is true then you are connected
```

## Feedback and documentation

For documentation, providing feedback, issues and finding out how to contribute please refer to the [Home repo](https://github.com/nanoframework/Home).

Join our Discord community [here](https://discord.gg/gCyBu8T).

## Credits

The list of contributors to this project can be found at [CONTRIBUTORS](https://github.com/nanoframework/Home/blob/main/CONTRIBUTORS.md).

## License

The **nanoFramework** Class Libraries are licensed under the [MIT license](LICENSE.md).

## Code of Conduct

This project has adopted the code of conduct defined by the Contributor Covenant to clarify expected behaviour in our community.
For more information see the [.NET Foundation Code of Conduct](https://dotnetfoundation.org/code-of-conduct).

### .NET Foundation

This project is supported by the [.NET Foundation](https://dotnetfoundation.org).
