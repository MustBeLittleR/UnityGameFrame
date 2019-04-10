using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using System;
using System.IO;
using System.Text;
using LuaInterface;
using System.Net;


namespace WCG
{
    public class CrashHandler
    {

        private static CrashHandler m_instance;
        public static CrashHandler Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new CrashHandler();
                }
                return m_instance;
            }
        }

        private static string HOST = "ftp://121.40.18.163";
        private static string PORT = ":2020";
        private static string REMOTE_PATH = "/";
        //private static string USER = "uploadinner";
        //private static string PASS = "1#Dpsgdumpupinner";
        private static string USER = "nfxy";
        private static string PASS = "29iUJxbAzc";
        //private static string currentTime = "";

        public void Init()
        {
            Application.logMessageReceived += OnLog;
        }

        void OnLog(string message, string stacktrace, LogType type)
        {
            if (type == LogType.Exception)
            {
                string luastack = "";
                GameObject manager = GameObject.Find("GameManager");
                if (manager != null)
                {
                    luastack = LuaScriptMgr.Instance.GetStackString();
                }

                //currentTime = getCurrentTime().ToString();
                string bugmsg = "{\n" +
                "\"version\":\"" + ExtInfo.ms_sVersion + " -- " + ExtInfo.ms_sClientVersion + "\"" +
                ",\n\"message\":\"" + message + "\"" +
                ",\n\"stacktrace\":\"" + stacktrace + "\"" +
                ",\n\"luastacktrace\":\"" + luastack + "\"" +
                //",\n\"time\":" + "\"" + currentTime + "\"" +  // 去掉这个时间的目的是：可以用工具筛选重复的dump文件，反正文件名也带上了时间的
                //",\n\"device\":\"" + getDeviceinfo() + "\"" + 
                "\n}";

                uploadBug(bugmsg);
            }
        }

        string getDeviceinfo()
        {
            return SystemInfo.deviceName + "'s : " + SystemInfo.deviceModel + "," + SystemInfo.graphicsDeviceName;
        }

        void uploadBug(string bugMsg)
        {
            string local_file = getCurrentTime() + "_" + SystemInfo.deviceName + "_" + SystemInfo.deviceModel + ".txt";
            string upload_filename = "exdump/dump" + local_file;
            createFile(Application.temporaryCachePath, local_file, bugMsg);
            #if UNITY_STANDALONE_WIN || UNITY_IPHONE || UNITY_ANDROID
            //ftpUpload(upload_filename, bugMsg); 
            #endif
        }

        public static string getCurrentTime()
        {
            System.DateTime now = System.DateTime.Now;
            return now.Year + "-" + now.Month + "-" + now.Day + " " + now.Hour + "-" + now.Minute + "-" + now.Second;
        }

        //生成本地文件，调试用
        void createFile(string path, string name, string info)
        {
            try
            {
                StreamWriter sw;
                FileInfo t = new FileInfo(path + "/" + name);

                if (!t.Exists)
                {
                    sw = t.CreateText();
                }
                else
                {
                    sw = t.AppendText();
                }

                sw.WriteLine(info);
                sw.Close();
                sw.Dispose();
            }
            catch (System.Exception e)
            {
                string errinfo = e.Message;
                Debug.LogWarning("createFile failed : " + e.Message);
            }
        }

        // 上传FTP服务器
        private bool ftpUpload(string filename, string info)
        {
            string uri = HOST + PORT + REMOTE_PATH + filename;
            FtpWebRequest reqFTP;
            reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));
            reqFTP.Credentials = new NetworkCredential(USER, PASS);
            reqFTP.KeepAlive = false;
            reqFTP.Method = WebRequestMethods.Ftp.UploadFile;
            reqFTP.UseBinary = true;
             
            try
            {
                byte[] byteArray = System.Text.Encoding.Default.GetBytes(info);
                reqFTP.ContentLength = byteArray.Length;
                Stream strm = reqFTP.GetRequestStream();
                strm.Write(byteArray, 0, byteArray.Length);
                strm.Close();
            }
            catch (Exception e)
            {
                createFile(Application.temporaryCachePath, filename, "\n upload dump failed:\n" + e.Message);
                Debug.LogWarning("upload dump failed : " + e.Message);
                return false;
            }

            return true;
        }
    }

}
