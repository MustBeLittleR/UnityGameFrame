using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using WCG;
using LitJson;

// 打包ab不区分大小写，还是Win10不区分大小写！！！？？？？
public static class AppPackage
{
    // Assets 路径
	static string ms_sAssetPath = GameConfigMgr.ms_sAssetsPath + GameConfigMgr.ms_sDataFolderName + "/";

    // 真实的本地路径
	static string ms_sDataDir = GameConfigMgr.ms_sDataDir + GameConfigMgr.ms_sDataFolderName + "/";
	static string ms_sMapDir = ms_sDataDir + GameConfigMgr.ms_sMapFolderName + "/";
	static string ms_sMediaDir = ms_sDataDir + GameConfigMgr.ms_sMediaFolderName + "/";
	static string ms_sUIDir = ms_sDataDir + GameConfigMgr.ms_sUIFolderName + "/";
	static string ms_sObjDir = ms_sDataDir + GameConfigMgr.ms_sObjFolderName + "/";
	static string ms_sEffectDir = ms_sDataDir + GameConfigMgr.ms_sEffectFolderName + "/";
	static string ms_sCharDir = ms_sDataDir + GameConfigMgr.ms_sCharFolderName + "/";
    
	// Resources 目录的相关路径
	static string ms_sResourcesDir = GameConfigMgr.ms_sDataDir + GameConfigMgr.ms_sResrcFolderName + "/" + GameConfigMgr.ms_sABFolderName + "/";
	static string ms_sABType = GameConfigMgr.ms_sABType;
	static string ms_sDataOutDir = GameConfigMgr.ms_sPatcherDir + GameConfigMgr.ms_sABFolderName + "/";

	//小版本Copy参数
	static string[] ms_ArrLittleFiles = { "Designer/", "Engine/", "Etc/", "Game/", "Script/", "Tools/" };
	static string[][] ms_ArrLittleCheck = { new string[] { "*.lua" }, new string[] { "*.lua" } , new string[] { "*" } , new string[] { "*.sql" } , new string[] { "*.lua" } , new string[] { "*.lua" } };
	static string[] ms_ArrRemoveFiles = { "Engine/Bin", "Engine/SDK" };

	//LuaJitTool
	static string ms_sLuaJitToolPath = Application.dataPath + "/../../Tools/LuaJIT/luajit.exe";
    static string ms_sLuaScriptDir = GameConfigMgr.ms_sLuaScriptDir;
    static string ms_sDesignerDir = GameConfigMgr.ms_sDesignerDir;
    static string ms_sLuaScriptName = GameConfigMgr.ms_sLuaScriptFolderName;
    static string ms_sDesignerName = GameConfigMgr.ms_sDesignerFolderName;
	static string[] ms_ArrGacLuaCheck = { "/Gac/", "/GacGasCommon/", "/Common/" };

	static string ms_sManifestExt = ".manifest";
	static string ms_sMetaExt = ".meta";

    // 版本号
    public static string ms_sNewVersion = "0.0.0.1000";

    // 平台
    public static BuildTarget ms_target
    {
        get
        {
#if UNITY_ANDROID           // Android
            return BuildTarget.Android;
#elif UNITY_IPHONE          // iOS
            return BuildTarget.iOS;
#elif UNITY_STANDALONE_WIN  // windows平台
            return BuildTarget.StandaloneWindows;
#else
            return BuildTarget.StandaloneWindows;
#endif
        }
    }
    
