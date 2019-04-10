using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using LitJson;
using System.IO;
using System;
using WCG;

public class PackageScatteredWnd : EditorWindow
{
    static readonly string ms_sServVerFileName = "ServerVersion.json";
    static readonly string ms_sPatchLstFileName = "PatcherList.json";

    [MenuItem("WCTool/PackageScatteredWindow", false, 4)]
    static void OpenWindow()
    {
        EditorWindow.GetWindow<PackageScatteredWnd>("PackageScatteredWnd");
    }

    // 版本号相关
    private string m_sOldVersion;       // 老的版本号
    private string m_sNewVersion;       // 新的版本号
    private string m_sServNewVer;       // 服务器最新版本号
    private string m_sServMidVer;       // 服务器禁戒版本号
    private string m_sServOldVer;       // 服务器最低版本号
    private Vector2 m_vScroll;
    private List<string> m_lstServVer;  // 服务器版本号列表
    // 是否需要调试信息
    private int m_nBuildSelect = 1;     // 0:Debug    1:Release
    // Build相关
    private bool m_bBundleLua;      // 是否需要将Lua脚本和策划配置打成unity3d资源包，BundleAll已包含此功能。
    private bool m_bBundleMap;      // Map是否需要打unity3d资源包
    private bool m_bBundleData;      // Data是否需要打unity3d资源包
    //private bool m_bBundleUI;      // UI是否需要打unity3d资源包
    //private bool m_bBundleChar;      // Char是否需要打unity3d资源包
    //private bool m_bBundleObj;      // Obj是否需要打unity3d资源包
    //private bool m_bBundleEff;      // Effect是否需要打unity3d资源包
    //private bool m_bBundleMedia;      // Media是否需要打unity3d资源包
    private bool m_bGenNewVersion;  // 是否需要生成新的版本号
    private bool m_bPackage;        // 是否需要打整包（wcr资源包）
    private bool m_bBuildApp;       // 是否需要Build生成App，C#脚本有改动时，需要生成大版本
    private bool m_bCopyBundle;     // 是否需要拷贝unity3d资源包，以便可以生成Patcher
    private bool m_bCopyWinRsrc;      // windows平台下，是否需要拷贝 Lua脚本、策划配置、DataBundle
    // Patcher更新包相关
    private bool m_bShowBuildPatcher;
    private bool m_bIgnoreMap;  // 是否忽略maps
    private string m_sOldABDir;
    private string m_sNewABDir;
    private string m_sNewWcpPath;

    private bool m_bBuildServerVL;
    // 平台相关
    private BuildTarget m_BuildTarget;  // 平台
    private string m_sPlatform;         // 平台
    private bool m_bBuildProject;     // 平台工程

    private string m_sPatcherDir;   // 生成更新包目录

