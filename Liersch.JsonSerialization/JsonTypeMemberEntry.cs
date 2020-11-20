/*--------------------------------------------------------------------------*\
::
::  Copyright © 2020 Steffen Liersch
::  https://www.steffen-liersch.de/
::
\*--------------------------------------------------------------------------*/

using System;
using System.Reflection;
using Liersch.Reflection;

namespace Liersch.Json
{
  sealed class JsonTypeMemberEntry
  {
    public readonly bool IsEscapingRequired;
    public readonly Type MemberType;
    public readonly Func1 ReadValue;
    public readonly Action2 WriteValue;

    public JsonTypeMemberEntry(string jsonName, FieldInfo fieldInfo)
    {
      IsEscapingRequired=JsonWriter.IsEscapingRequired(jsonName);
      MemberType=fieldInfo.FieldType;

      ReadValue=Accelerator.CreateInstanceGetter(fieldInfo);

      if(!fieldInfo.IsInitOnly)
        WriteValue=Accelerator.CreateInstanceSetter(fieldInfo);
    }

    public JsonTypeMemberEntry(string jsonName, Type propertyType, MethodInfo getMethod, MethodInfo setMethod)
    {
      IsEscapingRequired=JsonWriter.IsEscapingRequired(jsonName);
      MemberType=propertyType;

      if(getMethod!=null)
        ReadValue=Accelerator.CreateFunction1(getMethod);

      if(setMethod!=null)
        WriteValue=Accelerator.CreateAction2(setMethod);
    }
  }
}