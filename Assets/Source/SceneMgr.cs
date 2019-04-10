using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using LuaInterface;

namespace WCG
{

    public class SceneMgr
    {
		public enum LoadedState
		{
			None,
			Async,
			Sync,
		}

        public class PreItem
        {
            public string m_sPath;
            public int m_nCount;
            public bool m_bRemove;

            public PreItem(string path, int nCount, bool bRemove)
            {
                m_sPath = path;
                m_nCount = nCount;
                m_bRemove = bRemove;
            }
        }

        public static readonly SceneMgr Instance = new SceneMgr();

		private UnityAction<Scene> ms_aOnUnloadedScene = null;
		private UnityAction<Scene, Scene> ms_aOnActiveScene = null;
		private UnityAction<Scene, LoadSceneMode> ms_aOnLoadedScene = null;

		private static LuaFunction m_fOnSceneUnloaded = null;
		private static LuaFunction m_fOnLoadProgress = null;
		private static LuaFunction m_fOnSceneLoaded = null;
		private static LuaFunction m_fOnEnterScene = null;
		private static LuaFunction m_fOnLuaPreLoad = null;
        private static LuaFunction m_fOnSceneBlockCheck = null;
        //private static LuaFunction m_fOnSceneShotComplete = null;
		private static LuaFunction m_fOnObjAutoDestroy = null;

		private static string ms_sLoadingSceneName = "";
		private static int ms_nLoadingSceneId = -1;
		private float m_fResouerRate = 0.0f;
        private float m_fSceneRate = 0.0f;
        private LoadedState m_eLoadSate = LoadedState.None;
		private AsyncOperation m_oLoadAsync = null;
        private static bool m_bBlockCheck = false;

		//Scene辅助参数
		private GameObject m_goMainCamera = null;
        private GameObject m_CameraShader = null;
        private Material m_CameraShaderMat = null;
        private Quaternion m_quaRootRotation = Quaternion.identity;
		private Vector3 m_v3RootRotAxis = Vector3.zero;
		private static float m_nRootHeight = -5;
		private float m_fRootNodePosY = .0f;
		private float m_fTanAnglesX = .0f;
        private float m_nWidth = 0;
        private float m_nHeight = 0;
        private GameObject m_goRootNode = null;
        private Dictionary<int, GameObject> m_dicMapObjCache = new Dictionary<int, GameObject>();
        private GameObject m_goSceneNode = null;

        //逻辑cache
        private Stack<PreItem> m_pLoadAsset = new Stack<PreItem>();

        private List<Material> m_vecSceneShader = new List<Material>();

        private List<GameObject> m_SceneLightObj = new List<GameObject>();
        
        private SceneMgr()
        {
			SceneManager.sceneUnloaded += OnUnloadedScene;
			//SceneManager.activeSceneChanged += OnActiveScene;
			SceneManager.sceneLoaded += OnLoadedScene;
        }

		public void Init()
		{
            m_fSceneRate = 0.2f;
            m_fResouerRate = 1 - m_fSceneRate;
        }

		public void InitSceneCallBack()
		{
			if (null == m_fOnSceneUnloaded)
				m_fOnSceneUnloaded = GameMgr.ms_luaMgr.GetLuaFunction("gSceneMgr.OnSceneUnloaded");
			if (null == m_fOnLoadProgress)
				m_fOnLoadProgress = GameMgr.ms_luaMgr.GetLuaFunction("gSceneMgr.OnSceneProgress");
			if (null == m_fOnSceneLoaded)
				m_fOnSceneLoaded = GameMgr.ms_luaMgr.GetLuaFunction("gSceneMgr.OnSceneLoaded");
			if (null == m_fOnEnterScene)
				m_fOnEnterScene = GameMgr.ms_luaMgr.GetLuaFunction("gSceneMgr.OnEnterScene");
			if (null == m_fOnLuaPreLoad)
				m_fOnLuaPreLoad = GameMgr.ms_luaMgr.GetLuaFunction("gSceneMgr.OnLuaPreLoad");
			if (null == m_fOnObjAutoDestroy)
				m_fOnObjAutoDestroy = GameMgr.ms_luaMgr.GetLuaFunction("gSceneMgr.OnObjAutoDestroy");
            if (null == m_fOnSceneBlockCheck)
                m_fOnSceneBlockCheck = GameMgr.ms_luaMgr.GetLuaFunction("gSceneMgr.SceneBlockCheck");
        }

