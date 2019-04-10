using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using LitJson;
using System.Collections.Generic;
using System;

namespace WCG
{
	// 游戏的额外信息
	public static class ExtInfo
	{
		public static string ms_sVersion = string.Empty;     // 客户端的随包版本号，可以供dump模块使用
		public static string ms_sClientVersion = string.Empty;     // 客户端的资源版本号，可以供dump模块使用

		private static bool ms_bIsAppleReview = false;     // 是否是苹果的审核服
		private static bool ms_bIsFirstRunGame = false;     // 是否第一次运行游戏
		private static int ms_nChannel = 0;     // 渠道
		private static string ms_sPatchInfo = string.Empty;  // 更新公告的内容  
		private static DateTime ms_BeginTime;
		private static int ms_nCostTime = 0; //更新耗时
		public static bool IsAppleReview()
		{
			return ms_bIsAppleReview;
		}
		public static void SetAppleReview(bool bIsAppleReview)
		{
			ms_bIsAppleReview = bIsAppleReview;
		}

		public static bool GetFirstRunFlag()
		{
			return ms_bIsFirstRunGame;
		}
		public static void SetFirstRunFlag(bool bIsFirstRunGame)
		{
			ms_bIsFirstRunGame = bIsFirstRunGame;
		}

		public static int GetChannel()
		{
			return ms_nChannel;
		}
		public static void SetChannel(int channel)
		{
			ms_nChannel = channel;
		}

		public static string GetPatcherInfo()
		{
			return ms_sPatchInfo;
		}
		public static void SetPatcherInfo(string info)
		{
			ms_sPatchInfo  = info;
			if (ms_sPatchInfo == null)
				ms_sPatchInfo = "";
		}

		public static void SetUpdateBeginTime(DateTime beginTime)
		{
			ms_BeginTime = beginTime;
		}

		public static DateTime GetUpdateBeginTime()
		{
			return ms_BeginTime;
		}

		public static void SetUpdateCostTime(int nCostTime)
		{
			ms_nCostTime = nCostTime;
		}

		public static int GetUpdateCostTime()
		{
			return ms_nCostTime;
		}
	}

	struct FileMd5
	{
		public string file; // file path
		public string md5;
	}
	enum WCDataType
	{
		Package = 1,
		Patcher = 2,
	}

	public class Launcher : MonoBehaviour
	{
		private GameObject m_btnQuit;
		private GameObject m_btnRetry;
		private GameObject m_oEventSystem;  // EventSystem
		private GameObject m_goUnPackProgress;    // 进度条
		private Slider m_oUnPackProgress;    // 进度条
		//private Text m_oUnPackText;        // 进度文字
		private Text m_oUnPackTxtInfo;        // 信息 TxtInfo
		private GameObject m_oPatcherRect;  // Patcher
		private GameObject m_oBtnPatcher;   // 更新按钮 BtnPatcher
		private GameObject m_oBtnIgnore;    // 忽略更新 BtnIgnore
		private Text m_oPatcherTxtTitle;    // 信息 TxtTitle
		private bool m_bIgnoreUpdate = false;   // 是否忽略更新（提示更新时）
		private GameObject m_oErrorRect;  // Error
		private Text m_oErrorInfo;  // TxtErrInfo

        private bool m_bDownload = false;   // 是否下载相关更新
        private float m_fTotalDownloadSize = 0;
        private GameObject m_oBtnDownload;  // Patcher
        private GameObject m_oBtnQuit;  // Patcher

        private CrashHandler m_CrashHandler;     // 上传崩溃信息

		private string m_sVersion = string.Empty;    // 随包的原始版本号
		private string m_sClientVersion = string.Empty;  // 客户端版本号
		//private string m_sServerVersion = string.Empty;  // 服务器版本号
		private string m_sServNewVersion = string.Empty;    // 服务器最高版本号
		private string m_sServMidVersion = string.Empty;    // 服务器禁戒版本号
		private string m_sServOldVersion = string.Empty;    // 服务器最低版本号
		private List<string> m_lstServVersion = new List<string>(); // 服务器版本号列表（每个程序大版本对应的最终版本号）

		private string m_sServerVersionUrl = string.Empty;        // 服务器版本号的网络地址
		private string m_sServerListUrl = string.Empty;     // 服务器列表的网络地址
		private string m_sPatchListUrl = string.Empty;        // 更新包列表文件的网络地址
		private string m_sPatchInfoUrl = string.Empty;        // 更新包公告信息文件的网络地址
		private string m_sAppDownLoadUrl = string.Empty;        // App下载地址
		private string m_sResourceListUrl = string.Empty;        // 资源包下载地址
		private string m_sPackFlag = string.Empty;          // 是否需要资源包的标志。1：不解压版本(无资源包)。2：解压版本（从包内或者服务器上获取资源包）
		private int    m_nChannel = 0;          // 渠道。0：无渠道，测试用。1：小米。

		private string m_sLauncherConfPath = string.Empty;    // 随包的Launcher配置的存放路径
		private string m_sLauncherNoticePath = string.Empty;    // 随包的Launcher提示信息的存放路径
		private string m_sVersionPath = string.Empty;              // 随包的原始版本号的存放路径
		private string m_sServVersionPath = string.Empty;      // 服务器版本号文件的存放路径
		private string m_sClientVersionPath = string.Empty;    // 客户端版本号文件的存放路径
		private string m_sServListPath = string.Empty;        // 服务器列表文件的存放路径

		//方便以后调整
		private int minSystemMemorySize = 1024; //最小系统内存，低于该值设置为低配效果，用于调整场景效果等级
		//private int maxSystemMemorySize = 2048; //最大系统内存，高于该值设置为高配效果，用于调整场景效果等级
		//private int pixelsWidth = 640; //像素宽度，中配的其中一个判断

		private Dictionary<string, string> m_dicNotice = new Dictionary<string, string>();   //提示信息字典

		// 初始化自身
		void Awake()
		{
			/*#if UNITY_IPHONE || UNITY_ANDROID
            Caching.CleanCache();
#endif*/

			Debug.Log("---------- 操作系统：" + SystemInfo.operatingSystem);

			int systemMemorySize = SystemInfo.systemMemorySize + SystemInfo.graphicsMemorySize;
			Debug.Log("---------- systemMemorySize + graphicsMemorySize = " + systemMemorySize);

			//double diagonal = Math.Sqrt(Screen.width * Screen.width + Screen.height * Screen.height);
			//float size = (float)diagonal / Screen.dpi;
			//Debug.Log("---------- Screen.size = " + size);


			if (systemMemorySize > minSystemMemorySize || SystemInfo.operatingSystem.IndexOf("IOS") != -1)
			{
				//高配
				QualitySettings.SetQualityLevel(2);
				Debug.Log("----------高配----------");
            }
			else if (systemMemorySize < minSystemMemorySize)
			{
				//低配
				QualitySettings.SetQualityLevel(0);
				Debug.Log("----------低配----------");
			}
			else
			{
				//中配
				QualitySettings.SetQualityLevel(1);
				Debug.Log("----------中配----------");
			}

            Application.targetFrameRate = 40;

            Screen.sleepTimeout = SleepTimeout.NeverSleep;
		}

