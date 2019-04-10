using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using LitJson;
using System.IO;

public class PackageWnd : EditorWindow
{
    [MenuItem("WCTool/PackageWindow", false, 2)]
    static void OpenWindow()
    {
        EditorWindow.GetWindow<PackageWnd>("Package");
    }

	private bool m_bShowReleaseBuild;
    private bool m_bShowBuildPatcher;
    private string m_sOldVersion;
    private string m_sNewVersion;
    private bool m_bSvnUpdate;
	private bool m_bBundleLua;
    private bool m_bBundleData;
    private bool m_bBundleMap;
    private bool m_bBuildApp;
    private bool m_bCopyData;
    private bool m_bBuildRelease;   // true 为 Release 版本，否则为 Debug 调试版本
	private bool m_bBuildProject;

    // 补丁包相关
    //private string m_sOldABDir = "";
    //private string m_sNewABDir = "";
    //private string m_sNewWcpPath = "";

    void Awake()
    {
        //this.ShowNotification(new GUIContent("Awake"));
        m_sOldVersion = AppPackage.ms_sNewVersion;
        m_sNewVersion = m_sOldVersion;
		m_bSvnUpdate = true;
        m_bBundleMap = true;
        m_bBundleData = true;
		m_bBundleLua = AppPackage.ms_target != BuildTarget.StandaloneWindows;
        m_bCopyData = true;
		m_bBuildApp = true;
        m_bBuildRelease = AppPackage.ms_target != BuildTarget.StandaloneWindows;
		m_bBuildProject = false;
    }

    void OnGUI()
    {
        EditorGUILayout.Separator();
        string sTarget = string.Format("<color=#C7C7C7>{0}</color><color=#{1}>{2}</color>", "Platforms: ", ColorUtility.ToHtmlStringRGBA(Color.red), AppPackage.ms_target.ToString());
        EditorGUILayout.LabelField(sTarget, EditorStyles.inspectorFullWidthMargins);

        EditorGUILayout.Separator();
		EditorGUILayout.BeginHorizontal();
		m_bSvnUpdate = EditorGUILayout.Toggle("SvnUpdate", m_bSvnUpdate);
		EditorGUILayout.LabelField("是否需要 Svn Update 资源");
		EditorGUILayout.EndHorizontal();

		DrawBuildDebug();

		//m_bShowReleaseBuild = EditorGUILayout.Foldout(m_bShowReleaseBuild, "2.Build");
		//if (m_bShowReleaseBuild)
			DrawBuildRelease();

        m_bShowBuildPatcher = EditorGUILayout.Foldout(m_bShowBuildPatcher, "3.Patcher");
        if (m_bShowBuildPatcher)
            DrawBuildPatcher();

        EditorGUILayout.Separator();
    }

	private void DrawBuildDebug()
	{
		EditorGUILayout.Separator();
		EditorGUILayout.LabelField("1.平时出版本用： （注：适合没有C++,CSharp变动时）");
		// 小版本，只是拷贝Lua脚本和策划配置文件
		if (GUILayout.Button("LittlePublic（小版本）"))
		{
            Debug.Log("LittlePublic - " + System.DateTime.Now);
            EditorGUIUtility.editingTextField = false;
			AppPackage.LittlePublic(m_bSvnUpdate);
		}
	}

	private void DrawBuildRelease()
	{
		EditorGUILayout.Separator ();
		EditorGUILayout.LabelField ("2.Build:", EditorStyles.boldLabel);
		m_sNewVersion = EditorGUILayout.TextField ("Version ( " + m_sOldVersion + " )", m_sNewVersion);   //输入新版本号

        // 是否需要打包Lua和Csv资源
        EditorGUILayout.BeginHorizontal ();
		m_bBundleLua = EditorGUILayout.Toggle ("BundleLua", m_bBundleLua);
		EditorGUILayout.LabelField ("是否需要打包Lua资源");
		EditorGUILayout.EndHorizontal ();

		// 是否需要打包data资源
		EditorGUILayout.BeginHorizontal ();
		m_bBundleData = EditorGUILayout.Toggle ("BundleData", m_bBundleData);
		EditorGUILayout.LabelField ("是否需要打包Data资源");
		EditorGUILayout.EndHorizontal ();

		// 是否需要打包Map资源
		EditorGUILayout.BeginHorizontal ();
		m_bBundleMap = EditorGUILayout.Toggle ("BundleMap", m_bBundleMap);
		EditorGUILayout.LabelField ("是否需要打包Map资源");
		EditorGUILayout.EndHorizontal ();

		// 是否需要拷贝Data资源到Resource目录
		EditorGUILayout.BeginHorizontal ();
		m_bCopyData = EditorGUILayout.Toggle ("CopyAB", m_bCopyData);
		EditorGUILayout.LabelField ("是否需要拷贝DataAB资源到Resource目录");
		EditorGUILayout.EndHorizontal ();

		// 是否需要BuildApp
		EditorGUILayout.BeginHorizontal ();
		m_bBuildApp = EditorGUILayout.Toggle ("BuildApp", m_bBuildApp);
		EditorGUILayout.LabelField ("是否需要BuildApp");
		EditorGUILayout.EndHorizontal ();

		// 是否为 Release 版本
		if (m_bBuildApp)
		{
			EditorGUILayout.BeginHorizontal ();
			m_bBuildRelease = EditorGUILayout.Toggle ("BuildRelease", m_bBuildRelease);
			EditorGUILayout.LabelField ("是否需要Release版本");
			EditorGUILayout.EndHorizontal ();
		}

		// 是否需要 安卓工程
		if (AppPackage.ms_target == BuildTarget.Android || AppPackage.ms_target == BuildTarget.iOS)
		{
			EditorGUILayout.BeginHorizontal ();
			m_bBuildProject = EditorGUILayout.Toggle ("BuildAndroidPrj", m_bBuildProject);
			EditorGUILayout.LabelField ("是否需要 安卓工程");
			EditorGUILayout.EndHorizontal ();
		}

        // 打包
        if (GUILayout.Button("Build"))
        {
            Build();
        }
    }

