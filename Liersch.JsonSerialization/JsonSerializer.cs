/*--------------------------------------------------------------------------*\
::
::  Copyright © 2013-2021 Steffen Liersch
::  https://www.steffen-liersch.de/
::
\*--------------------------------------------------------------------------*/

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;

namespace Liersch.Json
{
  public sealed class JsonSerializer
  {
    public bool IncludeNullValues { get; set; }

    public bool UseExactMemberTypeOnly { get; set; }


    public JsonSerializer()
    {
      IncludeNullValues=true;
      UseExactMemberTypeOnly=true;
    }


    public void RegisterConverter(Type type, JsonConverter<object, string> converter)
    {
      if(type==null)
        throw new ArgumentNullException("type");

      if(converter==null)
        throw new ArgumentNullException("converter");

      if(m_Converters==null)
        m_Converters=new Dictionary<Type, JsonConverter<object, string>>();

      m_Converters[type]=converter;
    }

    public void RegisterConverter<T>(JsonConverter<T, string> converter)
    {
      if(converter==null)
        throw new ArgumentNullException("converter");

      if(m_Converters==null)
        m_Converters=new Dictionary<Type, JsonConverter<object, string>>();

      m_Converters[typeof(T)]=x => converter((T)x);
    }


    public void Serialize<T>(T instance, JsonWriter writer)
    {
      SerializeValue(writer, instance, RetrieveType(instance));
    }

    public string Serialize<T>(T instance)
    {
      var wr=new JsonWriter();
      SerializeValue(wr, instance, RetrieveType(instance));
      return wr.ToString();
    }

    Type RetrieveType<T>(T instance)
    {
      if(UseExactMemberTypeOnly)
        return typeof(T);
      return instance!=null ? instance.GetType() : typeof(object);
    }


    void SerializeValue(JsonWriter writer, object value, Type type)
    {
      if(value==null)
        SerializeNull(writer, type);
      else RetrieveSerializer(type)(this, writer, value);
    }

    void SerializeArray(JsonWriter writer, IEnumerable values, Type elementType)
    {
      writer.BeginArray();

      foreach(object v in values)
      {
        if(v==null)
          SerializeNull(writer, elementType);
        else SerializeValue(writer, v, UseExactMemberTypeOnly ? elementType : v.GetType());
      }

      writer.EndArray();
    }

    void SerializeObject(JsonWriter writer, object instance, Type type, JsonTypeCacheEntry cache)
    {
      if(instance==null)
        SerializeNull(writer, type);
      else
      {
        writer.BeginObject();

        foreach(var k in cache.Members)
          SerializeProperty(writer, instance, k.Key, k.Value);

        writer.EndObject();
      }
    }

    void SerializeProperty(JsonWriter writer, object instance, string name, JsonTypeMemberEntry memberEntry)
    {
      object value=memberEntry.ReadValue(instance);
      if(value!=null || IncludeNullValues)
      {
        writer.BeginField(name, memberEntry.IsEscapingRequired);

        if(value==null)
          SerializeNull(writer, memberEntry.MemberType);
        else SerializeValue(writer, value, UseExactMemberTypeOnly ? memberEntry.MemberType : value.GetType());
      }
    }


    void SerializeNull(JsonWriter writer, Type type)
    {
      JsonConverter<object, string> conv;
      if(m_Converters!=null && m_Converters.TryGetValue(type, out conv))
        writer.WriteValue(conv(null));
      else writer.WriteValueNull();
    }

    SerializerDelegate RetrieveSerializer(Type type)
    {
      JsonConverter<object, string> conv;
      if(m_Converters!=null && m_Converters.TryGetValue(type, out conv))
        return (s, w, v) => w.WriteValue(conv(v));

      return m_Serializers.GetOrAdd(type, RetrieveSerializer2);
    }


    static SerializerDelegate RetrieveSerializer2(Type type)
    {
      if(type.IsArray)
      {
        if(type.GetArrayRank()!=1)
          throw new NotSupportedException("Multi-dimensional arrays are not supported");

        Type et=type.GetElementType();
        return (s, w, v) => s.SerializeArray(w, (IEnumerable)v, et);
      }

      if(type.IsGenericType)
      {
        Type[] ga=type.GetGenericArguments();
        if(ga.Length==1)
        {
          Type gt=type.GetGenericTypeDefinition();
          Type ga0=ga[0];

          if(typeof(IEnumerable<>).MakeGenericType(ga0).IsAssignableFrom(type))
            return (s, w, v) => s.SerializeArray(w, (IEnumerable)v, ga0);

          if(gt==typeof(Nullable<>))
            return (s, w, v) => s.SerializeValue(w, v, ga0);
        }
      }

      var cache=JsonTypeCache.Instance.RetrieveCacheEntry(type);
      return (s, w, v) => s.SerializeObject(w, v, type, cache);
    }

    static ConcurrentDictionary<Type, SerializerDelegate> CreateSerializers()
    {
      var res=new ConcurrentDictionary<Type, SerializerDelegate>();
      res.TryAdd(typeof(bool), (s, w, v) => w.WriteValue((bool)v));
      res.TryAdd(typeof(sbyte), (s, w, v) => w.WriteValue((sbyte)v));
      res.TryAdd(typeof(byte), (s, w, v) => w.WriteValue((byte)v));
      res.TryAdd(typeof(short), (s, w, v) => w.WriteValue((short)v));
      res.TryAdd(typeof(ushort), (s, w, v) => w.WriteValue((ushort)v));
      res.TryAdd(typeof(int), (s, w, v) => w.WriteValue((int)v));
      res.TryAdd(typeof(uint), (s, w, v) => w.WriteValue((uint)v));
      res.TryAdd(typeof(long), (s, w, v) => w.WriteValue((long)v));
      res.TryAdd(typeof(ulong), (s, w, v) => w.WriteValue((ulong)v));
      res.TryAdd(typeof(float), (s, w, v) => w.WriteValue((float)v));
      res.TryAdd(typeof(double), (s, w, v) => w.WriteValue((double)v));
      res.TryAdd(typeof(string), (s, w, v) => w.WriteValue((string)v));
      res.TryAdd(typeof(DateTime), (s, w, v) => w.WriteValue(((DateTime)v).ToString(@"yyyy\-MM\-dd HH\:mm\:ss", CultureInfo.InvariantCulture)));
      res.TryAdd(typeof(TimeSpan), (s, w, v) => w.WriteValue(Convert.ToString((TimeSpan)v, CultureInfo.InvariantCulture)));
      return res;
    }


    delegate void SerializerDelegate(JsonSerializer serializer, JsonWriter writer, object value);
    static readonly ConcurrentDictionary<Type, SerializerDelegate> m_Serializers=CreateSerializers();
    Dictionary<Type, JsonConverter<object, string>> m_Converters;
  }
}