		void Start()
		{
			m_CrashHandler = CrashHandler.Instance;
			m_CrashHandler.Init();

			m_oEventSystem = GameObject.Find("Canvas/EventSystem");
			m_goUnPackProgress = GameObject.Find("Canvas/PreLoading/Progress/Prg");
			m_oUnPackProgress = m_goUnPackProgress.GetComponent<Slider>();
			//m_oUnPackText = GameObject.Find("Canvas/PreLoading/Progress/Prg/HandleRect/Handle").GetComponent<Text>();
			m_oUnPackTxtInfo = GameObject.Find("Canvas/PreLoading/Progress/TxtInfo").GetComponent<Text>();
			m_goUnPackProgress.SetActive(false);

			m_oPatcherRect = GameObject.Find("Canvas/PreLoading/Patcher");  // Patcher
			m_oBtnPatcher = GameObject.Find("Canvas/PreLoading/Patcher/BtnPatcher");   // 更新按钮 BtnPatcher
			m_oBtnIgnore = GameObject.Find("Canvas/PreLoading/Patcher/BtnIgnore");      // 忽略更新 BtnIgnore
            GameObject.Find("Canvas/PreLoading/Patcher/TxtTitle").GetComponent<Text>().text = "版本更新提示";    // 信息 TxtTitle
            m_oPatcherTxtTitle = GameObject.Find("Canvas/PreLoading/Patcher/TxtDetail").GetComponent<Text>();    // 信息 TxtTitle
			m_oBtnPatcher.GetComponent<Button>().onClick.AddListener(() => _UpDateApp());
			m_oBtnIgnore.GetComponent<Button>().onClick.AddListener(() => _IgnoreUpDateApp());
			m_oPatcherRect.SetActive(false);

            m_oBtnDownload = GameObject.Find("Canvas/PreLoading/Patcher/BtnDownload"); //允许下载更新按钮
            m_oBtnDownload.GetComponent<Button>().onClick.AddListener(() => _DownloadTips());
            m_oBtnQuit = GameObject.Find("Canvas/PreLoading/Patcher/BtnQuit"); //退出游戏按钮
            m_oBtnQuit.GetComponent<Button>().onClick.AddListener(() => _QuitGame());


            m_oErrorRect = GameObject.Find("Canvas/PreLoading/Error");  // Error
			m_oErrorInfo = GameObject.Find("Canvas/PreLoading/Error/TxtErrInfo").GetComponent<Text>();  // TxtErrInfo

			m_btnQuit = GameObject.Find("Canvas/PreLoading/Error/BtnQuit");
			m_btnRetry = GameObject.Find("Canvas/PreLoading/Error/BtnRePlay");

			m_btnQuit.GetComponent<Button>().onClick.AddListener(() => Quit());
			m_btnRetry.GetComponent<Button>().onClick.AddListener(() => PrepareGame());

			m_oErrorRect.SetActive(false);


			m_sLauncherConfPath = GameConfigMgr.ms_sStreamDir + GameConfigMgr.ms_sLauncherConf;    // 随包的Launcher配置的存放路径
			m_sLauncherNoticePath = GameConfigMgr.ms_sStreamDir + GameConfigMgr.ms_sLauncherNotice;    // 随包的Launcher启动器提示信息的存放路径
			m_sVersionPath = GameConfigMgr.ms_sStreamDir + GameConfigMgr.ms_sVersion;              // 随包的原始版本号的存放路径
			m_sServVersionPath = GameConfigMgr.ms_sResrcDir + "ServerVersion.json";      // 服务器版本号文件的存放路径
			m_sClientVersionPath = GameConfigMgr.ms_sResrcDir + "Version.txt";    // 客户端版本号文件的存放路径
			m_sServListPath = GameConfigMgr.ms_sResrcDir + "ServerList.json";        // 服务器列表文件的存放路径

			// 默认没有渠道
			m_nChannel = 0;
			// 默认没有资源包
			m_sPackFlag = "1";
			// 资源包的路径（初始化时，默认为包内地址）
			m_sResourceListUrl = GameConfigMgr.ms_sStreamDir + GameConfigMgr.ms_sRootPackName;
			#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_IPHONE
			m_sResourceListUrl = "file://" + m_sResourceListUrl;
			#endif

			// 准备游戏
			PrepareGame();
		}

