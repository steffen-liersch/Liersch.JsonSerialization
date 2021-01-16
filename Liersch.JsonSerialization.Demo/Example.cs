/*--------------------------------------------------------------------------*\
::
::  Copyright © 2013-2021 Steffen Liersch
::  https://www.steffen-liersch.de/
::
\*--------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace Liersch.Json
{
  static class Example
  {
    public static void Run()
    {
      Console.WriteLine("Liersch.JsonSerialization Example");
      Console.WriteLine("=================================");
      Console.WriteLine();

      var c1=new Container();
      c1.PersonList=new List<Person>();
      c1.PersonList.Add(new Person() { LastName="Doe", FirstName="John" });
      c1.PersonList.Add(new Person() { LastName="Smith", FirstName="Jane" });
      c1.IntegerList=new List<int> { 10, 20, 30 };
      c1.IntegerArray=new int[] { 700, 800 };
      c1.StringValue="Example Text";
      c1.NotSerializedString="Other Text";

      string json=new JsonSerializer().Serialize(c1);
      Container c2=new JsonDeserializer().Deserialize<Container>(json);

      string f="{0,-24} => {1,16} - {2}";
      Console.WriteLine(string.Format(f, "Object", "e1", "e2"));

      CompareLists(f, "PersonList", c1.PersonList, c2.PersonList);
      CompareLists(f, "IntegerList", c1.IntegerList, c2.IntegerList);
      CompareLists(f, "IntegerArray", c1.IntegerArray, c2.IntegerArray);

      Console.WriteLine(string.Format(f, "StringValue", c1.StringValue, c2.StringValue));
      Console.WriteLine(string.Format(f, "NotSerializedString", c1.NotSerializedString, c2.NotSerializedString));
      Console.WriteLine();
    }

    static void CompareLists<T>(string format, string name, IList<T> list1, IList<T> list2)
    {
      int c1=list1.Count;
      int c2=list2.Count;

      Console.WriteLine(string.Format(format, name+".Count", c1, c2));

      int c=Math.Min(c1, c2);
      for(int i=0; i<c; i++)
        Console.WriteLine(string.Format(format, name+"["+i+"]", list1[i], list2[i]));
    }

    class Container
    {
      [JsonMember("PersonList")]
      public List<Person> PersonList;

      [JsonMember("IntegerList")]
      public List<int> IntegerList;

      [JsonMember("IntegerArray")]
      public int[] IntegerArray;

      [JsonMember("StringValue")]
      public string StringValue;

      public string NotSerializedString;
    }

    class Person
    {
      [JsonMember("LastName")]
      public string LastName;

      [JsonMember("FirstName")]
      public string FirstName;

      public override string ToString() { return FirstName+" "+LastName; }
    }
  }
}