/*--------------------------------------------------------------------------*\
::
::  Copyright © 2020 Steffen Liersch
::  https://www.steffen-liersch.de/
::
\*--------------------------------------------------------------------------*/

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Liersch.Json.Tests
{
  [TestClass]
  public class GenericTests
  {
    [TestMethod]
    public void TestInteger()
    {
      var t1=new TypedValues<int>() { IgnoredValue=100, DirectValue=200, PropertyValue=300 };
      
      var wr=new JsonWriter(indented: false);
      new JsonSerializer().Serialize(t1, wr);
      string json=wr.ToString();
      Assert.AreEqual(@"{""a"":200,""b"":300}", json);
      
      var t2=new JsonDeserializer().Deserialize<TypedValues<int>>(json);
      Assert.AreEqual(0, t2.IgnoredValue);
      Assert.AreEqual(200, t2.DirectValue);
      Assert.AreEqual(300, t2.PropertyValue);
    }

    [TestMethod]
    public void TestString()
    {
      var t1=new TypedValues<string>() { IgnoredValue="100", DirectValue="200", PropertyValue="300" };
      
      var wr=new JsonWriter(indented: false);
      new JsonSerializer().Serialize(t1, wr);
      string json=wr.ToString();
      Assert.AreEqual(@"{""a"":""200"",""b"":""300""}", json);
      
      var t2=new JsonDeserializer().Deserialize<TypedValues<string>>(json);
      Assert.AreEqual(null, t2.IgnoredValue);
      Assert.AreEqual("200", t2.DirectValue);
      Assert.AreEqual("300", t2.PropertyValue);
    }

    [TestMethod]
    public void TestContainerWithUntypedValues()
    {
      var t1=new ContainerWithUntypedValues();
      t1.X=new TypedValues<int>() { IgnoredValue=100, DirectValue=200, PropertyValue=300 };
      t1.Y=new TypedValues<string>() { IgnoredValue="400", DirectValue="500", PropertyValue="600" };
      t1.Z=null;
      
      var wr=new JsonWriter(indented: false);
      new JsonSerializer().Serialize(t1, wr);
      string json=wr.ToString();
      Assert.AreEqual(@"{""x"":{},""y"":{},""z"":null}", json);
      
      var t2=new JsonDeserializer().Deserialize<ContainerWithUntypedValues>(json);
      Assert.IsNotNull(t2.X);
      Assert.IsNotNull(t2.Y);
      Assert.IsNull(t2.Z);
    }

    [TestMethod]
    public void TestContainerWithTypedValues()
    {
      var t1=new ContainerWithTypedValues();
      t1.X=new TypedValues<int>() { IgnoredValue=100, DirectValue=200, PropertyValue=300 };
      t1.Y=new TypedValues<string>() { IgnoredValue="400", DirectValue="500", PropertyValue="600" };
      t1.Z=null;
      
      var wr=new JsonWriter(indented: false);
      new JsonSerializer().Serialize(t1, wr);
      string json=wr.ToString();
      Assert.AreEqual(@"{""x"":{""a"":200,""b"":300},""y"":{""a"":""500"",""b"":""600""},""z"":null}", json);
      
      var t2=new JsonDeserializer().Deserialize<ContainerWithTypedValues>(json);
      Assert.AreEqual(200, t2.X.DirectValue);
      Assert.AreEqual(300, t2.X.PropertyValue);
      Assert.AreEqual("500", t2.Y.DirectValue);
      Assert.AreEqual("600", t2.Y.PropertyValue);
      Assert.IsNull(t2.Z);
    }

    class TypedValues<T>
    {
      public T IgnoredValue;

      [JsonMember("a")]
      public T DirectValue;

      [JsonMember("b")]
      public T PropertyValue { get; set; }
    }

    class ContainerWithTypedValues
    {
      [JsonMember("x")]
      public TypedValues<int> X;

      [JsonMember("y")]
      public TypedValues<string> Y;

      [JsonMember("z")]
      public TypedValues<string> Z;
    }

    class ContainerWithUntypedValues
    {
      [JsonMember("x")]
      public object X;

      [JsonMember("y")]
      public object Y;

      [JsonMember("z")]
      public object Z;
    }
  }
}