    // 从一堆目录中获取符合模式的文件
    public static string[] GetFiles(string[] dirs, string[] patterns, bool bAsset = true)
    {
        string sAssetPath = ms_sAssetPath;
        string sDataPath = ms_sDataDir;
        int n = sDataPath.Length;
        List<string> lstFiles = new List<string>();
        for(int i = 0; i < dirs.Length; i++)
        {
            if (!Directory.Exists(dirs[i]))
                continue;
            for(int j = 0; j < patterns.Length; j++)
            {
                string[] files = Directory.GetFiles(dirs[i], patterns[j], SearchOption.AllDirectories);
                for(int k = 0; k < files.Length; k++)
                {
                    if(bAsset)
                        lstFiles.Add(sAssetPath + files[k].Replace("\\", "/").Substring(n));
                    else
                        lstFiles.Add(files[k].Replace("\\", "/"));
                }
            }
        }
        return lstFiles.ToArray();
    }
    // 从一堆字符串中获取符合模式的字符串
    public static string[] GetMatch(string[] files, string[] patterns)
    {
        List<string> lstFiles = new List<string>();
        for(int i = 0; i < files.Length; i++)
        {
            bool bOK = false;
            for(int j = 0; j < patterns.Length; j++)
            {
                // 不区分大小写
                if (Regex.IsMatch(files[i], patterns[j], RegexOptions.IgnoreCase))
                    bOK = true;
            }
            if(bOK == true)
            {
                lstFiles.Add(files[i]);
            }
        }
        //
        return lstFiles.ToArray();
    }
    // 拷贝文件
    public static bool CopyFiles(string srcDir, string dstDir, string[] patterns = null, bool bDelOldDstDir = true, bool bDisplayProgressBar = false)
    {
        if (string.IsNullOrEmpty(srcDir) || string.IsNullOrEmpty(dstDir) || !Directory.Exists(srcDir))
        {
            Debug.LogError("Not Exist Directory: " + srcDir);
            return false;
        }
        if (patterns == null)
            patterns = new string[] { "*.*" };
        if (bDelOldDstDir && Directory.Exists(dstDir))
            Directory.Delete(dstDir, true);
        int n = srcDir.Length;
        string[] files = GetFiles(new string[] { srcDir }, patterns, false);
		int nCount = files.Length;
		for (int i = 0; i < files.Length; i++)
        {
			if (bDisplayProgressBar)
			{
				float fProgress = i * 1.0f / nCount;
				EditorUtility.DisplayProgressBar("CopyFiles: " + srcDir, Math.Round(fProgress,3).ToString()+":" +files[i].Replace(srcDir, ""), fProgress);
			}
			string dstFile = dstDir + files[i].Replace("\\", "/").Substring(n);
            string sDir = dstFile.Substring(0, dstFile.LastIndexOf('/'));
            if (!Directory.Exists(sDir))
                Directory.CreateDirectory(sDir);
            File.Copy(files[i], dstFile, true);
        }

        return true;
    }

    // AssetBundle
    // 根据文件数组打包成一个资源包。（file： "Assets/Data/....../XXX.XXX"）。场景文件（".unity"）不能和其他资源一起打包!!!!
    public static void BundleFiles(string outDir, string[] files, BuildAssetBundleOptions abOptions, bool bDelOldDir = false, bool bOne = false, string abVariant = null)
    {
        if (bDelOldDir && Directory.Exists(outDir))
            Directory.Delete(outDir, true);
        if(!Directory.Exists(outDir))
            Directory.CreateDirectory(outDir);

        bool bScene = false;
        if (files.Length > 0 && files[0].EndsWith(".unity"))
            bScene = true;

        List<AssetBundleBuild> lstABB = new List<AssetBundleBuild>();
        if (bOne)
        {
            DirectoryInfo dir = new DirectoryInfo(outDir);
            AssetBundleBuild abb = new AssetBundleBuild();
            abb.assetBundleVariant = abVariant; // abVariant = "1024X768";
            abb.assetNames = files;
            abb.assetBundleName = dir.Name + ms_sABType;
            lstABB.Add(abb);
        }
        else
        {
            int n = ms_sAssetPath.Length;
            for (int i = 0; i < files.Length; i++)
            {
                AssetBundleBuild abb = new AssetBundleBuild();
                abb.assetBundleVariant = abVariant; // abVariant = "1024X768";
                abb.assetNames = new string[] { files[i] };
                abb.assetBundleName = files[i].Replace("\\", "/").Substring(n) + ms_sABType;
                lstABB.Add(abb);
            }
        }

        AssetDatabase.Refresh();
        if (bScene == false)
        {
            if(lstABB.Count > 0)
            {
                AssetBundleManifest abm = BuildPipeline.BuildAssetBundles(outDir, lstABB.ToArray(), abOptions, ms_target);
                if (abm == null)
                {
                    Debug.LogError("BundFiles Failed! -> " + outDir);
                }
            }
        }
        else
        {
            for(int i = 0; i < lstABB.Count; i++)
            {
                AssetBundleBuild abb = lstABB[i];
                string[] levels = abb.assetNames;
                string outPath = outDir + abb.assetBundleName;
                string sDir = outPath.Substring(0, outPath.LastIndexOf("/"));
                if (!Directory.Exists(sDir))
                    Directory.CreateDirectory(sDir);
                BuildPlayerOptions bpo = new BuildPlayerOptions();
                bpo.scenes = levels;
                bpo.locationPathName = outPath;
                bpo.target = ms_target;
                bpo.options = BuildOptions.BuildAdditionalStreamedScenes;
                string s = BuildPipeline.BuildPlayer(bpo);
                if(!string.IsNullOrEmpty(s))
                    Debug.LogError(s);
            }
        }
        AssetDatabase.Refresh();
    }

