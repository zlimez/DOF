using System.Collections.Generic;
using UnityEngine;

namespace Assets.URP.Runtime.Bokeh
{
    public class Logger : MonoBehaviour
    {
        static readonly HashSet<string> LOGGED = new();

        void Start()
        {
            LOGGED.Clear();
        }

        public static void Log<T>(T[] array, string pmsg, bool once = false, string key = "")
        {
            if (once && LOGGED.Contains(key)) return;
            if (once) LOGGED.Add(key);
            Debug.Log($"{pmsg}: {ArrayToString(array)}");
        }

        static string ArrayToString<T>(T[] array)
        {
            if (array == null || array.Length == 0)
            {
                return "Empty or null array";
            }
            return string.Join(", ", array);
        }
    }
}