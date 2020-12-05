[![NuGet](https://img.shields.io/nuget/v/Liersch.JsonSerialization.svg)](https://www.nuget.org/packages/Liersch.JsonSerialization)

# Liersch.JsonSerialization - JSON Support for .NET

`Liersch.JsonSerialization` extends [Liersch.Json](https://github.com/steffen-liersch/Liersch.Json) to include reflection-based serialization and deserialization. Based on [Liersch.Reflection](https://github.com/steffen-liersch/Liersch.Reflection), fields and properties are efficiently read and written using dynamically generated IL code.

The library supports the following .NET platforms:

- from .NET Framework 2.0
- from .NET Core 2.0
- from .NET Standard 2.1
- Mono

All major changes are logged in the [CHANGELOG.md](https://github.com/steffen-liersch/Liersch.JsonSerialization/blob/main/CHANGELOG.md) file.

## Getting Started

The easiest and the fastest way to integrate the library into a project is to use the [Liersch.JsonSerialization package published on NuGet](https://www.nuget.org/packages/Liersch.JsonSerialization). For older projects (before .NET Framework 4.0) the library and its dependent libraries have to be compiled and integrated manually.

## JsonSerializer and JsonDeserializer

The classes `JsonSerializer` and `JsonDeserializer` are based on reflection. Fields and properties to be processed by serialization and deserialization must be marked with `JsonMemberAttribute`. Only public fields and properties should be marked with this attribute. For deserialization a public standard constructor is required.

```cs
class Example
{
  [JsonMember("IntegerArray")]
  public int[] IntegerArray;

  [JsonMember("StringValue")]
  public string StringValue;

  public string NotSerializedString;
}
```

In the following example an instance of a serializable class is created, serialized and deserialized again.

```cs
var e1=new Example();
e1.IntegerArray=new int[] { 10, 20, 30, 700, 800 };
e1.StringValue="Example Text";
e1.NotSerializedString="Other Text";

string json=new JsonSerializer().Serialize(e1);
Example e2=new JsonDeserializer().Deserialize<Example>(json);
```

## License

The software is published under the conditions of an open source license. Alternatively, other terms can be agreed under a commercial license. You can support the maintenance and further development of the software with a [voluntary donation](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=NVXEQCNGJFK92).

## Copyright

Copyright © 2013-2020 Steffen Liersch  
https://www.steffen-liersch.de/

## Links

The source code is maintained on GitHub:  
https://github.com/steffen-liersch/Liersch.JsonSerialization

Packages can be downloaded through NuGet:  
https://www.nuget.org/packages/Liersch.JsonSerialization