		public void OnObjAutoDestroy(GameObject obj)
		{
			if (m_fOnObjAutoDestroy != null)
				m_fOnObjAutoDestroy.Call(obj);
		}

        // 转场景  path = "map/xinkaijie.unity"
		private void _ChangeScene(string pathName, bool bAsync, UnityAction<string> aLoaded = null, UnityAction<string> aUnLoaded = null)
        {
			//Debug.Log("_ChangeScene: " + pathName + " | " + bAsync);
			if (string.IsNullOrEmpty(pathName))
                return;

            string sSceneName = pathName.Substring(pathName.LastIndexOf("/") + 1);
			sSceneName = sSceneName.Substring(0, sSceneName.LastIndexOf("."));

			if (ms_sLoadingSceneName==sSceneName || (!string.IsNullOrEmpty(ms_sLoadingSceneName)))
			{
				string sSameWarn = "_ChangeScene Load Same Scene:";
				string sLoading = "_ChangeScene LastScene is Loading:";
				Debug.LogWarning((ms_sLoadingSceneName==sSceneName ? sSameWarn : sLoading) + ms_sLoadingSceneName);
				return;
			}

            //AsyncOperation op = null;
            UnityAction<string, AssetBundle> aComplete = (path, ab) =>
            {
				ms_aOnUnloadedScene = (oScene) =>
                {
                    if (aUnLoaded != null)
						aUnLoaded(oScene.name);
                };
				/*ms_aOnActiveScene = (oSceneA, oSceneB) =>
				{
					//SceneManager.UnloadSceneAsync(oSceneB.name);
				};*/
				ms_aOnLoadedScene = (oScene, eMode) =>
				{
					ResourceMgr.UnLoadSceneAB(ab);
					if (aLoaded != null)
						aLoaded(oScene.name.Equals("Empty") ? ms_sLoadingSceneName : oScene.name);
				};

				//LoadSceneAsync Loads the scene asynchronously in the background.
				//LoadScene, the loading does not happen immediately, it completes in the next frame
				if (bAsync)
				{
					m_eLoadSate = LoadedState.Async;
					//m_oLoadAsync = Application.LoadLevelAsync(ms_sLoadingSceneName);
					m_oLoadAsync = SceneManager.LoadSceneAsync(ms_sLoadingSceneName, LoadSceneMode.Single);
					GameMgr.Instance.StartCoroutine(StartLoadingAsync());
				}
				else
				{
					m_eLoadSate = LoadedState.Sync;
					//Application.LoadLevel(sSceneName);
					SceneManager.LoadScene(sSceneName, LoadSceneMode.Single);
				}
            };
            
			m_oLoadAsync = null;
			m_eLoadSate = LoadedState.None;
			ms_sLoadingSceneName = sSceneName;
			ResourceMgr.LoadSceneAB(pathName, aComplete, true);
        }

