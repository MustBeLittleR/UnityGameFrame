using UnityEngine;
using UnityEditor;
using System.Text;
using System.Collections.Generic;
using System.IO;
using WCG;

namespace WCTool
{    
	public static class WCTool
	{
		[MenuItem("GameObject/CopyHierarchy %#c", false,12)]
		static void CopyHierarchy()
		{
			GameObject go = Selection.activeGameObject;
			if (go == null)
			{
				Debug.Log("请选择Hierarchy物体");
				return;
			}
			string path = go.name;
			while (go.transform.parent != null && go.transform.parent.parent != null && go.transform.parent.parent.parent)
			{
				go = go.transform.parent.gameObject;
				path = go.name + "/" + path;
			}
			Debug.Log(path);
			TextEditor te = new TextEditor();//很强大的文本工具
			te.text = path;
			te.OnFocus();
			te.Copy();
		}

        //[MenuItem("WCTool/AlterAssets")]
        static void AlterAssets()
        {
            string[] uiFiles = AppPackage.GetFiles(new string[] { GameConfigMgr.ms_sDataDir + GameConfigMgr.ms_sDataFolderName + "/" + GameConfigMgr.ms_sUIFolderName + "/" }, new string[] { "*.prefab"}, true);
            for (int i = 0; i < uiFiles.Length; i++)
            {
                GameObject assObj = AssetDatabase.LoadAssetAtPath(uiFiles[i], typeof(GameObject)) as GameObject;
                Debug.LogWarningFormat("--------- Check GameObject CanvasRenderer: {0}", uiFiles[i]);

                Component[] coms = assObj.GetComponents(typeof(Component));
                if (coms.Length == 2)
                {
                    if ((coms[0].GetType() == typeof(RectTransform) && coms[1].GetType() == typeof(CanvasRenderer)) || 
                        ((coms[0].GetType() == typeof(CanvasRenderer) && coms[1].GetType() == typeof(RectTransform))))
                    {
                        CanvasRenderer crcom = null;
                        if (coms[0].GetType() == typeof(CanvasRenderer))
                            crcom = coms[0] as CanvasRenderer;
                        else
                            crcom = coms[1] as CanvasRenderer;

                        GameObject.DestroyImmediate(crcom, true);
                        Debug.LogWarningFormat("This Node Only Component(CanvasRenderer) {0}", assObj.name);
                    }
                }
                RemoveComponent(assObj);
            }
        }

        static void RemoveComponent(GameObject obj)
        {
            for (int j = 0; j < obj.transform.childCount; j++)
            {
                GameObject node = obj.transform.GetChild(j).gameObject;
                RemoveComponent(node);

                Component[] coms = node.GetComponents(typeof(Component));
                if (coms.Length == 2)
                {
                    if ((coms[0].GetType() == typeof(RectTransform) && coms[1].GetType() == typeof(CanvasRenderer)) ||
                        ((coms[0].GetType() == typeof(CanvasRenderer) && coms[1].GetType() == typeof(RectTransform))))
                    {
                        CanvasRenderer crcom = null;
                        if (coms[0].GetType() == typeof(CanvasRenderer))
                            crcom = coms[0] as CanvasRenderer;
                        else
                            crcom = coms[1] as CanvasRenderer;

                        GameObject.DestroyImmediate(crcom, true);
                        Debug.LogWarningFormat("This Node Only Component(CanvasRenderer) {0}", node.name);
                    }
                }
            }
        }

        //[MenuItem("WCTool/CheckSyncAsyn")]
        static void HandleAsynAssts()
        {
            string fullPath = "E:/U3D_Workspace/DGQB_Demo/Public/StandaloneWindows/DataAB/DataMF.manifest.bytes";
            if (File.Exists(fullPath))
            {
                FileStream fileStream = new FileStream("C:/Users/wenqiang/Desktop/LoadResLog.txt", FileMode.Create, FileAccess.Write, FileShare.Read);
                Resources.UnloadUnusedAssets();

                AssetBundle ab2 = AssetBundle.LoadFromFile(fullPath);
                AssetBundleManifest assMF = ab2.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                string[] as1 = assMF.GetAllAssetBundles();
                Dictionary<string, List<string>> depABs = new Dictionary<string, List<string>>();
                for (int i = 0; i < as1.Length; i++)
                {
                    string abNa = as1[i];
                    string[] listDep = assMF.GetAllDependencies(abNa);
                    for (int j = 0; j < listDep.Length; j++)
                    {
                        string sDep = listDep[j];
                        if (!depABs.ContainsKey(sDep))
                            depABs.Add(sDep, new List<string>());

                        List<string> depList = depABs[sDep];
                        if (!depList.Contains(abNa))
                            depList.Add(abNa);
                    }
                }

                int nTag = 0;
                Dictionary<string, List<string>>.Enumerator iter = depABs.GetEnumerator();
                while (iter.MoveNext())
                {
                    bool bChar = false;
                    bool bOther = false;
                    StringBuilder ms_sb = new StringBuilder();
                    List<string>.Enumerator iterEx = iter.Current.Value.GetEnumerator();
                    while (iterEx.MoveNext())
                    {
                        if (iterEx.Current.StartsWith("characters"))
                            bChar = true;
                        else
                            bOther = true;
                        ms_sb.AppendLine(iterEx.Current);
                    }
                    iterEx.Dispose();

                    if (bChar && bOther)
                    {
                        nTag++;
                        ms_sb.AppendLine("--->  " + iter.Current.Key);
                        ms_sb.AppendFormat("-------------------{0}-----------------------\n\n", nTag);

                        string msg = ms_sb.ToString();
                        fileStream.Write(Encoding.UTF8.GetBytes(msg), 0, msg.Length);
                        fileStream.Flush();
                    }
                }
                iter.Dispose();

                ab2.Unload(true);
                fileStream.Close();
                fileStream = null;
                Debug.LogWarning("--HandleAsynAssts Over--");
            }
        }
    }
}