    void Awake()
    {
        // 平台相关
#if UNITY_ANDROID           // Android
        m_BuildTarget = BuildTarget.Android;
        m_sPlatform = PackageScatteredApp.ms_sAdrPlatformName;
#elif UNITY_IPHONE          // iPhone
            m_BuildTarget = BuildTarget.iPhone;
            m_sPlatform = PackageScatteredApp.ms_sIosPlatformName;
#elif UNITY_STANDALONE_WIN  // windows平台
            m_BuildTarget = BuildTarget.StandaloneWindows;
            m_sPlatform = PackageScatteredApp.ms_sWinPlatformName;
#else
            m_sPlatform = "ErrorPlatform";
#endif
        // 版本号相关
        m_sOldVersion = PackageScatteredApp.ms_sVersion;
        m_sNewVersion = m_sOldVersion;
        m_sServNewVer = string.Empty;   // 服务器最新版本号
        m_sServMidVer = string.Empty;   // 服务器禁戒版本号
        m_sServOldVer = string.Empty;   // 服务器最低版本号
        m_vScroll = new Vector2(0, 0);
        // 是否需要调试信息
        m_nBuildSelect = 1;     // 0:Debug    1:Release
        // Build相关
        m_bBundleLua = true;       // 是否需要将Lua脚本和策划配置打成unity3d资源包，BundleAll已包含此功能。
        m_bBundleMap = true;      // Map是否需要打unity3d资源包
        m_bBundleData = true;     // Data是否需要打unity3d资源包
        //m_bBundleUI = true;      // UI是否需要打unity3d资源包
        //m_bBundleChar = true;      // Char是否需要打unity3d资源包
        //m_bBundleObj = true;      // Obj是否需要打unity3d资源包
        //m_bBundleEff = true;      // Effect是否需要打unity3d资源包
        //m_bBundleMedia = true;      // Media是否需要打unity3d资源包
        m_bGenNewVersion = true;    // 是否需要生成新的版本号
        m_bPackage = true;          // 是否需要打整包（wcr资源包）
        m_bBuildApp = true;         // 是否需要Build生成App
        m_bCopyBundle = false;       // 是否需要拷贝unity3d资源包，以便可以生成Patcher
        m_bCopyWinRsrc = true;      // windows平台下，是否需要拷贝 Lua脚本、策划配置、DataBundle

        m_bBuildProject = false;
        m_bIgnoreMap = true;

        m_bBuildServerVL = false;

        DirectoryInfo dir = new DirectoryInfo(PackageScatteredApp.ms_sPatcherDir);
        m_sPatcherDir = dir.FullName.Replace("\\", "/") + "GenPatcher/";   // 生成的更新包目录
        if (m_sPatcherDir.EndsWith("/"))
            m_sPatcherDir = m_sPatcherDir.Substring(0, m_sPatcherDir.Length - 1);


        m_lstServVer = new List<string>();   // 服务器版本号列表
        if (File.Exists(m_sPatcherDir + "/" + ms_sServVerFileName))
        {
            JsonData jd = Util.readJson(m_sPatcherDir + "/" + ms_sServVerFileName);
            m_sServNewVer = jd["new"].ToString();
            m_sServMidVer = jd["mid"].ToString();
            m_sServOldVer = jd["old"].ToString();
            if (jd["list"].GetJsonType() == JsonType.Array)
            {
                for (int i = 0; i < jd["list"].Count; i++)
                {
                    m_lstServVer.Add(jd["list"][i].ToString());
                }
            }
        }
    }

    // 补丁包路径
    private string GenWcpPath()
    {
        string platform = PackageScatteredApp.ms_target.ToString();
        string dir = "";
        string oldName = "";
        string newName = "";
        if (!string.IsNullOrEmpty(m_sOldABDir))
        {
            System.IO.DirectoryInfo oDir = new System.IO.DirectoryInfo(m_sOldABDir);
            if (!oDir.Name.StartsWith(platform))
            {
                Debug.LogError("platform: " + platform);
                return "";
            }
            oldName = oDir.Name.Substring(platform.Length + 1);
            dir = oDir.Parent.Parent.FullName.Replace("\\", "/") + "/" + "GenPatcher/";
        }
        if (!string.IsNullOrEmpty(m_sNewABDir))
        {
            System.IO.DirectoryInfo oDir = new System.IO.DirectoryInfo(m_sNewABDir);
            if (!oDir.Name.StartsWith(platform))
            {
                Debug.LogError("platform: " + platform);
                return "";
            }
            newName = oDir.Name.Substring(platform.Length + 1);
            dir = oDir.Parent.Parent.FullName.Replace("\\", "/") + "/" + "GenPatcher/";
        }

        return dir + platform + "_" + oldName + "_" + newName + GameConfigMgr.ms_sWcpType;
    }

