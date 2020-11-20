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
  public class SimpleTests
  {
    [TestMethod]
    public void TestSimple()
    {
      var t1=new TypedValues() {IgnoredValue=100, DirectValue=200, PropertyValue=300};

      var wr=new JsonWriter(indented: false);
      new JsonSerializer().Serialize(t1, wr);
      string json=wr.ToString();
      Assert.AreEqual(@"{""a"":200,""b"":300}", json);

      var t2=new JsonDeserializer().Deserialize<TypedValues>(json);
      Assert.AreEqual(0, t2.IgnoredValue);
      Assert.AreEqual(200, t2.DirectValue);
      Assert.AreEqual(300, t2.PropertyValue);
    }

    class TypedValues
    {
      public int IgnoredValue;

      [JsonMember("a")]
      public int DirectValue;

      [JsonMember("b")]
      public int PropertyValue { get; set; }
    }
  }
}