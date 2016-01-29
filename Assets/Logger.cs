using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;



	public static class Logger
	{
		static string applicationpath = AppDomain.CurrentDomain.BaseDirectory;
        static string filename = "BHS_Now_Emulator_Log_";
		public static int counter {get;set;}

        public static void WriteLine( string logtype, string threadname, string message,bool showdate)
        {
            
			DateTime now = DateTime.Now;

            StreamWriter sw = File.AppendText(applicationpath + filename + DateTime.Now.ToString("yyyyMMdd") + ".txt");
            if (logtype == "[ERROR]")
            {
                //message += "\r\n";
                //   message += "\n" + Environment.StackTrace.ToString();
            } 
            string prepstring;
            string spacebuf = "                                          ";
            if (showdate)
            {
                prepstring = now.ToString("yyyy-mm-dd HH:mm:ss") + " " + logtype + " " + threadname;
            }
            else
            {
                prepstring = "                     " + spacebuf.Substring(0, logtype.Length + threadname.Length);//21 spaces
            }
            sw.WriteLine(prepstring + " : " + message);
            sw.Close();

        }
	}

