/*--------------------------------------------------------------------------*\
::
::  Copyright © 2021 Steffen Liersch
::  https://www.steffen-liersch.de/
::
\*--------------------------------------------------------------------------*/

using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Liersch.Json.Tests
{
  [TestClass]
  public class EnumTests
  {
    [TestMethod]
    public void TestEnum()
    {
      Example x1=CreateExample();

      string json=new JsonSerializer().Serialize(x1);
      Example x2=new JsonDeserializer().Deserialize<Example>(json);

      Assert.AreEqual(x1.Error1, x2.Error1);
      Assert.AreEqual(x1.Error2, x2.Error2);
      Assert.AreEqual(x1.Error3, x2.Error3);
    }

    [TestMethod]
    public void TestEnumWithConverter()
    {
      Example x1=CreateExample();

      var ser=new JsonSerializer();
      ser.RegisterConverter<ErrorCode>(v => ((int)v).ToString(CultureInfo.InvariantCulture));

      var deser=new JsonDeserializer();
      deser.RegisterConverter(v => (ErrorCode)int.Parse(v, CultureInfo.InvariantCulture));

      string json=ser.Serialize(x1);
      Example x2=deser.Deserialize<Example>(json);

      Assert.AreEqual(x1.Error1, x2.Error1);
      Assert.AreEqual(x1.Error2, x2.Error2);
      Assert.AreEqual(x1.Error3, x2.Error3);
    }


    [TestMethod]
    public void TestSerialize()
    {
      var wr=new JsonWriter(false);
      new JsonSerializer().Serialize(CreateExample(), wr);
      string json=wr.ToString();
      Assert.AreEqual("{\"error1\":\"NotFound\",\"error2\":\"UnexpectedValue\",\"error3\":null}", json);
    }

    [TestMethod]
    public void TestSerializeWithConverter()
    {
      var ser=new JsonSerializer();
      ser.RegisterConverter<ErrorCode>(v => ((int)v).ToString(CultureInfo.InvariantCulture));

      var wr=new JsonWriter(false);
      ser.Serialize(CreateExample(), wr);
      string json=wr.ToString();
      Assert.AreEqual("{\"error1\":\"1\",\"error2\":\"2\",\"error3\":null}", json);
    }


    [TestMethod]
    public void TestDeserializeUnexpected()
    {
      string json="{\"error1\":\"NotFound\",\"error2\":true,\"error3\":null}";
      Example x=new JsonDeserializer().Deserialize<Example>(json);
      Assert.AreEqual(ErrorCode.NotFound, x.Error1);
      Assert.AreEqual(null, x.Error2);
      Assert.AreEqual(null, x.Error3);
    }


    static Example CreateExample()
    {
      return new Example
      {
        Error1=ErrorCode.NotFound,
        Error2=ErrorCode.UnexpectedValue,
        Error3=null,
      };
    }


    enum ErrorCode
    {
      None,
      NotFound,
      UnexpectedValue,
    }

    class Example
    {
      [JsonMember("error1")]
      public ErrorCode Error1;

      [JsonMember("error2")]
      public ErrorCode? Error2;

      [JsonMember("error3")]
      public ErrorCode? Error3;
    }
  }
}