    private void DrawBuildPatcher()
    {
        /*
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("4.Patcher:", EditorStyles.boldLabel);
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
            EditorGUIUtility.editingTextField = false;
			m_sNewABDir = EditorUtility.OpenFolderPanel("新资源包目录选择", GameConfigMgr.ms_sPatcherDir, "");
            m_sNewWcpPath = GenWcpPath();
        }
        EditorGUILayout.EndHorizontal();
        // 制作补丁包
        EditorGUILayout.BeginHorizontal();
        m_sNewWcpPath = EditorGUILayout.TextField(m_sNewWcpPath);
        if (GUILayout.Button("生成补丁包"))
        {
            EditorGUIUtility.editingTextField = false;
            AppPackage.GenPatcher(m_sOldABDir, m_sNewABDir, m_sNewWcpPath);
        }
        EditorGUILayout.EndHorizontal();
        //*/
    }

    /*
    // 补丁包路径
    private string GenWcpPath()
    {
        string platform = AppPackage.ms_target.ToString();
        string dir = "";
        string oldName = "";
        string newName = "";
        if (!string.IsNullOrEmpty(m_sOldABDir))
        {
            System.IO.DirectoryInfo oDir = new System.IO.DirectoryInfo(m_sOldABDir);
            if(!oDir.Name.StartsWith(platform))
            {
                Debug.LogError("platform: " + platform);
                return "";
            }
            oldName = oDir.Name.Substring(platform.Length + 1);
            dir = oDir.Parent.FullName.Replace("\\", "/") + "/";
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
            dir = oDir.Parent.FullName.Replace("\\", "/") + "/";
        }

		return dir + platform + "_" + oldName + "-" + newName + GameConfigMgr.ms_sWcpType;
    }
    //*/

    private void Build()
    {
        System.DateTime lastTime = System.DateTime.Now;
        Debug.Log("Build - " + lastTime);
        bool bOK = false;
        BuildOptions buildOptions = BuildOptions.None;
		//BuildOptions buildOptions = BuildOptions.UncompressedAssetBundle;
		if (m_bBuildRelease == false)
			buildOptions = BuildOptions.Development | BuildOptions.AllowDebugging;//| BuildOptions.ConnectWithProfiler;     // debug
		if(m_bBuildProject && AppPackage.ms_target == BuildTarget.Android)
            buildOptions = buildOptions | BuildOptions.AcceptExternalModificationsToPlayer;
        else if (m_bBuildProject && AppPackage.ms_target == BuildTarget.iOS)
            buildOptions = buildOptions | BuildOptions.AcceptExternalModificationsToPlayer | BuildOptions.Il2CPP;

		bOK = AppPackage.Build(buildOptions, m_bSvnUpdate, m_sNewVersion, m_bBundleLua, m_bBundleData, m_bBundleMap, m_bCopyData, m_bBuildApp);

        System.TimeSpan costTime = System.DateTime.Now - lastTime;
        Debug.Log("-->> Build Cost: " + (costTime.Hours * 3600 + costTime.Minutes * 60 + costTime.Seconds).ToString() + "s");

        if (bOK)
        {
            if (m_bBundleData && m_bBundleMap)
                AppPackage.LittlePublic(false, "Build");
            else
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Build", " Success!! ", "close");
            }
        }
        m_sOldVersion = AppPackage.ms_sNewVersion;
    }
}