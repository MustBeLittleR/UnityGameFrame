using UnityEngine;
using UnityEditor;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System;
using System.Collections.Generic;
using WCG;
using LitJson;

// 枚举Build时的一些选项
public enum GenOptions
{
    None = 0,
    bGenVersion = 1,
    bBundleLua = 2,
    bBundleUI = 4,
    bBundleMap = 8,
    bBundleChar = 16,
    bBundleObj = 32,
    bBundleEff = 64,
    bBundleMedia = 128,
    bPackage = 256,
    bBuildApp = 512,
    bCopyBundle = 1024,
    bCopyWinRsrc = 2048,
}

// 打包资源的工具类
public static class PackageScatteredApp
{
    static PackageScatteredApp()
    {
    }

    // 打包平台
    static BuildTarget target
    {
        get
        {
#if UNITY_ANDROID           // Android
            return BuildTarget.Android;
#elif UNITY_IPHONE          // iPhone
            return BuildTarget.iPhone;
#elif UNITY_STANDALONE_WIN  // windows平台
            return BuildTarget.StandaloneWindows;
#else
            return BuildTarget.WebPlayer;
#endif
        }
    }
    public static string ms_sWinPlatformName = "Windows";
    public static string ms_sAdrPlatformName = "Android";
    public static string ms_sIosPlatformName = "IOS";
    static string targetname
    {
        get
        {
#if UNITY_ANDROID           // Android
            return ms_sAdrPlatformName;
#elif UNITY_IPHONE          // iPhone
            return ms_sIosPlatformName;
#elif UNITY_STANDALONE_WIN  // windows平台
            return ms_sWinPlatformName;
#else
            return ErrorPlatform;
#endif
        }
    }
    public static BuildTarget ms_target = target;
    static string ms_sPlatform = targetname;
    
    // 版本号相关
    public static string ms_sVersion
    {
        get
        {
            string sVersion = "0.0.0.1000";
            if (File.Exists(ms_sStreamDir + ms_sVersionFile))
            {
                JsonData jd = Util.readJson(ms_sStreamDir + ms_sVersionFile);
                sVersion = jd["version"].ToString();
            }
            return sVersion;
        }
    }

    //版本相关
    public static readonly string ms_sVersionFile = "Version.txt";
    public static string ms_sNewVersion = "0.0.0.1000";

    //public static readonly string ms_sPackFileList = "ResourceList.json";     // 包索引文件
    //public static readonly string ms_sPatcherWcrLst = GameConfigMgr.ms_sPatcherWcrLst;     // 更新包包含的资源信息写入文件 "wcr_patcherinfo.bytes"
    //                                                                                       // 一些后缀名
    //public static readonly string ms_sFilterFileType = ".meta";   // 要被过滤的文件类型

    //public static readonly string ms_sWCRFileType = ".wcr";       // Package 包的后缀名

    //                                                                  // 资源组名，等同于文件夹的名字
    //public static readonly string ms_sScriptGroupName = "Script";

    //static readonly string ms_sUIGroupName = "ui";
    //static readonly string ms_sMapGroupName = "maps";
    //static readonly string ms_sCharacterGroupName = "characters";
    //static readonly string ms_sObjGroupName = "obj";
    //static readonly string ms_sEffectGroupName = "effects";
    //static readonly string ms_sMediaGroupName = "media";
    // 一些路径

    // 编辑器相关路径，StreamingAssets、Data、DataBundle
    public static readonly string ms_sStreamDir = Application.streamingAssetsPath + '/';
    public static readonly string ms_sDataDir = Application.dataPath + "/Data/";
    //public static readonly string ms_sBundleDir = Application.dataPath + "/DataBundle/";
    public static readonly string ms_sAssetsDir = "Assets/Data/";

    // AssetBundle 包的后缀名 ".bytes"
    public static readonly string ms_sABFileType = GameConfigMgr.ms_sBundleType;

    // Package 包的后缀名
    public static readonly string ms_sWCRFileType = ".wcr";
    // 包索引文件
    public static readonly string ms_sPackFileList = "ResourceList.json";

    // Patcher 包的后缀名
    public static readonly string ms_sPatcherFileType = ".wcp";
    //更新包相关路径
    public static readonly string ms_sBundleDir = Application.dataPath + "/DataBundle/";

    // 打包输出总路径 => public路径
    public static readonly string ms_sPublicDir = Application.dataPath + "/../../../Public/";
    public static readonly string ms_sPatcherDir = ms_sPublicDir + "Patcher/";
    public static readonly string ms_sDataOutDir = ms_sPatcherDir + "DataAB/";
    public static readonly string ms_sWcrDir = ms_sPatcherDir + "Wcr/";
    // Lua原始文件保存位置
    public static readonly string ms_sLuaFileDir = ms_sPatcherDir + "DataAB/Lua/";

    // Lua相关文件夹名
    static string ms_sLuaJitToolPath = Application.dataPath + "/../../Tools/LuaJIT/luajit.exe";
    public static readonly string ms_sLuaScriptGroupName = "LuaScript";
    public static readonly string ms_sLuaDesignerGroupName = "Designer";
    public static readonly string[] ms_ArrGacLuaCheck = { "/Gac/", "/GacGasCommon/", "/Common/" };
    public static readonly string ms_sLuaScriptDir = GameConfigMgr.ms_sLuaScriptDir; //Application.dataPath + "/../../LuaScript/";
    public static readonly string ms_sLuaDesignerDir = GameConfigMgr.ms_sDesignerDir; //Application.dataPath + "/../../Designer/";

    //public static readonly string ms_sResourceDir = Application.dataPath + "/Resources/DataBundle/";

    //public static readonly string ms_sScriptDir = Application.dataPath + "/../../Script/";

    //public static readonly string ms_sLuaDesignerRootDir = Application.dataPath + "/../../Designer/";

    // 真实的本地路径
    static string ms_sMapDir = ms_sDataDir + GameConfigMgr.ms_sMapFolderName + "/";
    static string ms_sMediaDir = ms_sDataDir + GameConfigMgr.ms_sMediaFolderName + "/";
    static string ms_sUIDir = ms_sDataDir + GameConfigMgr.ms_sUIFolderName + "/";
    static string ms_sObjDir = ms_sDataDir + GameConfigMgr.ms_sObjFolderName + "/";
    static string ms_sEffectDir = ms_sDataDir + GameConfigMgr.ms_sEffectFolderName + "/";
    static string ms_sCharDir = ms_sDataDir + GameConfigMgr.ms_sCharFolderName + "/";

    static string ms_sResrcDataDir = Application.dataPath + "/Resources/" + GameConfigMgr.ms_sABFolderName + "/";

    // 5.x默认自带 BuildAssetBundleOptions.CompleteAssets 和 BuildAssetBundleOptions.CollectDependencies
    public static readonly BuildAssetBundleOptions m_theAssetOptions = BuildAssetBundleOptions.ForceRebuildAssetBundle 
        | BuildAssetBundleOptions.UncompressedAssetBundle | BuildAssetBundleOptions.DeterministicAssetBundle;

    #region 文件和字符处理相关

    // 从一堆字符串中获取符合模式的字符串
    public static string[] GetMatch(string[] files, string[] patterns)
    {
        List<string> lstFiles = new List<string>();
        for (int i = 0; i < files.Length; i++)
        {
            bool bOK = false;
            for (int j = 0; j < patterns.Length; j++)
            {
                // 不区分大小写
                if (Regex.IsMatch(files[i], patterns[j], RegexOptions.IgnoreCase))
                    bOK = true;
            }
            if (bOK == true)
            {
                lstFiles.Add(files[i]);
            }
        }
        //
        return lstFiles.ToArray();
    }