		//异步加载进度检测 LoadedState.Async
		IEnumerator StartLoadingAsync()
		{
			//Application.LoadLevel("Empty");
			//SceneManager.LoadScene("Empty", LoadSceneMode.Single);

			GameMgr.ms_luaMgr.LuaGC();

            //AsyncOperation oAsync = Application.LoadLevelAsync(ms_sLoadingSceneName);
            //AsyncOperation oAsync = SceneManager.LoadSceneAsync(ms_sLoadingSceneName, LoadSceneMode.Single);
            //oAsync.allowSceneActivation = false;

            //两种方式 m_oLoadAsync || oAsync
            while (!m_oLoadAsync.isDone)
			{
                m_fOnLoadProgress.Call(ms_nLoadingSceneId, m_oLoadAsync.progress * m_fSceneRate);
                yield return new WaitForEndOfFrame();
            }
            
            while (!m_bBlockCheck)
            {
                m_fOnSceneBlockCheck.Call();
                yield return new WaitForEndOfFrame();
            }

			m_fOnLuaPreLoad.Call(ms_nLoadingSceneId);

			int iAssetSum = m_pLoadAsset.Count;
            if(iAssetSum > 0)
            {
                int iNumSum = iAssetSum;
                int iPreStep = 0;
                float fStepRate = m_fResouerRate / iNumSum;
                int nPreCount = Mathf.Max(iNumSum / 20, 1);
                //Debug.LogWarningFormat("preload {0}, {1} = ",iNumSum, nPreCount);

                while (iNumSum > 0)
                {
                    iNumSum -= nPreCount;
                    CheckPreloadAssetOk(nPreCount);
                    iPreStep += nPreCount;
                    m_fOnLoadProgress.Call(ms_nLoadingSceneId, m_fSceneRate + iPreStep * fStepRate);
                    yield return new WaitForEndOfFrame();
                }
                //m_fOnLoadProgress.Call(ms_nLoadingSceneId, 1.0f);
            }
            else
            {
                int iCount = 0;
                while (iCount <= 5)
                {
                    iCount++;
                    m_fOnLoadProgress.Call(ms_nLoadingSceneId, m_fSceneRate + m_fResouerRate * iCount / 5);
                    yield return new WaitForEndOfFrame();
                }

                m_fOnLoadProgress.Call(ms_nLoadingSceneId, 1.0f);
                yield return new WaitForEndOfFrame();
            }            

			TriggerOnEnterScene();
		}

		private void CheckPreloadAssetOk(int nPreCount)
		{
            for(int n=0; n< nPreCount; n++)
            {
                if (m_pLoadAsset.Count == 0)
                    break;

                PreItem pre = m_pLoadAsset.Pop();
                ResourceMgr.Instance.CacheLoadGameObject(pre.m_sPath, pre.m_nCount, pre.m_bRemove);
            }
		}
		public void AddCacheAsset(string pathName, int count, bool bRemove=true)
		{
			m_pLoadAsset.Push(new PreItem(pathName, count, bRemove));
		}

		private void OnUnloadedScene(Scene oScene)
        {
			if (ms_aOnUnloadedScene != null)
            {
				ms_aOnUnloadedScene(oScene);
				ms_aOnUnloadedScene = null;
            }
        }
		private void OnActiveScene(Scene oSceneA, Scene oSceneB)
		{
			if (ms_aOnActiveScene != null)
			{
				ms_aOnActiveScene(oSceneA, oSceneB);
				ms_aOnActiveScene = null;
			}
		}
		private void OnLoadedScene(Scene oScene, LoadSceneMode eMode)
		{
			if (ms_aOnLoadedScene != null)
			{
				ms_aOnLoadedScene(oScene, eMode);
				ms_aOnLoadedScene = null;

				if (m_eLoadSate == LoadedState.Sync)
					TriggerOnEnterScene();
			}
				
			/*Debug.LogWarning("SceneManager.sceneCount:" + SceneManager.sceneCount);
			for(int n=0; n<SceneManager.sceneCount; n++)
			{
				string output = "";
				Scene scene = SceneManager.GetSceneAt(n);
				output += scene.name + (scene.isLoaded ? " (Loaded, " : " (Not Loaded, ");
				output += (scene.isDirty ? " Dirty, " : " Clean, ") + scene.buildIndex + ")";
				Debug.LogWarning("output:" + output);
			}*/
		}
		private void TriggerOnEnterScene()
		{
			SyncSceneDataInfo();
            SceneShaderCheck();
            m_fOnEnterScene.Call(ms_nLoadingSceneId);
			ms_sLoadingSceneName = "";
			ms_nLoadingSceneId = -1;
			m_oLoadAsync = null;
		}

