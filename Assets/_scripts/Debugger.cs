using UnityEngine;

internal class Debugger
{
    internal static int Count1 = 0;
    internal static int Count2 = 0;

    [System.Diagnostics.Conditional("MYDEBUG")]
    internal static void Log(string v)
    {
        Debug.Log(v);
    }

}