    // 获得所有 后缀名在sPattern中的文件。（dir： "Assets/....../"）
    static string[] GetFiles(string[] sDirs, string[] sPatterns = null, bool bAssets = true, bool bAllDir = true)
    {
        List<string> files = new List<string>();
        if (sDirs == null || sDirs.Length < 1)
        {
            Debug.LogError("GetFiles: No sDirs!");
            return files.ToArray();
        }
        if (sPatterns == null)
            sPatterns = new string[] { ".*" };

        string sAssetDir = "Assets/";
        string sRealAssetDir = Application.dataPath + "/";
        for (int i = 0; i < sDirs.Length; i++)
        {
            if (!Directory.Exists(sDirs[i]))
                continue;

            for (int j = 0; j < sPatterns.Length; j++)
            {
                string sDirectory = sDirs[i];
                if (bAssets == true)
                    sDirectory = sRealAssetDir + sDirectory.Remove(0, sDirectory.IndexOf(sAssetDir) + sAssetDir.Length);
                string[] sArr = null;
                if (bAllDir)
                    sArr = Directory.GetFiles(sDirectory, sPatterns[j], SearchOption.AllDirectories);
                else
                    sArr = Directory.GetFiles(sDirectory, sPatterns[j], SearchOption.TopDirectoryOnly);
                for (int k = 0; k < sArr.Length; k++)
                {
                    string sKey = sArr[k].Replace('\\', '/');
                    if (!sKey.EndsWith(".meta"))
                    {
                        if (bAssets == true)
                            sKey = sAssetDir + sKey.Substring(sRealAssetDir.Length);
                        if (!files.Contains(sKey))
                            files.Add(sKey);
                    }
                }
            }
        }
        return files.ToArray();
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

    // 生成Lua字节码
    static bool LuaToByteCode(string sLuaSrcDir, string sLuaDstDir, string sGroupName, string[] includePath)
    {
        // luajit.exe打包所有Lua文件
        if (!File.Exists(ms_sLuaJitToolPath))
        {
            Debug.LogError("no File:" + ms_sLuaJitToolPath + " in system");
            return false;
        }
        //*/
        if (!Directory.Exists(sLuaSrcDir))
        {
            Debug.LogError("no Path:" + sLuaSrcDir + " in system");
            return false;
        }

        bool bSuccess = true;
        // 统计耗时
        System.DateTime now = System.DateTime.Now;
        Debug.Log(sGroupName + ": Lua To CodeByte Start!");

        // 删除之前的字节码
        if (Directory.Exists(sLuaDstDir))
            Directory.Delete(sLuaDstDir, true);
        Directory.CreateDirectory(sLuaDstDir);

        // 删除之前老的Lua文件
        string sLuaFileDir = AddStamp(ms_sLuaFileDir) + sGroupName + "/";
        if (Directory.Exists(sLuaFileDir))
            Directory.Delete(sLuaFileDir, true);
        Directory.CreateDirectory(sLuaFileDir);

        string[] sDirs = { sLuaSrcDir };
        string[] sPatterns = { "*.lua", "*.csv" };
        string[] sFiles = GetFiles(sDirs, sPatterns, false);
        int nCount = sFiles.Length;

        int num = 0;
        for (int i = 0; i < nCount; i++)
        {
            if (!IsInlcudePath(sFiles[i], includePath))
                continue;

            // 客户端要支持IOS的arm64，所以直接使用源码
            string sFilePath = sFiles[i].Replace("\\", "/");
            string sFileName = sFilePath.Substring(sLuaSrcDir.Length).Replace('/', '.') + ".bytes";
            string sDstFile = sLuaDstDir + "/" + sFileName;
            File.Copy(sFilePath, sDstFile, true);
            File.Copy(sFilePath, (sLuaFileDir + sFileName).ToLower(), true);

            float fProgress = i * 1.0f / nCount;
            EditorUtility.DisplayProgressBar("LuaCsv CodeByte", sFilePath + ": " + fProgress.ToString(), fProgress);
            // Debug.Log(sFilePath + " => " + sDstFile);
            num += 1;
        }
        EditorUtility.ClearProgressBar();
        // 强制垃圾回收
        System.GC.Collect();
        // 计算耗时
        System.TimeSpan disTime = System.DateTime.Now - now;
        Debug.Log(sLuaSrcDir + ": " + num.ToString() + " LuaFiles To ByteCode! Cost " + (disTime.Hours * 3600 + disTime.Minutes * 60 + disTime.Seconds).ToString());

        return bSuccess;
    }

    #endregion

    #region 版本号相关

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
        if (!CheckVersion(version))
        {
            Debug.LogError("Invalid Version!!!!");
            return false;
        }
        string sOutDir = AddStamp(ms_sDataOutDir);
        string path = sOutDir + GameConfigMgr.ms_sVersionFileName + GameConfigMgr.ms_sABType;
        string dir = path.Substring(0, path.LastIndexOf('/'));
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        JsonData jd = new JsonData();
        jd["version"] = version;
        Util.writeJson(jd, path);
        Util.writeJson(jd, ms_sStreamDir + ms_sVersionFile);
        //File.WriteAllBytes(path, Encoding.UTF8.GetBytes(version));
        return true;
    }

    #endregion

    #region 打包成AB资源

