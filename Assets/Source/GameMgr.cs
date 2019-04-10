using UnityEngine;
using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;
using System.Net.NetworkInformation;

namespace WCG
{
    public class GameMgr : MonoBehaviour
    {
        public static GameMgr Instance = null;
        public static LuaScriptMgr ms_luaMgr = null;
        public static SceneMgr ms_sceneMgr = null;
        public static ResourceMgr ms_resourceMgr = null;
        public static UIMgr ms_uiMgr = null;

		public readonly RunMode m_eRunMode = GameConfigMgr.ms_eArchive;

		public Shader m_HighlightShader;
		public Shader m_OutlineShader;
        public Shader m_CharacterOutLightShader;
		public Shader m_TrasparentHighlightShader;
		public Shader m_FadeoutShader;
		public Shader m_FossilisedShader;
		public Texture m_FossilisedMap;
		public Texture m_DissolveTexture;

		public Shader m_TintColorShader;
		public Shader m_DissolveShader;
		public Shader m_CharacterHighlightShader;

		public Shader m_FrozenShader;
		public Texture m_FrozenTexture;

		public Material m_UICDMaskMaterial;

		public AfterImageEffect afterImageEffect;
		public AfterImageWithScaleEffect afterImageWithScaleEffect;
        public Material m_ColorOffMat;
        public Shader m_SceneShader;        
		//场景物件根节点
		Transform m_ObjRootTran = null;

		bool m_bShowFPS = false;
		float m_fFpsCost = .0f;
		float m_fFPS = 0.0f;
		int m_nFpsFrame = 0;

        GameObject m_SDKEntryObj = null;
        SDKEntry m_SDKEntry = null;
        void Awake()
        {
			Instance = this;
			InitObjRoot();
			DontDestroyOnLoad(gameObject);            
		}

        void Start()
        {
            float a = Profile.ProfileBegin();
            ms_luaMgr = LuaScriptMgr.Instance;
			ms_resourceMgr = ResourceMgr.Instance;
            ms_sceneMgr = SceneMgr.Instance;
            ms_uiMgr = UIMgr.Instance;
			ms_resourceMgr.Init();
			Profile.ProfileEnd("Mgr To Instance", a);

            a = Profile.ProfileBegin();
			ms_sceneMgr.Init();
			Profile.ProfileEnd("sceneMgr.Init", a);

			a = Profile.ProfileBegin();
			ms_uiMgr.Init();
			Profile.ProfileEnd("ms_uiMgr.Init", a);

            a = Profile.ProfileBegin();
			ms_luaMgr.Init();
			Profile.ProfileEnd("luaMgr.Init", a);
            ms_luaMgr.PrintLuaUsedMem();

            InitSDKEntry();
        }

		void Update()
        {
			ConnectionMgr.instance.LoopDispatchRecvData();
			ms_luaMgr.Update();
            ms_resourceMgr.Update();

            if (m_bShowFPS) UpdateFPS();            
		}
		void LateUpdate()
		{
			ms_luaMgr.LateUpdate();
		}
		void FixedUpdate()
		{
			ms_luaMgr.FixedUpdate();
		}

		void UpdateFPS()
		{
			m_nFpsFrame++;
			m_fFpsCost += Time.deltaTime;
			if (m_fFpsCost > .5f)
			{
				m_fFPS = m_nFpsFrame / m_fFpsCost;
				m_fFpsCost = .0f;
				m_nFpsFrame = 0;
			}
		}

		void OnGUI()
		{
			if (m_bShowFPS)
				GUI.Label(new Rect(Screen.width-100, Screen.height-20, 100, 20), "FPS: " + m_fFPS);
		}

		void InitObjRoot()
		{
			if (m_ObjRootTran) return;
			GameObject ObjRoot = new GameObject("ObjRoot");
            DontDestroyOnLoad(ObjRoot);
            m_ObjRootTran = ObjRoot.transform;
			m_ObjRootTran.position = Vector3.zero;
			m_ObjRootTran.eulerAngles = new Vector3(-30, 0, 0);

            //BoxCollider
            GameObject goCollider = new GameObject("BoxCollider");
            goCollider.transform.SetParent(ObjRoot.transform);
            goCollider.transform.position = new Vector3(128, 128, 0);
            BoxCollider oCollider = goCollider.AddComponent<BoxCollider>();
            oCollider.size = new Vector3(256, 256, 1);
        }

		public bool SetParentByRoot(GameObject childObj)
		{
			if (null==childObj || null==m_ObjRootTran)
				return false;

			Transform objTran = childObj.transform;
			objTran.SetParent(m_ObjRootTran);
			objTran.localEulerAngles = Vector3.zero;
			objTran.position = Vector3.zero;
			return true;
		}

        public void InitSDKEntry()
        {
            if (m_SDKEntryObj)
            {
                return;
            }

            GameObject oObjSDKEntry = new GameObject("SDKEntry");
            DontDestroyOnLoad(oObjSDKEntry);
            m_SDKEntryObj = oObjSDKEntry;
            m_SDKEntry = m_SDKEntryObj.AddComponent<SDKEntry>();
        }

        public int GetChannel()
        {
            return SDKEntryMgr.GetChannel();
        }

        public int GetChanTag()
        {
            return SDKEntryMgr.GetChanTag();
        }

        public string GetGameVersion()
        {
            return "";
        }

        public void SetShowFPS(bool bShow)
		{
			m_bShowFPS = bShow;
		}

		public void SetFrameRate(int frameRate)
		{
			Application.targetFrameRate = frameRate;
		}

		public void InitLuaCallBackFunc()
		{
			ms_sceneMgr.InitSceneCallBack();
			ms_resourceMgr.InitResLoadCallBack();
		}

		public void SetLocalVal(string sKey, string sVal)
		{
			PlayerPrefs.SetString(sKey, sVal);
		}

		public string GetLocalVal(string sKey)
		{
			return PlayerPrefs.GetString(sKey, "");
		}

		void OnDestroy()
        {
			if (m_ObjRootTran)
			{
				UnityEngine.Object.Destroy(m_ObjRootTran.gameObject);
				m_ObjRootTran = null;
			}

			ConnectionMgr.instance.Close();
			ms_luaMgr.Destroy();
            ms_resourceMgr.Destroy();
        }

        #region public (Export interface)

        public static bool IsIPhone()
        {
#if UNITY_IPHONE 
            return true;
#else
            return false;
#endif
        }

        public static bool IsWin()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            return true;
#else
            return false;
#endif
        }

        public static bool IsAndroid()
        {
#if UNITY_ANDROID
            return true;
#else
            return false;
#endif
        }
        public void Exit()
		{
			#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
			#else
			Application.Quit();
			#endif
		}
		#endregion
    }
}
