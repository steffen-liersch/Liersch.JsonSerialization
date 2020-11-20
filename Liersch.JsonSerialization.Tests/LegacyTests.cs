/*--------------------------------------------------------------------------*\
::
::  Copyright © 2013-2020 Steffen Liersch
::  https://www.steffen-liersch.de/
::
\*--------------------------------------------------------------------------*/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Liersch.Json.Tests
{
  [TestClass]
  public class LegacyTests
  {
    [TestMethod]
    public void TestNormal()
    {
      ExampleOuter o1=CreateExample();
      Assert.AreEqual(2, o1.ValueObject.Value);
      Assert.AreEqual(4, o1.ValueObject.OtherValue);

      string s1=new JsonSerializer().Serialize(o1);

      var d=new JsonDeserializer();
      ExampleOuter o2=d.Deserialize<ExampleOuter>(s1);

      CompareSomeFields(o1, o2);
      Assert.AreEqual(2.345f, o1.RetrievePrivateValue());
      Assert.AreEqual(0, o2.RetrievePrivateValue());
      Assert.AreEqual(0, o2.ValueObject.OtherValue);

      string s2=new JsonSerializer().Serialize(o2);
      Assert.AreEqual(s1, s2);

      JsonNode n1=JsonParser.Parse(s1);
      JsonNode n2=n1.Clone();
      JsonNode n3=JsonParser.Parse(n2.AsJson);
      JsonNode n4=JsonParser.Parse(n3.AsJsonCompact);
      Assert.IsTrue(n1!=n2);
      CompareNodes(n1, n1);
      CompareNodes(n1, n2);
      CompareNodes(n1, n3);
      CompareNodes(n1, n4);
    }

    static ExampleOuter CreateExample()
    {
      var res=new ExampleOuter()
      {
        ValueString="Test",
        ValueStringArray=new string[] { "A", "B,", "C" },
        ValueDoubleArray=new double[] { 2, 3.14, 10000 },
        PropertyInteger=27,
        PropertyDateTime=new DateTime(2017, 12, 27, 14, 30, 0),
      };

      res.ValueObject=new ExampleInner(2);
      res.ValueObjectArray=new ExampleInner[] { new ExampleInner(4), new ExampleInner(6) };
      res.ChangePrivateValue(2.345f);
      return res;
    }

    static void CompareSomeFields(ExampleOuter o1, ExampleOuter o2)
    {
      Assert.AreEqual(o1.ValueBoolean1, o2.ValueBoolean1);
      Assert.AreEqual(o1.ValueBoolean2, o2.ValueBoolean2);
      Assert.AreEqual(o1.ValueBoolean3, o2.ValueBoolean3);
      Assert.AreEqual(o1.ValueString, o2.ValueString);
      Assert.AreEqual(o1.PropertyInteger, o2.PropertyInteger);
      Assert.AreEqual(o1.PropertyDateTime, o2.PropertyDateTime);
    }

    void CompareNodes(JsonNode n1, JsonNode n2)
    {
      Assert.AreEqual(n1.NodeType, n2.NodeType);
      Assert.AreEqual(n1.AsString, n2.AsString);

      if(n1.IsArray && n2.IsArray)
      {
        int c1=n1.Count;
        int c2=n2.Count;
        Assert.AreEqual(c1, c2);
        if(c1==c2)
          for(int i = 0; i<c1; i++)
            CompareNodes(n1[i], n2[i]);
      }

      if(n1.IsObject && n2.IsObject)
      {
        int c1=n1.Count;
        int c2=n2.Count;
        Assert.AreEqual(c1, c2);
        if(c1==c2)
          foreach(string k in n1.Names)
            CompareNodes(n1[k], n2[k]);
      }
    }


    [TestMethod]
    public void TestWithConverter1()
    {
      var ser=new JsonSerializer();

      const string prefix="prefix: ";
      ser.RegisterConverter<DateTime>(x => prefix+x.ToString(@"yyyy\-MM\-dd HH\:mm\:ss", CultureInfo.InvariantCulture));

      var o1=new ExampleOuter();
      o1.PropertyDateTime=new DateTime(1950, 7, 20, 12, 34, 56);

      string s=ser.Serialize(o1);
      Assert.IsTrue(s.Contains(prefix+o1.PropertyDateTime.ToString(@"yyyy\-MM\-dd HH\:mm\:ss", CultureInfo.InvariantCulture)));

      var des=new JsonDeserializer();
      des.RegisterConverter<DateTime>(x =>
      {
        string z=x.Substring(x.IndexOf(prefix)+prefix.Length);
        return DateTime.Parse(z, CultureInfo.InvariantCulture);
      });

      var o2=des.Deserialize<ExampleOuter>(s);
      Assert.AreEqual(o1.PropertyDateTime, o2.PropertyDateTime);
    }


    [TestMethod]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "Called function has assertions")]
    public void TestWithConverter2()
    {
      var ser=new JsonSerializer();
      TestStringConversion(ser, null);

      ser.RegisterConverter<string>(x => x ?? string.Empty);
      TestStringConversion(ser, string.Empty);

      ser.RegisterConverter<string>(x => x ?? "empty");
      TestStringConversion(ser, "empty");
    }

    static void TestStringConversion(JsonSerializer serializer, string expectedValue)
    {
      var o1=new ExampleOuter();
      string s=serializer.Serialize(o1);
      var o2=new JsonDeserializer().Deserialize<ExampleOuter>(s);
      Assert.AreEqual(expectedValue, o2.ValueString);
    }


    class ExampleOuter
    {
      [JsonMember("ValuePrivate")]
      float ValuePrivate;

      [JsonMember("ValueObject")]
      public ExampleInner ValueObject;

      [JsonMember("ValueObjectArray")]
      public ExampleInner[] ValueObjectArray;

      [JsonMember("ValueBoolean1")]
      public bool? ValueBoolean1=null;

      [JsonMember("ValueBoolean2")]
      public bool? ValueBoolean2=false;

      [JsonMember("ValueBoolean3")]
      public bool? ValueBoolean3=true;

      [JsonMember("ValueString")]
      public string ValueString;

      [JsonMember("ValueStringArray")]
      public string[] ValueStringArray;

      [JsonMember("ValueDoubleArray")]
      public double[] ValueDoubleArray;

      [JsonMember("PropertyInteger")]
      public int PropertyInteger { get; set; }

      [JsonMember("PropertyDateTime ")]
      public DateTime PropertyDateTime { get; set; }

      public float RetrievePrivateValue() { return ValuePrivate; }

      public void ChangePrivateValue(float value) { ValuePrivate=value; }
    }


    class ExampleInner
    {
      [JsonMember("Value")]
      public int Value;

      public int OtherValue;

      public ExampleInner() { }
      public ExampleInner(int value) { Value=value; OtherValue=value*value; }
    }
  }
}