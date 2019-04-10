using UnityEngine;
using UnityEngine.Events;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;
using System.Text;
using UnityEngine.UI;

namespace WCG
{
    #region AssetObject Cache
    public class AssetObj
    {
        public int mCount = 0;
        public string mPath = null;
        public string mName = null;
        public string mExte = null;
        public AssetBundle mAB = null;
        Dictionary<string, UnityEngine.Object> m_dicAssObj = new Dictionary<string, UnityEngine.Object>();

        public AssetObj(string path, string name, AssetBundle ab)
        {
            mPath = path;
            mName = name;
            mAB = ab;
            mExte = Path.GetExtension(path);
        }
        public void SyncLoad()
        {
            if (null == mAB) return;

            if (mExte.Equals(".prefab"))
            {
                GameObject assObj = mAB.LoadAsset<GameObject>(mName);
                ResourceMgr.Instance.AddAssObjToCache(mPath, mName, assObj, null);
            }
            else if (mExte.Equals(".ogg") || mExte.Equals(".mp3"))
            {
                AudioClip assObj = mAB.LoadAsset<AudioClip>(mName);
                ResourceMgr.Instance.AddAssObjToCache(mPath, mName, assObj, null);
            }
            else if (mExte.Equals(".png") || mExte.Equals(".jpg") || mExte.Equals(".atlas"))
            {
                Sprite[] objs = mAB.LoadAllAssets<Sprite>();
                for (int i = 0; i < objs.Length; i++)
                {
                    Sprite assObj = objs[i];
                    string name = assObj.name;
                    ResourceMgr.Instance.AddAssObjToCache(mPath, name, assObj, null);
                }
            }
            else if (mExte.Equals(".manifest"))
            {
                AssetBundleManifest assObj = mAB.LoadAsset<AssetBundleManifest>(mName);
                ResourceMgr.Instance.AddAssObjToCache(mPath, mName, assObj, null);
            }
            else if (mExte.Equals(".rendertexture"))
            {
                RenderTexture assObj = mAB.LoadAsset<RenderTexture>(mName);
                ResourceMgr.Instance.AddAssObjToCache(mPath, mName, assObj, null);
            }
            else if (mExte.Equals(".bytes"))
            {
                TextAsset assObj = mAB.LoadAsset<TextAsset>(mName);
                ResourceMgr.Instance.AddAssObjToCache(mPath, mName, assObj, null);
            }
            else if (mExte.Equals(".unity") || mExte.Equals(".ttf") || mExte.Equals(".json"))
            {
            }
            else
                Debug.AssertFormat(false, "AssetObjAB OnLoad Error: {0}, {1}", mPath, mName);

            //UnLoadAB(false);
        }
        public void PushAssObj(string name, UnityEngine.Object assObj)
        {
            if (!m_dicAssObj.ContainsKey(name))
                m_dicAssObj.Add(name, assObj);
            else
                Debug.LogWarningFormat("AssetObj Load Repeat! {0}, {1}", mPath, name);
        }
        public void UnLoadAll()
        {
            mCount = 0;
            Dictionary<string, UnityEngine.Object>.Enumerator iter = m_dicAssObj.GetEnumerator();
            while (iter.MoveNext())
            {
                Type assType = iter.Current.Value.GetType();
                // UnloadAsset may only be used on individual assets and can not be used on GameObject's / Components or AssetBundles
                if (assType == ResourceMgr.ms_tyGameObject || assType == ResourceMgr.ms_tyComponent || assType == ResourceMgr.ms_tyAssetBundle)
                {
                    ;//UnityEngine.Object.DestroyImmediate(m_assObj, true);
                }
                else
                {
                    Resources.UnloadAsset(iter.Current.Value);
                }
            }
            iter.Dispose();
            m_dicAssObj.Clear();

            UnLoadAB(true);
            //Resources.UnloadUnusedAssets();
        }
        public void UnLoadAB(bool unloadAllLoadedObjects=false)
        {
            if (null != mAB)
            {
                mAB.Unload(unloadAllLoadedObjects);
                mAB = null;
            }
        }
        public UnityEngine.Object GetAssObj(string name, int nAddCount)
        {
            UnityEngine.Object assObj = null;
            if (m_dicAssObj.TryGetValue(name, out assObj))
            {
                AddCount(nAddCount);
                return assObj;
            }
            return null;
        }
        public void AddCount(int i)
        {
            mCount += i;
        }
    }
    #endregion AssetObject Cache

    #region Async Item
    public sealed class BatchItem
    {
        public string mPath;
        public string mName;
        public System.Type mType;
        public UnityAction<UnityEngine.Object> mAction;

        public BatchItem(string path, string name, System.Type type, UnityAction<UnityEngine.Object> action)
        {
            mPath = path;
            mName = name;
            mType = type;
            mAction = action;
        }
    }
    #endregion Async Item

    #region InsItem
    public class InsItem
    {
        public string mPath;
        public int mAssId;
        public string mName;
        public InsItem(string path, int nAssId, string name)
        {
            if (!path.EndsWith(".prefab"))
                Debug.LogWarningFormat("only prefab can instance:{0}", path);

            mPath = path;
            mAssId = nAssId;
            mName = name;
        }
    }
    #endregion InsItem

    #region ABAsync Load
    public class AsynAB
    {
        public string mPath;
        public AssetBundleCreateRequest mABCReq;
        public UnityAction<string, AssetBundle> mCall;
        public AsynAB(string path, AssetBundleCreateRequest abReq, UnityAction<string, AssetBundle> call)
        {
            mPath = path;
            mABCReq = abReq;
            mCall = call;
        }
        public void OnLoadABOk()
        {
            mCall(mPath, mABCReq.assetBundle);
        }
        public bool IsDone()
        {
            return mABCReq.isDone;
        }
    }
    #endregion ABAsync Load

    #region MulABAsync Load
    public class AsynABMul
    {
        public string mPath;
        public UnityAction<AssetBundle> mCall;

        int nNeedNum = 0;
        int nLoadNum = 0;
        AssetBundle mAB = null;

        public AsynABMul(string path, UnityAction<AssetBundle> call)
        {
            mPath = path;
            mCall = call;
        }
        public void AsynLoad()
        {
            nNeedNum = 0;
            nLoadNum = 0;

            string[] files = ResourceMgr.Instance.GetAllDependencies(mPath);
            if (null != files)
            {
                nNeedNum = files.Length;
                for (int i = 0; i < nNeedNum; i++)
                {
                    string pathEx = files[i].Substring(0, files[i].LastIndexOf("."));
                    if (ResourceMgr.Instance.AsynABIsLoaded(pathEx))
                        nLoadNum++;
                    else if (!ResourceMgr.Instance.AsynABIsLoading(pathEx, OnLoadABOk))
                        ResourceMgr.Instance.LoadAssetBundleAsync(pathEx, OnSelfLoadABOk);
                }
            }

            nNeedNum++;
            if (ResourceMgr.Instance.AsynABIsLoaded(mPath))
                nLoadNum++;
            else if (!ResourceMgr.Instance.AsynABIsLoading(mPath, OnLoadABOk))
                ResourceMgr.Instance.LoadAssetBundleAsync(mPath, OnSelfLoadABOk);

            //所有的AB已经在Cache中
            if (nLoadNum == nNeedNum)
            {
                mAB = ResourceMgr.Instance.GetAsynABLoaded(mPath);
                if (null == mAB)
                    Debug.LogErrorFormat("AsynABMul.OnLoadABOk Err:{0}", mPath);

                mCall(mAB);
            }
        }
        public void OnSelfLoadABOk(string path, AssetBundle ab)
        {
            ResourceMgr.Instance.OnAsynLoadABOk(path, ab);
            OnLoadABOk(path, ab);
        }
        void OnLoadABOk(string path, AssetBundle ab)
        {
            nLoadNum++;
            if (null == mAB && mPath.Equals(path))
                mAB = ab;

            if (nLoadNum == nNeedNum)
            {
                if (null == mAB)
                    Debug.LogErrorFormat("AsynABMul.OnLoadABOk Err:{0}", mPath);

                mCall(mAB);
            }
        }
    }
    #endregion MulABAsync Load