    [MenuItem("WCTool/GamePackage/BundleLuaCsv", false, 33)]
	public static bool BundleLuaCsv()
    {
        System.DateTime lastTime = System.DateTime.Now;
        string sOutDir = AppPackage.AddStamp(ms_sDataOutDir);
        string sDataLuaScriptPath = ms_sDataDir + ms_sLuaScriptName;
        string sDataDesignerPath = ms_sDataDir + ms_sDesignerName;
        bool bSuccess = true;

        bSuccess &= AppPackage.LuaToByteCode(ms_sLuaScriptDir, sDataLuaScriptPath, ms_sLuaScriptName, ms_ArrGacLuaCheck);
        bSuccess &= AppPackage.LuaToByteCode(ms_sDesignerDir, sDataDesignerPath, ms_sDesignerName, new string[] { "/Config/" });

        AssetDatabase.Refresh();
        if (!bSuccess) return false;

        BuildAssetBundleOptions theAssetOptions = BuildAssetBundleOptions.ForceRebuildAssetBundle;
        //theAssetOptions = theAssetOptions | BuildAssetBundleOptions.CompleteAssets;
        //theAssetOptions = theAssetOptions | BuildAssetBundleOptions.CollectDependencies;
        theAssetOptions = theAssetOptions | BuildAssetBundleOptions.UncompressedAssetBundle;
        theAssetOptions = theAssetOptions | BuildAssetBundleOptions.DeterministicAssetBundle;

        // Lua Handle
		AppPackage.DeleteABManifest(sOutDir + ms_sLuaScriptName.ToLower(), sOutDir + GameConfigMgr.ms_sLuaMFileName);
        string[] luaFilesList = AppPackage.GetFiles(new string[] { sDataLuaScriptPath }, new string[] { "*.bytes" }, true);
		AppPackage.BundleFiles(sOutDir, luaFilesList, theAssetOptions, false);
		bSuccess &= AppPackage.BackUpABManifest(sOutDir, sOutDir + GameConfigMgr.ms_sLuaMFileName + GameConfigMgr.ms_sABType);
		if (!bSuccess) return false;

		// Csv Handle
		AppPackage.DeleteABManifest(sOutDir + ms_sDesignerName.ToLower(), sOutDir + GameConfigMgr.ms_sCsvMFileName);
        string[] csvFilesList = AppPackage.GetFiles(new string[] { sDataDesignerPath }, new string[] { "*.bytes" }, true);
        AppPackage.BundleFiles(sOutDir, csvFilesList, theAssetOptions, false);
		bSuccess &= AppPackage.BackUpABManifest(sOutDir, sOutDir + GameConfigMgr.ms_sCsvMFileName + GameConfigMgr.ms_sABType);
		if (!bSuccess) return false;

		//Clean LuaCsv Temp Files
		if (Directory.Exists(sDataLuaScriptPath))
        {
            Directory.Delete(sDataLuaScriptPath, true);
			File.Delete(sDataLuaScriptPath + ms_sMetaExt);
        }
        if (Directory.Exists(sDataDesignerPath))
        {
            Directory.Delete(sDataDesignerPath, true);
			File.Delete(sDataDesignerPath + ms_sMetaExt);
        }
        AssetDatabase.Refresh();

        System.TimeSpan costTime = System.DateTime.Now - lastTime;
        Debug.Log("-->> BundleLuaCsv Cost: " + (costTime.Hours * 3600 + costTime.Minutes * 60 + costTime.Seconds).ToString() + "s");
        return true;
    }

