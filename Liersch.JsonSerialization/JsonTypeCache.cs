/*--------------------------------------------------------------------------*\
::
::  Copyright © 2020 Steffen Liersch
::  https://www.steffen-liersch.de/
::
\*--------------------------------------------------------------------------*/

using System;
using System.Collections.Concurrent;

namespace Liersch.Json
{
  sealed class JsonTypeCache
  {
    public static readonly JsonTypeCache Instance=new JsonTypeCache();

    public JsonTypeCacheEntry RetrieveCacheEntry(Type type)
    {
      return m_Dictionary.GetOrAdd(type, JsonTypeCacheEntry.Create);
    }

    readonly ConcurrentDictionary<Type, JsonTypeCacheEntry> m_Dictionary=new ConcurrentDictionary<Type, JsonTypeCacheEntry>();
  }
}