		// 退出游戏
		public void Quit()
		{
			ExitGame();
		}
		// 准备游戏
		public void PrepareGame()
		{
			m_bIgnoreUpdate = false;
			m_oEventSystem.SetActive(true);
			m_oPatcherRect.SetActive(false);
			m_oErrorRect.SetActive(false);

			if (GameConfigMgr.ms_eArchive != RunMode.eRelease || GameMgr.IsWin())
			{
				// 直接进入游戏
				InitGameMangager();
				return;
			}

			// 苹果有个烦人的cache机制，所以先删除文件，再下载文件。
			//m_sServVersionPath = GameConfigMgr.ms_sResrcDir + "ServerVersion.json";      // 服务器版本号文件的存放路径
			//m_sServListPath = GameConfigMgr.ms_sResrcDir + "ServerList.json";        // 服务器列表文件的存放路径
			Debug.Log("del: " + m_sServVersionPath);
			Debug.Log("del: " + m_sServListPath);
			if (File.Exists(m_sServVersionPath))
				File.Delete(m_sServVersionPath);
			if (File.Exists(m_sServListPath))
				File.Delete(m_sServListPath);

			// 下载更新公告
			Action<byte[]> aLauncherInfo = (bt) =>
			{
				//string str = System.Text.Encoding.Default.GetString(bt);
				string str = System.Text.Encoding.UTF8.GetString(bt);
				ExtInfo.SetPatcherInfo(str);
			};
			// 游戏是正式版本的时候，要下载服务器列表以及服务器版本号
			Action<byte[]> aGetServList = (bt) =>
			{
				// 生成服务器列表的json文件
				string sDstPath = m_sServListPath.Replace('\\', '/');
				string sDir = sDstPath.Substring(0, sDstPath.LastIndexOf('/'));
				if (!Directory.Exists(sDir))
					Directory.CreateDirectory(sDir);
				if (!CreateNewFile(sDstPath, bt))
					return;

				//string str = System.Text.Encoding.Default.GetString(bt);
				//JsonData jd = JsonMapper.ToObject(str);
				//Util.writeJson(jd, sDstPath);
				// 检查游戏版本（是否需要更新） 
				CheckVersion();
			};
			Action<byte[]> aGetServVersion = (bt) =>
			{
				// 生成服务器版本号的json文件
				string sDstPath = m_sServVersionPath.Replace('\\', '/');
				string sDir = sDstPath.Substring(0, sDstPath.LastIndexOf('/'));
				if (!Directory.Exists(sDir))
					Directory.CreateDirectory(sDir);
				if (!CreateNewFile(sDstPath, bt))
					return;

				string str = System.Text.Encoding.Default.GetString(bt);
				JsonData jd = JsonMapper.ToObject(str);
				m_sServNewVersion = jd["new"].ToString();
				m_sServMidVersion = jd["mid"].ToString();
				m_sServOldVersion = jd["old"].ToString();
				if (jd["list"].GetJsonType() != JsonType.Array)
				{
					// DealError("服务器版本号文件格式错误");
					DealError("ServerVersion.json Error!");
					return;
				}
				for (int i = 0; i < jd["list"].Count; i++)
				{
					m_lstServVersion.Add(jd["list"][i].ToString());
				}
				// 下载服务器列表
				//SetProgress(-1, "下载服务器列表...");
				//SetProgress(-1, "DownLoad ServerList ...");
				SetProgress(-1, m_dicNotice["download_server_list"]);
				StartCoroutine(ReadDataByWWW(m_sServerListUrl, aGetServList));
			};
			// 读取原始版本号和Launcher配置
			string sVersionPath = m_sVersionPath;
			string sLauncherPath = m_sLauncherConfPath;
			string sLauncherNoticePath = m_sLauncherNoticePath;
			#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_IPHONE
			sVersionPath = "file://" + sVersionPath;
			sLauncherPath = "file://" + sLauncherPath;
			sLauncherNoticePath = "file://" + sLauncherNoticePath;
			#endif
			Action<byte[]> aLaunch = (bt) =>
			{
				string str = System.Text.Encoding.UTF8.GetString(bt);
				JsonData jd = JsonMapper.ToObject(str);
				m_sServerVersionUrl = jd["VersionUrl"].ToString();
				m_sServerListUrl = jd["ServerListUrl"].ToString();
				m_sPatchListUrl = jd["PatcherListUrl"].ToString().Replace("\\", "/");
				m_sPatchInfoUrl = jd["PatcherInfoUrl"].ToString().Replace("\\", "/");
				m_sAppDownLoadUrl = jd["AppDownLoadUrl"].ToString();
				m_sPackFlag = jd["PackFlag"].ToString();
				m_nChannel = int.Parse(jd["Channel"].ToString());
				string sResourceListUrl = jd["ResourceListUrl"].ToString();
				if (!string.IsNullOrEmpty(sResourceListUrl))
					m_sResourceListUrl = sResourceListUrl;
				ExtInfo.SetChannel(m_nChannel);
				ExtInfo.SetPatcherInfo("");

				Debug.Log("m_sServerVersionUrl: " + m_sServerVersionUrl);
				Debug.Log("m_sServerListUrl: " + m_sServerListUrl);
				Debug.Log("m_sPatchListUrl: " + m_sPatchListUrl);
				Debug.Log("m_sPatchInfoUrl: " + m_sPatchInfoUrl);
				Debug.Log("m_sAppDownLoadUrl: " + m_sAppDownLoadUrl);
				Debug.Log("m_sPackFlag: " + m_sPackFlag);
				Debug.Log("m_sResourceListUrl: " + m_sResourceListUrl);
				Debug.Log("m_nChannel: " + m_nChannel);
				// 下载服务器版本号文件
				//SetProgress(-1, "下载服务器版本号...");
				//SetProgress(-1, "DownLoad ServerVersion.json ...");
				SetProgress(-1, m_dicNotice["download_server_version"]);
				StartCoroutine(ReadDataByWWW(m_sServerVersionUrl, aGetServVersion));
				// 下载更新公告
				StartCoroutine(ReadDataByWWW(m_sPatchInfoUrl, aLauncherInfo));
			};
			Action<byte[]> aVersion = (bt) =>
			{
				string str = System.Text.Encoding.Default.GetString(bt);
				JsonData jd = JsonMapper.ToObject(str);
				m_sVersion = jd["version"].ToString();
				ExtInfo.ms_sVersion = m_sVersion;
				Debug.Log("m_sVersion: " + m_sVersion);
				// 读取Launcher配置 
				//SetProgress(-1, "读取Launcher配置...");
				//SetProgress(-1, "Read Launcher.json ...");
				SetProgress(-1, m_dicNotice["read_launcher_conf"]);
				StartCoroutine(ReadDataByWWW(sLauncherPath, aLaunch));
			};
			// 读取原始版本号
			//SetProgress(-1, "读取原始版本号...");
			//SetProgress(-1, "Read Client Version ...");
			//StartCoroutine(ReadDataByWWW(sVersionPath, aVersion));

			Action<byte[]> aLauncherNotice = (bt) =>
			{
				string str = System.Text.Encoding.UTF8.GetString(bt);
				JsonData jd = JsonMapper.ToObject(str);

				m_dicNotice["download_server_list"] = jd["download_server_list"].ToString();
				m_dicNotice["download_server_version"] = jd["download_server_version"].ToString();
				m_dicNotice["read_launcher_conf"] = jd["read_launcher_conf"].ToString();
				m_dicNotice["read_client_version"] = jd["read_client_version"].ToString();
				m_dicNotice["check_version"] = jd["check_version"].ToString();
				m_dicNotice["enter_game"] = jd["enter_game"].ToString();
				m_dicNotice["prepare_game_data"] = jd["prepare_game_data"].ToString();
				m_dicNotice["update_game_date"] = jd["update_game_date"].ToString();
				m_dicNotice["generate_version"] = jd["generate_version"].ToString();
				m_dicNotice["prepare_game_data_error"] = jd["prepare_game_data_error"].ToString();
				m_dicNotice["lacuncher_json_error"] = jd["lacuncher_json_error"].ToString();

				m_dicNotice["read_data_error"] = jd["read_data_error"].ToString();
				m_dicNotice["resourcelist_json_error"] = jd["resourcelist_json_error"].ToString();
				m_dicNotice["game_run_error"] = jd["game_run_error"].ToString();
				m_dicNotice["version_error"] = jd["version_error"].ToString();
				m_dicNotice["network_error"] = jd["network_error"].ToString();
				m_dicNotice["patchlist_error"] = jd["patchlist_error"].ToString();
				m_dicNotice["patcher_not_found"] = jd["patcher_not_found"].ToString();
				m_dicNotice["unpack_file"] = jd["unpack_file"].ToString();
				m_dicNotice["read_disk_or_network_error"] = jd["read_disk_or_network_error"].ToString();
				m_dicNotice["client_verison_lost"] = jd["client_verison_lost"].ToString();
				m_dicNotice["write_file_error"] = jd["write_file_error"].ToString();

                //界面提示文字
				m_dicNotice["quit"] = jd["quit"].ToString();
				m_dicNotice["retry"] = jd["retry"].ToString();
                m_dicNotice["patcher"] = jd["patcher"].ToString();
                m_dicNotice["ignore"] = jd["ignore"].ToString();
                m_dicNotice["download"] = jd["download"].ToString();

                //更新提示信息
                m_dicNotice["sizeInfo"] = jd["sizeInfo"].ToString();
                m_dicNotice["firstIntoGame"] = jd["firstIntoGame"].ToString();
                m_dicNotice["versionUpdate"] = jd["versionUpdate"].ToString();
                m_dicNotice["patcherUpdate"] = jd["patcherUpdate"].ToString();
                m_dicNotice["updateAndUnPakage"] = jd["updateAndUnPakage"].ToString();

                m_dicNotice["game_run_mode_error"] = jd["game_run_mode_error"].ToString();

                //设置按钮提示文字
                m_btnQuit.transform.GetChild(0).gameObject.GetComponent<Text>().text = m_dicNotice["quit"];
				m_btnRetry.transform.GetChild(0).gameObject.GetComponent<Text>().text = m_dicNotice["retry"];

                m_oBtnPatcher.transform.GetChild(0).gameObject.GetComponent<Text>().text = m_dicNotice["patcher"];
                m_oBtnIgnore.transform.GetChild(0).gameObject.GetComponent<Text>().text = m_dicNotice["ignore"];

                m_oBtnDownload.transform.GetChild(0).gameObject.GetComponent<Text>().text = m_dicNotice["download"];
                m_oBtnQuit.transform.GetChild(0).gameObject.GetComponent<Text>().text = m_dicNotice["quit"];
                /*foreach(KeyValuePair<string, string> kv in m_dicNotice)
                {
                    Debug.Log(kv.Key + " = " + kv.Value);
                }*/

                //SetProgress(-1, "读取原始版本号...");
                SetProgress(-1, m_dicNotice["read_client_version"]);
				StartCoroutine(ReadDataByWWW(sVersionPath, aVersion));
			};
			StartCoroutine(ReadDataByWWW(sLauncherNoticePath, aLauncherNotice));
		}