    [MenuItem("WCTool/GamePackage/BundleData", false, 32)]
    public static bool BundleData()
    {
		bool bSuccess = true;
		System.DateTime lastTime = System.DateTime.Now;
        string sOutDir = AppPackage.AddStamp(ms_sDataOutDir);

        BuildAssetBundleOptions theAssetOptions = BuildAssetBundleOptions.ForceRebuildAssetBundle;
        //theAssetOptions = theAssetOptions | BuildAssetBundleOptions.CompleteAssets;
        //theAssetOptions = theAssetOptions | BuildAssetBundleOptions.CollectDependencies;
        theAssetOptions = theAssetOptions | BuildAssetBundleOptions.UncompressedAssetBundle;
        theAssetOptions = theAssetOptions | BuildAssetBundleOptions.DeterministicAssetBundle;

        List<string> lstFiles = new List<string>();
        string[] mediaFiles = AppPackage.GetFiles(new string[] { ms_sMediaDir }, new string[] { "*.ogg", "*.mp3" }, true);
        lstFiles.AddRange(mediaFiles);

        string[] objFiles = AppPackage.GetFiles(new string[] { ms_sObjDir }, new string[] { "*.prefab" }, true);
        lstFiles.AddRange(objFiles);

        string[] effectFiles = AppPackage.GetFiles(new string[] { ms_sEffectDir }, new string[] { "*.prefab" }, true);
        lstFiles.AddRange(effectFiles);

        string[] charFiles = AppPackage.GetFiles(new string[] { ms_sCharDir }, new string[] { "*.prefab" }, true);
        lstFiles.AddRange(charFiles);

        string[] uiFiles = AppPackage.GetFiles(new string[] { ms_sUIDir }, new string[] { "*.prefab", "*.png", "*.jpg", "*.ttf" }, true);
        lstFiles.AddRange(uiFiles);

		/*string[] cfgFiles = GetFiles(new string[] { ms_sConfigDir }, new string[] { "*.prefab", "*.json", "*.bytes" }, true);
        lstFiles.AddRange(cfgFiles);*/

		string[] mapFiles = AppPackage.GetFiles(new string[] { ms_sMapDir }, new string[] { "*.prefab" }, true);
		lstFiles.AddRange(mapFiles);

        AppPackage.DeleteABManifest(sOutDir + ms_sMediaDir.ToLower());
        AppPackage.DeleteABManifest(sOutDir + ms_sObjDir.ToLower());
        AppPackage.DeleteABManifest(sOutDir + ms_sEffectDir.ToLower());
        AppPackage.DeleteABManifest(sOutDir + ms_sCharDir.ToLower());
        //AppPackage.DeleteABManifest(sOutDir + ms_sConfigDir.ToLower());
        AppPackage.DeleteABManifest(sOutDir + ms_sMapDir.ToLower());

        AppPackage.DeleteABManifest(null, sOutDir + GameConfigMgr.ms_sABMFileName);
        string[] depFiles = AssetDatabase.GetDependencies(lstFiles.ToArray());
        string[] files = AppPackage.GetMatch(depFiles, new string[] { "^.+\\.ogg$|^.+\\.mp3$|^.+\\.prefab$|^.+\\.png$|^.+\\.jpg$|^.+\\.ttf$|^.+\\.json$|^.+\\.bytes$|^.+\\.renderTexture$" });
		AppPackage.BundleFiles(sOutDir, files, theAssetOptions, false);
		bSuccess &= AppPackage.BackUpABManifest(sOutDir, sOutDir + GameConfigMgr.ms_sABMFileName + GameConfigMgr.ms_sABType);
		if (!bSuccess) return false;

		System.TimeSpan costTime = System.DateTime.Now - lastTime;
        Debug.Log("-->> BundleData Cost: " + (costTime.Hours * 3600 + costTime.Minutes * 60 + costTime.Seconds).ToString() + "s");
        return true;
    }

    [MenuItem("WCTool/GamePackage/BundleMap", false, 31)]
    public static bool BundleMap()
    {
        System.DateTime lastTime = System.DateTime.Now;
        string sOutDir = AppPackage.AddStamp(ms_sDataOutDir);

        string[] files = AppPackage.GetFiles(new string[] { ms_sMapDir }, new string[] { "*.unity" }, true);
		AppPackage.BundleFiles(sOutDir, files, BuildAssetBundleOptions.DeterministicAssetBundle, false);

        /*// 生成MD5
        Dictionary<string, string> dicMD5  = new Dictionary<string, string>();
        Dictionary<string, int> dicSize = new Dictionary<string, int>();
		string[] fls = GetFiles(new string[] { sOutDir }, new string[] { "*" + GameConfigMgr.ms_sABType }, false);
        System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        for (int i = 0; i < fls.Length; i++)
        {
            byte[] bt = File.ReadAllBytes(fls[i]);
            int nLen = bt.Length;
            string sMD5 = System.BitConverter.ToString(md5.ComputeHash(bt));
			string path = fls[i].Substring(sOutDir.Length, fls[i].Length - sOutDir.Length - GameConfigMgr.ms_sABType.Length);
            dicMD5.Add(path, sMD5);
            dicSize.Add(path, nLen);
			File.WriteAllBytes(fls[i] + GameConfigMgr.ms_sMD5Type, System.Text.Encoding.UTF8.GetBytes(sMD5));
        }
        md5.Clear();*/

        System.TimeSpan costTime = System.DateTime.Now - lastTime;
        Debug.Log("-->> BundleMap Cost: " + (costTime.Hours * 3600 + costTime.Minutes * 60 + costTime.Seconds).ToString() + "s");
        return true;
    }

