[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=nanoframework_lib-System.Device.WiFi&metric=alert_status)](https://sonarcloud.io/dashboard?id=nanoframework_lib-System.Device.WiFi) [![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=nanoframework_lib-System.Device.WiFi&metric=reliability_rating)](https://sonarcloud.io/dashboard?id=nanoframework_lib-System.Device.WiFi) [![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE) [![NuGet](https://img.shields.io/nuget/dt/nanoFramework.System.Device.WiFi.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.System.Device.WiFi/) [![#yourfirstpr](https://img.shields.io/badge/first--timers--only-friendly-blue.svg)](https://github.com/nanoframework/Home/blob/main/CONTRIBUTING.md) [![Discord](https://img.shields.io/discord/478725473862549535.svg?logo=discord&logoColor=white&label=Discord&color=7289DA)](https://discord.gg/gCyBu8T)

![nanoFramework logo](https://raw.githubusercontent.com/nanoframework/Home/main/resources/logo/nanoFramework-repo-logo.png)

-----
文档语言: [English](README.md) | [简体中文](README.zh-cn.md)

### 欢迎使用 .NET **nanoFramework** System.Device.WiFi Library repository和NetworkHelper  

这个存储库包含了nanoFramework System.Device.WiFi和NetworkHelper。  

## 构建状态

| Component | Build Status | NuGet Package |
|:-|---|---|
| System.Device.WiFi | [![Build Status](https://dev.azure.com/nanoframework/System.Device.WiFi/_apis/build/status/System.Device.WiFi?repoName=nanoframework%2FSystem.Device.WiFi&branchName=main)](https://dev.azure.com/nanoframework/System.Device.WiFi/_build/latest?definitionId=13&repoName=nanoframework%2FSystem.Device.WiFi&branchName=main) | [![NuGet](https://img.shields.io/nuget/v/nanoFramework.System.Device.WiFi.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.System.Device.WiFi/) |
| System.Device.WiFi (预览版) | [![Build Status](https://dev.azure.com/nanoframework/System.Device.WiFi/_apis/build/status/System.Device.WiFi?repoName=nanoframework%2FSystem.Device.WiFi&branchName=develop)](https://dev.azure.com/nanoframework/System.Device.WiFi/_build/latest?definitionId=13&repoName=nanoframework%2FSystem.Device.WiFi&branchName=develop) | [![NuGet](https://img.shields.io/nuget/vpre/nanoFramework.System.Device.WiFi.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.System.Device.WiFi/) |

## NetworkHelper用法

NetworkHelper主要用于帮助您自动连接到WiFi网络。 也就是说，它也可以用来检查您是否有一个有效的IP地址和有效的日期在任何接口，包括在以太网上。  

### 推荐用法

下面的代码将允许您自动连接，等待有效的IP地址和有效日期与特定凭据:  

```csharp
const string Ssid = "您的wifi名称";
const string Password = "您的wifi密码";
// 连接wifi超时时间60秒
CancellationTokenSource cs = new(60000);
var success = NetworkHelper.ConnectWifiDhcp(Ssid, Password, setDateTime: true, token: cs.Token);
if (!success)
{
    // 如果出现异常你可以通过 ConnectionError 获取异常的详情信息：
    Debug.WriteLine($"无法连接到网络，错误: {NetworkHelper.ConnectionError.Error}");
    if (NetworkHelper.ConnectionError.Exception != null)
    {
        Debug.WriteLine($"ex: { NetworkHelper.ConnectionError.Exception}");
    }
}
    //否则 连接成功，您已经拥有有效的IP和时间
```

注意，这个函数将在设备上存储网络证书。  

### 使用存储证书

您还可以使用预先存储在设备上的证书连接到网络，这取决于您拥有的设备类型。 请检查您的设备是否有适当的支持。  

```csharp
if (!NetworkHelper.IsConfigurationStored())
{
    Debug.WriteLine("设备中没有配置信息");
}
else
{
    // wifi证书已经存储在设备上  
    // 给连接wifi的超时时间60秒
    CancellationTokenSource cs = new(60000);
    var success = NetworkHelper.ReconnectWifi(setDateTime: true, token: cs.Token);
    if (!success)
    {
        // 如果出现异常你可以通过 ConnectionError 获取异常的详情信息：
        Debug.WriteLine($"无法连接到网络，错误: {NetworkHelper.ConnectionError.Error}");
        if (NetworkHelper.ConnectionError.Exception != null)
        {
            Debug.WriteLine($"ex: { NetworkHelper.ConnectionError.Exception}");
        }
    }
    //否则 连接成功，您已经拥有有效的IP和时间
}
```

### 扫描并加入

您可以强制扫描网络，然后加入网络。 这种情况在特定条件下可能很有用。 请注意，您可能必须将此方法与前面的方法之一一起使用，这取决于您已经连接时对重新扫描的支持。  

```csharp
const string Ssid = "您的wifi名称";
const string Password = "您的wifi密码";
// 连接wifi超时时间60秒
CancellationTokenSource cs = new(60000);
var success = NetworkHelper.ScanAndConnectWifiDhcp(Ssid, Password, setDateTime: true, token: cs.Token);
if (!success)
{
        // 如果出现异常你可以通过 ConnectionError 获取异常的详情信息：
    Debug.WriteLine($"无法连接到网络，错误: {NetworkHelper.ConnectionError.Error}");
    if (NetworkHelper.ConnectionError.Exception != null)
    {
        Debug.WriteLine($"ex: { NetworkHelper.ConnectionError.Exception}");
    }
}
    //否则 连接成功，您已经拥有有效的IP和时间
```
注意，这个函数将在设备上存储网络证书。  

### 使用静态IP地址连接

你可以用一个静态IP地址加入一个WiFi网络  :

```csharp
const string Ssid = "您的wifi名称";
const string Password = "您的wifi密码";
// 连接wifi超时时间60秒
CancellationTokenSource cs = new(60000);
var success = NetworkHelper.ConnectWifiFixAddress(Ssid, Password, new IPConfiguration("192.168.1.7", "255.255.255.0", "192.168.1.1"), setDateTime: true, token: cs.Token);
```

### 检查有效的IP地址和日期

NetworkHelper提供了几个功能来检查您的IP地址的有效性，日期时间，并帮助设置它们 :

```csharp
var success = IsValidDateTime();
// 如果 success是true, 那就是一个有效的时间
success = IsValidIpAddress(NetworkInterfaceType.Wireless80211);
// 如果成功为true，表示您在无线适配器上有一个有效的IP地址  
//注意，像127.0.0.1这样的本地地址被认为是一个有效的以太网地址  
```

您也可以等待一个有效的IP地址和日期时间 :

```csharp
//60秒的超时时间检查是否是个有效ip和有效时间
CancellationTokenSource cs = new(60000);
var success = WaitForValidIPAndDate(true, NetworkInterfaceType.Ethernet, cs.Token);
// 如果 success是true，那就连接成功
```

## NetworkHelper用法

NetworkHelper主要用于帮助您自动连接到WiFi网络。 也就是说，它也可以用来检查您是否有一个有效的IP地址和有效的日期在任何接口，包括在以太网上。  

### 推荐用法

下面的代码将允许您自动连接，使用特定证书等待有效的IP地址和有效日期  :

```csharp
const string Ssid = "您的wifi名称";
const string Password = "您的wifi密码";
// 连接wifi超时时间60秒
CancellationTokenSource cs = new(60000);
var success = NetworkHelper.ConnectWifiDhcp(Ssid, Password, setDateTime: true, token: cs.Token);
if (!success)
{
    // 如果出现异常你可以通过 ConnectionError 获取异常的详情信息：
    Debug.WriteLine($"无法连接到网络，错误: {NetworkHelper.ConnectionError.Error}");
    if (NetworkHelper.ConnectionError.Exception != null)
    {
        Debug.WriteLine($"ex: { NetworkHelper.ConnectionError.Exception}");
    }
}
//否则 连接成功，您已经拥有有效的IP和时间
```
注意，这个函数将在设备上存储网络证书。  

### 使用存储证书

您还可以使用预先存储在设备上的证书连接到网络，这取决于您拥有的设备类型。 请检查您的设备是否有适当的支持。  

```csharp
// wifi证书已经存储在设备上  
// 给连接wifi的超时时间60秒
CancellationTokenSource cs = new(60000);
var success = NetworkHelper.ReconnectWifi(setDateTime: true, token: cs.Token);
if (!success)
{
    // 如果出现异常你可以通过 ConnectionError 获取异常的详情信息：
    Debug.WriteLine($"无法连接到网络，错误: {NetworkHelper.ConnectionError.Error}");
    if (NetworkHelper.ConnectionError.Exception != null)
    {
        Debug.WriteLine($"ex: { NetworkHelper.ConnectionError.Exception}");
    }
}
//否则 连接成功，您已经拥有有效的IP和时间
```

### 扫描并加入

您可以强制扫描网络，然后加入网络。 这种情况在特定条件下可能很有用。 请注意，您可能必须将此方法与前面的方法之一一起使用，这取决于您已经连接时对重新扫描的支持。  

```csharp
const string Ssid = "您的wifi名称";
const string Password = "您的wifi密码";
// 连接wifi超时时间60秒
CancellationTokenSource cs = new(60000);
var success = NetworkHelper.ScanAndConnectWifiDhcp(Ssid, Password, setDateTime: true, token: cs.Token);
if (!success)
{
    // 如果出现异常你可以通过 ConnectionError 获取异常的详情信息：
    Debug.WriteLine($"Can't connect to the network, error: {NetworkHelper.ConnectionError.Error}");
    if (NetworkHelper.ConnectionError.Exception != null)
    {
        Debug.WriteLine($"ex: { NetworkHelper.ConnectionError.Exception}");
    }
}
//否则 连接成功，您已经拥有有效的IP和时间
```

注意，这个函数将在设备上存储网络证书。  

### 使用静态IP地址连接

你可以用一个静态IP地址加入一个WiFi网络  :

```csharp
const string Ssid = "您的wifi名称";
const string Password = "您的wifi密码";
// 连接wifi的超时时间
CancellationTokenSource cs = new(60000);
var success = NetworkHelper.ConnectWifiFixAddress(Ssid, Password, new IPConfiguration("192.168.1.7", "255.255.255.0", "192.168.1.1"), setDateTime: true, token: cs.Token);
```

### Checking valid IP address and date

NetworkHelper提供了几个功能来检查您的IP地址的有效性，日期时间，并帮助设置它们 :

```csharp
var success = IsValidDateTime();
// 如果 success是true, 那就是一个有效的时间
success = IsValidIpAddress(NetworkInterfaceType.Wireless80211);
// 如果成功为true，表示您在无线适配器上有一个有效的IP地址  
//注意，像127.0.0.1这样的本地地址被认为是一个有效的以太网地址  
```

您也可以等待一个有效的IP地址和日期时间 :

```csharp
//60秒的超时时间检查是否是个有效ip和有效时间
CancellationTokenSource cs = new(60000);
var success = WaitForValidIPAndDate(true, NetworkInterfaceType.Ethernet, cs.Token);
// 如果 success是true，那就连接成功
```

## 反馈和文档

有关文档、提供反馈、问题和找出如何贡献，请参阅 [主存储库](https://github.com/nanoframework/Home).

加入我们的Discord社区 [这里](https://discord.gg/gCyBu8T).

## 贡献者

此项目的贡献者列表可在以下网站找到 [贡献者](https://github.com/nanoframework/Home/blob/main/CONTRIBUTORS.md).

## 许可证

**nanoFramework**类库在 [麻省理工学院的许可](LICENSE.md).

## 规范

这个项目采用了贡献者契约所定义的行为准则，以澄清我们社区的预期行为。  
有关更多信息，请参见 [.net基金会行为准则](https://dotnetfoundation.org/code-of-conduct).

### .NET Foundation

This project is supported by the [.NET Foundation](https://dotnetfoundation.org).
