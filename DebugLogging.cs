﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace RedditBackgroundChanger
{
    static class DebugLogging
    {
        public static void WriteToLog(string textToWrite)
        {
            StreamWriter sw;
            using(sw = new StreamWriter($@"{ConfigurationManager.AppSettings["debugOutputPath"]}WallpaperChanger.log.txt",true))
            {
                sw.WriteLine(DateTime.Today.ToString("MM/dd/yy t"));
                sw.WriteLine(textToWrite);
            }
        }
    }
}
