/*--------------------------------------------------------------------------*\
::
::  Copyright © 2020 Steffen Liersch
::  https://www.steffen-liersch.de/
::
\*--------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Liersch.Json
{
  sealed class JsonTypeCacheEntry
  {
    public Dictionary<string, JsonTypeMemberEntry> Members;

    public static JsonTypeCacheEntry Create(Type type) { return new JsonTypeCacheEntry(type); }

    JsonTypeCacheEntry(Type type)
    {
      Members=new Dictionary<string, JsonTypeMemberEntry>();

      foreach(FieldInfo fi in type.GetRuntimeFields())
      {
        if(fi.IsPublic)
        {
          object[] attrs=fi.GetCustomAttributes(typeof(JsonMemberAttribute), false);
          if(attrs!=null && attrs.Length>0)
          {
            var a=(JsonMemberAttribute)attrs[0];
            Members.Add(a.MemberName, new JsonTypeMemberEntry(a.MemberName, fi));
          }
        }
      }

      foreach(PropertyInfo pi in type.GetRuntimeProperties())
      {
        if(pi.CanRead && pi.CanWrite)
        {
          MethodInfo mi1=pi.GetGetMethod();
          if(mi1!=null && mi1.IsPublic && !mi1.IsStatic)
          {
            MethodInfo mi2=pi.GetSetMethod();
            if(mi2!=null && mi2.IsPublic && !mi2.IsStatic)
            {
              object[] attrs=pi.GetCustomAttributes(typeof(JsonMemberAttribute), false);
              if(attrs!=null && attrs.Length>0)
              {
                var a=(JsonMemberAttribute)attrs[0];
                Members.Add(a.MemberName, new JsonTypeMemberEntry(a.MemberName, pi.PropertyType, mi1, mi2));
              }
            }
          }
        }
      }
    }
  }
}