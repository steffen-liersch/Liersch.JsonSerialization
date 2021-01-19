/*--------------------------------------------------------------------------*\
::
::  Copyright © 2013-2021 Steffen Liersch
::  https://www.steffen-liersch.de/
::
\*--------------------------------------------------------------------------*/

using System;

namespace Liersch.Json
{
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
  public class JsonMemberAttribute : Attribute
  {
    public string MemberName { get; private set; }

    public JsonMemberAttribute(string memberName) { MemberName=memberName; }
  }
}