		private void SyncSceneDataInfo()
		{
			m_goMainCamera = GameObject.Find("Main Camera");
            m_goRootNode = GameObject.Find("rootnode");
			if (null != m_goRootNode)
			{
				//对象朝向及其旋转辅助参数
				Transform nodeTran = m_goRootNode.transform;
				m_quaRootRotation = nodeTran.rotation;
				m_v3RootRotAxis = nodeTran.up;

				//虚拟斜坡辅助参数
				float fAngle = Vector3.Angle(nodeTran.forward, Vector3.up);
				m_fTanAnglesX = Mathf.Tan(Mathf.Deg2Rad *(-fAngle));
			}
            InitSceneShader();
        }

        public void SetRootNodeHeight(float fRootNodeHeight)
        {
            m_fRootNodePosY = fRootNodeHeight;
            if (m_CameraShader != null)
            {
                if (m_CameraShader.transform.localScale.x > 1)
                {
                    return;
                }
                m_CameraShader.transform.localScale = new Vector3(m_fRootNodePosY * 1.2f, m_fRootNodePosY / 2, 1);

            }
        }

        private void InitSceneShader()
        {
            if (m_CameraShader == null)
            {
                m_CameraShader = ResourceMgr.LoadAssetEx("maps/prefabs/CameraShader", false);                
            }            
            Renderer shaderRender = m_CameraShader.GetComponent<Renderer>();
            m_CameraShaderMat = shaderRender.material;
            m_CameraShader.transform.SetParent(m_goMainCamera.transform.parent);
            Vector3 camerapos = m_goMainCamera.transform.localPosition;
            m_CameraShader.transform.localPosition = new Vector3(0, 0, camerapos.z+1);
            m_CameraShader.transform.localScale = new Vector3(1, 1, 1);
            if (m_goRootNode != null)
            {
                if(m_nWidth > 0 && m_nHeight > 0)
                {
                    m_CameraShader.transform.localScale = new Vector3(m_nWidth / 2, m_nHeight / 2, 1);
                    m_CameraShader.transform.localPosition = new Vector3(m_nWidth / 4, m_nHeight / 4, camerapos.z + 1);
                }
                else
                {
                    m_CameraShader.transform.localScale = new Vector3(m_goRootNode.transform.localPosition.y, m_goRootNode.transform.localPosition.y / 2, 1);
                    m_CameraShader.transform.localPosition = new Vector3(m_goRootNode.transform.localPosition.y, m_goRootNode.transform.localPosition.y / 2, camerapos.z + 1);
                }
                
            }
            
            m_CameraShader.SetActive(false);
        }

		// 同步转场景[bAsync=false] && 异步转场景[bAsync=true]
		private void changeScene(string path, bool bAsync, UnityAction<string> aLoaded = null, UnityAction<string> aUnLoaded = null)
        {
            //转场景之前的清理工作请添加到此处
			ResourceMgr.Instance.ClearInsGameObjOnChangeScene();
            ClearMapObjCache();
            m_SceneLightObj.Clear();
            m_vecSceneShader.Clear();
            m_bBlockCheck = false;
            m_goSceneNode = null;

            _ChangeScene(path, bAsync, aLoaded, aUnLoaded);
        }

		#region public (Export interface)
		public void ChangeScene(string path, bool bAsync, UnityAction<string> aLoaded = null, UnityAction<string> aUnLoaded = null)
		{
			Instance.changeScene(path, bAsync, aLoaded, aUnLoaded);
		}
		//Export_To_Lua
		public void ChangeSceneEx(int sceneid, string path, bool bAsync)
		{
			UnityAction<string> OnLoadedCal = (name) =>
			{
				m_fOnSceneLoaded.Call(sceneid);
			};
			UnityAction<string> OnUnLoadedCal = (name) =>
			{
				m_fOnSceneUnloaded.Call(sceneid);
			};
			ms_nLoadingSceneId = sceneid;
			Instance.changeScene(path, bAsync, OnLoadedCal, OnUnLoadedCal);
			//Instance.changeScene(path, bAsync);
		}
		public Quaternion GetRootNodeRot()
		{
			return m_quaRootRotation;
		}
		public Vector3 GetRootRotAxis()
		{
			return m_v3RootRotAxis;
		}
		public GameObject GetMainCameraObj()
		{
			return m_goMainCamera;
		}
		public Camera GetMainCamera()
		{
			if (null != m_goMainCamera)
				return m_goMainCamera.GetComponent<Camera>();
			return null;
		}
		public float GetGraphicsPosZ(float fGraphicsPosY)
		{
			float fHight = m_fTanAnglesX * Mathf.Abs(m_fRootNodePosY - fGraphicsPosY);
			return fHight + m_nRootHeight;
		}
        private GameObject GetSceneNode()
        {
            if (m_goSceneNode == null)
            {
                m_goSceneNode = GameObject.FindGameObjectWithTag("SceneNode");
            }
            return m_goSceneNode;
        }

