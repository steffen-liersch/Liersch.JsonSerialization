/*--------------------------------------------------------------------------*\
::
::  Copyright © 2013-2020 Steffen Liersch
::  https://www.steffen-liersch.de/
::
\*--------------------------------------------------------------------------*/

using System;

namespace Liersch.Json
{
  static class Program
  {
    static void Main()
    {
      try
      {
        Example.Run();
      }
      catch(Exception e)
      {
        Console.WriteLine(e.ToString());
      }

      Console.WriteLine("[Press any key!]");
      Console.ReadKey(true);
    }
  }
}