        // 检查游戏（是否需要更新）
        private void CheckVersionEx()
        {
            Debug.Log("CheckVersion:");
            //SetProgress(-1, "检查游戏版本...");
            //SetProgress(-1, "Check Version ...");
            SetProgress(-1, m_dicNotice["check_version"]);
            if (GameConfigMgr.ms_eArchive != RunMode.eRelease)
            {
                //DealError("运行模式错误");
                DealError(m_dicNotice["game_run_mode_error"]);
                return;
            }

            //Debug.Log("m_sClientVersion: " + m_sClientVersion);

            // 随包的原始版本号, 大版本号为1000前面的三个数字，小版本号就是1000
            Debug.Log("m_sVersion: " + m_sVersion);
            string sBigVersion = GetBigVersion(m_sVersion);
            int nBigVersion = int.Parse(sBigVersion.Replace(".", ""));
            string sSmallVersion = GetSmallVersion(m_sVersion);
            int nSmallVersion = int.Parse(sSmallVersion);

            // 服务端版本号
            Debug.Log("m_sServNewVersion: " + m_sServNewVersion);
            Debug.Log("m_sServMidVersion: " + m_sServMidVersion);
            Debug.Log("m_sServOldVersion: " + m_sServOldVersion);
            string sBigServNewVer = GetBigVersion(m_sServNewVersion);
            int nBigSerNewVer = int.Parse(sBigServNewVer.Replace(".", ""));
            //string sSmallServNewVer = GetSmallVersion(m_sServNewVersion);
            string sBigSerMidVer = GetBigVersion(m_sServMidVersion);
            int nBigSerMidVer = int.Parse(sBigSerMidVer.Replace(".", ""));
            //string sSmallServMidVer = GetSmallVersion(m_sServMidVersion);
            string sBigSerOldVer = GetBigVersion(m_sServOldVersion);
            int nBigSerOldVer = int.Parse(sBigSerOldVer.Replace(".", ""));
            //string sSmallServOldVer = GetSmallVersion(m_sServOldVersion);

            if (!(nBigSerOldVer <= nBigSerMidVer && nBigSerMidVer <= nBigSerNewVer))
            {
                Debug.Log("服务器哦版本号配置出错，请检查！！");
                return;
            }

            // 1、大版本号比较，大版本号相关需要重新下载游戏 （随包版本号与服务器版本号）
            // 如果客户端原始版本小于服务器的最低版本要求，就得强制更新
            if (nBigVersion < nBigSerOldVer)
            {
                Debug.Log("UpDateApp: " + nBigVersion + "->" + nBigSerOldVer);
                // 强制更新游戏
                UpDateApp(true); 
                return;
            }
            // 如果客户端原始版本小于服务器的禁戒版本要求，就得提示更新
            else if (nBigVersion < nBigSerMidVer && m_bIgnoreUpdate == false)
            {
                Debug.Log("UpDateApp: " + nBigVersion + "->" + nBigSerMidVer);
                // 提示更新游戏
                UpDateApp(false);
                return;
            }

            // 审核服专用 -> 客户端原始大版本号大于服务器的最高版本号
            if (nBigVersion > nBigSerNewVer)
            {
                ExtInfo.SetAppleReview(true);
            }
            else
            {
                ExtInfo.SetAppleReview(false);
            }

            // 客户端版本号文件不存在，需要第一次解压资源
            if (!Directory.Exists(GameConfigMgr.ms_sResrcDir) || !File.Exists(m_sClientVersionPath))
            {
                // 解压资源!
                UnPackage();
                return;
            }

            // 客户端版本号
            JsonData jd = Util.readJson(m_sClientVersionPath);
            m_sClientVersion = jd["version"].ToString();
            ExtInfo.ms_sClientVersion = m_sClientVersion;
            string sClientBigVersion = GetBigVersion(m_sClientVersion);
            string sClientSmallVersion = GetSmallVersion(m_sClientVersion);
            int nClientSmallVersion = int.Parse(sClientSmallVersion);

            Debug.Log("m_sClientVersion: " + m_sClientVersion);
            // 如果随包大版本号与客户端大版本号不同或者随包小版本号比客户端版本号大，需要解压资源
            if (!sBigVersion.Equals(sClientBigVersion) || nSmallVersion > nClientSmallVersion)
            {
                Debug.Log("UnPackage: " + m_sClientVersion + "->" + m_sVersion);
                // 解压资源!
                UnPackage();
                return;
            }

            // 2、小版本号比较，小版本号控制更新资源
            // 如果客户端小版本号小于服务端的小版本号，就需要更新
            string sHeader = sBigVersion + ".";
            for (int i = 0; i < m_lstServVersion.Count; i++)
            {
                string sCurrVer = m_lstServVersion[i];
                if (sCurrVer.StartsWith(sHeader))
                {
                    string sSmallVer = GetSmallVersion(sCurrVer);
                    if (nClientSmallVersion < int.Parse(sSmallVer))
                    {
                        Debug.Log("Patcher: " + m_sClientVersion + "->" + sCurrVer);
                        // 更新资源!
                        Patcher(sCurrVer);
                        return;
                    }
                }
            }

            // 如果上面的检测都不执行，就证明可以直接进入游戏
            InitGameMangager();
        }

        // 检查游戏（是否需要更新）
        private void CheckVersion()
		{
            CheckVersionEx();
            return;
            /*
            Debug.Log("CheckVersion:");
			//SetProgress(-1, "检查游戏版本...");
			//SetProgress(-1, "Check Version ...");
			SetProgress(-1, m_dicNotice["check_version"]);
			if (GameConfigMgr.ms_eArchive != RunMode.eRelease)
			{
				//DealError("运行模式错误");
				DealError(m_dicNotice["game_run_mode_error"]);
				return;
			}

			Debug.Log("m_sVersion: " + m_sVersion);
			//Debug.Log("m_sClientVersion: " + m_sClientVersion);
			Debug.Log("m_sServNewVersion: " + m_sServNewVersion);
			Debug.Log("m_sServMidVersion: " + m_sServMidVersion);
			Debug.Log("m_sServOldVersion: " + m_sServOldVersion);

			// 随包的原始版本号
			string sBigVersion = GetBigVersion(m_sVersion);
			int nBigVersion = int.Parse(sBigVersion.Replace(".", ""));
			string sSmallVersion = GetSmallVersion(m_sVersion);
			int nSmallVersion = int.Parse(sSmallVersion);
			// 服务端版本号
			string sBigServNewVer = GetBigVersion(m_sServNewVersion);
			int nBigSerNewVer = int.Parse(sBigServNewVer.Replace(".", ""));
			//string sSmallServNewVer = GetSmallVersion(m_sServNewVersion);
			string sBigSerMidVer = GetBigVersion(m_sServMidVersion);
			int nBigSerMidVer = int.Parse(sBigSerMidVer.Replace(".", ""));
			//string sSmallServMidVer = GetSmallVersion(m_sServMidVersion);
			string sBigSerOldVer = GetBigVersion(m_sServOldVersion);
			int nBigSerOldVer = int.Parse(sBigSerOldVer.Replace(".", ""));
			//string sSmallServOldVer = GetSmallVersion(m_sServOldVersion);

			// 如果客户端原始版本小于服务器的最低版本要求，就得强制更新
			if (nBigVersion < nBigSerOldVer)
			{
				Debug.Log("UpDateApp: " + nBigVersion + "->" + nBigSerOldVer);
				// 强制更新游戏
				UpDateApp(true);
				return;
			}
			// 如果客户端原始版本小于服务器的禁戒版本要求，就得提示更新
			if (nBigVersion < nBigSerMidVer && m_bIgnoreUpdate == false)
			{
				Debug.Log("UpDateApp: " + nBigVersion + "->" + nBigSerMidVer);
				// 提示更新游戏
				UpDateApp(false);
				return;
			}
			// 如果客户端原始大版本号大于服务器的最高版本号，就设置为审核服。
			if (nBigVersion > nBigSerNewVer)
			{
				ExtInfo.SetAppleReview(true);
			}
			else
			{
				ExtInfo.SetAppleReview(false);
			}
			// 客户端版本号文件不存在，需要第一次解压资源
			if (!Directory.Exists(GameConfigMgr.ms_sResrcDir) || !File.Exists(m_sClientVersionPath))
			{
				// 解压资源!
				UnPackage();
				return;
			}
			// 客户端版本号
			JsonData jd = Util.readJson(m_sClientVersionPath);
			m_sClientVersion = jd["version"].ToString();
			ExtInfo.ms_sClientVersion = m_sClientVersion;
			string sClientBigVersion = GetBigVersion(m_sClientVersion);
			string sClientSmallVersion = GetSmallVersion(m_sClientVersion);
			int nClientSmallVersion = int.Parse(sClientSmallVersion);

			Debug.Log("m_sClientVersion: " + m_sClientVersion);
			// 如果随包大版本号与客户端大版本号不同或者随包小版本号比客户端版本号大，需要解压资源
			if (!sBigVersion.Equals(sClientBigVersion) || nSmallVersion > nClientSmallVersion)
			{
				Debug.Log("UnPackage: " + m_sClientVersion + "->" + m_sVersion);
				// 解压资源!
				UnPackage();
				return;
			}

			// 如果客户端小版本号小于服务端的小版本号，就需要更新
			string sHeader = sBigVersion + ".";
			for (int i = 0; i < m_lstServVersion.Count; i++)
			{
				string sCurrVer = m_lstServVersion[i];
				if (sCurrVer.StartsWith(sHeader))
				{
					string sSmallVer = GetSmallVersion(sCurrVer);
					if (nClientSmallVersion < int.Parse(sSmallVer))
					{
						Debug.Log("Patcher: " + m_sClientVersion + "->" + sCurrVer);
						// 更新资源!
						Patcher(sCurrVer);
						return;
					}
				}
			}

			// 如果上面的检测都不执行，就证明可以直接进入游戏
			InitGameMangager();
            */
		}