    //更新包相关
    private void DrawBuildPatcher()
    {
        EditorGUILayout.Separator();
        //EditorGUILayout.LabelField("4.Patcher:", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        m_bIgnoreMap = EditorGUILayout.Toggle("IgnoreMap", m_bIgnoreMap);
        EditorGUILayout.LabelField("是否忽略Map(Unity场景是否有修改)");
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();
        // 老的资源包目录
        EditorGUILayout.BeginHorizontal();
        m_sOldABDir = EditorGUILayout.TextField(m_sOldABDir);
        if (GUILayout.Button("老的资源包目录"))
        {
            EditorGUIUtility.editingTextField = false;
            m_sOldABDir = EditorUtility.OpenFolderPanel("老资源包目录选择", GameConfigMgr.ms_sPatcherDir, "");
            m_sNewWcpPath = GenWcpPath();
        }
        EditorGUILayout.EndHorizontal();
        // 新的资源包目录
        EditorGUILayout.BeginHorizontal();
        m_sNewABDir = EditorGUILayout.TextField(m_sNewABDir);
        if (GUILayout.Button("新的资源包目录"))
        {
            Debug.Log(GameConfigMgr.ms_sPatcherDir);

            EditorGUIUtility.editingTextField = false;
            m_sNewABDir = EditorUtility.OpenFolderPanel("新资源包目录选择", GameConfigMgr.ms_sPatcherDir, "");
            m_sNewWcpPath = GenWcpPath();
        }
        EditorGUILayout.EndHorizontal();
        // 生成的更新包目录
        EditorGUILayout.BeginHorizontal();
        m_sNewWcpPath = EditorGUILayout.TextField(m_sNewWcpPath);
        EditorGUILayout.EndHorizontal();
        // 制作补丁包
        EditorGUILayout.Separator();
        if (GUILayout.Button("(1)生成补丁包"))
        {
            EditorGUIUtility.editingTextField = false;
            PackageScatteredApp.GenPatcher(m_sOldABDir, m_sNewABDir, m_sNewWcpPath, m_bIgnoreMap);
        }

        // 生成更新包列表文件
        EditorGUILayout.Separator();
        string sPatchLstPath = m_sNewWcpPath + "/GenPatcher/" + ms_sPatchLstFileName;
        EditorGUILayout.LabelField("(2)更新包列表文件制作: " + sPatchLstPath);
        if (GUILayout.Button("生成更新包列表文件"))
        {
            EditorGUIUtility.editingTextField = false;
            GenPatcherLst();
        }
    }

