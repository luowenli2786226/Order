using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Web;
namespace Comm
{
    /// <summary>
    /// ��־��¼��
    /// </summary>
    public static class LogInfo
    {
        //static string logfilename = HttpContext.Current.Server.MapPath("logInfo.txt");
        //static string cmcfgfile = HttpContext.Current.Server.MapPath("cmcfg.txt");
        static string logfilename = HttpContext.Current.Server.MapPath("~") + "logInfo.txt";
        static string cmcfgfile = HttpContext.Current.Server.MapPath("~") + "cmcfg.txt";
        static string SynchronousFile = HttpContext.Current.Server.MapPath("~") + "SynchronousFile.txt";

        /// <summary>
        /// д��־
        /// </summary>
        /// <param name="loginfo"></param>
        public static void WriteLog(string loginfo)
        {
            if (File.Exists(logfilename))
            {
                Comm.Utility.INIFile cfgfile = new Comm.Utility.INIFile(logfilename);

                StreamWriter sw = new StreamWriter(logfilename, true);
                sw.WriteLine(DateTime.Now.ToString() + ":" + loginfo);
                sw.Close();
            }
        }

        ///// <summary>
        ///// д��־
        ///// </summary>
        ///// <param name="loginfo"></param>
        //public static void WriteLog(string loginfo)
        //{
        //    if (File.Exists(cmcfgfile))
        //    {
        //        Comm.Utility.INIFile cfgfile = new Comm.Utility.INIFile(cmcfgfile);
        //        string val = cfgfile.IniReadValue("loginfo", "save");
        //        if (val.Trim() == "1")
        //        {
        //            StreamWriter sw = new StreamWriter(logfilename, true);
        //            sw.WriteLine(DateTime.Now.ToString() + ":" + loginfo);
        //            sw.Close();
        //        }
        //    }
        //}

        /// <summary>
        /// �������ֿ�ͬ����־
        /// </summary>
        /// <param name="loginfo"></param>
        public static void WriteSynchronousLog(string SynchronousInfo)
        {
            if (File.Exists(SynchronousFile))
            {
                Comm.Utility.INIFile cfgfile = new Comm.Utility.INIFile(SynchronousFile);

                StreamWriter sw = new StreamWriter(SynchronousFile, true);
                sw.WriteLine("ͬ��ʱ�� :" + DateTime.Now.ToString() + "     IP: " + SynchronousInfo);
                sw.Close();
            }
        }

        /// <summary>
        /// д��־
        /// </summary>
        /// <param name="loginfo"></param>
        public static void WriteLog(System.Exception ex)
        {
            if (File.Exists(cmcfgfile))
            {
                Comm.Utility.INIFile cfgfile = new Comm.Utility.INIFile(cmcfgfile);
                //string val = cfgfile.IniReadValue("loginfo", "save");
                //if (val.Trim() == "1")
                //{
                StreamWriter sw = new StreamWriter(logfilename, true);
                sw.WriteLine(DateTime.Now.ToString() + ":");
                sw.WriteLine("������Ϣ��" + ex.Message);
                sw.WriteLine("Stack Trace:" + ex.StackTrace);
                sw.WriteLine("");
                sw.Close();
                //}
            }
        }

        /// <summary>
        /// д��־
        /// </summary>
        /// <param name="loginfo"></param>
        //public static void WriteLog(System.Reflection.MethodBase mb, System.Exception ex)
        //{
        //    if (File.Exists(cmcfgfile))
        //    {
        //        LTP.Utility.INIFile cfgfile = new LTP.Utility.INIFile(cmcfgfile);
        //        string val = cfgfile.IniReadValue("loginfo", "save");
        //        if (val.Trim() == "1")
        //        {                    
        //            string loginfo = mb.ReflectedType.FullName+mb.Name+"������Ϣ��\n\n" + ex.Message + "\n\n" + "Stack Trace:\n" + ex.StackTrace;
        //            StreamWriter sw = new StreamWriter(logfilename, true);
        //            sw.WriteLine(DateTime.Now.ToString() + ":" + loginfo);
        //            sw.Close();
        //        }
        //    }
        //}

    }
}