		// 实例化游戏管理器
		private IEnumerator _InitGameMangager()
		{
			yield return null;
			yield return null;

			string name = "GameManager";
			GameObject manager = GameObject.Find(name);
			if (manager == null)
			{
				GameObject prefab = Resources.Load(name) as GameObject;
				manager = Instantiate(prefab) as GameObject;
				manager.name = name;
			}

			// 如果GameManager没有初始化成功，就报错退出
			if (manager == null)
			{
				Debug.Log("Error! The GameManager is null!");
				// 游戏退出
				ExitGame();
			}
		}
		// 实例化游戏管理器
		private void InitGameMangager()
		{
            m_bDownload = false;

            //删除下载目录
            if (Directory.Exists(GameConfigMgr.ms_sDownLoadDir))
                Directory.Delete(GameConfigMgr.ms_sDownLoadDir, true);

            Debug.Log("-->> InitGameMangager");
			System.GC.Collect();
			//SetProgress(-1, "正在进入游戏...");
			//SetProgress(-1, "Enter Game ...");
			if(m_dicNotice.ContainsKey("enter_game") == true)
			{
				SetProgress(-1, m_dicNotice["enter_game"]);
			}
			else
			{
				SetProgress(-1, "Game Entering ...");
			}


			m_oEventSystem.SetActive(false);

			//SDKEntryMgr.InitEntry();
			StartCoroutine(_InitGameMangager());
		}

		//--------------------------------------
		// 更新App
		public void _UpDateApp()
		{
			m_oPatcherRect.SetActive(false);
			// Todo 更新App
			Application.OpenURL(m_sAppDownLoadUrl);
			ExitGame();
		}
		// 忽略更新
		public void _IgnoreUpDateApp()
		{
			m_oPatcherRect.SetActive(false);
			m_bIgnoreUpdate = true;
			CheckVersion();
		}

		// 更新App
		private void UpDateApp(bool bForce = false)
		{
			m_oPatcherRect.SetActive(true);
            m_oBtnDownload.SetActive(false);
            m_oBtnQuit.SetActive(false);
            m_oPatcherTxtTitle.text = "New version is " + m_sServNewVersion + ". DownLoad new version？";
			if (bForce)
			{
				m_oBtnPatcher.SetActive(true);
				m_oBtnIgnore.SetActive(false);
			}
			else
			{
				m_oBtnPatcher.SetActive(true);
				m_oBtnIgnore.SetActive(true);
			}
		}


        // 允许下载
        public void _DownloadTips()
        {
            m_oPatcherRect.SetActive(false);
            m_bDownload = true;
            CheckVersion();
        }
        // 退出游戏
        public void _QuitGame()
        {
            m_oPatcherRect.SetActive(false);
            ExitGame();
        }

        // 提示下载相关更新
        private void DownloadTips(string info)
        {
            m_oPatcherRect.SetActive(true);
            m_oPatcherTxtTitle.text = info;
            //隐藏强制更新相关按钮
            m_oBtnPatcher.SetActive(false);
            m_oBtnIgnore.SetActive(false);
            //显示下载相关按钮
            m_oBtnDownload.SetActive(true);
            m_oBtnQuit.SetActive(true);
        }

        // 第一次资源解压
        private void UnPackage()
		{
			//Debug.Log("解压资源");
			ExtInfo.SetFirstRunFlag(true);
			StartCoroutine(_UnPackage());
		}

		// 更新资源到某个版本
		private void Patcher(string sDstVer)
		{

			ExtInfo.SetUpdateBeginTime(System.DateTime.Now);
			//Debug.Log("更新资源");
			StartCoroutine(_Patcher(sDstVer));
		}

		private bool ResourceUnPack(string srcpath)
		{
			string sRsrPath = GameConfigMgr.ms_sABFolderName + "/" + srcpath;
			string sDstPath = GameConfigMgr.ms_sResrcDir + srcpath + GameConfigMgr.ms_sBundleType;
			TextAsset ta = Resources.Load<TextAsset>(sRsrPath);
			bool bOK = CreateNewFile(sDstPath, ta.bytes);
			Resources.UnloadAsset(ta);
			return bOK;
		}
		// 第一次准备资源（解压资源完毕后从StreamAsset文件夹拷贝Version.json文件到Persistent目录）
		private IEnumerator _UnPackage()
		{
			//SetProgress(0, "初始化游戏资源...");
			//SetProgress(0, "Prepare data ...");
			SetProgress(0, m_dicNotice["prepare_game_data"]);
			string sDstDir = GameConfigMgr.ms_sResrcDir.Replace('\\', '/');
			if (sDstDir[sDstDir.Length - 1] != '/')
				sDstDir += '/';
			// 清除旧的资源
			if (Directory.Exists(sDstDir))
				Directory.Delete(sDstDir, true);
			Directory.CreateDirectory(sDstDir);
			yield return null;

			//*
			// 若没有资源包就解压脚本、配置、依赖、版本号；若有就解压资源包
			if (m_sPackFlag == "1")
			{
				bool bOK = true;
				bOK &= ResourceUnPack(GameConfigMgr.ms_sDependName.Substring(0, GameConfigMgr.ms_sDependName.LastIndexOf(".")));
				bOK &= ResourceUnPack(GameConfigMgr.ms_sVersion.Substring(0, GameConfigMgr.ms_sVersion.LastIndexOf(".")));
				bOK &= ResourceUnPack(GameConfigMgr.ms_sLuaScriptFolderName);
				bOK &= ResourceUnPack(GameConfigMgr.ms_sDesignerFolderName);
				Resources.UnloadUnusedAssets();
				System.GC.Collect();
				if (!bOK)
				{
					//DealError("初始化游戏资源异常");
					//DealError("Prepare data ... but error!");
					DealError(m_dicNotice["prepare_game_data_error"]);
					yield break;
				}
				AddProgress(1.0f);
				yield return null;
				// 解析客户端版本号
				UnBundleVersion();
				// 是否需要更新
				PrepareGame();
				//CheckVersion();
			}
			else if (m_sPackFlag == "2")
			{
				// 随包中的资源文件列表
				if (string.IsNullOrEmpty(m_sResourceListUrl))
				{
					//DealError("Lacuncher.json error : ResourceListUrl = " + m_sResourceListUrl);
					DealError(m_dicNotice["lacuncher_json_error"] + m_sResourceListUrl);
					yield break;
				}
				string sListFile = m_sResourceListUrl;
				string sListDir = m_sResourceListUrl.Substring(0, m_sResourceListUrl.LastIndexOf("/")) + "/";
				// 加载json文件,读取所有资源包的列表
				WWW wwww = new WWW(sListFile);
				yield return wwww;
				while (!wwww.isDone)
				{
					if (wwww.error != null && wwww.error.Length > 0)
					{
						// DealError("读取本地数据异常");
						//DealError("Read data ... but error!");
						DealError(m_dicNotice["read_data_error"]);
						yield break;
					}
					yield return 0;
				}
				if (wwww.error != null && wwww.error.Length > 0)
				{
					// DealError("读取本地数据异常");
					//DealError("Read data ... but error!");
					DealError(m_dicNotice["read_data_error"]);
					yield break;
				}
				JsonData jdwcr = JsonMapper.ToObject(wwww.text);
				wwww.Dispose();
				if (jdwcr.GetJsonType() != JsonType.Array)
				{
					//DealError("资源包列表文件格式错误");
					//DealError("ResourceList.json error!");
					DealError(m_dicNotice["resourcelist_json_error"]);
					yield break;
				}

                float downloadSize = 0;
				List<FileMd5> wcrList = new List<FileMd5>();
				for (int i = 0; i < jdwcr.Count; i++)
				{
					string file = jdwcr[i]["file"].ToString();
					string md5 = jdwcr[i]["md5"].ToString();

                    string dataFile = GameConfigMgr.ms_sDownLoadDir + (sListDir + file).Substring(sListDir.Length);
                    // 查看相应的磁盘文件的MD5码。伪断点续传，因为wcr、wcp更新频繁，文件的url有时候又相同，不能使用传统的断点续传
                    if (!CheckMd5(dataFile, md5))
                        downloadSize = downloadSize + float.Parse(jdwcr[i]["size"].ToString());

                    FileMd5 fm5 = new FileMd5();
					fm5.file = sListDir + file;
					fm5.md5 = md5;
					wcrList.Add(fm5);
				}

                //添加提示信息
                string str = m_sResourceListUrl;
                str = str.Remove(str.LastIndexOf("/"));
                str = str.Remove(0, str.LastIndexOf("/") + 1);
                str = str.Remove(0, str.IndexOf("_") + 1);
                m_fTotalDownloadSize = downloadSize + GetPatcherSize(str);
                //string sizeInfo = "\n this time will download " + m_fTotalDownloadSize.ToString("0.00") + "M";
                string sizeInfo = m_dicNotice["sizeInfo"] + m_fTotalDownloadSize.ToString("0.00") + "M";
                if (!m_bDownload && !File.Exists(m_sClientVersionPath))
                {
                    //DownloadTips("First time into the Game, Update the resources of the Game ?" + sizeInfo);
                    DownloadTips(m_dicNotice["firstIntoGame"] + sizeInfo);
                    yield break;
                }
                else if (!m_bDownload)
                {
                    //DownloadTips("Update UnPackage: " + m_sClientVersion + "->" + m_sVersion + " ?" + sizeInfo);
                    DownloadTips(m_dicNotice["versionUpdate"] + m_sClientVersion + "->" + m_sVersion + " ?" + sizeInfo);
                    yield break;
                }
                SetProgress(0);

                // 清除以前无用的unity3d资源
                if (Directory.Exists(GameConfigMgr.ms_sResrcDir))
				{
					if (File.Exists(m_sClientVersionPath))
						File.Delete(m_sClientVersionPath);
					string[] sOldABFiles = Directory.GetFiles(GameConfigMgr.ms_sResrcDir, "*" + GameConfigMgr.ms_sBundleType, SearchOption.AllDirectories);
					for (int i = 0; i < sOldABFiles.Length; i++)
					{
						File.Delete(sOldABFiles[i]);
					}
				}
				// 解压资源包完成后
				Action a = () =>
				{
					UnBundleVersion(); // 解析客户端版本号
					PrepareGame(); // 是否需要更新}
					//CheckVersion();
				};
				// 开协程解压资源包
				StartCoroutine(UnPackageFiles(WCDataType.Package, wcrList.ToArray(), sListDir, GameConfigMgr.ms_sResrcDir, a, downloadSize / m_fTotalDownloadSize));
			}
			else
			{
				//DealError("Lacuncher.json error : PackFlag = " + m_sPackFlag);
				DealError(m_dicNotice["lacuncher_json_error"] + "PackFlag = " + m_sPackFlag);
				yield break;
			}
		}