    #region AsynObj Load
    public class AsynObj
    {
        public string mPath;
        public string mName;
        UnityAction<string, GameObject> mCall = null;
        bool mLoadABOK = false;

        AssetBundleRequest mABReqt = null;
        AsynABMul mAsABMul = null;

        public AsynObj(string path, string name, UnityAction<string, GameObject> call)
        {
            mPath = path;
            mName = name;
            mCall = call;
            mLoadABOK = false;
            mAsABMul = new AsynABMul(path, OnLoadABOk);
        }
        public void AsynLoad()
        {
            mAsABMul.AsynLoad();
        }
        public void OnLoadABOk(AssetBundle ab)
        {
            mLoadABOK = true;
            if (null != ab)
                mABReqt = ab.LoadAssetAsync<GameObject>(mName);
            else
                Debug.LogErrorFormat("AsynObj.OnLoadABOk Err:{0},{1}", mPath, mName);
        }
        public void OnLoadObjOk()
        {
            GameObject assObj = mABReqt.asset as GameObject;
            ResourceMgr.Instance.OnInsAsynAssObj(mCall, mPath, assObj, true);
            ResourceMgr.Instance.OnAsynLoadObjOk(mPath, assObj);
        }
        public bool IsDone()
        {
            if (mLoadABOK)
                return mABReqt.isDone;

            return false;
        }
    }
    #endregion AsynObj Load

    public class ResourceMgr
    {
        public static readonly ResourceMgr Instance = new ResourceMgr();
        private AssetBundleManifest m_abm = null;

        private static string ms_sEditorDataDir = "Assets/Data/";
        private static readonly string ms_sResourcesABDir = GameConfigMgr.ms_sABFolderName + "/";
        private static readonly string ms_sABResrcDir = GameConfigMgr.ms_sResrcDir;

        private Dictionary<int, InsItem> m_dicInsObjCache = null;
        private Dictionary<string, AssetObj> m_dicAssObjCache = null;
        private Dictionary<int, string> m_dicAssPathCache = null;

        //切换场景会全部销毁
        private Dictionary<string, Stack<GameObject>> m_dicCacheInsGameObj = null;
        //不随切换场景而销毁
        private Dictionary<string, Stack<GameObject>> m_dicCacheInsGameObjEx = null;
        //常驻内存资源列表
        private List<int> m_listStaticObj = null;

        private bool m_bLoadBatch = false;
        private List<BatchItem> m_lstBatchItem = null;

        public delegate UnityEngine.Object InstantiateDelegate(UnityEngine.Object goObj);

        public static Type ms_tyGameObject = typeof(GameObject);
        public static Type ms_tyComponent = typeof(Component);
        public static Type ms_tyAssetBundle = typeof(AssetBundle);
        public static Type ms_tySprite = typeof(Sprite);
        public static Type ms_tyABManifest = typeof(AssetBundleManifest);

        //异步加载完成回调
        private LuaFunction m_fOnResAsyncLoaded = null;

        //AB异步加载列表
        private Dictionary<string, AsynAB> m_dicAsynABLoading = null;
        private bool m_bABAsyncLoading = false;
        private int m_nAsynABNum = 0;

        private Dictionary<string, AsynObj> m_dicAsynObjLoading = null;
        private List<string> m_gcListDepSp = new List<string>();
        private List<string> m_gcListDepStr = new List<string>();
        private List<string> m_gcListStr = new List<string>();
        private List<int> m_gcListInt = new List<int>();
        private List<AsynAB> m_gcListAsAB = new List<AsynAB>();
        private List<AsynObj> m_gcListAsObj = new List<AsynObj>();

        private bool m_bObjAsyncLoading = false;
        private int m_nAsynObjNum = 0;

        private Dictionary<string, Stack<UnityAction<string, AssetBundle>>> m_dicAsynABRequest = null;
        private Dictionary<string, Stack<UnityAction<string, GameObject>>> m_dicAsynObjRequest = null;

        //调试辅助参数
        private bool M_DEBUG_MODE = false;
        FileStream m_fileStream = null;
        static StringBuilder ms_sb = new StringBuilder(200);

        private ResourceMgr()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            //M_DEBUG_MODE = true;
            //m_fileStream = new FileStream("C:/Users/wenqiang/Desktop/LoadResLog.txt", FileMode.Create, FileAccess.Write, FileShare.Read);
#endif
            m_dicInsObjCache = new Dictionary<int, InsItem>();
            m_lstBatchItem = new List<BatchItem>();
            m_dicAssObjCache = new Dictionary<string, AssetObj>();
            m_dicAssPathCache = new Dictionary<int, string>();
            m_dicCacheInsGameObj = new Dictionary<string, Stack<GameObject>>();
            m_dicCacheInsGameObjEx = new Dictionary<string, Stack<GameObject>>();
            m_listStaticObj = new List<int>();

            m_dicAsynABLoading = new Dictionary<string, AsynAB>();
            m_dicAsynObjLoading = new Dictionary<string, AsynObj>();
            m_dicAsynABRequest = new Dictionary<string, Stack<UnityAction<string, AssetBundle>>>();
            m_dicAsynObjRequest = new Dictionary<string, Stack<UnityAction<string, GameObject>>>();
        }

        public void Init()
        {
            RunMode eRunMode = GameMgr.Instance.m_eRunMode;
            if (eRunMode != RunMode.eDeveloper)
            {
                LoadABManifest();
            }
        }

        public void InitResLoadCallBack() 
        {
            if (null == m_fOnResAsyncLoaded)
                m_fOnResAsyncLoaded = GameMgr.ms_luaMgr.GetLuaFunction("gResourceMgr.OnResAsyncLoaded");
        }

        public void Destroy()
        {
            if (null != m_abm)
            {
                unLoadAsset(m_abm);
                m_abm = null;
            }

            if (null != m_fileStream)
            {
                m_fileStream.Close();
                m_fileStream = null;
            }
        }

        private void DebugMsg(string msg)
        {
            //Debug.LogWarning("ResMgr->\t" + msg);

            if (null != m_fileStream)
            {
                msg += "\n";
                m_fileStream.Write(Encoding.UTF8.GetBytes(msg), 0, msg.Length);
                m_fileStream.Flush();
            }
        }

