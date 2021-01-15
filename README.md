[![Contributor Covenant](https://img.shields.io/badge/Contributor%20Covenant-v2.0%20adopted-ff69b4.svg)](code_of_conduct.md)
![GitHub](https://img.shields.io/github/license/siemens/opc-ua-pubsub-dotnet)

![Build & Test](https://github.com/siemens/opc-ua-pubsub-dotnet/workflows/Build%20&%20Test/badge.svg?branch=main)
![Publish NuGet Packages](https://github.com/siemens/opc-ua-pubsub-dotnet/workflows/Publish%20NuGet%20Packages/badge.svg)

![Nuget](https://img.shields.io/nuget/v/opc.ua.pubsub.dotnet.binary?label=binary&logo=nuget)
![Nuget](https://img.shields.io/nuget/v/opc.ua.pubsub.dotnet.client?label=client&logo=nuget)

# opc-ua-pubsub-dotnet
`opc-ua-pubsub-dotnet` is a library which implements OPC UA PubSub encoding and decoding in a simplified way.

It's not offering the full flexibility of OPC UA PubSub, but it supports encoding and decoding of all data types which are used by Siemens SICAM A8000 and SICAM GridEdge. You can easily extend this library to support additional data types.

The library itself is written in .NET Standard 2.0 so it supports a wide range of targets (.NET Core, .NET Framework, Mono, Xamarin, Unity, UWP).

# Using

Add the Nuget package, e.g.:
```shell
dotnet add package opc.ua.pubsub.dotnet.client
```
or by using [Visual Studio](https://docs.microsoft.com/en-us/nuget/consume-packages/install-use-packages-visual-studio), [Visual Studio for Mac](https://docs.microsoft.com/en-us/visualstudio/mac/nuget-walkthrough?toc=%2Fnuget%2Ftoc.json&view=vsmac-2019) or the [Package Manager Console](https://docs.microsoft.com/en-us/nuget/consume-packages/install-use-packages-powershell)


## Decoding

Create a new instace of `DecodeMessage` and pass the binary message:
```csharp
DecodeMessage Decoder = new DecodeMessage();
NetworkMessage message = Decoder.ParseBinaryMessage(rawMessage)
```
Please keep in mind, that OPC UA PubSub is stateful. That means you need to have decoded the Meta-Message before you can decode any Key- or Delta-Message.

For more advanced use cases please see the developer documentation.

## Encoding

Create an instance of the `ProcessData` class:
```csharp
string publisherID = "my-publisher-id";
string dataSetName = "my-data-set-name";
ushort writerID    = 123;
ProcessDataSet dataSet = new ProcessDataSet( publisherID, dataSetName, writerID, ProcessDataSet.DataSetType.TimeSeries );
```

Now you can create a Datapoint and add it to the previously created `ProcessData` instance:
```csharp
DPSEvent dataPoint = new DPSEvent()
{
        Name      = "Sample DPS",
        Value     = 2,
        Orcat     = OrcatConstants.Process,
        Quality   = QualityConstants.Good,
        Timestamp = DateTime.Now.ToFileTimeUtc()
};

dataSet.AddDataPoint( dataPoint );
```

Now you can get the binary encoded Meta-Message and as well a binary encoded Key- or Delta-Message from the `ProcessData` instance:

```csharp
ushort sequenceNumber = 1;
byte[]         encodedMeta        = dataSet.GetEncodedMetaFrame( new EncodingOptions(), sequenceNumber++ );
byte[]         encodedKey         = dataSet.GetEncodedDeltaFrame( sequenceNumber++ );
```

More advanced use cases including the usage of MQTT are available in the developer documentation.

## Build
Building just the library requires only [.NET Core 3.1 SDK](https://dotnet.microsoft.com/download/dotnet-core/3.1) and can be done on any operating system supported by .NET Core 3.1.

Building the entire repository including sample applications can done only on Windows and requires additionally the installation of the [.NET Framework Developer Pack](https://dotnet.microsoft.com/download/dotnet-framework/thank-you/net48-developer-pack-offline-installer).
It's recommended to use Visual Studio for building the entire repository.

# Contributions
Contributions are always welcome! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for details.

# License

This project use the following license: [MIT](LICENSE.md)