        private void SceneShaderCheck()
        {
            GameObject[] SceneNodeInfo = GameObject.FindGameObjectsWithTag("ScreenShader");
            for (int i = 0; i < SceneNodeInfo.Length; i++)
            {
                GameObject sceneNode = SceneNodeInfo[i];
                Renderer render = sceneNode.GetComponent<Renderer>();

                Shader shader = render.material.shader;
                if (shader)
                {
                    render.material.shader = GameMgr.Instance.m_SceneShader;
                    m_vecSceneShader.Add(render.material);
                }
            }
        }
        public void SyncShderPos(float fRange, float fLuminance, float x, float y)
        {
            for (int i = 0; i < m_vecSceneShader.Count; i++)
            {
                Material mat = m_vecSceneShader[i];
                // mat.SetTexture("_Mask", m_shaderTexture);
                mat.SetFloat("_Range", fRange);
                mat.SetFloat("_Luminance", fLuminance);
                mat.SetFloat("_CoordX", x);
                mat.SetFloat("_CoordY", y);
            }
        }

        public void SetSceneBlockLoadOK()
        {
            m_bBlockCheck = true;
        }
        private int GetRateIndex(int[] vecBlockRate)
        {
            int nIndex = 0;
            int nTotalRate = 0;
            for(int n=0; n<vecBlockRate.Length; n++)
            {
                nTotalRate = nTotalRate + vecBlockRate[n];
            }
            int nRand = Random.Range(0, nTotalRate);
            int nDeltaRate = 0;
            for(int n=0; n<vecBlockRate.Length; n++)
            {
                nDeltaRate = nDeltaRate + vecBlockRate[n];
                if(nRand <= nDeltaRate)
                {
                    nIndex = n;
                    break;
                }
            }
            return nIndex;
        }
        public void GenerateMap(int nWidth, int nHeight, string sBlockInfo, string sBlockRate)
        {
            //传入的宽高是逻辑宽高 
            GameObject sceneNode = GetSceneNode();
            if (sceneNode == null)
            {
                Debug.LogError("GenerateMap Find NOT SceneNode!");
                return;
            }
            int nPixelWidth = nWidth * 50;
            int nPixelHeight = nHeight * 50;

            int nWidthCount = Mathf.CeilToInt(nPixelWidth / 512.0f);
            int nHeightCount = Mathf.CeilToInt(nPixelHeight / 512.0f);
            GameObject rootNode = GameObject.FindGameObjectWithTag("RootNode");
            if (rootNode != null)
            {
                rootNode.transform.position = new Vector3(0, nHeightCount*5.12f, 0);
            }
            m_nWidth = nWidth;
            m_nHeight = nHeight;
            BoxCollider boxcollider = sceneNode.GetComponent<BoxCollider>();
            if (boxcollider==null)
            {
                boxcollider = sceneNode.AddComponent<BoxCollider>();
                boxcollider.center = new Vector3(nWidth/4, nHeight/4, 0);                
                boxcollider.size = new Vector3(nWidth/2, nHeight/2, 1);
            }
            

            string[] vecBlockInfo = sBlockInfo.Split(';');
            string[] vecBlockRateInfo = sBlockRate.Split(';');
            int[] vecBlockRate = new int[vecBlockRateInfo.Length];
            for(int n=0; n<vecBlockRateInfo.Length; n++)
            {
                int nRate = 0;
                if(!int.TryParse(vecBlockRateInfo[n], out nRate))
                {
                    Debug.LogError("BlockRate Error:" + vecBlockRateInfo[n]);
                    return;
                }
                vecBlockRate[n] = nRate;
            }
            if(vecBlockInfo.Length == 0)
            {
                Debug.LogError("GenerateMap Error! BlockInfo error!");
                return;
            }
            if(vecBlockInfo.Length != vecBlockRate.Length)
            {
                Debug.LogError("GenerateMap Error! BlockRate NOT Match!");
                return;
            }

            string sScenePrefabPath = "maps/prefabs/";
            for(int i=0; i<nWidthCount; i++)
            {
                for(int j=0; j<nHeightCount; j++)
                {
                    int nIndex = GetRateIndex(vecBlockRate);
                    string sPrefab = vecBlockInfo[nIndex];
                    GameObject oBlock = ResourceMgr.LoadAssetEx(sScenePrefabPath + sPrefab, false);
                    if (oBlock)
                    {
                        oBlock.transform.SetParent(sceneNode.transform);
                        oBlock.transform.localPosition  = new Vector3(i*5.12f, j*5.12f, 0);

                        int insID = oBlock.GetInstanceID();
                        if (!m_dicMapObjCache.ContainsKey(insID))
                        {
                            m_dicMapObjCache[insID] = oBlock;
                        }
                    }
                }
            }
        }
        private void ClearMapObjCache()
        {
            foreach (int InsId in m_dicMapObjCache.Keys)
            {
                GameObject obj = m_dicMapObjCache[InsId];
                if (obj) ResourceMgr.UnLoadAsset(obj);
            }
            m_dicMapObjCache.Clear();
        }

