using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;
using LitJson;

namespace WCG
{
	// 运行模式
	public enum RunMode
	{
		eNone,
		eDeveloper,
		eDebug,
		eRelease,
	}

	public static class GameConfigMgr
	{
		// 资源加载相关
		public static readonly string ms_sDataFolderName = "Data";
		public static readonly string ms_sResrcFolderName = "Resources";
		public static readonly string ms_sABFolderName = "DataAB";
		//public static readonly string ms_sLocalFolderName = "Local";
		public static readonly string ms_sDumpFolderName = "Dump";

		// 游戏中用得配置
		//public static readonly string ms_sConfigFolderName = "config";
		public static readonly string ms_sCharFolderName = "characters";
		public static readonly string ms_sEffectFolderName = "effects";
		public static readonly string ms_sMapFolderName = "maps";
		public static readonly string ms_sUIFolderName = "ui";
		public static readonly string ms_sMediaFolderName = "media";
		public static readonly string ms_sObjFolderName = "obj";
		public static readonly string ms_sLuaScriptFolderName = "LuaScript";
		public static readonly string ms_sDesignerFolderName = "Designer";

		// 打包用的路径
		public static readonly string ms_sDataDir = Application.dataPath + "/";
		public static readonly string ms_sPublicDir = Application.dataPath + "/../../../Public/";
		public static readonly string ms_sPatcherDir = ms_sPublicDir + "Patcher/";

		// Game逻辑使用路径
		public static readonly string ms_sAssetsPath = "Assets/";
		public static readonly string ms_sStreamDir = Application.streamingAssetsPath + '/';

		public static readonly string ms_sABType = ".bytes";    // ".AssetBundle" //".unity3d";    // AssetBundle包的扩展名
		public static readonly string ms_sWcpType = ".wcp";     // 补丁包的后缀名
		//manifest文件
		public static readonly string ms_sABMFileName = "DataMF.manifest";
        public static readonly string ms_sLuaMFileName = "LuaMF.manifest";
        public static readonly string ms_sCsvMFileName = "CfgMF.manifest";
        // 版本号文件
        public static readonly string ms_sVersionFileName = "Version";
		// Map 相关
		public static readonly string ms_sMapCfgFileName = "MapCfg.json";
		public static readonly string ms_sMD5Type = ".md5";

		// AB 资源路径 ui、音效、模型 等等
	#if UNITY_EDITOR
		public static readonly string ms_sABResrcDir = Application.dataPath + "/" + ms_sResrcFolderName + "/" + ms_sABFolderName + "/";
	#elif UNITY_STANDALONE_WIN
		public static readonly string ms_sABResrcDir = Application.streamingAssetsPath + "/" + ms_sABFolderName + "/";
	#elif UNITY_ANDROID || UNITY_IPHONE
		public static readonly string ms_sABResrcDir = Application.persistentDataPath + "/" + ms_sABFolderName + "/";
	#else
		public static readonly string ms_sABResrcDir = null;
	#endif

		/*// 外包本地数据
		#if UNITY_EDITOR
		public static readonly string ms_sLocalResrcDir = Application.dataPath + "/" + ms_sLocalFolderName + "/";
		#elif UNITY_STANDALONE_WIN
		public static readonly string ms_sLocalResrcDir = Application.streamingAssetsPath + "/" + ms_sLocalFolderName + "/";
		#elif UNITY_ANDROID || UNITY_IPHONE
		public static readonly string ms_sLocalResrcDir = Application.persistentDataPath + "/" + ms_sLocalFolderName + "/";
		#else
		public static readonly string ms_sLocalResrcDir = null;
		#endif*/

	#if UNITY_EDITOR
		public static readonly string ms_sAppDumpDir = Application.dataPath + "/" + ms_sDumpFolderName + "/";
	#elif UNITY_STANDALONE_WIN
		public static readonly string ms_sAppDumpDir = Application.streamingAssetsPath + "/" + ms_sDumpFolderName + "/";
	#elif UNITY_ANDROID || UNITY_IPHONE
		public static readonly string ms_sAppDumpDir = Application.persistentDataPath + "/" + ms_sDumpFolderName + "/";
	#else
		public static readonly string ms_sAppDumpDir = null;
	#endif

		//AddEx
		public static readonly string ms_sClientConf = "Client.json";
        public static string ms_sLuaScriptDir;
        public static string ms_sDesignerDir;
		public static List<string> ms_lstLuaPath;
        public static List<string> ms_lstCsvPath;

        // Launcher用的配置
        public static readonly string ms_sDependName = "Dependencies.txt";     // json依赖表名
		public static readonly string ms_sLauncherConf = "Launcher.json";
		public static readonly string ms_sLauncherNotice = "LauncherNotice.json";
		public static readonly string ms_sVersion = "Version.txt";
		public static readonly string ms_sRootPackName = "ResourceList.json";
		public static readonly string ms_sWinPlatformName = "Windows";
		public static readonly string ms_sAdrPlatformName = "Android";
		public static readonly string ms_sIosPlatformName = "IOS";
		public static readonly string ms_sPatcherWcrLst = "wcr_patcherinfo.bytes";     // 更新包包含的资源信息写入文件

