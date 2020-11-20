/*--------------------------------------------------------------------------*\
::
::  Copyright © 2013-2020 Steffen Liersch
::  https://www.steffen-liersch.de/
::
\*--------------------------------------------------------------------------*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Liersch.Reflection;

namespace Liersch.Json
{
  public sealed class JsonDeserializer
  {
    public void RegisterConverter(Type type, JsonConverter<string, object> converter)
    {
      if(type==null)
        throw new ArgumentNullException("type");

      if(converter==null)
        throw new ArgumentNullException("converter");

      if(m_Converters==null)
        m_Converters=new Dictionary<Type, JsonConverter<string, object>>();

      m_Converters[type]=converter;
    }

    public void RegisterConverter<T>(JsonConverter<string, T> converter)
    {
      if(converter==null)
        throw new ArgumentNullException("converter");

      if(m_Converters==null)
        m_Converters=new Dictionary<Type, JsonConverter<string, object>>();

      m_Converters[typeof(T)]=x => converter(x);
    }


    public T Deserialize<T>(string json) where T : new()
    {
      var tokenizer=new JsonTokenizer(json);
      return Deserialize<T>(tokenizer);
    }

    T Deserialize<T>(JsonTokenizer tokenizer) where T : new()
    {
      if(tokenizer.TryReadNext() && tokenizer.HasSpecialChar && tokenizer.SpecialChar=='{')
      {
        object v;
        if(TryDeserializeValue(tokenizer, typeof(T), out v))
          return (T)v;
      }
      return new T();
    }


    bool TryDeserializeValue(JsonTokenizer tokenizer, Type type, out object value)
    {
      JsonConverter<string, object> conv;

      if(tokenizer.Token=="null")
      {
        if(m_Converters!=null && m_Converters.TryGetValue(type, out conv))
        {
          value=conv(null);
          return true;
        }

        value=null;
        return !type.IsValueType || type.GetGenericTypeDefinition()==typeof(Nullable<>);
      }

      if(m_Converters!=null && tokenizer.TokenIsString && m_Converters.TryGetValue(type, out conv))
      {
        value=conv(tokenizer.Token);
        return true;
      }

      return m_Deserializers.GetOrAdd(type, RetrieveDeserializer2)(this, tokenizer, out value);
    }

    object DeserializeArray(JsonTokenizer tokenizer, Type elementType)
    {
      if(!tokenizer.BeginReadArray())
        return Array.CreateInstance(elementType, 0);

      var list=new List<object>();
      DeserializeCollection(tokenizer, elementType, list.Add);

      int c=list.Count;
      var res=Array.CreateInstance(elementType, c);
      for(int i = 0; i<c; i++)
        res.SetValue(list[i], i);

      return res;
    }

    void DeserializeCollection(JsonTokenizer tokenizer, Type elementType, Action<object> add)
    {
      while(true)
      {
        object v;
        if(TryDeserializeValue(tokenizer, elementType, out v))
          add(v);

        tokenizer.ReadNext();
        switch(tokenizer.SpecialChar)
        {
          case ',': tokenizer.ReadNext(); break;
          case ']': return;
          default: throw new JsonException("Unexpected token");
        }
      }
    }

    void DeserializeObject(JsonTokenizer tokenizer, object instance, JsonTypeCacheEntry cache)
    {
      bool isNameExpected=false;
      while(true)
      {
        if(isNameExpected)
          tokenizer.ReadString();
        else
        {
          tokenizer.ReadNext();
          if(tokenizer.SpecialChar=='}')
            return;

          if(!tokenizer.TokenIsString)
            throw new JsonException("String expected");

          isNameExpected=true;
        }

        string n=tokenizer.Token;

        tokenizer.ReadColon();

        JsonTypeMemberEntry me;
        if(cache.Members!=null && cache.Members.TryGetValue(n, out me) && me.WriteValue!=null)
        {
          tokenizer.ReadNext();

          object v;
          if(TryDeserializeValue(tokenizer, me.MemberType, out v))
            me.WriteValue(instance, v);
        }
        else tokenizer.SkipValue();

        tokenizer.ReadNext();

        switch(tokenizer.SpecialChar)
        {
          case ',': break;
          case '}': return;
          default: throw new JsonException("Unexpected token");
        }
      }
    }


    static DeserializerDelegate RetrieveDeserializer2(Type type)
    {
      if(type.IsArray)
      {
        if(type.GetArrayRank()!=1)
          throw new NotSupportedException("Multi-dimensional arrays are not supported");

        Type et=type.GetElementType();
        return (JsonDeserializer deserializer, JsonTokenizer tokenizer, out object value) =>
        {
          value=deserializer.DeserializeArray(tokenizer, et);
          return true;
        };
      }

      if(type.IsGenericType)
      {
        Type[] ga=type.GetGenericArguments();
        if(ga.Length==1)
        {
          Type gt=type.GetGenericTypeDefinition();
          Type ga0=ga[0];

          Type lt=typeof(IList<>).MakeGenericType(ga0);
          if(lt.IsAssignableFrom(type))
            return RetrieveDeserializer2_IList(type, ga0);

          if(gt==typeof(Nullable<>))
            return (JsonDeserializer deserializer, JsonTokenizer tokenizer, out object value) =>
              deserializer.TryDeserializeValue(tokenizer, ga0, out value);
        }
      }

      return RetrieveDeserializer2_object(type);
    }

    static DeserializerDelegate RetrieveDeserializer2_IList(Type type, Type ga0)
    {
      Func0 create=Accelerator.CreateStandardConstructor(type);

      MethodInfo m=type.GetMethod("Add", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { ga0 }, null);
      if(m==null)
        throw new InvalidOperationException();

      Action2 a=Accelerator.CreateAction2(m);

      return (JsonDeserializer deserializer, JsonTokenizer tokenizer, out object value) =>
      {
        if(tokenizer.BeginReadArray())
        {
          object list=create();
          deserializer.DeserializeCollection(tokenizer, ga0, x => a(list, x));
          value=list;
          return true;
        }
        else
        {
          value=null;
          return false;
        }
      };
    }

    static DeserializerDelegate RetrieveDeserializer2_object(Type type)
    {
      Func0 create=Accelerator.CreateStandardConstructor(type);
      var cache=JsonTypeCache.Instance.RetrieveCacheEntry(type);

      return (JsonDeserializer deserializer, JsonTokenizer tokenizer, out object value) =>
      {
        value=create();
        deserializer.DeserializeObject(tokenizer, value, cache);
        return true;
      };
    }


    static ConcurrentDictionary<Type, DeserializerDelegate> CreateDeserializers()
    {
      var res=new ConcurrentDictionary<Type, DeserializerDelegate>();
      res.TryAdd(typeof(bool), DeserializeBoolean);
      res.TryAdd(typeof(sbyte), DeserializeInteger_sbyte);
      res.TryAdd(typeof(byte), DeserializeInteger_byte);
      res.TryAdd(typeof(short), DeserializeInteger_short);
      res.TryAdd(typeof(ushort), DeserializeInteger_ushort);
      res.TryAdd(typeof(int), DeserializeInteger_int);
      res.TryAdd(typeof(uint), DeserializeInteger_uint);
      res.TryAdd(typeof(long), DeserializeInteger_long);
      res.TryAdd(typeof(ulong), DeserializeInteger_ulong);
      res.TryAdd(typeof(float), DeserializeInteger_float);
      res.TryAdd(typeof(double), DeserializeInteger_double);
      res.TryAdd(typeof(string), DeserializeInteger_string);
      res.TryAdd(typeof(DateTime), DeserializeDateTime);
      res.TryAdd(typeof(TimeSpan), DeserializeTimeSpan);
      return res;
    }

    static bool DeserializeBoolean(JsonDeserializer deserializer, JsonTokenizer tokenizer, out object value)
    {
      if(tokenizer.HasSpecialChar)
        tokenizer.SkipValueBody();
      else
      {
        string t=tokenizer.Token;
        if(t=="false") { value=false; return true; }
        if(t=="true") { value=true; return true; }

        double v;
        if(JsonConvert.TryParse(t, out v))
        {
          value=Math.Abs(v)>=1-1e-7;
          return true;
        }
      }

      value=null;
      return false;
    }

    static bool DeserializeInteger_sbyte(JsonDeserializer deserializer, JsonTokenizer tokenizer, out object value)
    {
      if(tokenizer.HasSpecialChar)
        tokenizer.SkipValueBody();
      else
      {
        double v;
        if(JsonConvert.TryParse(tokenizer.Token, out v))
        {
          v=Math.Round(v);
          sbyte i=unchecked((sbyte)v);
          bool res=v-i==0;
          value=res ? (object)i : null;
          return res;
        }
      }

      value=null;
      return false;
    }

    static bool DeserializeInteger_byte(JsonDeserializer deserializer, JsonTokenizer tokenizer, out object value)
    {
      if(tokenizer.HasSpecialChar)
        tokenizer.SkipValueBody();
      else
      {
        double v;
        if(JsonConvert.TryParse(tokenizer.Token, out v))
        {
          v=Math.Round(v);
          byte i=unchecked((byte)v);
          bool res=v-i==0;
          value=res ? (object)i : null;
          return res;
        }
      }

      value=null;
      return false;
    }

    static bool DeserializeInteger_short(JsonDeserializer deserializer, JsonTokenizer tokenizer, out object value)
    {
      if(tokenizer.HasSpecialChar)
        tokenizer.SkipValueBody();
      else
      {
        double v;
        if(JsonConvert.TryParse(tokenizer.Token, out v))
        {
          v=Math.Round(v);
          short i=unchecked((short)v);
          bool res=v-i==0;
          value=res ? (object)i : null;
          return res;
        }
      }

      value=null;
      return false;
    }

    static bool DeserializeInteger_ushort(JsonDeserializer deserializer, JsonTokenizer tokenizer, out object value)
    {
      if(tokenizer.HasSpecialChar)
        tokenizer.SkipValueBody();
      else
      {
        double v;
        if(JsonConvert.TryParse(tokenizer.Token, out v))
        {
          v=Math.Round(v);
          ushort i=unchecked((ushort)v);
          bool res=v-i==0;
          value=res ? (object)i : null;
          return res;
        }
      }

      value=null;
      return false;
    }

    static bool DeserializeInteger_int(JsonDeserializer deserializer, JsonTokenizer tokenizer, out object value)
    {
      if(tokenizer.HasSpecialChar)
        tokenizer.SkipValueBody();
      else
      {
        double v;
        if(JsonConvert.TryParse(tokenizer.Token, out v))
        {
          v=Math.Round(v);
          int i=unchecked((int)v);
          bool res=v-i==0;
          value=res ? (object)i : null;
          return res;
        }
      }

      value=null;
      return false;
    }

    static bool DeserializeInteger_uint(JsonDeserializer deserializer, JsonTokenizer tokenizer, out object value)
    {
      if(tokenizer.HasSpecialChar)
        tokenizer.SkipValueBody();
      else
      {
        double v;
        if(JsonConvert.TryParse(tokenizer.Token, out v))
        {
          v=Math.Round(v);
          uint i=unchecked((uint)v);
          bool res=v-i==0;
          value=res ? (object)i : null;
          return res;
        }
      }

      value=null;
      return false;
    }

    static bool DeserializeInteger_long(JsonDeserializer deserializer, JsonTokenizer tokenizer, out object value)
    {
      if(tokenizer.HasSpecialChar)
        tokenizer.SkipValueBody();
      else
      {
        double v;
        if(JsonConvert.TryParse(tokenizer.Token, out v))
        {
          v=Math.Round(v);
          long i=unchecked((long)v);
          bool res=v-i==0;
          value=res ? (object)i : null;
          return res;
        }
      }

      value=null;
      return false;
    }

    static bool DeserializeInteger_ulong(JsonDeserializer deserializer, JsonTokenizer tokenizer, out object value)
    {
      if(tokenizer.HasSpecialChar)
        tokenizer.SkipValueBody();
      else
      {
        double v;
        if(JsonConvert.TryParse(tokenizer.Token, out v))
        {
          v=Math.Round(v);
          ulong i=unchecked((ulong)v);
          bool res=v-i==0;
          value=res ? (object)i : null;
          return res;
        }
      }

      value=null;
      return false;
    }

    static bool DeserializeInteger_float(JsonDeserializer deserializer, JsonTokenizer tokenizer, out object value)
    {
      if(tokenizer.HasSpecialChar)
        tokenizer.SkipValueBody();
      else
      {
        double v;
        if(JsonConvert.TryParse(tokenizer.Token, out v))
        {
          value=(float)v;
          return true;
        }
      }

      value=null;
      return false;
    }

    static bool DeserializeInteger_double(JsonDeserializer deserializer, JsonTokenizer tokenizer, out object value)
    {
      if(tokenizer.HasSpecialChar)
        tokenizer.SkipValueBody();
      else
      {
        double v;
        if(JsonConvert.TryParse(tokenizer.Token, out v))
        {
          value=v;
          return true;
        }
      }

      value=null;
      return false;
    }

    static bool DeserializeInteger_string(JsonDeserializer deserializer, JsonTokenizer tokenizer, out object value)
    {
      if(tokenizer.HasSpecialChar)
      {
        tokenizer.SkipValueBody();
        value=null;
        return false;
      }

      value=tokenizer.Token;
      return true;
    }

    static bool DeserializeDateTime(JsonDeserializer deserializer, JsonTokenizer tokenizer, out object value)
    {
      if(tokenizer.HasSpecialChar)
        tokenizer.SkipValueBody();
      else if(tokenizer.TokenIsString)
      {
        DateTime d;
        if(DateTime.TryParse(tokenizer.Token, CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
        {
          value=d;
          return true;
        }
      }
      else
      {
        double v;
        if(JsonConvert.TryParse(tokenizer.Token, out v))
        {
          value=ConvertUnixDateTime(v);
          return true;
        }
      }

      value=null;
      return false;
    }

    static bool DeserializeTimeSpan(JsonDeserializer deserializer, JsonTokenizer tokenizer, out object value)
    {
      if(tokenizer.HasSpecialChar)
        tokenizer.SkipValueBody();
      else if(tokenizer.TokenIsString)
      {
        TimeSpan t;
        if(TryParseTime(tokenizer.Token, out t))
        {
          value=t;
          return true;
        }
      }
      else
      {
        double v;
        if(JsonConvert.TryParse(tokenizer.Token, out v))
        {
          value=TimeSpan.FromSeconds(v);
          return true;
        }
      }

      value=null;
      return false;
    }

    static DateTime ConvertUnixDateTime(double unixDateTime)
    {
      long ticks=(long)(unixDateTime*TimeSpan.TicksPerSecond);
      return new DateTime(c_UnixBase+ticks, DateTimeKind.Utc);
    }

    static bool TryParseTime(string value, out TimeSpan result)
    {
#if NET20 || NET30 || NET35
      try
      {
        result=TimeSpan.Parse(value);
        return true;
      }
      catch
      {
        result=default(TimeSpan);
        return false;
      }
#else
      return TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out result);
#endif
    }


    static readonly long c_UnixBase=new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).Ticks;
    delegate bool DeserializerDelegate(JsonDeserializer deserializer, JsonTokenizer tokenizer, out object value);
    static readonly ConcurrentDictionary<Type, DeserializerDelegate> m_Deserializers=CreateDeserializers();
    Dictionary<Type, JsonConverter<string, object>> m_Converters;
  }
}