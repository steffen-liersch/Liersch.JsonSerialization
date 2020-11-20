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
  public class NullableTests
  {
    [TestMethod]
    public void TestZero()
    {
      var t1=new Values();
   
      var wr=new JsonWriter(indented: false);
      new JsonSerializer().Serialize(t1, wr);
      string json=wr.ToString();
      Assert.AreEqual(@"{""b"":null,""s8"":null,""u8"":null,""s16"":null,""u16"":null,""s32"":null,""u32"":null,""s64"":null,""u64"":null,""f32"":null,""f64"":null}", json);
      
      var t2=new JsonDeserializer().Deserialize<Values>(json);
      Assert.IsNull(t2.B);
      Assert.IsNull(t2.S32);
      Assert.IsNull(t2.U32);
    }

    [TestMethod]
    public void TestValues()
    {
      var t1=new Values() { B=true, S8=1, U8=2, S16=3, U16=4, S32=5, U32=6, S64=7, U64=8, F32=9, F64=10 };
      
      var wr=new JsonWriter(indented: false);
      new JsonSerializer().Serialize(t1, wr);
      string json=wr.ToString();
      Assert.AreEqual(@"{""b"":true,""s8"":1,""u8"":2,""s16"":3,""u16"":4,""s32"":5,""u32"":6,""s64"":7,""u64"":8,""f32"":9,""f64"":10}", json);
      
      var t2=new JsonDeserializer().Deserialize<Values>(json);
      Assert.AreEqual(true, t2.B);
      Assert.AreEqual((sbyte)1, t2.S8);
      Assert.AreEqual((byte)2, t2.U8);
      Assert.AreEqual((short)3, t2.S16);
      Assert.AreEqual((ushort)4, t2.U16);
      Assert.AreEqual(5, t2.S32);
      Assert.AreEqual(6U, t2.U32);
      Assert.AreEqual(7, t2.S64);
      Assert.AreEqual(8LU, t2.U64);
      Assert.AreEqual(9F, t2.F32);
      Assert.AreEqual(10.0, t2.F64);
    }


    class Values
    {
      [JsonMember("b")]
      public bool? B;

      [JsonMember("s8")]
      public sbyte? S8;

      [JsonMember("u8")]
      public byte? U8;

      [JsonMember("s16")]
      public short? S16;

      [JsonMember("u16")]
      public ushort? U16;

      [JsonMember("s32")]
      public int? S32;

      [JsonMember("u32")]
      public uint? U32;

      [JsonMember("s64")]
      public long? S64;

      [JsonMember("u64")]
      public ulong? U64;

      [JsonMember("f32")]
      public float? F32;

      [JsonMember("f64")]
      public double? F64;
    }
  }
}