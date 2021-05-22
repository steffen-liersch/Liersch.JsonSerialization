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
  public class OptionTests
  {
    [TestMethod]
    public void TestGeneral()
    {
      var a=new Container
      {
        X="abc",
        Y=123,
        Z=new object()
      };

      string json=new JsonSerializer().Serialize(a);
      Container b=new JsonDeserializer().Deserialize<Container>(json);

      Assert.AreEqual(a.X, b.X);
      Assert.AreEqual(a.Y, b.Y);
      Assert.IsNotNull(b.Z);
    }

    [TestMethod]
    public void TestClassWithDifferentOptions()
    {
      var c=new Container();
      c.Z=123;
      Assert.AreEqual("{\"z\":123}", Serialize(c, false, false));
      Assert.AreEqual("{\"z\":{}}", Serialize(c, false, true));
      Assert.AreEqual("{\"x\":null,\"y\":null,\"z\":123}", Serialize(c, true, false));
      Assert.AreEqual("{\"x\":null,\"y\":null,\"z\":{}}", Serialize(c, true, true));
    }

    [TestMethod]
    public void TestValuesWithDifferentOptions()
    {
      Assert.AreEqual("{}", Serialize<object>(123, false, true));
      Assert.AreEqual("123", Serialize<object>(123, false, false));
      Assert.AreEqual("123", Serialize(123, false, true));
      Assert.AreEqual("123", Serialize(123, false, false));

      TestSerializeAllOptions<object>("null", null);
      TestSerializeAllOptions("false", false);
      TestSerializeAllOptions("true", true);
      TestSerializeAllOptions("0", 0);
      TestSerializeAllOptions("123", 123);
      TestSerializeAllOptions("3.14", 3.14);
      TestSerializeAllOptions("\"abc\"", "abc");
    }

    static void TestSerializeAllOptions<T>(string json, T value)
    {
      for(int i = 0; i<=1; i++)
        for(int j = 0; j<=1; j++)
          Assert.AreEqual(json, Serialize(value, i!=0, j!=0));
    }

    static string Serialize<T>(T value, bool includeNullValues, bool useExactMemberTypeOnly)
    {
      var wr=new JsonWriter(indented: false);

      var ser=new JsonSerializer();
      ser.IncludeNullValues=includeNullValues;
      ser.UseExactMemberTypeOnly=useExactMemberTypeOnly;

      ser.Serialize(value, wr);
      return wr.ToString();
    }

    class Container
    {
      [JsonMember("x")]
      public string X;

      [JsonMember("y")]
      public int? Y;

      [JsonMember("z")]
      public object Z;
    }
  }
}