        private bool CheckAssetPath(ref string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                /*if (string.IsNullOrEmpty(Path.GetExtension(path)))
                    path = path + ".prefab";*/

                if (string.IsNullOrEmpty(Path.GetExtension(path)))
                {
                    ms_sb.Clear();
                    path = ms_sb.Append(path).Append(".prefab").ToString();
                }

                if (GameMgr.Instance.m_eRunMode != RunMode.eDeveloper)
                    path = path.ToLower();

                return true;
            }
            return false;
        }

        private void LoadABManifest()
        {
            m_abm = (AssetBundleManifest)loadAsset(GameConfigMgr.ms_sABMFileName, "AssetBundleManifest", ms_tyABManifest);
            m_dicAssObjCache[GameConfigMgr.ms_sABMFileName.ToLower()].UnLoadAB(false);
        }

        private string loadVersion()
        {
            string sVersion = "";
            TextAsset ta = Resources.Load<TextAsset>(ms_sResourcesABDir + GameConfigMgr.ms_sVersionFileName);
            if (ta != null)
                sVersion = Encoding.UTF8.GetString(ta.bytes);
            return sVersion;
        }

        #region SyncLoad Or UnLoad AssetObj
        private UnityEngine.Object loadAsset(string path, string name, System.Type type = null, InstantiateDelegate InsAc = null)
        {
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(name))
                return null;

            if (type == null)
                type = typeof(UnityEngine.Object);

            int nAddCount = 0;
            RunMode eRunMode = GameMgr.Instance.m_eRunMode;
            if (eRunMode != RunMode.eDeveloper)
            {
                //Under AB Mode All path Use ToLower()
                path = path.ToLower();
                if (ms_tyABManifest != type)
                    name = name.ToLower();

                /*string pathA = ms_sABResrcDir + path + GameConfigMgr.ms_sABType;
                string pathB = Path.GetDirectoryName(pathA) + ".atlas" + GameConfigMgr.ms_sABType;
                if (pathB.Contains("/atlas/") && File.Exists(pathB))
                    path = Path.GetDirectoryName(path) + ".atlas";*/

                ms_sb.Clear();
                string pathA = ms_sb.Append(ms_sABResrcDir).Append(path).Append(GameConfigMgr.ms_sABType).ToString();
                ms_sb.Clear();
                string pathB = ms_sb.Append(Path.GetDirectoryName(pathA)).Append(".atlas").Append(GameConfigMgr.ms_sABType).ToString();
                if (pathB.Contains("/atlas/") && File.Exists(pathB))
                {
                    ms_sb.Clear();
                    path = ms_sb.Append(Path.GetDirectoryName(path)).Append(".atlas").ToString();
                }
            }

            if (M_DEBUG_MODE)
                DebugMsg(string.Format("\nLoadAsset---1---:\t{0},InsObjNum:{1},AssObjNum:{2}", path, m_dicInsObjCache.Count, m_dicAssObjCache.Count));

            if (eRunMode == RunMode.eDeveloper)
            {
                if (!m_dicAssObjCache.ContainsKey(path))
                    LoadAssetFromEditor(path, name, type);
            }
            else if (eRunMode == RunMode.eDebug || eRunMode == RunMode.eRelease)
            {
                //对已有资源做引用计数
                if (m_dicAssObjCache.ContainsKey(path))
                    nAddCount = 1;

                LoadAssetFromABFile(path, name);
            }

            AssetObj oAssObj = null;
            UnityEngine.Object assObj = null;
            m_dicAssObjCache.TryGetValue(path, out oAssObj);
            if (null != oAssObj)
                assObj = oAssObj.GetAssObj(name, nAddCount);

            if (null == assObj)
            {
                if (!path.EndsWith(".ogg"))
                    Debug.LogWarningFormat("loadAsset error: {0}, {1}", path, name);
                return null;
            }

            if (M_DEBUG_MODE && nAddCount > 0)
                DebugMsg(string.Format("\t+++\t assObj_2:{0}, Name:{1}, RefNum:{2}", path, name, oAssObj.mCount));

            UnityEngine.Object insObj = null;
            int assId = assObj.GetInstanceID();
            Type assType = assObj.GetType();

            //UnloadAsset may only be used on individual assets and can not be used on GameObject's / Components or AssetBundles
            if (assType == ms_tyGameObject || assType == ms_tyComponent || assType == ms_tyAssetBundle)
            {
                CheckAssObjDepMulRef(assType, assObj);

                if (null != InsAc)
                    insObj = InsAc(assObj);
                else
                    insObj = Instantiate(assObj);

                //Cache InsObj
                int insId = insObj.GetInstanceID();
                if (!m_dicInsObjCache.ContainsKey(insId))
                    m_dicInsObjCache.Add(insId, new InsItem(path, assId, name));
                else
                    Debug.Assert(false, "m_dicInsObjCache Error path:" + path + ",insId:" + insId);
            }
            else
            {
                //其他类型就直接引用 AssetObject 即可
                insObj = assObj;
            }

            if (M_DEBUG_MODE)
                DebugMsg(string.Format("LoadAsset---2---:\t{0},InsObjNum:{1},AssObjNum:{2}", path, m_dicInsObjCache.Count, m_dicAssObjCache.Count));

            return insObj;
        }

        //本体对依赖项大于一次依赖引用的处理流程
        private void CheckAssObjDepMulRef(Type type, UnityEngine.Object assObj)
        {
            if (type == ms_tyGameObject)
            {
                GameObject obj = assObj as GameObject;
                if (null != obj)
                {
                    m_gcListDepStr.Clear();
                    Image[] Images = obj.GetComponentsInChildren<Image>(true);
                    for (int i = 0; i < Images.Length; i++)
                    {
                        Image img = Images[i];
                        if (img != null && img.sprite != null)
                        {
                            string path = null;
                            int assId = img.sprite.GetInstanceID();
                            if (m_dicAssPathCache.TryGetValue(assId, out path))
                            {
                                if (m_gcListDepStr.Contains(path))
                                {
                                    m_dicAssObjCache[path].AddCount(1);

                                    if (M_DEBUG_MODE)
                                        DebugMsg(string.Format("\t+++\t assObj_4:{0}, Name:{1}, RefNum:{2}", path, img.sprite.name, m_dicAssObjCache[path].mCount));
                                }
                                else
                                    m_gcListDepStr.Add(path);
                            }
                        }
                    }

                    SpriteRenderer[] SpriteRenderers = obj.GetComponentsInChildren<SpriteRenderer>(true);
                    for (int i = 0; i < SpriteRenderers.Length; i++)
                    {
                        SpriteRenderer sprRen = SpriteRenderers[i];
                        if (sprRen != null && sprRen.sprite != null)
                        {
                            string path = null;
                            int assId = sprRen.sprite.GetInstanceID();
                            if (m_dicAssPathCache.TryGetValue(assId, out path))
                            {
                                if (m_gcListDepStr.Contains(path))
                                {
                                    m_dicAssObjCache[path].AddCount(1);

                                    if (M_DEBUG_MODE)
                                        DebugMsg(string.Format("\t+++\t assObj_5:{0}, Name:{1}, RefNum:{2}", path, sprRen.sprite.name, m_dicAssObjCache[path].mCount));
                                }
                                else
                                    m_gcListDepStr.Add(path);
                            }
                        }
                    }
                    m_gcListDepStr.Clear();
                }
            }
        }

        private void LoadAssetFromEditor(string path, string name, System.Type type = null)
        {
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(name))
                return;

            if (type == null)
                type = typeof(UnityEngine.Object);

            // 没有AB 不做引用计数
            if (m_dicAssObjCache.ContainsKey(path))
                return;

            UnityEngine.Object assObj = null;
