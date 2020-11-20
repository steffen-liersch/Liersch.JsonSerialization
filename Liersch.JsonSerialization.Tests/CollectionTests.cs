/*--------------------------------------------------------------------------*\
::
::  Copyright © 2020 Steffen Liersch
::  https://www.steffen-liersch.de/
::
\*--------------------------------------------------------------------------*/

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Liersch.Json.Tests
{
  [TestClass]
  public class CollectionTests
  {
    [TestMethod]
    public void TestIntegerCollection()
    {
      var t1=new Container();
      t1.List=CreateList1();
      t1.Array=CreateList2().ToArray();

      var wr=new JsonWriter(indented: false);
      new JsonSerializer().Serialize(t1, wr);
      string json=wr.ToString();
      Assert.AreEqual(@"{""x"":[{""a"":100,""b"":0},{""a"":0,""b"":200}],""y"":[{""a"":""abc"",""b"":null},{""a"":null,""b"":""def""}]}", json);

      var t2=new JsonDeserializer().Deserialize<Container>(json);
      Assert.AreEqual(2, t2.List.Count);
      Assert.AreEqual(100, t2.List[0].DirectValue);
      Assert.AreEqual(200, t2.List[1].PropertyValue);
      Assert.AreEqual(2, t2.Array.Length);
      Assert.AreEqual("abc", t2.Array[0].DirectValue);
      Assert.AreEqual("def", t2.Array[1].PropertyValue);
    }


    static List<TypedValues<int>> CreateList1()
    {
      var res=new List<TypedValues<int>>();
      res.Add(new TypedValues<int>() { DirectValue=100 });
      res.Add(new TypedValues<int>() { PropertyValue=200 });
      return res;
    }

    static List<TypedValues<string>> CreateList2()
    {
      var res=new List<TypedValues<string>>();
      res.Add(new TypedValues<string>() { DirectValue="abc" });
      res.Add(new TypedValues<string>() { PropertyValue="def" });
      return res;
    }


    class TypedValues<T>
    {
      [JsonMember("a")]
      public T DirectValue;

      [JsonMember("b")]
      public T PropertyValue { get; set; }
    }


    class Container
    {
      [JsonMember("x")]
      public List<TypedValues<int>> List;

      [JsonMember("y")]
      public TypedValues<string>[] Array;
    }
  }
}