	#if UNITY_ANDROID           // Android
		public static readonly string ms_sPlatformName = ms_sAdrPlatformName;
	#elif UNITY_IPHONE          // iPhone
		public static readonly string ms_sPlatformName = ms_sIosPlatformName;
	#elif UNITY_STANDALONE_WIN  // windows平台
		public static readonly string ms_sPlatformName = ms_sWinPlatformName;
    #else
		public static readonly string ms_sPlatformName = string.Empty;
    #endif

        public static readonly string ms_sBundleDir = Application.dataPath + "/../" + ms_sABFolderName + "/";
        //public static readonly string ms_sBundleDir = ms_sPublicDir + "StandaloneWindows/DataAB/"; //"StandaloneWindows/DataAB/" //"Patcher/DataAB/StandaloneWindows_0.0.0.1000/"
        public static readonly string ms_sBundleType = ".bytes";

		public static string ms_sResrcDir;
		public static string ms_sDownLoadDir;
		public static RunMode ms_eArchive;
		public static string ms_sGasIp = string.Empty;
		public static string ms_sGasPort = string.Empty;
		//public static string ms_sGatIp = string.Empty;
		//public static string ms_sGatPort = string.Empty;

		static GameConfigMgr()
		{
            bool bLogPlatformInfo = false;
            if (bLogPlatformInfo)
            {
                Debug.Log("-->> platform:" + Application.platform.ToString());
                Debug.Log("-->> dataPath:" + Application.dataPath);
                Debug.Log("-->> streamingAssetsPath:" + Application.streamingAssetsPath);
                Debug.Log("-->> persistentDataPath:" + Application.persistentDataPath);
                Debug.Log("-->> temporaryCachePath:" + Application.temporaryCachePath);
            }

            ms_lstLuaPath = new List<string>();
            ms_lstCsvPath = new List<string>();
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            Debug.Log("-->> StreamingAssets Client.json");
			JsonData jd = Util.readJson(Application.streamingAssetsPath + "/" + ms_sClientConf);

			int iArchive = int.Parse(jd["Archive"].ToString());
			Debug.Log("-->> iArchive = " + iArchive);
			ms_sGasIp = jd["sGasIp"].ToString();
			ms_sGasPort = jd["nGasPort"].ToString();

			if (iArchive == 1 || iArchive == 2 || iArchive == 3)
			{
				ms_sLuaScriptDir = jd["GameScriptDir"].ToString();
				ms_sDesignerDir = jd["DesignerScriptDir"].ToString();
			}
			if (iArchive == 1)
			{
				ms_sDownLoadDir = Application.dataPath + "/DownLoad/";
				ms_sResrcDir = GameConfigMgr.ms_sDataDir + GameConfigMgr.ms_sDataFolderName + "/";
				ms_eArchive = RunMode.eDeveloper;
				ms_lstLuaPath.Add(ms_sLuaScriptDir);
				ms_lstLuaPath.Add(ms_sDesignerDir);
			}
			else if (iArchive == 2)
			{
				ms_sDownLoadDir = Application.dataPath + "/DownLoad/";
				ms_sResrcDir = ms_sBundleDir;
				ms_eArchive = RunMode.eDebug;
				ms_lstLuaPath.Add(ms_sLuaScriptDir);
				ms_lstLuaPath.Add(ms_sDesignerDir);
			}
			else if (iArchive == 3)
			{
				ms_sDownLoadDir = Application.dataPath + "/DownLoad/";
				ms_sResrcDir = ms_sBundleDir;
				ms_eArchive = RunMode.eRelease;
				ms_lstLuaPath.Add(ms_sLuaScriptFolderName + ".");
				ms_lstLuaPath.Add(ms_sDesignerFolderName + ".Config.");
			}
			else
			{
				ms_eArchive = RunMode.eNone;
				Debug.LogError("-->> Error GameConfig.Archive !!!,iArchive: " + iArchive);
				return;
			}
#else
		#if UNITY_IPHONE || UNITY_ANDROID
			Debug.LogError("Error GameConfig.Archive !!!, iArchive: ");
			ms_sDownLoadDir = Application.temporaryCachePath + "/DownLoad/";
			ms_sResrcDir = Application.persistentDataPath + "/" + ms_sABFolderName + "/";
			ms_eArchive = RunMode.eRelease;
			ms_lstLuaPath.Add(ms_sLuaScriptFolderName + ".");
			ms_lstLuaPath.Add(ms_sDesignerFolderName + ".Config.");
			//ms_lstCsvPath.Add(ms_sDesignerFolderName + ".Config.");
		#else
			ms_eArchive = RunMode.eNone;
			Debug.LogError("Error Platform!");
		#endif
#endif

	#if UNITY_EDITOR
		DirectoryInfo pubDir = new DirectoryInfo(ms_sPublicDir);
		ms_sPublicDir = pubDir.FullName.Replace("\\", "/");
		DirectoryInfo patDir = new DirectoryInfo(ms_sPatcherDir);
		ms_sPatcherDir = patDir.FullName.Replace("\\", "/");
        //Debug.Log(ms_sPublicDir + "\n" + ms_sPatcherDir);
	#endif
        }
    }
}