        private float GetPatcherSize(string clientVersion)
        {
            string sClientSmallVersion = GetSmallVersion(clientVersion);
            int nClientSmallVersion = int.Parse(sClientSmallVersion);

            // 如果客户端小版本号小于服务端的小版本号，就需要更新
            string sBigVersion = GetBigVersion(m_sVersion);
            string sHeader = sBigVersion + ".";
            for (int i = 0; i < m_lstServVersion.Count; i++)
            {
                string sCurrVer = m_lstServVersion[i];
                if (sCurrVer.StartsWith(sHeader))
                {
                    string sSmallVer = GetSmallVersion(sCurrVer);
                    if (nClientSmallVersion < int.Parse(sSmallVer))
                    {
                        return _GetPatcherSize(sCurrVer, clientVersion);
                    }
                }
            }

            return 0;
        }

        private float _GetPatcherSize(string sDstVer,string clientVersion)
        {
            SetProgress(0, m_dicNotice["update_game_date"]);
            if (clientVersion.Equals(string.Empty) || sDstVer.Equals(string.Empty))
            {
                DealError(m_dicNotice["game_run_error"]);
                return 0;
            }
            // 客户端版本号、目标版本号
            string sClientBigVer = GetBigVersion(clientVersion);
            string sServerBigVer = GetBigVersion(sDstVer);
            if (!sClientBigVer.Equals(sServerBigVer))
            {

                DealError(m_dicNotice["version_error"] + clientVersion + " -> " + sDstVer);
                return 0;
            }
            int nClientSmallVer = int.Parse(GetSmallVersion(clientVersion));
            int nServerSmallVer = int.Parse(GetSmallVersion(sDstVer));
            if (nClientSmallVer >= nServerSmallVer)
            {
                DealError(m_dicNotice["version_error"] + clientVersion + " -> " + sDstVer);
                return 0;
            }

            // 下载更新包列表
            string sWcpDir = m_sPatchListUrl.Substring(0, m_sPatchListUrl.LastIndexOf("/")) + "/";
            WWW patcherList = new WWW(m_sPatchListUrl);
            while (!patcherList.isDone)
            {
                // 无法从服务器获得补丁列表文件
                if (patcherList.error != null && patcherList.error.Length > 0)
                {
                    DealError(m_dicNotice["network_error"]);
                    return 0;
                }
            }
            if (patcherList.error != null && patcherList.error.Length > 0)
            {
                DealError(m_dicNotice["network_error"]);
                return 0;
            }
            JsonData jdPatcherList = JsonMapper.ToObject(patcherList.text);
            if (jdPatcherList.GetJsonType() != JsonType.Array)
            {
                DealError(m_dicNotice["patchlist_error"]);
                return 0;
            }

            // 寻找所有需要的更新包
            List<FileMd5> wcpList = new List<FileMd5>();
            int nTemp = nClientSmallVer;
            float downloadSize = 0;
            for (int i = 0; i < jdPatcherList.Count; i++)
            {
                string sPatcherFile = jdPatcherList[i]["file"].ToString();
                string sFileMd5 = jdPatcherList[i]["md5"].ToString();
                if (!sPatcherFile.StartsWith(GameConfigMgr.ms_sPlatformName.ToLower() + "_" + sServerBigVer + "."))
                    continue;
                string[] str = sPatcherFile.Substring(0, sPatcherFile.LastIndexOf('.')).Split('_');
                if (str.Length != 3 || !str[0].Equals(GameConfigMgr.ms_sPlatformName.ToLower()))
                    continue;
                int nOldSmallVer = int.Parse(GetSmallVersion(str[1]));
                int nNewSmallVer = int.Parse(GetSmallVersion(str[2]));
                if (nOldSmallVer < nNewSmallVer && nOldSmallVer == nTemp && nNewSmallVer <= nServerSmallVer)
                {
                    string sFileUrl = sWcpDir + sPatcherFile;
                    nTemp = nNewSmallVer;
                    downloadSize = downloadSize + float.Parse(jdPatcherList[i]["size"].ToString());

                    FileMd5 fm5 = new FileMd5();
                    fm5.file = sFileUrl;
                    fm5.md5 = sFileMd5;
                    wcpList.Add(fm5);
                }
            }

            return downloadSize;
        }

