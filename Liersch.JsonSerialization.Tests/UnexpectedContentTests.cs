/*--------------------------------------------------------------------------*\
::
::  Copyright © 2021 Steffen Liersch
::  https://www.steffen-liersch.de/
::
\*--------------------------------------------------------------------------*/

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Liersch.Json.Tests
{
  [TestClass]
  public class UnexpectedContentTests
  {
    [TestMethod]
    public void TestGeneral()
    {
      var x1=new Example
      {
        ArrayValue=new NameValuePair[2],
        ObjectValue=new NameValuePair
        {
          Name="abc",
          Value=123
        },
        BooleanValue=true,
        IntegerValue=456,
        StringValue="def"
      };

      string json=new JsonSerializer().Serialize(x1);
      Example x2=new JsonDeserializer().Deserialize<Example>(json);

      Assert.AreEqual(x1.ArrayValue.Length, x2.ArrayValue.Length);
      for(int i = 0; i<x2.ArrayValue.Length; i++)
        Assert.AreEqual(null, x2.ArrayValue[i]);

      Assert.IsNotNull(x2.ObjectValue);
      Assert.AreEqual(x1.ObjectValue.Name, x2.ObjectValue.Name);
      Assert.AreEqual(x1.ObjectValue.Value, x2.ObjectValue.Value);

      Assert.AreEqual(x1.BooleanValue, x2.BooleanValue);
      Assert.AreEqual(x1.IntegerValue, x2.IntegerValue);
      Assert.AreEqual(x1.StringValue, x2.StringValue);
    }

    [TestMethod]
    public void TestEmpty()
    {
      Example x=new JsonDeserializer().Deserialize<Example>("{}");
      Assert.AreEqual(null, x.ArrayValue);
      Assert.AreEqual(null, x.ObjectValue);
      Assert.AreEqual(false, x.BooleanValue);
      Assert.AreEqual(0, x.IntegerValue);
      Assert.AreEqual(null, x.StringValue);
    }

    [TestMethod]
    public void TestUnexpectedForArray()
    {
      Example x=new JsonDeserializer().Deserialize<Example>("{\"arr\": true}");
      Assert.AreEqual(null, x.ArrayValue);
      Assert.AreEqual(null, x.ObjectValue);
      Assert.AreEqual(false, x.BooleanValue);
      Assert.AreEqual(0, x.IntegerValue);
      Assert.AreEqual(null, x.StringValue);
    }

    [TestMethod]
    public void TestUnexpectedForObject()
    {
      Example x=new JsonDeserializer().Deserialize<Example>("{\"obj\": true}");
      Assert.AreEqual(null, x.ArrayValue);
      Assert.AreEqual(null, x.ObjectValue);
      Assert.AreEqual(false, x.BooleanValue);
      Assert.AreEqual(0, x.IntegerValue);
      Assert.AreEqual(null, x.StringValue);
    }

    [TestMethod]
    public void TestUnexpectedForBoolean1()
    {
      Example x=new JsonDeserializer().Deserialize<Example>("{\"bool\": 0}");
      Assert.AreEqual(null, x.ArrayValue);
      Assert.AreEqual(null, x.ObjectValue);
      Assert.AreEqual(false, x.BooleanValue);
      Assert.AreEqual(0, x.IntegerValue);
      Assert.AreEqual(null, x.StringValue);
    }

    [TestMethod]
    public void TestUnexpectedForBoolean2()
    {
      Example x=new JsonDeserializer().Deserialize<Example>("{\"bool\": 1}");
      Assert.AreEqual(null, x.ArrayValue);
      Assert.AreEqual(null, x.ObjectValue);
      Assert.AreEqual(true, x.BooleanValue);
      Assert.AreEqual(0, x.IntegerValue);
      Assert.AreEqual(null, x.StringValue);
    }

    [TestMethod]
    public void TestUnexpectedForBoolean3()
    {
      Example x=new JsonDeserializer().Deserialize<Example>("{\"bool\": \"false\"}");
      Assert.AreEqual(null, x.ArrayValue);
      Assert.AreEqual(null, x.ObjectValue);
      Assert.AreEqual(false, x.BooleanValue);
      Assert.AreEqual(0, x.IntegerValue);
      Assert.AreEqual(null, x.StringValue);
    }

    [TestMethod]
    public void TestUnexpectedForBoolean4()
    {
      Example x=new JsonDeserializer().Deserialize<Example>("{\"bool\": \"true\"}");
      Assert.AreEqual(null, x.ArrayValue);
      Assert.AreEqual(null, x.ObjectValue);
      Assert.AreEqual(true, x.BooleanValue);
      Assert.AreEqual(0, x.IntegerValue);
      Assert.AreEqual(null, x.StringValue);
    }

    [TestMethod]
    public void TestUnexpectedForInteger()
    {
      Example x=new JsonDeserializer().Deserialize<Example>("{\"int\": true}");
      Assert.AreEqual(null, x.ArrayValue);
      Assert.AreEqual(null, x.ObjectValue);
      Assert.AreEqual(false, x.BooleanValue);
      Assert.AreEqual(0, x.IntegerValue);
      Assert.AreEqual(null, x.StringValue);
    }

    [TestMethod]
    public void TestUnexpectedForString1()
    {
      Example x=new JsonDeserializer().Deserialize<Example>("{\"str\": true}");
      Assert.AreEqual(null, x.ArrayValue);
      Assert.AreEqual(null, x.ObjectValue);
      Assert.AreEqual(false, x.BooleanValue);
      Assert.AreEqual(0, x.IntegerValue);
      Assert.AreEqual("true", x.StringValue);
    }

    [TestMethod]
    public void TestUnexpectedForString2()
    {
      Example x=new JsonDeserializer().Deserialize<Example>("{\"str\": 123}");
      Assert.AreEqual(null, x.ArrayValue);
      Assert.AreEqual(null, x.ObjectValue);
      Assert.AreEqual(false, x.BooleanValue);
      Assert.AreEqual(0, x.IntegerValue);
      Assert.AreEqual("123", x.StringValue);
    }

    class Example
    {
      [JsonMember("arr")]
      public NameValuePair[] ArrayValue;

      [JsonMember("obj")]
      public NameValuePair ObjectValue;

      [JsonMember("bool")]
      public bool BooleanValue;

      [JsonMember("int")]
      public int IntegerValue;

      [JsonMember("str")]
      public string StringValue;
    }

    class NameValuePair
    {
      [JsonMember("name")]
      public string Name;

      [JsonMember("value")]
      public int Value;
    }
  }
}