        public GameObject GenerateBlockInfo(float nPosX, float nPosY, int nWidth, int nHeight, string sBlockName, float order)
        {
            GameObject sceneNode = GetSceneNode();
            if (sceneNode == null)
            {
                Debug.LogError("GenerateMap Find NOT SceneNode!");
                return null;
            }

            string sScenePrefabPath = "maps/prefabs/";
            GameObject oBlock = ResourceMgr.LoadAssetEx(sScenePrefabPath + sBlockName, false);
            if (oBlock)
            {
                oBlock.transform.SetParent(sceneNode.transform);
                oBlock.transform.localPosition = new Vector3(nPosX, nPosY, order);

                int insID = oBlock.GetInstanceID();
                if (!m_dicMapObjCache.ContainsKey(insID))
                {
                    m_dicMapObjCache[insID] = oBlock;
                }
                return oBlock;
            }
            return null;   
        }

        public void SetSceneLightObjState(bool bOpen)
        {
            if(m_SceneLightObj.Count == 0)
            {
                GameObject[] lightObj = GameObject.FindGameObjectsWithTag("Scenelight");
                for (int i = 0; i < lightObj.Length; i++)
                {
                    m_SceneLightObj.Add(lightObj[i]);
                }                
            }
            for(int m=0; m<m_SceneLightObj.Count; m++)
            {
                GameObject obj = m_SceneLightObj[m];                
                if (obj.activeSelf != bOpen)
                {
                    obj.SetActive(bOpen);
                }                
            }
        }
        public void SynCameraShader(float fRange, float fLuminance, float fPosx, float fPosy, float fAlpha)
        {
            if (m_CameraShaderMat != null)
            {
                if (!m_CameraShader.activeSelf)
                {
                    m_CameraShader.SetActive(true);
                }
                
                m_CameraShaderMat.SetFloat("_Range", fRange);
                m_CameraShaderMat.SetFloat("_Luminance", fLuminance);
                m_CameraShaderMat.SetFloat("_CoordX", fPosx);
                m_CameraShaderMat.SetFloat("_CoordY", fPosy);
                m_CameraShaderMat.SetFloat("_Alpha", fAlpha/255);
            }
        }
		#endregion
	}
}
