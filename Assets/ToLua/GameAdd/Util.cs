using UnityEngine;
using System.Collections.Generic;
using System.IO;
using LitJson;
using WCG;

public class Util
{
	public static void writeJson(JsonData data, string path)
	{
		string json = JsonMapper.ToJson(data);
		FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
		byte[] bt;
		bt = System.Text.Encoding.UTF8.GetBytes(json);
		fs.Write(bt, 0, bt.Length);
		fs.Flush();
		fs.Close();
	}
	public static JsonData readJson(string path)
	{
		byte[] bt = File.ReadAllBytes(path);
		string json = System.Text.Encoding.Default.GetString(bt);
		JsonData data = JsonMapper.ToObject(json);
		return data;
	}
	public static string GetLuaPath(string name, string ext = ".lua")
	{
		return LuaCsvConfig.GetLuaFullPath(name, ext);
	}
	public static byte[] GetLuaBytes(string fullpath)
	{
		return LuaCsvConfig.GetLuaBytes(fullpath);
	}
}

internal static class LuaCsvConfig
{
	public static RunMode m_eRunMode = GameConfigMgr.ms_eArchive;
	public static List<string> m_LuaPathList = GameConfigMgr.ms_lstLuaPath;
    public static List<string> m_CsvPathList = GameConfigMgr.ms_lstCsvPath;
    private static AssetBundleManifest ms_luaABMF = null;
    private static AssetBundleManifest ms_csvABMF = null;
    public static Dictionary<string, byte[]> md_LuaFileList = new Dictionary<string, byte[]>();
	public static Dictionary<string, byte[]> md_CsvFileList = new Dictionary<string, byte[]>();

	static LuaCsvConfig()
	{
        ms_luaABMF = null;
        ms_csvABMF = null;
        if (m_eRunMode == RunMode.eRelease)
            ReadAllLuaCsvFromUnity();
	}

    public static string GetLuaFullPath(string name, string ext)
	{
		return name.Replace("/", ".").Replace(ext, "") + ext; ;
	}

    public static byte[] GetLuaBytes(string fullpath)
	{
		byte[] str = null;
		if (string.IsNullOrEmpty(fullpath))
			return str;

		string sLuaFileKey = fullpath.ToLower();
		if (md_LuaFileList.TryGetValue(sLuaFileKey, out str))
			md_LuaFileList.Remove(sLuaFileKey);

		if (str == null)
		{
			Debug.LogError("GetLuaCsvBytes Exception:" + fullpath);
            str = new byte[0];
		}

		return str;
	}

    private static void ReadAllLuaCsvFromUnity ()
	{
        //整包模式 WQ_TODO

        //散包模式
        ms_luaABMF = (AssetBundleManifest)ResourceMgr.LoadAsset(GameConfigMgr.ms_sLuaMFileName, "AssetBundleManifest", typeof(AssetBundleManifest));
        if (null == ms_luaABMF) Debug.LogError("Load Failed: " + GameConfigMgr.ms_sLuaMFileName);
        string[] LuafilesArr = ms_luaABMF.GetAllAssetBundles();
		for(int i=0; i<LuafilesArr.Length; ++i)
		{
			string sABPath = LuafilesArr.GetValue(i).ToString();
			string sABName = Path.GetFileNameWithoutExtension(sABPath).Replace(GameConfigMgr.ms_sABType, "");
			sABPath = sABPath.Substring(0, sABPath.LastIndexOf(GameConfigMgr.ms_sABType));
			TextAsset oText = ResourceMgr.LoadAsset(sABPath, sABName, typeof(TextAsset)) as TextAsset;
			if (oText == null) Debug.LogError("Load Failed: " + sABPath);
			md_LuaFileList.Add(sABName.Replace(GameConfigMgr.ms_sABType, ""), oText.bytes);
		}
        ResourceMgr.UnLoadAsset(ms_luaABMF);
		ms_luaABMF = null;

		ms_csvABMF = (AssetBundleManifest)ResourceMgr.LoadAsset(GameConfigMgr.ms_sCsvMFileName, "AssetBundleManifest", typeof(AssetBundleManifest));
		if (null == ms_csvABMF) Debug.LogError("Load Failed: " + GameConfigMgr.ms_sCsvMFileName);
		string[] CsvfilesArr = ms_csvABMF.GetAllAssetBundles();
		for (int i = 0; i < CsvfilesArr.Length; ++i)
		{
			string sABPath = CsvfilesArr.GetValue(i).ToString();
			string sABName = Path.GetFileNameWithoutExtension(sABPath).Replace(GameConfigMgr.ms_sABType, "");
			sABPath = sABPath.Substring(0, sABPath.LastIndexOf(GameConfigMgr.ms_sABType));
			TextAsset oText = ResourceMgr.LoadAsset(sABPath, sABName, typeof(TextAsset)) as TextAsset;
			if (oText == null) Debug.LogError("Load Failed: " + sABPath);
			md_LuaFileList.Add(sABName.Replace(GameConfigMgr.ms_sABType, ""), oText.bytes);
		}
		ResourceMgr.UnLoadAsset(ms_csvABMF);
		ms_csvABMF = null;
	}
}