        // 更新处理 
        private IEnumerator _Patcher(string sDstVer)
		{
            //SetProgress(0, "正在更新资源...");
            //SetProgress(0, "Update ...");
            SetProgress(0, m_dicNotice["update_game_date"]);
            if (m_sClientVersion.Equals(string.Empty) || sDstVer.Equals(string.Empty))
			{
				//DealError("游戏运行错误！");
				//DealError("Run error！");
				DealError(m_dicNotice["game_run_error"]);
				yield break;
			}
			// 客户端版本号、目标版本号
			string sClientBigVer = GetBigVersion(m_sClientVersion);
			string sServerBigVer = GetBigVersion(sDstVer);
			if (!sClientBigVer.Equals(sServerBigVer))
			{
				//DealError("客户端大版本号目标大版本号不相等！" + m_sClientVersion + " -> " + sDstVer);
				//DealError("Version error！" + m_sClientVersion + " -> " + sDstVer);
				DealError(m_dicNotice["version_error"] + m_sClientVersion + " -> " + sDstVer);
				yield break;
			}
			int nClientSmallVer = int.Parse(GetSmallVersion(m_sClientVersion));
			int nServerSmallVer = int.Parse(GetSmallVersion(sDstVer));
			if (nClientSmallVer >= nServerSmallVer)
			{
				//DealError("客户端小版本号比服务端小版本号大！" + m_sClientVersion + " -> " + sDstVer);
				DealError(m_dicNotice["version_error"] + m_sClientVersion + " -> " + sDstVer);
				yield break;
			}

			// 下载更新包列表
			string sWcpDir = m_sPatchListUrl.Substring(0, m_sPatchListUrl.LastIndexOf("/")) + "/";
			WWW patcherList = new WWW(m_sPatchListUrl);
			while (!patcherList.isDone)
			{
				// 无法从服务器获得补丁列表文件
				if (patcherList.error != null && patcherList.error.Length > 0)
				{
					//DealError("读取网络数据异常");
					//DealError("Network error!");
					DealError(m_dicNotice["network_error"]);
					yield break;
				}
				yield return 0;
			}
			if (patcherList.error != null && patcherList.error.Length > 0)
			{
				//DealError("读取网络数据异常");
				//DealError("Network error!");
				DealError(m_dicNotice["network_error"]);
				yield break;
			}
			JsonData jdPatcherList = JsonMapper.ToObject(patcherList.text);
			if (jdPatcherList.GetJsonType() != JsonType.Array)
			{
				//DealError("服务器补丁列表文件格式错误!");
				//DealError("PatchList.json error!");
				DealError(m_dicNotice["patchlist_error"]);
				yield break;
			}
			// 寻找所有需要的更新包
			List<FileMd5> wcpList = new List<FileMd5>();
			int nTemp = nClientSmallVer;
            float downloadSize = 0;
            for (int i = 0; i < jdPatcherList.Count; i++)
			{
				string sPatcherFile = jdPatcherList[i]["file"].ToString();
				string sFileMd5 = jdPatcherList[i]["md5"].ToString();
				if (!sPatcherFile.StartsWith(GameConfigMgr.ms_sPlatformName.ToLower() + "_" + sServerBigVer + "."))
					continue;
				string[] str = sPatcherFile.Substring(0, sPatcherFile.LastIndexOf('.')).Split('_');
				if (str.Length != 3 || !str[0].Equals(GameConfigMgr.ms_sPlatformName.ToLower()))
					continue;
				int nOldSmallVer = int.Parse(GetSmallVersion(str[1]));
				int nNewSmallVer = int.Parse(GetSmallVersion(str[2]));
				if (nOldSmallVer < nNewSmallVer && nOldSmallVer == nTemp && nNewSmallVer <= nServerSmallVer)
				{
					string sFileUrl = sWcpDir + sPatcherFile;
					nTemp = nNewSmallVer;

                    string dataFile = GameConfigMgr.ms_sDownLoadDir + sFileUrl.Substring(sWcpDir.Length);
                    // 查看相应的磁盘文件的MD5码。伪断点续传，因为wcr、wcp更新频繁，文件的url有时候又相同，不能使用传统的断点续传
                    if (!CheckMd5(dataFile, sFileMd5))
                        downloadSize = downloadSize + float.Parse(jdPatcherList[i]["size"].ToString());

                    FileMd5 fm5 = new FileMd5();
					fm5.file = sFileUrl;
					fm5.md5 = sFileMd5;
					wcpList.Add(fm5);
				}
			}

            //添加提示信息
            float partPrg = 1.0f;
            //string sizeInfo = "\n this time will download " + downloadSize.ToString("0.00") + "M";
            string sizeInfo = m_dicNotice["sizeInfo"] + downloadSize.ToString("0.00") + "M";
            if (!m_bDownload)
            {
                m_fTotalDownloadSize = downloadSize;
                //DownloadTips("Update Patcher: " + m_sClientVersion + "->" + sDstVer + " ?" + sizeInfo);
                DownloadTips(m_dicNotice["patcherUpdate"] + m_sClientVersion + "->" + sDstVer + " ?" + sizeInfo);
                yield break;
            }
            else
                partPrg = downloadSize / m_fTotalDownloadSize;

            float down = m_fTotalDownloadSize * (1 - partPrg);
            string info = m_dicNotice["updateAndUnPakage"] + "：" + down.ToString("0.00") + "M / " + m_fTotalDownloadSize.ToString("0.00") + "M";
            SetProgress(1 - partPrg, info);

            if (nTemp != nServerSmallVer)
			{
				//DealError("未找到符合要求的补丁!");
				//DealError("No patcher!");
				DealError(m_dicNotice["patcher_not_found"]);
				yield break;
			}
			// 更新资源
			Action a = () =>
			{
				// 解析客户端版本号
				UnBundleVersion();
				// 直接进入游戏
				InitGameMangager();
				DateTime timenow = System.DateTime.Now;
				DateTime beginTime = ExtInfo.GetUpdateBeginTime();
				System.TimeSpan nDeltaTime = timenow - beginTime;
				ExtInfo.SetUpdateCostTime(nDeltaTime.Seconds);                
			};
			StartCoroutine(UnPackageFiles(WCDataType.Patcher, wcpList.ToArray(), sWcpDir, GameConfigMgr.ms_sResrcDir, a, partPrg));
		}

		// 检查MD5
		private bool CheckMd5(string path, string md5code)
		{
			if (!File.Exists(path))
				return false;
			byte[] bt = File.ReadAllBytes(path);
			System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
			string sMd5 = BitConverter.ToString(md5.ComputeHash(bt));
			md5.Clear();
			if (0 == string.Compare(md5code, sMd5))
				return true;
			else
				return false;
		}

