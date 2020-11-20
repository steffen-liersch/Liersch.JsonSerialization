/*--------------------------------------------------------------------------*\
::
::  Copyright © 2020 Steffen Liersch
::  https://www.steffen-liersch.de/
::
\*--------------------------------------------------------------------------*/

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Liersch.Json.Tests
{
  [TestClass]
  public class DateTimeTests
  {
    [TestMethod]
    public void TestDateTime()
    {
      string json=@"
{
  ""d"": 123,
  ""t"": 456
}
";

      var e1=new JsonDeserializer().Deserialize<Example>(json);
      Assert.AreEqual(new DateTime(1970, 1, 1).AddSeconds(123), e1.Date);
      Assert.AreEqual(TimeSpan.FromSeconds(456), e1.Time);

      e1.Date=e1.Date.AddDays(1);
      e1.Time=e1.Time.Add(TimeSpan.FromHours(1));

      var wr=new JsonWriter(indented: false);
      new JsonSerializer().Serialize(e1, wr);
      string s=wr.ToString();
      Assert.AreEqual(@"{""d"":""1970-01-02 00:02:03"",""t"":""01:07:36""}", s);

      var e2=new JsonDeserializer().Deserialize<Example>(s);
      Assert.AreEqual(e1.Date, e2.Date);
      Assert.AreEqual(e1.Time, e2.Time);
    }

    class Example
    {
      [JsonMember("d")]
      public DateTime Date;

      [JsonMember("t")]
      public TimeSpan Time;
    }
  }
}