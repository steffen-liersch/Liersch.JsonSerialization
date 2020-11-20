/*--------------------------------------------------------------------------*\
::
::  Copyright © 2013-2020 Steffen Liersch
::  https://www.steffen-liersch.de/
::
\*--------------------------------------------------------------------------*/

namespace Liersch.Json
{
  // Type Func<T, TResult> isn't used instead due to .NET 3.5 is required for it.
  public delegate TResult JsonConverter<in T, out TResult>(T arg);
}