		// 用www解压文件, sDstDir默认为：GameConfigMgr.ms_sResrcDir
		private IEnumerator UnPackageFiles(WCDataType wcdt, FileMd5[] sUrl, string sSrcDir, string sDstDir = null, Action a = null, float fPartPrg = 1.0f)
		{
			bool bProgress = (fPartPrg > 0);
			if (sUrl == null || sSrcDir == null)
				yield break;
			if (sDstDir == null)
				sDstDir = GameConfigMgr.ms_sResrcDir;
			sDstDir = sDstDir.Replace("\\", "/");
			if (!sDstDir.EndsWith("/")) sDstDir = sDstDir + "/";

			// 解压
			float fAddPrg = (1.0f / sUrl.Length) * fPartPrg;
			for (int i = 0; i < sUrl.Length; i++)
			{
				bool bNeedCreateFile = true;
				string dataPath = sUrl[i].file;
				string md5Code = sUrl[i].md5;
				string dataFile = GameConfigMgr.ms_sDownLoadDir + dataPath.Substring(sSrcDir.Length);
				// 查看相应的磁盘文件的MD5码。伪断点续传，因为wcr、wcp更新频繁，文件的url有时候又相同，不能使用传统的断点续传
				if (CheckMd5(dataFile, md5Code))
				{
					dataPath = "file://" + dataFile;
					bNeedCreateFile = false;
				}
				Debug.Log("UnPackageFiles: " + dataPath);
				WWW www = new WWW(dataPath);
				yield return null;
				while (!www.isDone)
				{
					if (www.error != null && www.error.Length > 0)
					{
						//DealError("读取本地或网络数据异常");
						//DealError("Read disk or network data error!");
						DealError(m_dicNotice["read_disk_or_network_error"]);
						yield break;
					}
					yield return null;
				}
				if (www.error != null && www.error.Length > 0)
				{
					//DealError("读取本地或网络数据异常");
					//DealError("Read disk or network data error!");
					DealError(m_dicNotice["read_disk_or_network_error"]);
					yield break;
				}
				// 写入磁盘，若意外导致不能解压，下次启动的时候，就不用去网上下载
				if (bNeedCreateFile)
				{
					if (!CreateNewFile(dataFile, www.bytes))
					{
						www.Dispose();
						yield break;
					}
				}
				// 解压资源
				AssetBundle ab = www.assetBundle;
				UnityEngine.Object[] objs = ab.LoadAllAssets();
                //int nStep = sUrl.Length * objs.Length / 100 + 1;
                for (int j = 0; j < objs.Length; j++)
				{
					TextAsset ta = (TextAsset)objs[j];
					string sFilePath = sDstDir + ta.name.Replace("^", "/") + ".bytes";
					// Debug.Log(sFilePath);
					string sFileDir = sFilePath.Substring(0, sFilePath.LastIndexOf("/"));
					if (!Directory.Exists(sFileDir))
						Directory.CreateDirectory(sFileDir);
					if (!CreateNewFile(sFilePath, ta.bytes))
					{
						ab.Unload(true);
						www.Dispose();
						yield break;
					}
                }
				ab.Unload(true);
				www.Dispose();
				System.GC.Collect();

                
				if (bProgress)
                {
                    float fProgress = m_oUnPackProgress.value + fAddPrg;
                    if (fProgress < 0)
                        fProgress = 0;
                    else if (fProgress > 1)
                        fProgress = 1;

                    float down = m_fTotalDownloadSize * fProgress;
                    string info = m_dicNotice["updateAndUnPakage"] + "：" + down.ToString("0.00") + "M / " + m_fTotalDownloadSize.ToString("0.00") + "M";

                    AddProgress(fAddPrg, info);
                }

                yield return null;

				// 更新包需要删掉一些文件
				string sPatcherWcrLst = sDstDir + GameConfigMgr.ms_sPatcherWcrLst;
				if (wcdt == WCDataType.Patcher && File.Exists(sPatcherWcrLst))
				{
					string info = System.Text.Encoding.Default.GetString(File.ReadAllBytes(sPatcherWcrLst));
					JsonData jd = JsonMapper.ToObject(info);
					if (jd["del"].GetJsonType() == JsonType.Array)
					{
						for (int ii = 0; ii < jd["del"].Count; ii++)
						{
							string delfile = sDstDir + jd["del"][ii].ToString();
							if (File.Exists(delfile))
								File.Delete(delfile);
						}
					}
					File.Delete(sPatcherWcrLst);
				}
			}

			// 解压完以后执行的操作
			if (a != null)
				a();
		}

		// 用www读取数据（包括：网络读取、本地读取）；只读取文本和二进制数据
		private IEnumerator ReadDataByWWW(string url, Action<byte[]> a = null)
		{
			Debug.Log("ReadDataByWWW: " + url);
			WWW www = new WWW(url);
			yield return www;
			while (!www.isDone)
			{
				if (www.error != null && www.error.Length > 0)
				{
					//DealError("读取本地或网络数据异常");
					//DealError("Read disk or network data error!");
					DealError(m_dicNotice["read_disk_or_network_error"]);
					Debug.Log(url + " 1:\n" + www.error);
					yield break;
				}
				yield return www;
			}
			if (www.error != null && www.error.Length > 0)
			{
				//DealError("读取本地或网络数据异常");
				//DealError("Read disk or network data error!");
				DealError(m_dicNotice["read_disk_or_network_error"]);
				Debug.Log(url + " 2:\n" + www.error);
				yield break;
			}
			yield return www;
			if (a != null)
			{
				a(www.bytes);
			}
			www.Dispose();
		}

		// 解析客户端版本号，并生成Version.txt
		private void UnBundleVersion()
		{
			//SetProgress(-1, "生成版本号...");
			//SetProgress(-1, "Generate version ...");
			SetProgress(-1, m_dicNotice["generate_version"]);
			// 把原始版本号写入到资源目录的客户端版本号文件中
			string sVersionAB = GameConfigMgr.ms_sResrcDir + GameConfigMgr.ms_sVersion.Replace(".txt", GameConfigMgr.ms_sBundleType);
			if (File.Exists(sVersionAB))
			{
				string sDstPath = m_sClientVersionPath.Replace('\\', '/');
				string sDir = sDstPath.Substring(0, sDstPath.LastIndexOf('/'));
				if (!Directory.Exists(sDir))
					Directory.CreateDirectory(sDir);
				// 读取版本号
				//AssetBundle abVer = AssetBundle.LoadFromMemory(File.ReadAllBytes(sVersionAB));
				//TextAsset sVer = abVer.LoadAsset(GameConfigMgr.ms_sVersion.Substring(0, GameConfigMgr.ms_sVersion.LastIndexOf(".")), typeof(TextAsset)) as TextAsset;
                /*
                JsonData jdd = JsonMapper.ToObject(sVer.text);
                // 写入版本号
                Util.writeJson(jdd, sDstPath);
                //*/

                byte[] bytes = File.ReadAllBytes(sVersionAB);
                //string verson = System.Text.Encoding.Default.GetString(bytes);

                if (!CreateNewFile(sDstPath, bytes))
				{
					//abVer.Unload(true);
					return;
				}
				//abVer.Unload(true);
			}
			else
			{
				//DealError("丢失客户端版本号文件： " + sVersionAB);
				//DealError("Lost ClientVersion.json： " + sVersionAB);
				DealError(m_dicNotice["client_verison_lost"] + sVersionAB);
			}
		}

		// 设置进度
		private void SetProgress(float fProgress, string sInfo = null)
		{
            if (m_bDownload && sInfo !=null &&  sInfo.IndexOf(m_dicNotice["updateAndUnPakage"]) != 0)
                return;

			if (sInfo != null)
				m_oUnPackTxtInfo.text = sInfo;
			if (fProgress < 0)
			{
				m_goUnPackProgress.SetActive(false);
			}
			else
			{
				m_goUnPackProgress.SetActive(true);
				m_oUnPackProgress.value = fProgress;
				// m_oUnPackText.text = ((int)(fProgress * 100)).ToString() + "%";
			}
		}
		// 增加进度
		private void AddProgress(float fAdd, string info = null)
		{
			if (m_goUnPackProgress.activeSelf)
			{
				float fProgress = m_oUnPackProgress.value + fAdd;
				if (fProgress < 0)
					fProgress = 0;
				else if (fProgress > 1)
					fProgress = 1;
				SetProgress(fProgress, info);
			}
		}

		// 获取大版本号
		private string GetBigVersion(string ver)
		{
			return ver.Substring(0, ver.LastIndexOf('.'));
		}
		// 获取小版本号
		private string GetSmallVersion(string ver)
		{
			return ver.Substring(ver.LastIndexOf('.') + 1);
		}

		// 错误面板
		private bool DealError(string sError)
		{
			if (sError == null || sError.Length <= 0)
				return false;
			m_oErrorRect.SetActive(true);
			m_oErrorInfo.text = sError;

			//GameObject.Find("Canvas/PreLoading/Error/BtnQuit/Text").GetComponent<Text>().text = m_dicNotice["quit"];
			//GameObject.Find("Canvas/PreLoading/Error/BtnRePlay/Text").GetComponent<Text>().text = m_dicNotice["retry"];

			return true;
		}

		bool CreateNewFile(string path, byte[] bt)
		{
			if (path == null || bt == null)
				return false;
			try
			{
				string sDstPath = path.Replace('\\', '/');
				string sDir = sDstPath.Substring(0, sDstPath.LastIndexOf('/'));
				if (!Directory.Exists(sDir))
					Directory.CreateDirectory(sDir);
				FileStream file = new FileStream(path, FileMode.Create, FileAccess.Write);
				file.Write(bt, 0, bt.Length);
				//file.Flush();
				file.Close();
				//file.Dispose();
				file = null;
				return true;
			}
			catch (System.Exception ex)
			{
				//DealError("写文件异常。读写文件权限不足或者空间不足！");
				//DealError("Write file error!" + ex.ToString());
				DealError(m_dicNotice["write_file_error"] + ex.ToString());
				return false;
			}
		}

		// 退出游戏
		public static void ExitGame()
		{
		#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
		#else
			Application.Quit();
		#endif
		}
	}
}
