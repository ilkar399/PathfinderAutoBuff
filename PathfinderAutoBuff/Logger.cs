using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityModManagerNet;

namespace PathfinderAutoBuff
/*
* Based on Hsinyu Chan KingmakerModMaker
* https://github.com/hsinyuhcan/KingmakerModMaker
* And Cabarius ToyBox
* https://github.com/cabarius/ToyBox/
*/
{
    public class Logger
    {
        public static UnityModManager.ModEntry.ModLogger modLogger;

        public static string modEntryPath = null;
        public static readonly string logFile = "PathfinderAutoBuff";
        internal String path;

        private bool useTimeStamp = true;
        public bool UseTimeStamp { get => useTimeStamp; set => useTimeStamp = value; }


        public Logger() : this(logFile)
        {

        }

        public Logger(String fileName, String fileExtension = ".log")
        {
            path = Path.Combine(modEntryPath, (fileName + fileExtension));
            Clear();
        }

        public static void Critical(string str)
        {
            modLogger.Critical(str);
        }

        public static void Critical(object obj)
        {
            modLogger.Critical(obj?.ToString() ?? "null");
        }

        public static void Error(Exception e)
        {
            modLogger.Error($"{e.Message}\n{e.StackTrace}");
            if (e.InnerException != null)
                Error(e.InnerException);
        }

        public static void Error(string str)
        {
            modLogger.Error(str);
        }

        public static void Error(object obj)
        {
            modLogger.Error(obj?.ToString() ?? "null");
        }

        public static void Log(string str)
        {
            modLogger.Log(str);
        }

        public static void Log(object obj)
        {
            modLogger.Log(obj?.ToString() ?? "null");
        }

        public static void Warning(string str)
        {
            modLogger.Warning(str);
        }

        public static void Warning(object obj)
        {
            modLogger.Warning(obj?.ToString() ?? "null");
        }

        [Conditional("DEBUG")]
        public static void Debug(MethodBase method, params object[] parameters)
        {
            modLogger.Log($"{method.DeclaringType.Name}.{method.Name}({string.Join(", ", parameters)})");
        }

        [Conditional("DEBUG")]
        public static void Debug(string str)
        {
            modLogger.Log(str);
        }

        [Conditional("DEBUG")]
        public static void Debug(object obj)
        {
            modLogger.Log(obj?.ToString() ?? "null");
        }

        private static string TimeStamp()
        {
            return "[" + DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss.ff") + "]";
        }

        //Logging to files
        public void LogToFiles(string str)
        {

            if (UseTimeStamp)
            {
                ToFile(TimeStamp() + " " + str);
            }
            else
            {
                ToFile(str);

            }
        }

        private void ToFile(string s)
        {
            try
            {
                using (StreamWriter stream = File.AppendText(path))
                {
                    stream.WriteLine(s);
                }
            }
            catch (Exception e)
            {
                modLogger.Log(e.ToString());
            }
        }

        public void Clear()
        {
            if (File.Exists(path))
            {
                try
                {
                    File.Delete(path);
                    using (File.Create(path))
                    {
                    }
                }
                catch (Exception e)
                {
                    modLogger.Log(e.ToString());
                }
            }
        }
    }
}