    static bool LuaToByteCode(string sSrcFileDir, string sDstFileDir, string sGroupName, string[] includePath)
    {
        bool bJITExists = File.Exists(ms_sLuaJitToolPath);
        if (!bJITExists || !Directory.Exists(sSrcFileDir))
        {
            string sJitErr = "no File:" + ms_sLuaJitToolPath;
            string sLuaErr = "no Path:" + sSrcFileDir;
            Debug.LogError(bJITExists ? sLuaErr : sJitErr);
            return false;
        }

        //Debug.Log(sGroupName + ": Lua To CodeByte Start!");
        bool bSuccess = true;
        System.DateTime now = System.DateTime.Now;

        if (Directory.Exists(sDstFileDir))
            Directory.Delete(sDstFileDir, true);
        Directory.CreateDirectory(sDstFileDir);

        /*string sLuaCsvTmp = Application.dataPath + "/../../LuaCsvBytes/" + sGroupName + "/";
        //Debug.Log("LuaCsvTmp:" + sLuaCsvTmp);

        if (Directory.Exists(sLuaCsvTmp))
            Directory.Delete(sLuaCsvTmp, true);
        Directory.CreateDirectory(sLuaCsvTmp);*/

        string[] sDirs = { sSrcFileDir };
        string[] sPatterns = { "*.lua", "*.csv" };
        string[] sFiles = AppPackage.GetFiles(sDirs, sPatterns, false);
        int nCount = sFiles.Length;

		for (int i = 0; i < nCount; i++)
        {
			if (!IsInlcudePath(sFiles[i], includePath))
				continue;

			// 客户端要支持IOS的arm64，所以直接使用源码
			string sFilePath = sFiles[i].Replace("\\", "/");
			string sFileName = sFilePath.Substring(sSrcFileDir.Length).Replace('/', '.') + ".bytes";
            string sDstFile = sDstFileDir + "/" + sFileName;
            File.Copy(sFilePath, sDstFile, true);

            
            /*// 服务端使用luajit，lua文件编译成字节码，csv文件直接拷贝
            string sLSFile = sLuaCsvTmp + sFilePath.Substring(sSrcFileDir.Length);
            string sBCDir = sLSFile.Substring(0, sLSFile.LastIndexOf('/'));
            if (!Directory.Exists(sBCDir))
                Directory.CreateDirectory(sBCDir);

            if (sFilePath.EndsWith(".lua"))
            {
                // 调用luajit.exe进程打包Lua文件。1、检查lua代码的语法；2、正式服务端不能使用源码
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.FileName = ms_sLuaJitToolPath;
                p.StartInfo.Arguments = " -bg " + sFilePath + " " + sLSFile;//LuaJIT的命令 //p.StartInfo.Arguments = " -o " + sDstFile + " " + sFilePath; //LuaC的命令
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.CreateNoWindow = true;

                // p.Exited += (object sender, System.EventArgs e) => { p.Close(); };
                p.Start();
                string sErrInfo = p.StandardError.ReadToEnd();
                // string sOutInfo = p.StandardOutput.ReadToEnd();
                // p.StandardInput.WriteLine("exit");
                p.WaitForExit();
                p.Close();

                if (sErrInfo != null && sErrInfo != "")
                {
                    bSuccess = false;
                    Debug.LogError(sErrInfo);
                }
            }
            else
            {
                File.Copy(sFilePath, sLSFile, true);
            }*/

            float fProgress = i * 1.0f / nCount;
            EditorUtility.DisplayProgressBar("LuaCsv CodeByte", sFilePath + ": " + fProgress.ToString(), fProgress);
            // Debug.Log(sFilePath + " => " + sDstFile);
        }

        EditorUtility.ClearProgressBar();
        System.GC.Collect();

        System.TimeSpan disTime = System.DateTime.Now - now;
        Debug.Log(sSrcFileDir + ": " + nCount.ToString() + " Files To ByteCode! Cost " + (disTime.Hours * 3600 + disTime.Minutes * 60 + disTime.Seconds).ToString());

        return bSuccess;
    }