    private void BuildServerVersionLst()
    {
        // 服务器版本号制作
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("(1)服务器版本号制作: " + m_sPatcherDir + "/" + ms_sServVerFileName);
        m_sServNewVer = EditorGUILayout.TextField("Server New Version:", m_sServNewVer);   //输入新版本号
        m_sServMidVer = EditorGUILayout.TextField("Server Mid Version:", m_sServMidVer);   //输入新版本号
        m_sServOldVer = EditorGUILayout.TextField("Server Old Version:", m_sServOldVer);   //输入新版本号
        EditorGUILayout.LabelField("历史版本号（每次C#相关有修改出的大版本的完整版本号(x.x.x.xxxx)）:");
        // 历史版本号列表
        if (m_lstServVer.Count == 0)
            m_lstServVer.Add("");
        for (int i = 0; i < m_lstServVer.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            m_lstServVer[i] = EditorGUILayout.TextField(i.ToString(), m_lstServVer[i]);
            if (GUILayout.Button("－", GUILayout.Width(32)))
            {
                EditorGUIUtility.editingTextField = false;
                m_lstServVer.RemoveAt(i);
            }
            if (GUILayout.Button("＋", GUILayout.Width(32)))
            {
                EditorGUIUtility.editingTextField = false;
                m_lstServVer.Insert(i + 1, "");
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Separator();
        if (GUILayout.Button("服务器版本号制作"))
        {
            EditorGUIUtility.editingTextField = false;
            GenServVersion();
        }
    }

    void OnGUI()
    {
        //================================== 版本号的处理 ==================================//
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("1.Version:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Old Version: " + m_sOldVersion);
        m_sNewVersion = EditorGUILayout.TextField("New Version:", m_sNewVersion);   //输入新版本号

        //================================== Svn更新 ==================================//
        //if (GUILayout.Button("SvnUpdate"))
        //{
        //    EditorGUIUtility.editingTextField = false;
        //    PackageScatteredWnd.SvnUpdate();
        //}

        //================================== 平台相关 ==================================//
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("2.Platforms: " + m_sPlatform, EditorStyles.boldLabel);

        //================================== 是否需要调试信息 ==================================//
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("3.BuildOption:", EditorStyles.boldLabel);
        m_nBuildSelect = EditorGUILayout.Popup("BuildOption", m_nBuildSelect, new string[] { "DeBug", "Release" });

        //================================== Build打包相关 ==================================//
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("4.Build:", EditorStyles.boldLabel);
        // 是否需要生成新的版本号
        EditorGUILayout.BeginHorizontal();
        m_bGenNewVersion = EditorGUILayout.Toggle("GenNewVersion", m_bGenNewVersion);
        EditorGUILayout.LabelField("是否需要生成新的版本号");
        EditorGUILayout.EndHorizontal();
        // 是否需要将Lua脚本和策划配置打成unity3d资源包，BundleAll已包含此功能。
        EditorGUILayout.BeginHorizontal();
        m_bBundleLua = EditorGUILayout.Toggle("BundleLua", m_bBundleLua);
        EditorGUILayout.LabelField("是否需要将Lua脚本和策划配置打成 AssetBundle 资源包");
        EditorGUILayout.EndHorizontal();
        // Map是否需要打unity3d资源包
        EditorGUILayout.BeginHorizontal();
        m_bBundleMap = EditorGUILayout.Toggle("BundleMap", m_bBundleMap);
        EditorGUILayout.LabelField("Map是否需要打成 AssetBundle 资源包");
        EditorGUILayout.EndHorizontal();
        // Map是否需要打unity3d资源包
        EditorGUILayout.BeginHorizontal();
        m_bBundleData = EditorGUILayout.Toggle("BundleData", m_bBundleData);
        EditorGUILayout.LabelField("Data是否需要打成 AssetBundle 资源包");
        EditorGUILayout.EndHorizontal();
        /*// UI是否需要打unity3d资源包
        EditorGUILayout.BeginHorizontal();
        m_bBundleUI = EditorGUILayout.Toggle("BundleUI", m_bBundleUI);
        EditorGUILayout.LabelField("UI是否需要打成 AssetBundle 资源包");
        EditorGUILayout.EndHorizontal();
        // Char是否需要打unity3d资源包
        EditorGUILayout.BeginHorizontal();
        m_bBundleChar = EditorGUILayout.Toggle("BundleChar", m_bBundleChar);
        EditorGUILayout.LabelField("Character是否需要打成 AssetBundle 资源包");
        EditorGUILayout.EndHorizontal();
        // Obj是否需要打unity3d资源包
        EditorGUILayout.BeginHorizontal();
        m_bBundleObj = EditorGUILayout.Toggle("BundleObj", m_bBundleObj);
        EditorGUILayout.LabelField("Obj是否需要打成 AssetBundle 资源包");
        EditorGUILayout.EndHorizontal();
        // Effect是否需要打unity3d资源包
        EditorGUILayout.BeginHorizontal();
        m_bBundleEff = EditorGUILayout.Toggle("BundleEff", m_bBundleEff);
        EditorGUILayout.LabelField("Effect是否需要打成 AssetBundle 资源包");
        EditorGUILayout.EndHorizontal();
        // Media是否需要打unity3d资源包
        EditorGUILayout.BeginHorizontal();
        m_bBundleMedia = EditorGUILayout.Toggle("BundleMedia", m_bBundleMedia);
        EditorGUILayout.LabelField("Media是否需要打成 AssetBundle 资源包");
        EditorGUILayout.EndHorizontal();*/
        // 是否需要打整包（wcr资源包）
        EditorGUILayout.BeginHorizontal();
        m_bPackage = EditorGUILayout.Toggle("Package", m_bPackage);
        EditorGUILayout.LabelField("是否需要打散包（资源包打散包）");
        EditorGUILayout.EndHorizontal();
        // 是否需要拷贝unity3d资源包，以便可以生成Patcher
        EditorGUILayout.BeginHorizontal();
        m_bCopyBundle = EditorGUILayout.Toggle("CopyBundle", m_bCopyBundle);
        EditorGUILayout.LabelField("是否需要拷贝 AssetBundle 资源包，生成完整安装包");
        EditorGUILayout.EndHorizontal();
        // 是否需要Build生成App，C#脚本有改动时，需要生成大版本
        EditorGUILayout.BeginHorizontal();
        m_bBuildApp = EditorGUILayout.Toggle("BuildApp", m_bBuildApp);
        EditorGUILayout.LabelField("是否需要Build生成App(安装文件)，C#脚本有改动时，需要生成大版本");
        EditorGUILayout.EndHorizontal();

        // 是否需要 安卓工程
		if (AppPackage.ms_target == BuildTarget.Android || AppPackage.ms_target == BuildTarget.iOS)
		{
			EditorGUILayout.BeginHorizontal ();
			m_bBuildProject = EditorGUILayout.Toggle ("BuildAndroidPrj", m_bBuildProject);
			EditorGUILayout.LabelField ("是否需要 Android/IOS工程(接外部功能,比如:接SDK才需要选中)");
			EditorGUILayout.EndHorizontal ();
		}

        // windows平台下，是否需要拷贝 Lua脚本、策划配置、DataBundle
        if (m_sPlatform == PackageScatteredApp.ms_sWinPlatformName)
        {
            EditorGUILayout.BeginHorizontal();
            m_bCopyWinRsrc = EditorGUILayout.Toggle("CopyWinRsrc", m_bCopyWinRsrc);
            EditorGUILayout.LabelField("windows平台下，是否需要拷贝 Lua脚本、策划配置");
            EditorGUILayout.EndHorizontal();
        }
        
        // 根据需求进行各项Build操作
        if (GUILayout.Button("Build（上面的设置只针对Build，出的是需要解压的版本）"))
        {
            EditorGUIUtility.editingTextField = false;
            Debug.Log("Build");
            Build();
        }

        //================================== 更新包相关 ==================================//
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("5.Patcher:", EditorStyles.boldLabel);
        if (!Directory.Exists(GameConfigMgr.ms_sPatcherDir))
            return;
        m_bShowBuildPatcher = EditorGUILayout.Foldout(m_bShowBuildPatcher, "5.Patcher(更新补丁包相关)");
        if (m_bShowBuildPatcher)
            DrawBuildPatcher();

        //================================== 更新包相关服务器版本号制作 ==================================//
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("6.服务器版本号管理文件:", EditorStyles.boldLabel);
        m_bBuildServerVL = EditorGUILayout.Foldout(m_bBuildServerVL, "6.服务器版本号相关制作");
        if (m_bBuildServerVL)
            BuildServerVersionLst();
    }

    


    // 比较两个版本的大小。0：相等    -1：小    1：大。
    private int CompareVer(string ver1, string ver2)
    {
        int nBig1 = int.Parse(ver1.Substring(0, ver1.LastIndexOf('.')).Replace(".", ""));
        int nSmall1 = int.Parse(ver1.Substring(ver1.LastIndexOf('.') + 1));
        int nBig2 = int.Parse(ver2.Substring(0, ver2.LastIndexOf('.')).Replace(".", ""));
        int nSmall2 = int.Parse(ver2.Substring(ver2.LastIndexOf('.') + 1));
        if (nBig1 > nBig2)
            return 1;
        else if (nBig1 < nBig2)
            return -1;
        else
        {
            if (nSmall1 > nSmall2)
                return 1;
            else if (nSmall1 < nSmall2)
                return -1;
            else
                return 0;
        }
    }

    // 生成更新包列表文件
    void GenPatcherLst()
    {
        string sPatchLstPath = m_sPatcherDir + "/" + ms_sPatchLstFileName;
        string[] files = Directory.GetFiles(m_sPatcherDir, "*" + PackageScatteredApp.ms_sPatcherFileType, SearchOption.TopDirectoryOnly);
        List<string> lstPatcher = new List<string>();
        for (int i = 0; i < files.Length; i++)
        {
            string sNewFileName = files[i].Substring(0, files[i].LastIndexOf(".")).Substring(m_sPatcherDir.Length + 1);
            Debug.Log(files[i] + " <=> " + sNewFileName);
            string[] sNewVer = sNewFileName.Split('_');
            if (sNewVer.Length != 3)
                continue;
            int nPos = lstPatcher.Count;
            for (int j = lstPatcher.Count - 1; j >= 0; j--)
            {
                string sFileName = lstPatcher[j];
                string[] sVer = sFileName.Split('_');
                if (sVer.Length != 3)
                    continue;
                if (-1 == CompareVer(sNewVer[1], sVer[1]) || (0 == CompareVer(sNewVer[1], sVer[1]) && 1 == CompareVer(sNewVer[2], sVer[2])))
                {
                    nPos = j;
                }
            }
            lstPatcher.Insert(nPos, sNewFileName);
        }
        // 生成更新包列表文件
        JsonData jd = new JsonData();
        jd.SetJsonType(JsonType.Array);
        System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] bt;
        string sMd5 = "";
        for (int i = 0; i < lstPatcher.Count; i++)
        {
            Debug.Log("GenPatcherLst: " + lstPatcher[i]);
            bt = File.ReadAllBytes(m_sPatcherDir + "/" + lstPatcher[i] + PackageScatteredApp.ms_sPatcherFileType);
            sMd5 = BitConverter.ToString(md5.ComputeHash(bt));
            JsonData jdd = new JsonData();
            jdd.SetJsonType(JsonType.Object);
            jdd["file"] = lstPatcher[i] + PackageScatteredApp.ms_sPatcherFileType;
            jdd["md5"] = sMd5;
            jdd["size"] = (bt.Length / 1024.0f / 1024.0f).ToString();
            jd.Add(jdd);
        }
        md5.Clear();
        Util.writeJson(jd, sPatchLstPath);
    }

    // 服务器版本号制作
    private void GenServVersion()
    {
        JsonData jd = new JsonData();
        jd["new"] = m_sServNewVer;
        jd["mid"] = m_sServMidVer;
        jd["old"] = m_sServOldVer;
        JsonData jdd = new JsonData();
        for (int i = 0; i < m_lstServVer.Count; i++)
        {
            if (m_lstServVer[i].Length < 7)
                Debug.LogError("历史版本号格式错误" + m_lstServVer[i]);
            else
                jdd.Add(m_lstServVer[i]);
        }
        jd["list"] = jdd;
        if (jdd.GetJsonType() == JsonType.Array && jdd.Count == m_lstServVer.Count)
            Util.writeJson(jd, m_sPatcherDir + "/" + ms_sServVerFileName);
        else
            Debug.LogError("服务器版本号生成失败");
    }

    void Build(bool bUnInitData = false)
    {
        GenOptions genOpt = GenOptions.None;
        if (m_bGenNewVersion) genOpt = genOpt | GenOptions.bGenVersion;
        if (m_bBundleLua) genOpt = genOpt | GenOptions.bBundleLua;
        if (m_bBundleMap) genOpt = genOpt | GenOptions.bBundleMap;
        if (m_bBundleData) genOpt = genOpt | GenOptions.bBundleUI;
        //if (m_bBundleUI) genOpt = genOpt | GenOptions.bBundleUI;
        //if (m_bBundleChar) genOpt = genOpt | GenOptions.bBundleChar;
        //if (m_bBundleObj) genOpt = genOpt | GenOptions.bBundleObj;
        //if (m_bBundleEff) genOpt = genOpt | GenOptions.bBundleEff;
        //if (m_bBundleMedia) genOpt = genOpt | GenOptions.bBundleMedia;
        if (m_bPackage) genOpt = genOpt | GenOptions.bPackage;
        if (m_bBuildApp) genOpt = genOpt | GenOptions.bBuildApp;
        if (m_bCopyBundle) genOpt = genOpt | GenOptions.bCopyBundle;
        if (m_bCopyWinRsrc) genOpt = genOpt | GenOptions.bCopyWinRsrc;

        //Release版本
        BuildOptions buildOptions = BuildOptions.None;
        if (m_nBuildSelect == 0)
            buildOptions = BuildOptions.Development | BuildOptions.ConnectWithProfiler | BuildOptions.AllowDebugging;     // debug
        else
            buildOptions = BuildOptions.None;   // release

        //平台工程
        if (m_bBuildProject && AppPackage.ms_target == BuildTarget.Android)
            buildOptions = buildOptions | BuildOptions.AcceptExternalModificationsToPlayer;
        else if (m_bBuildProject && AppPackage.ms_target == BuildTarget.iOS)
            buildOptions = buildOptions | BuildOptions.AcceptExternalModificationsToPlayer | BuildOptions.Il2CPP;

        PackageScatteredApp.Build(m_sNewVersion, m_BuildTarget, buildOptions, genOpt, bUnInitData);

        m_sOldVersion = PackageScatteredApp.ms_sVersion;
    }
}