    // AssetBundle
    // 根据文件数组打包成一个资源包。（file： "Assets/Data/....../XXX.XXX"）。场景文件（".unity"）不能和其他资源一起打包!!!!
    public static void BundleFiles(string outDir, string[] files, BuildAssetBundleOptions abOptions, bool bDelOldDir = false, bool bOne = false, string abName = "", string abVariant = null)
    {
        if (files.Length < 1)
        {
            Debug.LogError(" => No File to Bundle!");
            return;
        }

        bool bScene = false;
        if (files.Length > 0 && files[0].EndsWith(".unity"))
            bScene = true;

        if (bDelOldDir && Directory.Exists(outDir))
            Directory.Delete(outDir, true);
        if (!Directory.Exists(outDir))
            Directory.CreateDirectory(outDir);

        List<AssetBundleBuild> lstABB = new List<AssetBundleBuild>();
        //主要用于Lua相关打包
        if (bOne)
        {
            if (string.IsNullOrEmpty(abName))
            {
                Debug.LogError("BundleFiles be one , abName is null or empty");
                return;
            }

            AssetBundleBuild abb = new AssetBundleBuild();
            abb.assetBundleVariant = abVariant; // abVariant = "1024X768";
            abb.assetNames = files;
            abb.assetBundleName = abName;
            lstABB.Add(abb);
        }
        //主要用于资源相关打包
        else if (!bScene && false)
        {
            Dictionary<string, int> DependenciesNum = new Dictionary<string, int>();
            for (int i = 0; i < files.Length; ++i)
            {
                DependenciesNum[files[i]] = 0;
            }

            for (int i = 0; i < files.Length; ++i)
            {
                string[] depFiles = AssetDatabase.GetDependencies(files[i]);
                string[] lastDepFiles = GetMatch(depFiles, new string[] { "^.+\\.ogg$|^.+\\.mp3$|^.+\\.prefab$|^.+\\.png$|^.+\\.jpg$|^.+\\.ttf$|^.+\\.json$|^.+\\.bytes$|^.+\\.renderTexture$" });
                for (int j = 0; j < lastDepFiles.Length; ++j)
                {
                    DependenciesNum[lastDepFiles[j]] += 1;
                }
            }

            List<string> keys = new List<string>(DependenciesNum.Keys);

            //计数，查看是否有错漏
            int count2 = 0;
            for (int i = 0; i < keys.Count; ++i)
            {
                if (DependenciesNum[keys[i]] == 2)
                    count2 += 1;
            }

            //打包设置
            int n = ms_sAssetsDir.Length;
            for (int i = keys.Count - 1; i >= 0; --i)
            {
                //没有被依赖，或者被依赖次数大于1
                if (DependenciesNum[keys[i]] != 2)
                {
                    List<string> abFiles = new List<string>();
                    abFiles.Add(keys[i]);
                    string[] depFiles = AssetDatabase.GetDependencies(keys[i]);
                    string[] lastDepFiles = GetMatch(depFiles, new string[] { "^.+\\.ogg$|^.+\\.mp3$|^.+\\.prefab$|^.+\\.png$|^.+\\.jpg$|^.+\\.ttf$|^.+\\.json$|^.+\\.bytes$|^.+\\.renderTexture$" });
                    for (int j = 0; j < lastDepFiles.Length; ++j)
                    {
                        int num = DependenciesNum[lastDepFiles[j]];
                        if (num <= 2 && !lastDepFiles[j].Equals(keys[i]))
                        {
                            if (num < 2)
                                Debug.LogError(lastDepFiles[j] + ": " + j + " : num < 2");
                            else
                            {
                                abFiles.Add(lastDepFiles[j]);
                                count2 -= 1;
                            }
                        }
                    }

                    AssetBundleBuild abb = new AssetBundleBuild();
                    abb.assetBundleVariant = abVariant; // abVariant = "1024X768";
                    abb.assetNames = abFiles.ToArray();
                    abb.assetBundleName = keys[i].Replace("\\", "/").Substring(n) + GameConfigMgr.ms_sABType;
                    lstABB.Add(abb);
                }
            }

            if (count2 != 0)
                Debug.LogError("Error : DependenciesNum[keys[i]] == 2 count is error !");
        }
        else
        {
            int n = ms_sAssetsDir.Length;
            for (int i = 0; i < files.Length; i++)
            {
                AssetBundleBuild abb = new AssetBundleBuild();
                abb.assetBundleVariant = abVariant; // abVariant = "1024X768";
                abb.assetNames = new string[] { files[i] };
                abb.assetBundleName = files[i].Replace("\\", "/").Substring(n) + GameConfigMgr.ms_sABType;
                lstABB.Add(abb);
            }
        }

        AssetDatabase.Refresh();
        if (bScene == false)
        {
            if (lstABB.Count > 0)
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
            for (int i = 0; i < lstABB.Count; i++)
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
                if (!string.IsNullOrEmpty(s))
                    Debug.LogError(s);
            }
        }
        AssetDatabase.Refresh();
    }

    // AssetBundle
    // 根据文件数组打包成一个资源包。（file： "Assets/Data/....../XXX.XXX"）。场景文件（".unity"）不能和其他资源一起打包!!!!
    public static void BundleFilesEX(string outDir, List<List<string>> files, List<string> abNames, BuildAssetBundleOptions abOptions, bool bDelOldDir = false, string abVariant = null)
    {
        if (files.Count < 1)
        {
            Debug.LogError(" => No File to Bundle!");
            return;
        }

        if (bDelOldDir && Directory.Exists(outDir))
            Directory.Delete(outDir, true);
        if (!Directory.Exists(outDir))
            Directory.CreateDirectory(outDir);

        List<AssetBundleBuild> lstABB = new List<AssetBundleBuild>();
        
        if (abNames.Count < 1)
        {
            Debug.LogError("abNames is empty");
            return;
        }

        for (int i = 0; i < files.Count; i++)
        {
            AssetBundleBuild abb = new AssetBundleBuild();
            abb.assetBundleVariant = abVariant; // abVariant = "1024X768";
            abb.assetNames = files[i].ToArray();
            abb.assetBundleName = abNames[i];
            lstABB.Add(abb);
        }

        AssetDatabase.Refresh();

        if (lstABB.Count > 0)
        {
            AssetBundleManifest abm = BuildPipeline.BuildAssetBundles(outDir, lstABB.ToArray(), abOptions, ms_target);
            if (abm == null)
            {
                Debug.LogError("BundFiles Failed! -> " + outDir);
            }
        }
        
        AssetDatabase.Refresh();
    }

    //删除相关的老的文件夹和manifest文件
    static void DeleteABManifest(string sOldABGroupDir, string sOldABMFName)
    {
        if (Directory.Exists(sOldABGroupDir))
            Directory.Delete(sOldABGroupDir, true);
        if (File.Exists(sOldABMFName))
            File.Delete(sOldABMFName);
        if (File.Exists(sOldABMFName + ".manifest"))
            File.Delete(sOldABMFName + ".manifest");
    }

    //重命名manifest文件为对应项目
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

                string sFMDir = sBytesDir + ".manifest";
                string sBackUpMFDir = sBackUpDir + ".manifest";
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

    //[MenuItem("WCTool/GamePackage/BundleLuaCsv")]
    public static bool BundleLuaCsv()
    {
        System.DateTime lastTime = System.DateTime.Now;
        string sOutDir = AddStamp(ms_sDataOutDir);
        string sDataLuaScriptPath = ms_sDataDir + ms_sLuaScriptGroupName;
        string sDataDesignerPath = ms_sDataDir + ms_sLuaDesignerGroupName;
        bool bSuccess = true;

        bSuccess &= LuaToByteCode(ms_sLuaScriptDir, sDataLuaScriptPath, ms_sLuaScriptGroupName, ms_ArrGacLuaCheck);
        bSuccess &= LuaToByteCode(ms_sLuaDesignerDir, sDataDesignerPath, ms_sLuaDesignerGroupName, new string[] { "/Config/" });

        AssetDatabase.Refresh();
        if (!bSuccess) return false;

        // Lua Handle
        DeleteABManifest(sOutDir + ms_sLuaScriptGroupName.ToLower(), sOutDir + GameConfigMgr.ms_sLuaMFileName);
        string[] luaFilesList = GetFiles(new string[] { sDataLuaScriptPath }, new string[] { "*.bytes" }, true);
        BundleFiles(sOutDir, luaFilesList, m_theAssetOptions, false/*, true, ms_sLuaScriptGroupName + GameConfigMgr.ms_sABType*/);
        bSuccess &= BackUpABManifest(sOutDir, sOutDir + GameConfigMgr.ms_sLuaMFileName + GameConfigMgr.ms_sABType);
        if (!bSuccess) return false;

        // Csv Handle
        DeleteABManifest(sOutDir + ms_sLuaDesignerGroupName.ToLower(), sOutDir + GameConfigMgr.ms_sCsvMFileName);
        string[] csvFilesList = GetFiles(new string[] { sDataDesignerPath }, new string[] { "*.bytes" }, true);
        BundleFiles(sOutDir, csvFilesList, m_theAssetOptions, false/*, true, ms_sLuaDesignerGroupName + GameConfigMgr.ms_sABType*/);
        bSuccess &= BackUpABManifest(sOutDir, sOutDir + GameConfigMgr.ms_sCsvMFileName + GameConfigMgr.ms_sABType);
        if (!bSuccess) return false;

        //Clean LuaCsv Temp Files
        if (Directory.Exists(sDataLuaScriptPath))
        {
            Directory.Delete(sDataLuaScriptPath, true);
            File.Delete(sDataLuaScriptPath + ".meta");
        }
        if (Directory.Exists(sDataDesignerPath))
        {
            Directory.Delete(sDataDesignerPath, true);
            File.Delete(sDataDesignerPath + ".meta");
        }
        AssetDatabase.Refresh();

        System.TimeSpan costTime = System.DateTime.Now - lastTime;
        Debug.Log("-->> BundleLuaCsv Cost: " + (costTime.Hours * 3600 + costTime.Minutes * 60 + costTime.Seconds).ToString() + "s");
        return bSuccess;
    }

    //[MenuItem("WCTool/GamePackage/BundleData")]
    public static bool BundleData(bool makeAlast = false)
    {
        bool bSuccess = true;
        System.DateTime lastTime = System.DateTime.Now;
        string sOutDir = AddStamp(ms_sDataOutDir);

        List<string> lstFiles = new List<string>();
        string[] mediaFiles = GetFiles(new string[] { ms_sMediaDir }, new string[] { "*.ogg", "*.mp3" }, true);
        lstFiles.AddRange(mediaFiles);

        string[] objFiles = GetFiles(new string[] { ms_sObjDir }, new string[] { "*.prefab" }, true);
        lstFiles.AddRange(objFiles);

        string[] effectFiles = GetFiles(new string[] { ms_sEffectDir }, new string[] { "*.prefab" }, true);
        lstFiles.AddRange(effectFiles);

        string[] charFiles = GetFiles(new string[] { ms_sCharDir }, new string[] { "*.prefab" }, true);
        lstFiles.AddRange(charFiles);

        //ui图片资源涉及到打包图集，需要单独处理下
        string[] uiFiles = GetFiles(new string[] { ms_sUIDir }, new string[] { "*.prefab", "*.png", "*.jpg", "*.ttf" }, true);
        //lstFiles.AddRange(uiFiles);
        List<string> lstAloneFiles = new List<string>();
        List<string> lstAlastFiles = new List<string>();
        if (!makeAlast)
            lstFiles.AddRange(uiFiles);
        else
        {
            //单独获取依赖相关所有资源
            string[] depUIFiles = AssetDatabase.GetDependencies(uiFiles);
            string[] buildUIFiles = GetMatch(depUIFiles, new string[] { "^.+\\.ogg$|^.+\\.mp3$|^.+\\.prefab$|^.+\\.png$|^.+\\.jpg$|^.+\\.ttf$|^.+\\.json$|^.+\\.bytes$|^.+\\.renderTexture$" });
            //删除里面有关图集的资源
            string alastPath = ms_sUIDir + "textures/atlas/";
            alastPath = alastPath.Remove(0, UnityEngine.Application.dataPath.Length);
            alastPath = "Assets" + alastPath;

            //对UI资源进行分类
            lstAloneFiles.AddRange(buildUIFiles);

            for (int i = lstAloneFiles.Count - 1; i >= 0; --i)
            {
                if (lstAloneFiles[i].Contains(alastPath))
                {
                    string ext = Path.GetExtension(lstAloneFiles[i]).ToLower();
                    if (!ext.Equals(".jpg") && !ext.Equals(".png"))
                    {
                        Debug.Log("this is not UI Alast file:" + lstAloneFiles[i]);
                        return false;
                    }

                    lstAlastFiles.Add(lstAloneFiles[i]);
                    lstAloneFiles.RemoveAt(i);
                }
            }
        }

        /*string[] cfgFiles = GetFiles(new string[] { ms_sConfigDir }, new string[] { "*.prefab", "*.json", "*.bytes" }, true);
        lstFiles.AddRange(cfgFiles);*/

        string[] mapFiles = GetFiles(new string[] { ms_sMapDir }, new string[] { "*.prefab" }, true);
        lstFiles.AddRange(mapFiles);

        string[] depFiles = AssetDatabase.GetDependencies(lstFiles.ToArray());
        string[] files = GetMatch(depFiles, new string[] { "^.+\\.ogg$|^.+\\.mp3$|^.+\\.prefab$|^.+\\.png$|^.+\\.jpg$|^.+\\.ttf$|^.+\\.json$|^.+\\.bytes$|^.+\\.renderTexture$" });

        //添加UI不需要打包图集的资源进去
        if (!makeAlast)
            BundleFiles(sOutDir, files, m_theAssetOptions, false, false);
        else
        {
            lstAloneFiles.AddRange(files);

            //删除重复资源
            for (int i = lstAloneFiles.Count - 1; i >= 0; --i)
            {
                for (int index = i - 1; index >= 0; --index)
                {
                    if (lstAloneFiles[index].ToLower().Equals(lstAloneFiles[i].ToLower()))
                    {
                        lstAloneFiles.RemoveAt(i);
                        continue;
                    }
                }
            }

            for (int i = lstAloneFiles.Count - 1; i >= 0; --i)
            {
                for (int index = 0; index < lstAlastFiles.Count; ++index)
                {
                    if (lstAloneFiles[i].ToLower().Equals(lstAlastFiles[index].ToLower()))
                        lstAloneFiles.RemoveAt(i);
                }
            }

            BundleAAFiles(sOutDir, lstAloneFiles.ToArray(), lstAlastFiles.ToArray(), m_theAssetOptions, false, false);
        }

        bSuccess &= BackUpABManifest(sOutDir, sOutDir + GameConfigMgr.ms_sABMFileName + GameConfigMgr.ms_sABType);
        if (!bSuccess) return false;

        System.TimeSpan costTime = System.DateTime.Now - lastTime;
        Debug.Log("-->> BundleData Cost: " + (costTime.Hours * 3600 + costTime.Minutes * 60 + costTime.Seconds).ToString() + "s");
        return bSuccess;
    }

    // AssetBundle
    // ui图片相关打包 Alone Alast
    public static void BundleAAFiles(string outDir, string[] files, string[] alastFiles, BuildAssetBundleOptions abOptions, bool bDelOldDir = false, bool bOne = false, string abName = "", string abVariant = null)
    {
        if (files.Length < 1)
        {
            Debug.LogError(" => No File to Bundle!");
            return;
        }

        bool bScene = false;
        if (files.Length > 0 && files[0].EndsWith(".unity"))
            bScene = true;

        if (bDelOldDir && Directory.Exists(outDir))
            Directory.Delete(outDir, true);
        if (!Directory.Exists(outDir))
            Directory.CreateDirectory(outDir);

        List<AssetBundleBuild> lstABB = new List<AssetBundleBuild>();

        //把UI图集相关资源加进去
        if (alastFiles.Length >= 0)
        {
            List<string> lstAlastFiles = new List<string>();
            lstAlastFiles.AddRange(alastFiles);
            //按文件夹名字分类图集
            Dictionary<string, List<string>> dicAlastFiles = new Dictionary<string, List<string>>();
            for (int i = 0; i < lstAlastFiles.Count; ++i)
            {
                string key = lstAlastFiles[i].Replace("\\", "/").Substring(0, lstAlastFiles[i].LastIndexOf("/"));
                //key = key.Substring(key.LastIndexOf("/") + 1);
                if (!dicAlastFiles.ContainsKey(key))
                    dicAlastFiles[key] = new List<string>();

                dicAlastFiles[key].Add(lstAlastFiles[i]);
            }

            //2、图集打包
            lstAlastFiles.Clear();
            lstAlastFiles = new List<string>(dicAlastFiles.Keys);
            for (int i = 0; i < lstAlastFiles.Count; ++i)
            {
                if (dicAlastFiles[lstAlastFiles[i]].Count == 1)
                    Debug.LogError("count == 1, texture in the parh: " + lstAlastFiles[i]);

                AssetBundleBuild abb = new AssetBundleBuild();
                abb.assetBundleVariant = abVariant; // abVariant = "1024X768";
                abb.assetNames = dicAlastFiles[lstAlastFiles[i]].ToArray();

                //string folderName = lstAlastFiles[i].Substring(lstAlastFiles[i].LastIndexOf("/") + 1);
                abb.assetBundleName = lstAlastFiles[i].Replace("\\", "/").Substring(ms_sAssetsDir.Length) + ".atlas" + GameConfigMgr.ms_sABType;
                lstABB.Add(abb);
            }
        }


        //主要用于Lua相关打包
        if (bOne)
        {
            if (string.IsNullOrEmpty(abName))
            {
                Debug.LogError("BundleFiles be one , abName is null or empty");
                return;
            }

            AssetBundleBuild abb = new AssetBundleBuild();
            abb.assetBundleVariant = abVariant; // abVariant = "1024X768";
            abb.assetNames = files;
            abb.assetBundleName = abName;
            lstABB.Add(abb);
        }
        //主要用于资源相关打包
        else if (!bScene && false)
        {
            Dictionary<string, int> DependenciesNum = new Dictionary<string, int>();
            for (int i = 0; i < files.Length; ++i)
            {
                DependenciesNum[files[i]] = 0;
            }

            for (int i = 0; i < files.Length; ++i)
            {
                string[] depFiles = AssetDatabase.GetDependencies(files[i]);
                string[] lastDepFiles = GetMatch(depFiles, new string[] { "^.+\\.ogg$|^.+\\.mp3$|^.+\\.prefab$|^.+\\.png$|^.+\\.jpg$|^.+\\.ttf$|^.+\\.json$|^.+\\.bytes$|^.+\\.renderTexture$" });
                for (int j = 0; j < lastDepFiles.Length; ++j)
                {
                    DependenciesNum[lastDepFiles[j]] += 1;
                }
            }

            List<string> keys = new List<string>(DependenciesNum.Keys);

            //计数，查看是否有错漏
            int count2 = 0;
            for (int i = 0; i < keys.Count; ++i)
            {
                if (DependenciesNum[keys[i]] == 2)
                    count2 += 1;
            }

            //打包设置
            int n = ms_sAssetsDir.Length;
            for (int i = keys.Count - 1; i >= 0; --i)
            {
                //没有被依赖，或者被依赖次数大于1
                if (DependenciesNum[keys[i]] != 2)
                {
                    List<string> abFiles = new List<string>();
                    abFiles.Add(keys[i]);
                    string[] depFiles = AssetDatabase.GetDependencies(keys[i]);
                    string[] lastDepFiles = GetMatch(depFiles, new string[] { "^.+\\.ogg$|^.+\\.mp3$|^.+\\.prefab$|^.+\\.png$|^.+\\.jpg$|^.+\\.ttf$|^.+\\.json$|^.+\\.bytes$|^.+\\.renderTexture$" });
                    for (int j = 0; j < lastDepFiles.Length; ++j)
                    {
                        int num = DependenciesNum[lastDepFiles[j]];
                        if (num <= 2 && !lastDepFiles[j].Equals(keys[i]))
                        {
                            if (num < 2)
                                Debug.LogError(lastDepFiles[j] + ": " + j + " : num < 2");
                            else
                            {
                                abFiles.Add(lastDepFiles[j]);
                                count2 -= 1;
                            }
                        }
                    }

                    AssetBundleBuild abb = new AssetBundleBuild();
                    abb.assetBundleVariant = abVariant; // abVariant = "1024X768";
                    abb.assetNames = abFiles.ToArray();
                    abb.assetBundleName = keys[i].Replace("\\", "/").Substring(n) + GameConfigMgr.ms_sABType;
                    lstABB.Add(abb);
                }
            }

            if (count2 != 0)
                Debug.LogError("Error : DependenciesNum[keys[i]] == 2 count is error !");
        }
        else
        {
            int n = ms_sAssetsDir.Length;
            for (int i = 0; i < files.Length; i++)
            {
                AssetBundleBuild abb = new AssetBundleBuild();
                abb.assetBundleVariant = abVariant; // abVariant = "1024X768";
                abb.assetNames = new string[] { files[i] };
                abb.assetBundleName = files[i].Replace("\\", "/").Substring(n) + GameConfigMgr.ms_sABType;
                lstABB.Add(abb);
            }
        }

        AssetDatabase.Refresh();
        if (bScene == false)
        {
            if (lstABB.Count > 0)
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
            for (int i = 0; i < lstABB.Count; i++)
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
                if (!string.IsNullOrEmpty(s))
                    Debug.LogError(s);
            }
        }
        AssetDatabase.Refresh();
    }

    //[MenuItem("WCTool/GamePackage/BundleMap")]
    public static bool BundleMap()
    {
        System.DateTime lastTime = System.DateTime.Now;
        string sOutDir = AddStamp(ms_sDataOutDir);

        string[] files = GetFiles(new string[] { ms_sMapDir }, new string[] { "*.unity" }, true);
        BundleFiles(sOutDir, files, BuildAssetBundleOptions.DeterministicAssetBundle, false);

        System.TimeSpan costTime = System.DateTime.Now - lastTime;
        Debug.Log("-->> BundleMap Cost: " + (costTime.Hours * 3600 + costTime.Minutes * 60 + costTime.Seconds).ToString() + "s");
        return true;
    }

    // BundleAll 将所有资源打成AB包
    public static bool BundAll(string sNewVersion = null, bool bVersion = true, bool bLua = true, bool bData = true, bool bMap = true, bool bDelOld = false)
    {
        if (!CheckVersion(sNewVersion))
        {
            Debug.LogError("Invalid Version!!!!");
            return false;
        }

        ms_sNewVersion = sNewVersion;
        string sDataOutDir = AddStamp(ms_sDataOutDir);
        if (bVersion && Directory.Exists(sDataOutDir))
        {
            Debug.LogError("Exists Directory: " + sDataOutDir);
            return false;
        }

        //// 生成新的版本号
        if (bVersion && !GenVersion(sNewVersion)) return false;
        // 打包 Lua
        if (bLua && !BundleLuaCsv()) return false;
        // 打包 UI
        if (bData && !BundleData(true)) return false;
        // 打包 Map
        if (bMap && !BundleMap()) return false;

        // 刷新Unity3D视图里面的资源
        AssetDatabase.Refresh();
        return true;
    }
    #endregion

    #region 分包和更新相关

    #region 分包

    // 拷贝文件操作，将目录结构扁平化，并加上后缀".bytes"
    static bool FlatDir(string sSrcDir, string sDstDir, string[] sFiles = null)
    {
        if (!Directory.Exists(sSrcDir))
        {
            Debug.LogError("Not Exist Directory: " + sSrcDir);
            return false;
        }
        if (sFiles == null)
        {
            string[] sDirs = { sSrcDir };
            string[] sPatterns = { "*" + ms_sABFileType };
            sFiles = GetFiles(sDirs, sPatterns, false);
        }
        else
        {
            for (int i = 0; i < sFiles.Length; ++i)
            {
                sFiles[i] = sSrcDir + sFiles[i];
            }
        }

        if (sFiles.Length < 1)
            return false;

        if (Directory.Exists(sDstDir))
            Directory.Delete(sDstDir, true);
        Directory.CreateDirectory(sDstDir);
        for (int i = 0; i < sFiles.Length; i++)
        {
            string sFileName = sDstDir + sFiles[i].Substring(sSrcDir.Length).Replace("\\", "/").Replace("/", "^")/* + ".bytes"*/;
            File.Copy(sFiles[i], sFileName);

            float fProgress = i * 1.0f / sFiles.Length;
            EditorUtility.DisplayProgressBar("FlatDir", "Copy File: " + sFiles[i] + " => " + sFileName + "  " + fProgress.ToString(), fProgress);
        }
        EditorUtility.ClearProgressBar();

        // 刷新Unity3D视图里面的资源
        AssetDatabase.Refresh();

        return true;
    }

    // 打包文件
    static string[] PackageFiles(string sGroupName, string[] sFiles, string sOutDir, int nFileLimit = 0)
    {
        List<string> lstPackageFiles = new List<string>();  // 资源包的文件列表
        if (string.IsNullOrEmpty(sGroupName))
        {
            Debug.LogError("sGroupName is null!");
            return lstPackageFiles.ToArray();
        }
        //string sOutDir = ms_sWcrDir + ms_sVersion + "/" + ms_sPlatform + "/";
        if (string.IsNullOrEmpty(sOutDir))
        {
            Debug.LogError("sOutDir is null!");
            return lstPackageFiles.ToArray();
        }
        if (sFiles == null || sFiles.Length < 1)
        {
            Debug.LogError("sFiles is null!");
            return lstPackageFiles.ToArray();
        }
        // 统计耗时
        System.DateTime now = System.DateTime.Now;
        Debug.Log(sGroupName + ": Package Start!");

        BuildAssetBundleOptions theAssetOptions = BuildAssetBundleOptions.ForceRebuildAssetBundle | BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.DisableWriteTypeTree;

        bool bSuccess = false;
        // 文件大小限制，是否需要分包
        if (nFileLimit <= 0)
        {
            string sOutFile = sGroupName + ms_sWCRFileType;
            //bSuccess = BundleFiles(sFiles, sOutDir + sOutFile, true);
            BundleFiles(sOutDir, sFiles, theAssetOptions, false, true, sOutFile);
            lstPackageFiles.Add(sOutFile);
        }
        else    // 需要分包
        {
            bSuccess = true;
            int nFlag = 1001;
            long nSize = 0;
            List<List<string>> LastList = new List<List<string>>();
            List<string> abNames = new List<string>();
            List<string> lstFile = new List<string>();
            for (int i = 0; i < sFiles.Length; i++)
            {
                string sAssetDir = "Assets/";
                string sRealAssetDir = Application.dataPath + "/";
                string sFile = sRealAssetDir + sFiles[i].Substring(sAssetDir.Length);
                FileInfo fi = new FileInfo(sFile);
                nSize += fi.Length;
                lstFile.Add(sFiles[i]);
                if (nSize >= nFileLimit || i == sFiles.Length - 1)
                {
                    string sOutFile = sGroupName + nFlag.ToString() + ms_sWCRFileType;
                    //bSuccess &= BundleFiles(lstFile.ToArray(), sOutDir + sOutFile, true);
                    //BundleFilesEX(sOutDir, lstFile.ToArray(), theAssetOptions, false, sOutFile);
                    List<string> files = new List<string>();
                    files.AddRange(lstFile);

                    LastList.Add(files);
                    abNames.Add(sOutFile);

                    lstPackageFiles.Add(sOutFile);
                    nFlag++;
                    nSize = 0;
                    lstFile.Clear();
                }
            }

            BundleFilesEX(sOutDir, LastList, abNames, theAssetOptions, false);
        }
        // 刷新Unity3D视图里面的资源
        AssetDatabase.Refresh();
        // 强制垃圾回收
        System.GC.Collect();
        // 计算耗时
        System.TimeSpan disTime = System.DateTime.Now - now;
        // 打印成功与否的信息
        if (bSuccess)
        {
            Debug.Log(sGroupName + ": " + sFiles.Length.ToString() + " Files Package! Cost " + (disTime.Hours * 3600 + disTime.Minutes * 60 + disTime.Seconds).ToString() + "s");
            return lstPackageFiles.ToArray();
        }
        else
        {
            Debug.LogError(sGroupName + ": " + " Package Failed! Cost " + (disTime.Hours * 3600 + disTime.Minutes * 60 + disTime.Seconds).ToString() + "s");
            return null;
        }
    }

    // 打包，解压版本
    public static bool Package()
    {
        string sWcrDir = AddStamp(ms_sWcrDir);
        // 删除老的资源包
        if (Directory.Exists(sWcrDir))
            Directory.Delete(sWcrDir, true);
        //if (Directory.Exists(ms_sResourceDir))
            //Directory.Delete(ms_sResourceDir, true);
        // 刷新Unity3D视图里面的资源
        AssetDatabase.Refresh();

        // 拷贝文件操作，将目录结构扁平化
        string path = AddStamp(ms_sDataOutDir);
        if (!FlatDir(path, ms_sBundleDir + "_Package/"))
            return false;

        // 资源打整包
        string sGroupName = "data";
        string[] sourceDirs = { "Assets/DataBundle/_Package/" };
        string[] sPatterns = { "*.bytes" };
        string[] files = GetFiles(sourceDirs, sPatterns, false);
        int nFileLimit = 16 * 1024 * 1024;
        string[] sPacgkageFiles = PackageFiles(sGroupName, files, sWcrDir, nFileLimit);
        if (Directory.Exists(ms_sBundleDir))
            Directory.Delete(ms_sBundleDir, true);
        if (sPacgkageFiles == null || sPacgkageFiles.Length < 1)
        {
            Debug.LogError("Package Failed!");
            return false;
        }

        // 记录资源包文件列表到json文件
        JsonData jd = new JsonData();
        jd.SetJsonType(JsonType.Array);
        System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] bt;
        string sMd5 = "";
        for (int i = 0; i < sPacgkageFiles.Length; i++)
        {
            bt = File.ReadAllBytes(sWcrDir + sPacgkageFiles[i]);
            sMd5 = BitConverter.ToString(md5.ComputeHash(bt));
            //jd[sPacgkageFiles[i]] = sMd5;
            JsonData jdd = new JsonData();
            jdd.SetJsonType(JsonType.Object);
            jdd["file"] = sPacgkageFiles[i];
            jdd["md5"] = sMd5;
            jdd["size"] = (bt.Length / 1024.0f / 1024.0f).ToString();
            jd.Add(jdd);
        }
        md5.Clear();
        Util.writeJson(jd, sWcrDir + ms_sPackFileList);

        // 刷新Unity3D视图里面的资源
        AssetDatabase.Refresh();
        return true;
    }

    #endregion

    #region 更新包

    // 生成补丁包
    public static void GenPatcher(string oldABPath, string newABPath, string wcpPath, bool bIgnoreMap = false)
    {
        Debug.Log("GenPatcher: " + wcpPath + "\n" + oldABPath + "\n" + newABPath + "\n" + bIgnoreMap);
        if (string.IsNullOrEmpty(oldABPath) || string.IsNullOrEmpty(newABPath) || string.IsNullOrEmpty(wcpPath))
        {
            Debug.LogError("GenPatcher Path Error!!!");
            return;
        }

        if (Directory.Exists(wcpPath))
            Directory.Delete(wcpPath, true);
        if (File.Exists(wcpPath + ".info"))
            File.Delete(wcpPath + ".info");

        //文件路径处理
        oldABPath = oldABPath.Replace("\\", "/");
        if (!oldABPath.EndsWith("/")) oldABPath += "/";
        newABPath = newABPath.Replace("\\", "/");
        if (!newABPath.EndsWith("/")) newABPath += "/";

        //获取路径下面的所有新老文件
        string[] newFiles = GetFiles(new string[] { newABPath }, new string[] { "*" + GameConfigMgr.ms_sABType }, false);
        string[] oldFiles = GetFiles(new string[] { oldABPath }, new string[] { "*" + GameConfigMgr.ms_sABType }, false);

        //老文件
        List<string> lstOld = new List<string>(oldFiles);

        //MD5码生成器
        System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();

        //需要删除的老文件
        List<string> lstDel = new List<string>();
        //查找老的资源，把需要删除的老资源找出来
        int tptalCount = lstOld.Count;
        for (int i = lstOld.Count - 1; i >= 0; --i)
        {
            string sOldFile = lstOld[i].Substring(oldABPath.Length);
            bool bDel = true;
            for (int j = 0; j < newFiles.Length; j++)
            {
                string sNewFile = newFiles[j].Substring(newABPath.Length);
                if (sOldFile == sNewFile)
                {
                    bDel = false;
                    break;
                }
            }

            //需要被删除
            if (bDel)
            {
                lstDel.Add(sOldFile);
                //老的文件有，新文件没有的，可以直接剔除，后面不再做比较
                lstOld.RemoveAt(i);
            }

            float fProgress = ((tptalCount - i) * 0.5f) / tptalCount;
            EditorUtility.DisplayProgressBar("Patcher Del", sOldFile + "    " + fProgress.ToString(), fProgress);
        }

        //需要新增和更新的文件
        List<string> lstDiff = new List<string>();
        //查找需要更新和新增的资源
        for (int i = 0; i < newFiles.Length; i++)
        {
            string sNewFile = newFiles[i];
            if (bIgnoreMap && sNewFile.StartsWith(newABPath + GameConfigMgr.ms_sMapFolderName + "/"))
            {
                continue;
            }

            string fileNew = sNewFile.Substring(newABPath.Length);
            bool same = false;
            for (int j = lstOld.Count - 1; j >= 0; --j)
            {
                string fileOld = lstOld[j].Substring(oldABPath.Length);
                if (fileOld.Equals(fileNew))
                {
                    FileInfo fiNew = new FileInfo(sNewFile);
                    FileInfo fiOld = new FileInfo(lstOld[j]);
                    if (fiNew.Length == fiOld.Length)
                    {
                        string sMD5New = System.BitConverter.ToString(md5.ComputeHash(File.ReadAllBytes(sNewFile)));
                        string sMD5Old = System.BitConverter.ToString(md5.ComputeHash(File.ReadAllBytes(lstOld[j])));
                        if (sMD5New.Equals(sMD5Old))
                            same = true;
                    }
                    break;
                }
            }
            //需要新增和更新的文件
            if (same == false)
                lstDiff.Add(fileNew);

            float fProgress = 0.5f + i * 0.5f / newFiles.Length;
            EditorUtility.DisplayProgressBar("Patcher Add/Change", sNewFile + "    " + fProgress.ToString(), fProgress);
        }
        //md5.Clear();
        EditorUtility.ClearProgressBar();

        // Lua相关原始文件保存的位置
        string oldVersion = oldABPath.Remove(oldABPath.LastIndexOf("/"));
        oldVersion = oldVersion.Remove(0, oldVersion.LastIndexOf("/") + 1);
        string oldLuaPath = oldABPath + "../Lua/" + oldVersion + "/";
        string newVersion = newABPath.Remove(newABPath.LastIndexOf("/"));
        newVersion = newVersion.Remove(0, newVersion.LastIndexOf("/") + 1);
        string newLuaPath = newABPath + "../Lua/" + newVersion + "/";

        //获取路径下面的所有新老文件
        string[] newLuaFiles = GetFiles(new string[] { newLuaPath }, new string[] { "*" + GameConfigMgr.ms_sABType }, false);
        string[] oldLuaFiles = GetFiles(new string[] { oldLuaPath }, new string[] { "*" + GameConfigMgr.ms_sABType }, false);

        //查找需要更新和新增的Lua相关文件
        for (int i = 0; i < newLuaFiles.Length; i++)
        {
            string sNewFile = newLuaFiles[i];
            string fileNew = sNewFile.Substring(newLuaPath.Length);
            for (int j = oldLuaFiles.Length - 1; j >= 0; --j)
            {
                string fileOld = oldLuaFiles[j].Substring(oldLuaPath.Length);
                if (fileOld.Equals(fileNew))
                {
                    FileInfo fiNew = new FileInfo(sNewFile);
                    FileInfo fiOld = new FileInfo(oldLuaFiles[j]);
                    if (fiNew.Length == fiOld.Length)
                    {
                        string sMD5New = System.BitConverter.ToString(md5.ComputeHash(File.ReadAllBytes(sNewFile)));
                        string sMD5Old = System.BitConverter.ToString(md5.ComputeHash(File.ReadAllBytes(oldLuaFiles[j])));
                        if (sMD5New.Equals(sMD5Old))
                            lstDiff.Remove(fileNew.ToLower() + GameConfigMgr.ms_sABType);
                    }
                    break;
                }
            }

            float fProgress = 0.5f + i * 0.5f / newLuaFiles.Length;
            EditorUtility.DisplayProgressBar("Patcher Lua Add/Change", sNewFile + "    " + fProgress.ToString(), fProgress);
        }
        md5.Clear();
        EditorUtility.ClearProgressBar();

        Debug.Log("更新资源列表:");
        for (int i = 0; i < lstDiff.Count; ++i)
        {
            Debug.Log(lstDiff[i]);
        }

        Debug.Log("删除资源列表:");
        for (int i = 0; i < lstDel.Count; ++i)
        {
            Debug.Log(lstDel[i]);
        }

        //把需要更新和新增的文件拷贝到Unity工程目录下
        FlatDir(newABPath, ms_sBundleDir + "_Patcher/", lstDiff.ToArray());
        //需要删除和更新的资源对应表
        JsonData jddel = new JsonData();
        jddel.SetJsonType(JsonType.Array);
        for (int i = 0; i < lstDel.Count; i++)
        {
            jddel.Add(lstDel[i]);
        }
        JsonData jddif = new JsonData();
        jddif.SetJsonType(JsonType.Array);
        for (int i = 0; i < lstDiff.Count; i++)
        {
            jddif.Add(lstDiff[i]);
        }
        JsonData jd = new JsonData();
        jd.SetJsonType(JsonType.Object);
        jd["dir"] = oldABPath + "--" + newABPath;
        jd["del"] = jddel;
        jd["dif"] = jddif;
        Util.writeJson(jd, ms_sBundleDir + "_Patcher/" + GameConfigMgr.ms_sPatcherWcrLst);

        // 资源打整包
        string[] files = GetFiles(new string[] { ms_sBundleDir + "_Patcher/" }, new string[] { "*.bytes" }, true);
        BundleFiles(Path.GetDirectoryName(wcpPath), files, m_theAssetOptions, false, true, Path.GetFileName(wcpPath));
        Util.writeJson(jd, wcpPath + ".info");
        if (Directory.Exists(ms_sBundleDir))
            Directory.Delete(ms_sBundleDir, true);

        // 刷新Unity3D视图里面的资源
        AssetDatabase.Refresh();
        // 强制垃圾回收
        GC.Collect();

        EditorUtility.DisplayDialog("Build", "Success!", "OK");

        #region 根据hash码来

        /*
        Debug.Log("GenPatcher: " + wcpPath + "\n" + oldABPath + "\n" + newABPath + "\n" + bIgnoreMap);
        if (string.IsNullOrEmpty(oldABPath) || string.IsNullOrEmpty(newABPath) || string.IsNullOrEmpty(wcpPath))
        {
            Debug.LogError("GenPatcher Path Error!!!");
            return;
        }

        //文件路径处理
        oldABPath = oldABPath.Replace("\\", "/");
        if (!oldABPath.EndsWith("/")) oldABPath += "/";
        newABPath = newABPath.Replace("\\", "/");
        if (!newABPath.EndsWith("/")) newABPath += "/";

        string cfgMF = "CfgMF.manifest.bytes";
        string dataMF = "DataMF.manifest.bytes";
        string luaMF = "LuaMF.manifest.bytes";

        List<string> maniFest = new List<string>();
        maniFest.Add(cfgMF);
        maniFest.Add(dataMF);
        maniFest.Add(luaMF);

        //需要删除的老文件
        List<string> delFiles = new List<string>();
        //需要新增和更新的文件, 新名字 -> 老名字 ,老名字为null的文件是新增
        Dictionary<string, string> diffFiles = new Dictionary<string, string>();
        for (int i = 0; i < maniFest.Count; ++i)
        {
            //获取相关文件
            AssetBundle oldManifestAB = AssetBundle.LoadFromFile(oldABPath + maniFest[i]);
            AssetBundleManifest oldManifest = null;
            string[] oldFiles = null;
            if (oldManifestAB != null)
            {
                oldManifest = (AssetBundleManifest)oldManifestAB.LoadAsset("AssetBundleManifest");
                oldFiles = oldManifest.GetAllAssetBundles();
            }

            AssetBundle newManifestAB = AssetBundle.LoadFromFile(newABPath + maniFest[i]);
            AssetBundleManifest newManifest = null;
            string[] newFiles = null;
            if (newManifestAB != null)
            {
                newManifest = (AssetBundleManifest)newManifestAB.LoadAsset("AssetBundleManifest");
                newFiles = newManifest.GetAllAssetBundles();
            }

            //查找是否需要删除
            for (int oi = 0; oi < oldFiles.Length; ++oi)
            {
                string oldName = oldFiles[oi];
                Hash128 oldHash = oldManifest.GetAssetBundleHash(oldName);
                string oldNameEx = oldName.Remove(oldName.IndexOf(oldHash.ToString()));
                bool doFind = false;
                for (int ni = 0; ni < newFiles.Length; ++ni)
                {
                    string newName = newFiles[ni];
                    Hash128 newHash = newManifest.GetAssetBundleHash(newName);

                    //hash值和名字全部相同 || hash值不同,或者名字不同，但是去除hash值之后的名字相同,文件有修改
                    if (oldHash.Equals(newHash) && oldName.Equals(newName) || oldNameEx.Equals(newName.Remove(newName.IndexOf(newHash.ToString()))))
                    {
                        doFind = true;
                        break;
                    }
                }

                if (!doFind)
                    delFiles.Add(oldName);

                float fProgress = (oi * 0.5f) / oldFiles.Length;
                EditorUtility.DisplayProgressBar("Patcher Del", oldName + "    " + fProgress.ToString(), fProgress);
            }

            //查找新增和修改
            for (int ni = 0; ni < newFiles.Length; ++ni)
            {
                string newName = newFiles[ni];
                Hash128 newHash = newManifest.GetAssetBundleHash(newName);
                string newNameEx = newName.Remove(newName.IndexOf(newHash.ToString()));
                bool doSame = false;
                string name = null;
                for (int oi = 0; oi < oldFiles.Length; ++oi)
                {
                    string oldName = oldFiles[oi];
                    Hash128 oldHash = oldManifest.GetAssetBundleHash(oldName);

                    //hash值和名字全部相同
                    if (oldHash.Equals(newHash) && oldName.Equals(newName))
                    {
                        doSame = true;
                        break;
                    }
                    //hash值不同,或者名字不同，但是去除hash值之后的名字相同,文件有修改
                    else if (newNameEx.Equals(oldName.Remove(oldName.IndexOf(oldHash.ToString()))))
                    {
                        name = oldName;
                    }
                }

                if (!doSame)
                    diffFiles.Add(newName, name);

                float fProgress = 0.5f + ni * 0.5f / newFiles.Length;
                EditorUtility.DisplayProgressBar("Patcher Add/Changge", newName + "    " + fProgress.ToString(), fProgress);
            }

            newManifestAB.Unload(true);
            oldManifestAB.Unload(true);
        }

        List<string> addFiles = new List<string>(diffFiles.Keys);
        //把需要更新和新增的文件拷贝到Unity工程目录下
        FlatDir(newABPath, ms_sResrcDataDir + "_Patcher/", addFiles.ToArray());

        //需要删除和更新的资源对应表
        JsonData jddel = new JsonData();
        jddel.SetJsonType(JsonType.Array);
        for (int i = 0; i < delFiles.Count; i++)
        {
            jddel.Add(delFiles[i]);
        }
        JsonData jddif = new JsonData();
        jddif.SetJsonType(JsonType.Array);
        for (int i = 0; i < addFiles.Count; i++)
        {
            JsonData dif = new JsonData();
            dif[addFiles[i]] = diffFiles[addFiles[i]];
            jddif.Add(dif);
        }
        JsonData jd = new JsonData();
        jd.SetJsonType(JsonType.Object);
        jd["dir"] = oldABPath + "--" + newABPath;
        jd["del"] = jddel;
        jd["dif"] = jddif;
        Util.writeJson(jd, ms_sResrcDataDir + "_Patcher/" + GameConfigMgr.ms_sPatcherWcrLst);

        // 资源打整包
        BuildAssetBundleOptions theAssetOptions = BuildAssetBundleOptions.ForceRebuildAssetBundle;
        //theAssetOptions = theAssetOptions | BuildAssetBundleOptions.CompleteAssets;
        //theAssetOptions = theAssetOptions | BuildAssetBundleOptions.CollectDependencies;
        theAssetOptions = theAssetOptions | BuildAssetBundleOptions.UncompressedAssetBundle;
        theAssetOptions = theAssetOptions | BuildAssetBundleOptions.DeterministicAssetBundle;
        string[] files = GetFiles(new string[] { ms_sResrcDataDir + "_Patcher/" }, new string[] { "*.bytes" }, true);
        BundleFiles(wcpPath, files, theAssetOptions, false, true);
        Util.writeJson(jd, wcpPath + ".info");
        //if (Directory.Exists(ms_sResrcDataDir))
            //Directory.Delete(ms_sResrcDataDir, true);

        // 刷新Unity3D视图里面的资源
        AssetDatabase.Refresh();
        // 强制垃圾回收
        GC.Collect();
        */

        #endregion
    }

    #endregion

    #endregion

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
                EditorUtility.DisplayProgressBar("CopyFiles: " + srcDir, Math.Round(fProgress, 3).ToString() + ":" + files[i].Replace(srcDir, ""), fProgress);
            }
            string dstFile = dstDir + files[i].Replace("\\", "/").Substring(n);
            string sDir = dstFile.Substring(0, dstFile.LastIndexOf('/'));

            if (!Directory.Exists(sDir))
                Directory.CreateDirectory(sDir);
            File.Copy(files[i], dstFile, true);
        }

        return true;
    }

    // 最终生成App。string[] scenes = { "Assets/Launcher.unity", "Assets/Empty.unity" };
    public static bool BuildAPP(string sOutDir, BuildOptions buildOptions = BuildOptions.None, string[] scenes = null, bool bDelOldOutDir = true)
    {
        System.DateTime lastTime = System.DateTime.Now;
        string sOutPath = sOutDir + ms_target.ToString() + "/";
        if (bDelOldOutDir && Directory.Exists(sOutPath))
            Directory.Delete(sOutPath, true);
        Directory.CreateDirectory(sOutPath);

        //处理路径，防止包含Assets报错
        while (sOutPath.IndexOf("/..") != -1)
        {
            string sFront = sOutPath.Remove(sOutPath.IndexOf("/.."));
            string sEnd = sOutPath.Remove(0, sOutPath.IndexOf("/..") + 3);

            sOutPath = sFront.Remove(sFront.LastIndexOf("/")) + sEnd;
        }

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

    public static bool BuildForPack(string sNewVersion, BuildTarget buildTarget, BuildOptions buildOptions, GenOptions genOptions)
    {
        /*
        BuildOptions buildOptions = BuildOptions.None;  // release
        if (bBuildDebug)
            buildOptions = BuildOptions.Development | BuildOptions.ConnectWithProfiler | BuildOptions.AllowDebugging;   // debug
        //*/
        bool bGenVersion = (genOptions & GenOptions.bGenVersion) != 0;
        bool bBundleLua = (genOptions & GenOptions.bBundleLua) != 0;
        bool bBundleData = (genOptions & GenOptions.bBundleUI) != 0;
        bool bBundleMap = (genOptions & GenOptions.bBundleMap) != 0;
        //bool bBundleChar = (genOptions & GenOptions.bBundleChar) != 0;
        //bool bBundleObj = (genOptions & GenOptions.bBundleObj) != 0;
        //bool bBundleEff = (genOptions & GenOptions.bBundleEff) != 0;
        //bool bBundleMedia = (genOptions & GenOptions.bBundleMedia) != 0;
        bool bPackage = (genOptions & GenOptions.bPackage) != 0;
        bool bBuildApp = (genOptions & GenOptions.bBuildApp) != 0;
        bool bCopyBundle = (genOptions & GenOptions.bCopyBundle) != 0;
        bool bCopyWinRsrc = (genOptions & GenOptions.bCopyWinRsrc) != 0;

        // 打AB包
        if (!BundAll(sNewVersion, bGenVersion, bBundleLua, bBundleData, bBundleMap))
            return false;

        // 打整包
        if (bPackage && !Package())
            return false;

        // 拷贝资源到编辑器，用于打包App
        string sOutDir = AddStamp(ms_sDataOutDir);
        if (bCopyBundle && !CopyFiles(sOutDir, ms_sResrcDataDir, new string[] { "*" + ms_sABFileType, /*"*.manifest"*/ }, true))
            return false;

        AssetDatabase.Refresh();
        // 最终生成
        if (bBuildApp && !BuildAPP(ms_sPublicDir, buildOptions, null))
            return false;

        if (Directory.Exists(ms_sResrcDataDir))
            Directory.Delete(ms_sResrcDataDir, true);
        if (File.Exists(ms_sResrcDataDir + ".meta"))
            File.Delete(ms_sResrcDataDir + ".meta");

        AssetDatabase.Refresh();

        // windows下拷贝资源和脚本
        if (buildTarget == BuildTarget.StandaloneWindows && bCopyWinRsrc)
        {
            //CopyScript();
            //CopyDir(GamePackage.ms_sBundleDir, sOutDir + "Windows/nfxy_Data/DataBundle/", new string[] { GamePackage.ms_sABFileType });
        }

        Debug.Log("BuildForPack Success!");
        return true;
    }

    public static bool Build(string sNewVersion, BuildTarget buildTarget, BuildOptions buildOptions, GenOptions genOptions, bool bUnInitData = false)
    {
        bool bOK = false;
        try
        {
            bOK = BuildForPack(sNewVersion, buildTarget, buildOptions, genOptions); //散包
        }
        catch (System.Exception ex)
        {
            bOK = false;
            Debug.LogError(ex.Message + "\n" + ex.StackTrace);
        }

        if (bOK)
            EditorUtility.DisplayDialog("Build", "Success!", "OK");
        else
        {
            EditorUtility.DisplayDialog("Build", "Failed!!!", "X");
        }

        System.GC.Collect();
        // 刷新Unity3D视图里面的资源
        AssetDatabase.Refresh();

        return bOK;
    }
}