	static bool IsInlcudePath(string sFilePath, string[] include)
	{
		bool bIsInclude = true;
		int includeLen = include.Length;
		if (includeLen > 0)
		{
			bIsInclude = false;
			for (int j = 0; j < includeLen && !bIsInclude; j++)
				bIsInclude = sFilePath.Contains(include[j]);
		}
		return bIsInclude;
	}

	static void DeleteABManifest(string sOldABGroupDir=null, string sOldABMFName=null)
	{
		if (sOldABGroupDir != null && Directory.Exists(sOldABGroupDir))
			Directory.Delete(sOldABGroupDir, true);
		if (sOldABMFName != null && File.Exists(sOldABMFName))
			File.Delete(sOldABMFName);
		if (sOldABMFName != null && File.Exists(sOldABMFName + ms_sManifestExt))
			File.Delete(sOldABMFName + ms_sManifestExt);
	}

   	static bool BackUpABManifest(string sOutDir, string sBackUpDir)
    {
        DirectoryInfo csvOutDir = new DirectoryInfo(sOutDir);
        string sBytesDir = sOutDir + csvOutDir.Name;
        try
        {
            if (File.Exists(sBytesDir))
            {
                if (File.Exists(sBackUpDir))
                    File.Delete(sBackUpDir);

                //File.Copy(sBytesDir, sBackUpDir, true);
                File.Move(sBytesDir, sBackUpDir);

				string sFMDir = sBytesDir + ms_sManifestExt;
				string sBackUpMFDir = sBackUpDir + ms_sManifestExt;
                if (File.Exists(sBackUpMFDir))
                    File.Delete(sBackUpMFDir);

                //File.Copy(sFMDir, sBackUpMFDir, true);
                File.Move(sFMDir, sBackUpMFDir);
                return true;
            }
            else
            {
                Debug.LogError("BackUpABManifest File Not Exists: " + sBytesDir);
                return false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("BackUpABManifest Failed: " + e.Message + "\n" + sBytesDir + "\n" + sBackUpDir);
            return false;
        }
    }

    public static bool CheckVersion(string version)
    {
        if (string.IsNullOrEmpty(version))
            return false;
        if (Regex.IsMatch(version, "^[0-9]\\.[0-9]\\.[0-9]\\.[1-9][0-9]{3,7}$"))
            return true;
        return false;
    }

    public static bool GenVersion(string version)
    {
        if (!AppPackage.CheckVersion(version))
            return false;
        string sOutDir = AppPackage.AddStamp(ms_sDataOutDir);
		string path = sOutDir + GameConfigMgr.ms_sVersionFileName + GameConfigMgr.ms_sABType;
        string dir = path.Substring(0, path.LastIndexOf('/'));
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        File.WriteAllBytes(path, Encoding.UTF8.GetBytes(version));
        return true;
    }

    // Add  BuildTarget_Info Flag
    public static string AddStamp(string dir)
    {
        string version = ms_sNewVersion;
        if (string.IsNullOrEmpty(dir))
            return dir;
        dir = dir.Replace("\\", "/");
        if (!dir.EndsWith("/"))
            dir += "/";
        string newDir = dir + ms_target.ToString();
        if (string.IsNullOrEmpty(version))
            newDir += "/";
        else
            newDir += "_" + version + "/";
        return newDir;
    }

    // 最终生成App。string[] scenes = { "Assets/Launcher.unity", "Assets/Empty.unity" };
    public static bool BuildAPP(string sOutDir, BuildOptions buildOptions = BuildOptions.None, string[] scenes = null, bool bDelOldOutDir = false)
    {
        System.DateTime lastTime = System.DateTime.Now;
        string sOutPath = sOutDir + ms_target.ToString() + "/";
        if (bDelOldOutDir && Directory.Exists(sOutPath))
            Directory.Delete(sOutPath, true);
        Directory.CreateDirectory(sOutPath);

        if (ms_target == BuildTarget.StandaloneWindows)
        {
            sOutPath = sOutPath + "client.exe";
        }
        else if (ms_target == BuildTarget.Android)
        {
            sOutPath = sOutPath + "client.apk";
        }
        else if (ms_target == BuildTarget.iOS)
        {
        }
        else
        {
            Debug.LogError("Not Support PlatForm!");
            return false;
        }
        // 版本号
        // PlayerSettings.bundleVersion = version;
        if (scenes == null)
            scenes = new string[] { "Assets/Launcher.unity", "Assets/Empty.unity" };

        BuildPlayerOptions bpo = new BuildPlayerOptions();
        bpo.scenes = scenes;
        bpo.locationPathName = sOutPath;
        bpo.target = ms_target;
        bpo.options = buildOptions;
        string str = BuildPipeline.BuildPlayer(bpo);
        System.GC.Collect();
        AssetDatabase.Refresh();

        System.TimeSpan costTime = System.DateTime.Now - lastTime;
        Debug.Log("-->> BuildAPP Cost: " + (costTime.Hours * 3600 + costTime.Minutes * 60 + costTime.Seconds).ToString() + "s");

        if (!string.IsNullOrEmpty(str))
        {
            Debug.LogError("Build Failed! :" + str);
            return false;
        }
        else
        {
            Debug.Log("Build Finish: " + sOutPath);
            return true;
        }
    }

    // buildOptions = BuildOptions.Development | BuildOptions.ConnectWithProfiler | BuildOptions.AllowDebugging;     // debug
    // buildOptions = BuildOptions.None;   // release
	public static bool Build(BuildOptions buildOptions, bool bSvnUpdate, string newVersion, bool bBundleLuaCsv, bool bBundleData, bool bBundleMap, bool bCopyData, bool bBuildApp)
    {
        if (!AppPackage.CheckVersion(newVersion))
        {
            Debug.LogError("Invalid Version!!!!");
			return false;
        }

        ms_sNewVersion = newVersion;
        bool IsWinBuild = ms_target == BuildTarget.StandaloneWindows;
        string sDataOutDir = AppPackage.AddStamp(ms_sDataOutDir);
        if (!IsWinBuild && Directory.Exists(sDataOutDir))
        {
            Debug.LogError("Exists Directory: " + sDataOutDir);
            return false;
        }

        if (bSvnUpdate && !AppPackage.SvnUpdate())
        {
			Debug.LogError("SvnUpdate Failed !!!!");
			return false;
        }

        if (bBundleLuaCsv && !AppPackage.BundleLuaCsv())
            return false;

        if (bBundleData && !AppPackage.BundleData())
            return false;

        if (bBundleMap && !AppPackage.BundleMap())
            return false;

        if (!AppPackage.GenVersion(newVersion))
            return false;

        //真机随包资源处理
        if (bCopyData && !IsWinBuild)
        {
            if (!AppPackage.CopyFiles(sDataOutDir, ms_sResourcesDir, new string[] { "*" + ms_sABType, /*"*.manifest"*/ }, true))
                return false;
        }

        AssetDatabase.Refresh();
        if (bBuildApp && !AppPackage.BuildAPP(GameConfigMgr.ms_sPublicDir, buildOptions, null, bCopyData))
            return false;

        //PC资源处理
        if (bCopyData && IsWinBuild)
        {
            string sWinData = GameConfigMgr.ms_sPublicDir + ms_target.ToString() + "/" + GameConfigMgr.ms_sABFolderName + "/";
            if (!AppPackage.CopyFiles(sDataOutDir, sWinData, new string[] { "*" + ms_sABType, /*"*.manifest"*/ }, true, true))
                return false;
        }

        return true;
    }

	// only copy lua && csv
	public static void LittlePublic(bool bSvnUpdate, string sFinishMsg = null)
	{
		bool bOK = false;
		try
		{
			if (bSvnUpdate && !AppPackage.SvnUpdate(true))
			{
				Debug.LogError("SvnUpdate Failed !!!!");
				return;
			}

			string sSourcePath = GameConfigMgr.ms_sLuaScriptDir + "../../";
			string sPublicPath = GameConfigMgr.ms_sPublicDir + "Trunk/";
			string[] fileArr = ms_ArrLittleFiles;
			int nCount = fileArr.Length;
			for (int i = 0; i < nCount; i++)
				AppPackage.CopyFiles(sSourcePath + fileArr[i], sPublicPath + fileArr[i], ms_ArrLittleCheck[i], false, true);

			for (int i = 0; i < ms_ArrRemoveFiles.Length; i++)
			{
				string sDelPath = sPublicPath + ms_ArrRemoveFiles[i];
				if (Directory.Exists(sDelPath))
					Directory.Delete(sDelPath, true);
			}
			bOK = true;
		}
		catch (System.Exception ex)
		{
			bOK = false;
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
		EditorUtility.ClearProgressBar();
		EditorUtility.DisplayDialog(string.IsNullOrEmpty(sFinishMsg) ? "LittlePublic": sFinishMsg, bOK ? " Success!! ":" Failed!! ", "close");
	}

    public static bool SvnUpdate(bool bDeleteOldAB = false)
	{
		// 删除老的打包遗留物
		/*if (bDeleteOldAB && Directory.Exists(ms_sResrcDataDir))
			Directory.Delete(ms_sResrcDataDir, true);*/
		
		/*// 调用python脚本进行svn更新
		string sPath = Application.dataPath + "/../../UpdateSVN.py";
		System.Diagnostics.Process p = new System.Diagnostics.Process();
		p.StartInfo.FileName = sPath;
		p.Start();
		p.WaitForExit();
		p.Close();
		Debug.Log("python: " + sPath + " called!");
		//刷新Unity3D视图里面的资源
		AssetDatabase.Refresh();*/
		return true;
	}

    /* ============================================= 补丁包相关 ============================================= */
    /*public static bool CopyFiles(Dictionary<string, string> files, string dstDir, bool bDelOldDstDir = true)
    {
        if (files == null || string.IsNullOrEmpty(dstDir))
            return false;
        if (bDelOldDstDir && Directory.Exists(dstDir))
            Directory.Delete(dstDir, true);
        foreach (KeyValuePair<string, string> kv in files)
        {
            if(!string.IsNullOrEmpty(kv.Key) && !string.IsNullOrEmpty(kv.Value))
            {
                string dstFile = (dstDir + kv.Key).Replace("\\", "/");
                string sDir = dstFile.Substring(0, dstFile.LastIndexOf('/'));
                if (!Directory.Exists(sDir))
                    Directory.CreateDirectory(sDir);
                File.Copy(kv.Value, dstFile, true);
            }
        }
        return true;
    }
    // 生成补丁包
    public static void GenPatcher(string oldABPath, string newABPath, string wcpPath)
    {
        Debug.Log("GenPatcher: " + wcpPath + "\n" + oldABPath + "\n" + newABPath);
        if(string.IsNullOrEmpty(oldABPath) || string.IsNullOrEmpty(newABPath) || string.IsNullOrEmpty(wcpPath))
        {
            Debug.LogError("GenPatcher Path Error!!!");
            return;
        }
        oldABPath = oldABPath.Replace("\\", "/");
        if (!oldABPath.EndsWith("/")) oldABPath += "/";
        newABPath = newABPath.Replace("\\", "/");
        if (!newABPath.EndsWith("/")) newABPath += "/";

        // 通过MD5来找出差异文件
		string[] newFiles = GetFiles(new string[] { newABPath }, new string[] { "*" + GameConfigMgr.ms_sABType }, false);
		string[] oldFiles = GetFiles(new string[] { oldABPath }, new string[] { "*" + GameConfigMgr.ms_sABType }, false);
        List<string> lstOld = new List<string>(oldFiles);
        Dictionary<string, string> dicDiff = new Dictionary<string, string>();
        System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        for (int i = 0; i < newFiles.Length; i++)
        {
            string fileNew = newFiles[i].Substring(newABPath.Length);
            int k = -1;
            bool same = false;
            for(int j = 0; j < lstOld.Count; j++)
            {
                string fileOld = lstOld[j].Substring(oldABPath.Length);
                if(fileOld.Equals(fileNew))
                {
                    k = j;
                    FileInfo fiNew = new FileInfo(newFiles[i]);
                    FileInfo fiOld = new FileInfo(lstOld[j]);
                    if (fiNew.Length == fiOld.Length)
                    {
                        string sMD5New = System.BitConverter.ToString(md5.ComputeHash(File.ReadAllBytes(newFiles[i])));
                        string sMD5Old = System.BitConverter.ToString(md5.ComputeHash(File.ReadAllBytes(lstOld[j])));
                        if (sMD5New.Equals(sMD5Old))
                            same = true;
                    }
                    break;
                }
            }
            if(k >= 0)
                lstOld.RemoveAt(k);
            if (same == false)
                dicDiff.Add(fileNew, newFiles[i]);
        }
        // 
        for (int i = 0; i < lstOld.Count; i++)
        {
            dicDiff.Add(lstOld[i].Substring(oldABPath.Length), "");
        }
        md5.Clear();
        // 打包差异文件
        Debug.Log(" =========== diff: " + dicDiff.Count);
        foreach (KeyValuePair<string, string> kv in dicDiff)
        {
            Debug.Log(kv.Key + " = " + kv.Value);
        }
    }*/
}
