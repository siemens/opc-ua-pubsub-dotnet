# Simulated Publisher

This is a commandline tool which can send OPC UA PubSub messages to any MQTT broker. Configuration is done via the "Simulation" section of the settings.json / via commandline arguments.
The OPC UA PubSub messages are configured via an Excel sheet which must be provided via the "InputFile" setting.

```JSON
    "Simulation": {
      "BrokerHostname": "localhost",
      "ClientCertP12": "D:\\Certs\\IoT-Client-RSA2k.p12",
      "ClientCertPassword": "12345678",
      "ClientRootCACertDER": "D:\\Certs\\IoT-Broker-CA-RSA4k.der",
      "BrokerCACertDER": "D:\\Certs\\IoT-Broker-CA-RSA4k.der",

      // Time in ms to wait before sending the meta message.
      // This is sometime useful if the local broker is used so it has enough time to start.
      "WaitBeforeStarting": 1500,

      // Time in ms to wait after sending the meta message before sending the key message.
      "WaitAfterMetaMessage": 1500,

      // Time in ms to wait after sending the key message to proceed with the delta message.
      "WaitAfterKeyMessage": 1500,

      // Time to wait before exiting the Simulation.
      // This is required, because MQTTNet does implement correctly
      // the async-wait pattern and it's not possible to wait until
      // the last publishing action is finished.
      "WaitBeforeClosing": 5000,

      
      //"InputFile": "Input-ProcessData.xlsx",
      "PublisherID": "SIP6~Thomas~Name~BM00000001"
    }
```

If you want to override settings from the file you can pass them via command line. For every setting you need to use the prefix "Settings:Simulation:" similar to this example:

```shell
SimulatedPublisher.exe --Settings:Simulation:InputFile=lib\Simu-A.xlsx --Settings:Simulation:WaitBeforeStarting=0 --Settings:Simulation:SkipMeta=true --Settings:Simulation:SkipKey=true
```


## Configuration of OPC UA PubSub