#if UNITY_EDITOR
            if (type == ms_tySprite)
            {
                UnityEngine.Object[] objs = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(ms_sEditorDataDir + path);
                for (int i = 0; i < objs.Length; i++)
                {
                    if (objs[i].name.Equals(name) && objs[i].GetType() == type)
                    {
                        assObj = objs[i];
                        break;
                    }
                }
            }
            else
            {
                assObj = UnityEditor.AssetDatabase.LoadAssetAtPath(ms_sEditorDataDir + path, type);
            }
#endif
            AddAssObjToCache(path, name, assObj, null);
        }

        private void LoadAssetFromABFile(string path, string name)
        {
            LoadAssetBundle(path, name);
        }

        public void AddAssObjToCache(string path, string name, UnityEngine.Object assObj, AssetBundle ab, bool syncLoad = true)
        {
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(name))
                return;

            if (null == assObj && null == ab)
                return;

            if (!m_dicAssObjCache.ContainsKey(path))
            {
                m_dicAssObjCache.Add(path, new AssetObj(path, name, ab));
                if (null != ab && syncLoad)
                    m_dicAssObjCache[path].SyncLoad();

                m_dicAssObjCache[path].AddCount(1);

                if (M_DEBUG_MODE)
                    DebugMsg(string.Format("\t+++\t assObj_1:{0}, Name:{1}, RefNum:{2}", path, name, m_dicAssObjCache[path].mCount));
            }
        
            if (null != assObj)
            {
                int nAssId = assObj.GetInstanceID();
                if (!m_dicAssPathCache.ContainsKey(nAssId))
                {
                    m_dicAssPathCache.Add(nAssId, path);

                    AssetObj oAssObj = null;
                    if (m_dicAssObjCache.TryGetValue(path, out oAssObj))
                        oAssObj.PushAssObj(name, assObj);
                }
            }
        }

        private void unLoadAsset(UnityEngine.Object obj)
        {
            if (obj == null)
                return;

            int nInsId = obj.GetInstanceID();
            InsItem insItem = null;
            m_dicInsObjCache.TryGetValue(nInsId, out insItem);
            if (null != insItem)
            {
                string path = insItem.mPath;
                int nAssId = insItem.mAssId;
                string name = insItem.mName;

                if (M_DEBUG_MODE)
                    DebugMsg(string.Format("\nUnLoadAsset---1---:\t{0},IncObjNum:{1},AssObjNum:{2}", path, m_dicInsObjCache.Count, m_dicAssObjCache.Count));

                bool bCheckDep = CheckUnLoadUI(obj, path, name);

                m_dicInsObjCache.Remove(nInsId);
                UnityEngine.Object.DestroyImmediate(obj);

                AssetObj oAssObj = null;
                if (m_dicAssObjCache.TryGetValue(path, out oAssObj))
                {
                    oAssObj.AddCount(-1);
                    if (oAssObj.mCount <= 0)
                    {
                        OnUnLoadAssetAB(-1, oAssObj.mPath, true, bCheckDep);
                    }
                    else
                    {
                        if (M_DEBUG_MODE)
                            DebugMsg(string.Format("\t---\t assObj_11:{0},Name:{1},RefNum:{2}", path, oAssObj.mName, oAssObj.mCount));
                    }
                }

                if (M_DEBUG_MODE)
                    DebugMsg(string.Format("UnLoadAsset---2---:\t{0},IncObjNum:{1},AssObjNum:{2}", path, m_dicInsObjCache.Count, m_dicAssObjCache.Count));
            }
            else
            {
                int nAssId = nInsId;
                string path = null;
                m_dicAssPathCache.TryGetValue(nAssId, out path);
                if (!string.IsNullOrEmpty(path))
                {
                    if (M_DEBUG_MODE)
                        DebugMsg(string.Format("\nUnLoadAsset---11---:\t{0},IncObjNum:{1},AssObjNum:{2}", path, m_dicInsObjCache.Count, m_dicAssObjCache.Count));

                    AssetObj oAssObj = null;
                    if (m_dicAssObjCache.TryGetValue(path, out oAssObj))
                    {
                        oAssObj.AddCount(-1);
                        if (oAssObj.mCount <= 0)
                        {
                            OnUnLoadAssetAB(-1, oAssObj.mPath, true);
                        }
                        else
                        {
                            if (M_DEBUG_MODE)
                                DebugMsg(string.Format("\t---\t assObj_22:{0},Name:{1},RefNum:{2}", path, oAssObj.mName, oAssObj.mCount));
                        }
                    }

                    if (M_DEBUG_MODE)
                        DebugMsg(string.Format("UnLoadAsset---22---:\t{0},IncObjNum:{1},AssObjNum:{2}", path, m_dicInsObjCache.Count, m_dicAssObjCache.Count));
                }
            }
        }

        private bool CheckUnLoadUI(UnityEngine.Object obj, string path, string name)
        {
            GameObject uiObj = obj as GameObject;
            if (null != uiObj && null != uiObj.GetComponent<RectTransform>())
            {
                m_gcListDepSp.Clear();
                Image[] Images = uiObj.GetComponentsInChildren<Image>(true);
                for (int i = 0; i < Images.Length; i++)
                {
                    Image img = Images[i];
                    if (img != null && img.sprite != null)
                    {
                        int assId = img.sprite.GetInstanceID();

                        string sPath = null;
                        if (m_dicAssPathCache.TryGetValue(assId, out sPath))
                            if (!m_gcListDepSp.Contains(sPath))
                                m_gcListDepSp.Add(sPath);

                        UnLoadAsset(img.sprite);
                    }
                }

                SpriteRenderer[] SpriteRenderers = uiObj.GetComponentsInChildren<SpriteRenderer>(true);
                for (int i = 0; i < SpriteRenderers.Length; i++)
                {
                    SpriteRenderer sprRen = SpriteRenderers[i];
                    if (sprRen != null && sprRen.sprite != null)
                    {
                        int assId = sprRen.sprite.GetInstanceID();

                        string sPath = null;
                        if (m_dicAssPathCache.TryGetValue(assId, out sPath))
                            if (!m_gcListDepSp.Contains(sPath))
                                m_gcListDepSp.Add(sPath);

                        UnLoadAsset(sprRen.sprite);
                    }
                }

                //检测该InsObj节点是否被修改过Image(SetImage)
                AssetObj oAssObj = null;
                if (m_abm && m_dicAssObjCache.TryGetValue(path, out oAssObj))
                {
                    UnityEngine.Object assObj = null;
                    if (null != oAssObj)
                    {
                        assObj = oAssObj.GetAssObj(name, 0);
                        if (null != assObj)
                        {
                            string[] files = m_abm.GetAllDependencies(path + GameConfigMgr.ms_sABType);
                            for (int i = 0; i < files.Length; i++)
                            {
                                string sPath = files[i].Substring(0, files[i].LastIndexOf("."));
                                if (!m_gcListDepSp.Contains(sPath))
                                    m_gcListDepSp.Add(sPath);
                            }
                        }
                    }
                }

                return m_gcListDepSp.Count > 0;
            }
            return false;
        }

        private UnityEngine.Object Instantiate(UnityEngine.Object assObj)
        {
            if (assObj == null)
                return null;
            UnityEngine.Object insObj = null;
            if (assObj.GetType() == typeof(GameObject))
            {
                Transform objTrans = ((GameObject)assObj).transform;
                insObj = UnityEngine.Object.Instantiate(assObj, objTrans.position, objTrans.rotation);
            }
            else
            {
                insObj = UnityEngine.Object.Instantiate(assObj);
            }
            insObj.name = assObj.name;
            return insObj;
        }
        #endregion SyncLoad Or UnLoad AssetObj

        #region AsyncLoad Or UnLoad AssetObj
        private IEnumerator _loadAssetAsync(string assetPath, string name, System.Type type = null, UnityAction<UnityEngine.Object> cal = null)
        {
            yield return null;
            UnityEngine.Object obj = loadAsset(assetPath, name, type);
            if (cal != null)
                cal(obj);
        }
        private void loadAssetAsync(string assetPath, string name, System.Type type = null, UnityAction<UnityEngine.Object> cal = null, bool queue = false)
        {
            if (queue == false)
            {
                GameMgr.Instance.StartCoroutine(_loadAssetAsync(assetPath, name, type, cal));
                return;
            }
            else
            {
                if (string.IsNullOrEmpty(assetPath) == false && string.IsNullOrEmpty(name) == false)
                    m_lstBatchItem.Add(new BatchItem(assetPath, name, type, cal));
                if (m_bLoadBatch == false && m_lstBatchItem.Count > 0)
                {
                    m_bLoadBatch = true;
                    BatchItem bi = m_lstBatchItem[0];
                    m_lstBatchItem.Remove(bi);
                    UnityAction<UnityEngine.Object> action = (obj) =>
                    {
                        m_bLoadBatch = false;
                        loadAssetAsync(null, null, null, null, true);
                        if (bi.mAction != null)
                            bi.mAction(obj);
                    };
                    GameMgr.Instance.StartCoroutine(_loadAssetAsync(bi.mPath, bi.mName, bi.mType, action));
                }
                return;
            }
        }
        private void loadAssetAsyncEx(string path, string name, int iAsyncId, bool underRoot)
        {
            if (string.IsNullOrEmpty(path))
                return;

            RunMode eRunMode = GameMgr.Instance.m_eRunMode;
            if (eRunMode == RunMode.eDeveloper)
            {
                AssetObj oAssObj = null;
                m_dicAssObjCache.TryGetValue(path, out oAssObj);
                if (null != oAssObj)
                {
                    UnityAction<string, GameObject> callEx = (sPath, obj) =>
                    {
                        if (null == obj) Debug.LogWarningFormat("ResourceMgr.loadAssetAsync Failed_1:{0}, {1}", path, name);
                        if (underRoot) GameMgr.Instance.SetParentByRoot(obj);
                        m_fOnResAsyncLoaded.Call(iAsyncId, obj);
                    };

                    UnityEngine.Object assObj = oAssObj.GetAssObj(name, 1);
                    if (null == assObj)
                        Debug.LogWarningFormat("loadAssetAsyncEx Error_1:{0}, {1}", path, name);

                    OnInsAsynAssObj(callEx, path, assObj as GameObject, false);
                    return;
                }

                UnityAction<UnityEngine.Object> call = (goObj) =>
                {
                    GameObject obj = (GameObject)goObj;
                    if (null == obj) Debug.LogWarningFormat("ResourceMgr.loadAssetAsync Failed_2:{0}, {1}", path, name);
                    if (underRoot) GameMgr.Instance.SetParentByRoot((GameObject)goObj);
                    m_fOnResAsyncLoaded.Call(iAsyncId, goObj);
                };

                LoadAssetAsync(path, name, typeof(GameObject), call, true);
            }
            else if (eRunMode == RunMode.eDebug || eRunMode == RunMode.eRelease)
            {
                UnityAction<string, GameObject> call = (sPath, obj) =>
                {
                    if (null == obj) Debug.LogWarningFormat("ResourceMgr.loadAssetAsync Failed_3:{0}, {1}", path, name);
                    if (underRoot) GameMgr.Instance.SetParentByRoot(obj);
                    m_fOnResAsyncLoaded.Call(iAsyncId, obj);
                };

                AssetObj oAssObj = null;
                m_dicAssObjCache.TryGetValue(path, out oAssObj);
                if (null != oAssObj)
                {
                    UnityEngine.Object assObj = oAssObj.GetAssObj(name, 1);
                    if (null == assObj)
                        Debug.LogWarningFormat("loadAssetAsyncEx Error_2:{0}, {1}", path, name);

                    OnInsAsynAssObj(call, path, assObj as GameObject, false);
                    return;
                }

                if (m_dicAsynObjLoading.ContainsKey(path))
                {
                    if (!m_dicAsynObjRequest.ContainsKey(path))
                        m_dicAsynObjRequest.Add(path, new Stack<UnityAction<string, GameObject>>());

                    Stack<UnityAction<string, GameObject>> stackAc = m_dicAsynObjRequest[path];
                    stackAc.Push(call);
                }
                else
                {
                    m_dicAsynObjLoading.Add(path, new AsynObj(path, name, call));
                    m_dicAsynObjLoading[path].AsynLoad();
                    m_bObjAsyncLoading = true;
                }
            }
            else
                Debug.AssertFormat(false, "Error eRunMode:{0}", eRunMode);
        }
        public void Update()
        {
            if (m_bABAsyncLoading)
            {
                int nAllNum = m_dicAsynABLoading.Count;
                m_gcListStr.Clear();
                m_gcListAsAB.Clear();
                if (nAllNum > 0)
                {
                    m_nAsynABNum = 0;
                    Dictionary<string, AsynAB>.Enumerator iter = m_dicAsynABLoading.GetEnumerator();
                    while (iter.MoveNext())
                    {
                        if (iter.Current.Value.IsDone())
                        {
                            m_nAsynABNum++;
                            //iter.Current.Value.OnLoadABOk();
                            m_gcListAsAB.Add(iter.Current.Value);
                            m_gcListStr.Add(iter.Current.Key);

                            if (m_nAsynABNum >= 10)
                                break;
                        }
                    }
                    iter.Dispose();

                    if (m_nAsynABNum > 0)
                        m_bABAsyncLoading = (nAllNum != m_nAsynABNum);
                }
                if (m_gcListAsAB.Count > 0)
                {
                    List<AsynAB>.Enumerator gciter = m_gcListAsAB.GetEnumerator();
                    while (gciter.MoveNext())
                    {
                        gciter.Current.OnLoadABOk();
                    }
                    gciter.Dispose();
                    m_gcListAsAB.Clear();
                }
                if (m_gcListStr.Count > 0)
                {
                    List<string>.Enumerator gciter = m_gcListStr.GetEnumerator();
                    while (gciter.MoveNext())
                    {
                        string path = gciter.Current;
                        AsynAB asAB = null;
                        if (m_dicAsynABLoading.TryGetValue(path, out asAB))
                        {
                            string name = Path.GetFileNameWithoutExtension(path);
                            AddAssObjToCache(path, name, null, asAB.mABCReq.assetBundle, false);

                            m_dicAsynABLoading.Remove(path);
                        }
                    }
                    gciter.Dispose();
                    m_gcListStr.Clear();
                }
            }

            if (m_bObjAsyncLoading)
            {
                m_gcListStr.Clear();
                m_gcListAsObj.Clear();
                int nAllNum = m_dicAsynObjLoading.Count;
                if (nAllNum > 0)
                {
                    m_nAsynObjNum = 0;
                    //m_dicAsynObjLoading.Keys.GetEnumerator();
                    Dictionary<string, AsynObj>.Enumerator iter = m_dicAsynObjLoading.GetEnumerator();
                    while (iter.MoveNext())
                    {
                        if (iter.Current.Value.IsDone())
                        {
                            m_nAsynObjNum++;
                            //iter.Current.Value.OnLoadObjOk();
                            m_gcListAsObj.Add(iter.Current.Value);
                            m_gcListStr.Add(iter.Current.Key);

                            if (m_nAsynObjNum >= 10)
                                break;
                        }
                    }
                    iter.Dispose();

                    if (m_nAsynObjNum > 0)
                        m_bObjAsyncLoading = (nAllNum != m_nAsynObjNum);
                }
                if (m_gcListAsObj.Count > 0)
                {
                    List<AsynObj>.Enumerator gciter = m_gcListAsObj.GetEnumerator();
                    while (gciter.MoveNext())
                    {
                        gciter.Current.OnLoadObjOk();
                    }
                    gciter.Dispose();
                    m_gcListAsObj.Clear();
                }
                if (m_gcListStr.Count > 0)
                {
                    List<string>.Enumerator gciter = m_gcListStr.GetEnumerator();
                    while (gciter.MoveNext())
                    {
                        m_dicAsynObjLoading.Remove(gciter.Current);
                    }
                    gciter.Dispose();
                    m_gcListStr.Clear();
                }
            }
        }

        #endregion AsyncLoadAsset

        #region AB Load or UnLoad
        private void LoadAssetBundle(string path, string name, bool addRef=false)
        {
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(name))
                return;

            if (m_dicAssObjCache.ContainsKey(path))
            {
                if (addRef)
                {
                    m_dicAssObjCache[path].AddCount(1);

                    if (M_DEBUG_MODE)
                        DebugMsg(string.Format("\t+++\t assObj_3:{0}, Name:{1}, RefNum:{2}", path, name, m_dicAssObjCache[path].mCount));
                }

                // iteration AddRef
                string[] files = m_abm.GetAllDependencies(path + GameConfigMgr.ms_sABType);
                for (int i = 0; i < files.Length; i++)
                {
                    string sPath = files[i].Substring(0, files[i].LastIndexOf("."));
                    string sName = Path.GetFileNameWithoutExtension(sPath);

                    //该种方式可以有效避免同资源依赖卸载后作为新引用时的依赖丢失,该种方式避免资源二级引用关系(否则可能出现依赖循环问题)
                    LoadAssetBundle(sPath, sName, true);
                }
                return;
            }

            if (m_abm != null)
            {
                // iteration LoadAsset
                string[] files = m_abm.GetAllDependencies(path + GameConfigMgr.ms_sABType);
                for (int i = 0; i < files.Length; i++)
                {
                    string sPath = files[i].Substring(0, files[i].LastIndexOf("."));
                    string sName = Path.GetFileNameWithoutExtension(sPath);
                    LoadAssetBundle(sPath, sName, true);
                }
            }

            //TryTo First Read Outside Assets
            string fullPath = ms_sABResrcDir + path + GameConfigMgr.ms_sABType;
            AssetBundle ab = null;
            if (File.Exists(fullPath))
            {
                ab = AssetBundle.LoadFromFile(fullPath);
            }
            else
            {
                TextAsset ta = Resources.Load<TextAsset>(ms_sResourcesABDir + path);
                if (null == ta)
                {
                    if (!path.EndsWith(".ogg"))
                        Debug.LogWarning("Resources.Load failed:" + ms_sResourcesABDir + path);
                    return;
                }
                ab = AssetBundle.LoadFromMemory(ta.bytes);
            }

            AddAssObjToCache(path, name, null, ab);
        }
        public void LoadAssetBundleAsync(string path, UnityAction<string, AssetBundle> call)
        {
            if (string.IsNullOrEmpty(path))
                return;

            AssetBundle ab = null;
            RunMode eRunMode = GameMgr.Instance.m_eRunMode;

            if (eRunMode != RunMode.eDeveloper)
            {
                string fullPath = ms_sABResrcDir + path + GameConfigMgr.ms_sABType;
                if (File.Exists(fullPath))
                {
                    AssetBundleCreateRequest abRq = AssetBundle.LoadFromFileAsync(fullPath);
                    m_dicAsynABLoading.Add(path, new AsynAB(path, abRq, call));
                    m_bABAsyncLoading = true;
                }
                else
                {
                    TextAsset textAss = Resources.Load<TextAsset>(ms_sResourcesABDir + path);
                    if (null == textAss)
                        Debug.LogWarning("Resources.loadSceneAB failed:" + ms_sResourcesABDir + path);
                    else
                    {
                        AssetBundleCreateRequest abRq = AssetBundle.LoadFromMemoryAsync(textAss.bytes);
                        m_dicAsynABLoading.Add(path, new AsynAB(path, abRq, call));
                        m_bABAsyncLoading = true;
                    }
                }
            }
            else
                call(path, ab);
        }
        public void OnAsynLoadABOk(string path, AssetBundle ab)
        {
            string abKey = path;
            Stack<UnityAction<string, AssetBundle>> stackAc = null;
            if (m_dicAsynABRequest.TryGetValue(abKey, out stackAc))
            {
                if (stackAc.Count > 0)
                {
                    Stack<UnityAction<string, AssetBundle>>.Enumerator gciter = stackAc.GetEnumerator();
                    while (gciter.MoveNext())
                    {
                        gciter.Current(path, ab);
                    }
                    gciter.Dispose();
                    m_dicAsynABRequest.Remove(abKey);
                }
            }
        }
        public void OnAsynLoadObjOk(string path, GameObject assObj)
        {
            Stack<UnityAction<string, GameObject>> stackAc = null;
            if (m_dicAsynObjRequest.TryGetValue(path, out stackAc))
            {
                if (stackAc.Count > 0)
                {
                    Stack<UnityAction<string, GameObject>>.Enumerator gciter = stackAc.GetEnumerator();
                    while (gciter.MoveNext())
                    {
                        OnInsAsynAssObj(gciter.Current, path, assObj);
                    }
                    gciter.Dispose();
                    m_dicAsynObjRequest.Remove(path);
                }
            }
        }
        public AssetBundle GetAsynABLoaded(string path)
        {
            AssetObj oAssObj = null;
            if (m_dicAssObjCache.TryGetValue(path, out oAssObj))
            {
                return oAssObj.mAB;
            }
            return null;
        }
        public bool AsynABIsLoaded(string path)
        {
            return m_dicAssObjCache.ContainsKey(path);
        }
        public bool AsynABIsLoading(string path, UnityAction<string, AssetBundle> call)
        {
            if (m_dicAsynABLoading.ContainsKey(path))
            {
                if (!m_dicAsynABRequest.ContainsKey(path))
                    m_dicAsynABRequest.Add(path, new Stack<UnityAction<string, AssetBundle>>());

                Stack<UnityAction<string, AssetBundle>> stackAc = m_dicAsynABRequest[path];
                stackAc.Push(call);
                return true;
            }
            return false;
        }
        public void OnInsAsynAssObj(UnityAction<string, GameObject> call, string path, GameObject assObj, bool pushAssObj = false)
        {
            if (null != assObj)
            {
                if (pushAssObj)
                    AddAssObjToCache(path, assObj.name, assObj, null);

                int assId = assObj.GetInstanceID();
                GameObject insObj = Instantiate(assObj) as GameObject;

                //Cache InsObj
                int insId = insObj.GetInstanceID();
                if (!m_dicInsObjCache.ContainsKey(insId))
                    m_dicInsObjCache.Add(insId, new InsItem(path, assId, assObj.name));
                else
                    Debug.Assert(false, "m_dicInsObjCache Error path:" + path + ",insId:" + insId);

                call(path, insObj);
            }
        }
        public string[] GetAllDependencies(string path)
        {
            if (m_abm != null)
                return m_abm.GetAllDependencies(path + GameConfigMgr.ms_sABType);

            return null;
        }
        public void OnUnLoadAssetAB(int nAssId, string path = null, bool unLoad = false, bool checkDep = false)
        {
            if (string.IsNullOrEmpty(path))
                m_dicAssPathCache.TryGetValue(nAssId, out path);

            if (string.IsNullOrEmpty(path))
                return;

            AssetObj oAssObj = null;
            if (m_dicAssObjCache.TryGetValue(path, out oAssObj))
            {
                if (unLoad)
                {
                    m_gcListInt.Clear();
                    Dictionary<int, string>.Enumerator iter = m_dicAssPathCache.GetEnumerator();
                    while (iter.MoveNext())
                    {
                        if (iter.Current.Value.Equals(path))
                            m_gcListInt.Add(iter.Current.Key);
                    }
                    iter.Dispose();
                    if (m_gcListInt.Count > 0)
                    {
                        List<int>.Enumerator gciter = m_gcListInt.GetEnumerator();
                        while (gciter.MoveNext())
                        {
                            m_dicAssPathCache.Remove(gciter.Current);
                        }
                        gciter.Dispose();
                        m_gcListInt.Clear();
                    }

                    oAssObj.UnLoadAll();
                    m_dicAssObjCache.Remove(path);

                    if (M_DEBUG_MODE)
                        DebugMsg(string.Format("\t---\t assObj_33:{0},Name:{1},RefNum:{2}", path, oAssObj.mName, oAssObj.mCount));
                }
                else
                {
                    oAssObj.AddCount(-1);
                    if (oAssObj.mCount <= 0)
                        OnUnLoadAssetAB(-1, oAssObj.mPath, true);

                    if (M_DEBUG_MODE && m_dicAssObjCache.ContainsKey(path))
                        DebugMsg(string.Format("\t---\t assObj_44:{0},Name:{1},RefNum:{2}", path, oAssObj.mName, oAssObj.mCount));
                }

                if (m_abm != null)
                {
                    // iteration UnLoadAsset
                    string[] files = m_abm.GetAllDependencies(path + GameConfigMgr.ms_sABType);
                    for (int i = 0; i < files.Length; i++)
                    {
                        string sPath = files[i].Substring(0, files[i].LastIndexOf("."));
                        if (checkDep && m_gcListDepSp.Contains(sPath))
                            continue;

                        OnUnLoadAssetAB(-1, sPath, false);
                    }
                }
            }
        }
        #endregion AB Load or UnLoad

        #region Cache InsGameObj
        private GameObject _GetInsGameObj(string path)
        {
            GameObject go = null;
            Stack<GameObject> stack = null;
            if (m_dicCacheInsGameObj.TryGetValue(path, out stack))
            {
                if (stack.Count > 0)
                    go = stack.Pop();
                else
                    m_dicCacheInsGameObj.Remove(path);

                if (M_DEBUG_MODE)
                    DebugMsg(string.Format("\t---\t getInsObj_1:{0}, Num:{1}", path, stack.Count));
            }
            else if (m_dicCacheInsGameObjEx.TryGetValue(path, out stack))
            {
                if (stack.Count > 0)
                    go = stack.Pop();
                else
                    m_dicCacheInsGameObjEx.Remove(path);

                if (M_DEBUG_MODE)
                    DebugMsg(string.Format("\t---\t getInsObj_2:{0}, Num:{1}", path, stack.Count));
            }
            return go;
        }

        private void _PutInsGameObj(GameObject go, bool underUIRootdy)
        {
            if (null == go) return;

            int insId = go.GetInstanceID();
            InsItem insItem = null;
            m_dicInsObjCache.TryGetValue(insId, out insItem);
            if (null != insItem)
            {
                string path = insItem.mPath;
                if (!go.name.Equals(path)) go.name = path;

                go.SetActive(false);

                if (underUIRootdy)
                    go.transform.SetParent(UIMgr.Instance.GetUIRootDy().transform);
                else
                    GameMgr.Instance.SetParentByRoot(go);

                Stack<GameObject> stack = null;
                if (!m_listStaticObj.Contains(insId))
                {
                    if (m_dicCacheInsGameObj.TryGetValue(path, out stack))
                        stack.Push(go);
                    else
                    {
                        stack = new Stack<GameObject>();
                        stack.Push(go);
                        m_dicCacheInsGameObj.Add(path, stack);
                    }

                    if (M_DEBUG_MODE)
                        DebugMsg(string.Format("\t+++\t putInsObj_1:{0}, Num:{1}", path, m_dicCacheInsGameObj[path].Count));
                }
                else
                {
                    if (m_dicCacheInsGameObjEx.TryGetValue(path, out stack))
                        stack.Push(go);
                    else
                    {
                        stack = new Stack<GameObject>();
                        stack.Push(go);
                        m_dicCacheInsGameObjEx.Add(path, stack);
                    }

                    if (M_DEBUG_MODE)
                        DebugMsg(string.Format("\t+++\t putInsObj_1:{0}, Num:{1}", path, m_dicCacheInsGameObjEx[path].Count));
                }
            }
            else
                Debug.LogErrorFormat("_PutInsGameObj Error: name={0}", go.name);
        }
        #endregion Cache InsGameObj

        #region Load Map
        private void loadSceneAB(string path, UnityAction<string, AssetBundle> call = null, bool bAsync = false)
        {
            if (string.IsNullOrEmpty(path))
                return;

            AssetBundle ab = null;
            RunMode eRunMode = GameMgr.Instance.m_eRunMode;

            if (bAsync)
            {
                if (eRunMode != RunMode.eDeveloper)
                    LoadAssetBundleAsync(path, call);
                else
                    call(path, ab);
            }
            else
            {
                if (eRunMode != RunMode.eDeveloper)
                {
                    string fullPath = ms_sABResrcDir + path + GameConfigMgr.ms_sABType;
                    if (File.Exists(fullPath))
                    {
                        ab = AssetBundle.LoadFromFile(fullPath);
                    }
                    else
                    {
                        TextAsset textAss = Resources.Load<TextAsset>(ms_sResourcesABDir + path);
                        if (null == textAss)
                            Debug.LogWarning("Resources.loadSceneAB failed:" + ms_sResourcesABDir + path);
                        else
                            ab = AssetBundle.LoadFromMemory(textAss.bytes);
                    }
                }
                call(path, ab);
            }
        }

        private void unLoadSceneAB(AssetBundle ab)
        {
            if (ab != null)
                ab.Unload(false);
        }
        #endregion Load Map

        #region AssetSet interface
        private void SetResetAsset(GameObject obj)
        {
            Delay oDelay = obj.GetComponent<Delay>();
            if (oDelay)
            {
                oDelay.CancelDelay();
            }
            Delay[] oDelayList = obj.GetComponentsInChildren<Delay>(true);
            for (int i = 0; i < oDelayList.Length; i++)
            {
                oDelayList[i].CancelDelay();
            }
            obj.SetActive(false);
        }
        private void CheckEffectState(GameObject goObj)
        {
            if (!goObj) return;
            TrailRenderer[] trailList = goObj.GetComponentsInChildren<TrailRenderer>(true);
            for (int i = 0; i < trailList.Length; i++)
            {
                trailList[i].Clear();
            }
        }
        #endregion AssetSet interface

        #region public_static (Export interface)
        public static string LoadVersion()
        {
            return Instance.loadVersion();
        }
        public static void LoadSceneAB(string path, UnityAction<string, AssetBundle> ac = null, bool bAsync = false)
        {
            Instance.loadSceneAB(path, ac, bAsync);
        }
        public static void UnLoadSceneAB(AssetBundle ab)
        {
            Instance.unLoadSceneAB(ab);
        }
        public static UnityEngine.Object LoadAsset(string path, string name, System.Type type = null)
        {
            return Instance.loadAsset(path, name, type);
        }
        public static void LoadAssetAsync(string assetPath, string name, System.Type type = null, UnityAction<UnityEngine.Object> ac = null, bool queue = false)
        {
            Instance.loadAssetAsync(assetPath, name, type, ac, queue);
        }
        public static void UnLoadAsset(UnityEngine.Object obj)
        {
            Instance.unLoadAsset(obj);
        }
        public static GameObject GetInsGameObj(string path)
        {
            return Instance._GetInsGameObj(path);
        }
        public static void PutInsGameObj(GameObject go, bool bUIRes = false)
        {
            Instance._PutInsGameObj(go, bUIRes);
        }
        public void CacheLoadGameObject(string path, int cacheCount, bool bRemove = true)
        {
            if (!Instance.CheckAssetPath(ref path))
                return;

            int nBegin = 0;
            GameObject obj = GetInsGameObj(path);
            if (!obj)
            {
                nBegin = 1;
                obj = LoadAssetEx(path, false);
            }

            if (!obj) return;

            int insId = obj.GetInstanceID();
            InsItem insItem = null;
            if (!m_dicInsObjCache.TryGetValue(insId, out insItem))
                return;

            int nAssId = insItem.mAssId;
            path = insItem.mPath;
            string name = insItem.mName;

            if (!bRemove)
                m_listStaticObj.Add(insId);

            bool bUIRes = path.Contains("ui/prefabs");
            PutInsGameObj(obj, bUIRes);

            for (int count = nBegin; count < cacheCount; count++)
            {
                GameObject goCache = UnityEngine.Object.Instantiate(obj) as GameObject;
                int cacheId = goCache.GetInstanceID();

                // Cache InsObj
                if (!m_dicInsObjCache.ContainsKey(cacheId))
                    m_dicInsObjCache.Add(cacheId, new InsItem(path, nAssId, name));
                else
                    Debug.Assert(false, "m_dicInsObjCache Error path:" + path + ",cacheId:" + cacheId);

                SetResetAsset(goCache);

                if (!bRemove)
                    m_listStaticObj.Add(cacheId);

                PutInsGameObj(goCache, bUIRes);
            }
        }
        public void ClearInsGameObjOnChangeScene()
        {
            foreach (KeyValuePair<string, Stack<GameObject>> iter in m_dicCacheInsGameObj)
            {
                Stack<GameObject> stack = iter.Value;
                while (stack.Count > 0)
                {
                    GameObject go = stack.Pop();
                    UnLoadAsset(go);
                }
            }
            m_dicCacheInsGameObj.Clear();
        }
        public void ClearInsCacheGameObj(string path, int clearCount)
        {
            while (clearCount > 0)
            {
                GameObject goCache = GetInsGameObj(path);
                if (!goCache) break;

                UnLoadAsset(goCache);
                clearCount--;
            }
        }

        //Export_For_Lua
        public static GameObject LoadAssetEx(string path, bool underRoot)
        {
            if (!Instance.CheckAssetPath(ref path))
                return null;

            GameObject goCache = GetInsGameObj(path);
            if (goCache)
            {
                goCache.SetActive(true);
                return goCache;
            }

            string name = Path.GetFileNameWithoutExtension(path);
            GameObject obj = (GameObject)Instance.loadAsset(path, name, typeof(GameObject));

            if (null == obj) Debug.LogWarning("ResourceMgr.LoadAssetEx Failed:" + path);
            if (underRoot) GameMgr.Instance.SetParentByRoot(obj);

            return obj;
        }
        //Export_For_Lua
        public static GameObject LoadAssetWithStateEx(string path, Vector3 position, Quaternion quaternion, bool underRoot)
        {
            if (!Instance.CheckAssetPath(ref path))
                return null;

            GameObject goCache = GetInsGameObj(path);
            if (goCache)
            {
                goCache.transform.position = position;
                goCache.transform.rotation = quaternion;
                Instance.CheckEffectState(goCache);
                goCache.SetActive(true);
                return goCache;
            }

            InstantiateDelegate insAc = (assObj) =>
            {
                if (assObj == null)
                    return null;
                UnityEngine.Object insObj = null;
                if (assObj.GetType() == typeof(GameObject))
                {
                    Transform objTrans = ((GameObject)assObj).transform;
                    insObj = UnityEngine.Object.Instantiate(assObj, position, quaternion);
                }
                else
                {
                    insObj = UnityEngine.Object.Instantiate(assObj);
                }
                insObj.name = assObj.name;
                return insObj;
            };

            string name = Path.GetFileNameWithoutExtension(path);
            GameObject obj = (GameObject)Instance.loadAsset(path, name, typeof(GameObject), insAc);

            if (null == obj) Debug.LogWarning("ResourceMgr.LoadAssetWithStateEx Failed:" + path);
            if (underRoot) GameMgr.Instance.SetParentByRoot(obj);

            return obj;
        }
        //Export_For_Lua
        public static void LoadAssetAsyncEx(int iAsyncId, string path, bool queue, bool underRoot)
        {
            if (!Instance.CheckAssetPath(ref path))
                return;

            GameObject goCache = GetInsGameObj(path);
            if (goCache)
            {
                goCache.SetActive(true);
                Instance.m_fOnResAsyncLoaded.Call(iAsyncId, goCache);
                return;
            }

            string name = Path.GetFileNameWithoutExtension(path);
            Instance.loadAssetAsyncEx(path, name, iAsyncId, underRoot);
        }
        #endregion public_static (Export interface)
    }
}