using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;

namespace WCG
{
    public class UIMgr
    {
        public static readonly UIMgr Instance = new UIMgr();
        private Material m_DropColorMat;
        private Material m_ColorOffMat;

        private GameObject m_UIRoot;
        private GameObject m_UIRootDy;
        private Rect m_RootRect;
		private Vector2 m_RootXYRate = Vector2.zero;
		private static string ms_sUiPrefab = GameConfigMgr.ms_sUIFolderName + "/prefabs/";
		private static string ms_sUITexture = GameConfigMgr.ms_sUIFolderName + "/textures/";
		private static string ms_sAtlasFlag = "|";

        Vector3 m_TmplocalScale;
		Vector3 m_TmplocalPosition;
		Quaternion m_TmplocalRotation;
		Vector2 m_TmpanchoredPosition;
		Vector3 m_TmpanchoredPosition3D;
		Vector2 m_TmpoffsetMin;
		Vector2 m_TmpoffsetMax;

		public UIMgr(){}

        public void Init(){}
			
        public void Update(){}

        public void Destroy()
		{
			if (null != m_UIRoot)
			{
                ResourceMgr.UnLoadAsset(m_UIRoot);
				m_UIRoot = null;
			}

            if (null != m_UIRootDy)
            {
                ResourceMgr.UnLoadAsset(m_UIRootDy);
                m_UIRootDy = null;
            }
        }

		private GameObject LoadWndAsset(string assetPath, string assetName = null, GameObject parent = null, bool bAsync = false, bool bQueue = false, UnityAction<UnityEngine.Object> cal = null)
		{
			if (string.IsNullOrEmpty(assetPath))
				return null;

			if (string.IsNullOrEmpty(assetName))
                assetName = Path.GetFileNameWithoutExtension(assetPath);

			UnityAction<UnityEngine.Object> calEx = (obj) =>
			{
				if (null != parent)
					UIMgr.Instance.SyncRectTransform(obj, parent);
				if (null != cal) cal(obj);
			};

			if (bAsync)
			{
				ResourceMgr.LoadAssetAsync(assetPath, assetName, typeof(GameObject), calEx, bQueue);
				return null;
			}
			else
			{
				GameObject obj = ResourceMgr.LoadAsset(assetPath, assetName, typeof(GameObject)) as GameObject;
				calEx(obj);
				return obj;
			}
		}

		private void SyncRectTransform(UnityEngine.Object obj, GameObject parent)
		{
			GameObject oWin  = obj as GameObject;
			if (null != oWin && null != parent)
			{
				RectTransform rectransform = oWin.GetComponent<RectTransform>();
				if (rectransform != null)
				{
					m_TmplocalScale = rectransform.localScale;
					m_TmplocalPosition = rectransform.localPosition;
					m_TmplocalRotation = rectransform.localRotation;
					m_TmpanchoredPosition = rectransform.anchoredPosition;
					m_TmpanchoredPosition3D = rectransform.anchoredPosition3D;
					m_TmpoffsetMin = rectransform.offsetMin;
					m_TmpoffsetMax = rectransform.offsetMax;
					rectransform.SetParent(parent.GetComponent<RectTransform>());
					rectransform.localScale = m_TmplocalScale;
					rectransform.localPosition = m_TmplocalPosition;
					rectransform.localRotation = m_TmplocalRotation;
					rectransform.anchoredPosition = m_TmpanchoredPosition;
					rectransform.anchoredPosition3D = m_TmpanchoredPosition3D;
					rectransform.offsetMin = m_TmpoffsetMin;
					rectransform.offsetMax = m_TmpoffsetMax;
				}
			}
		}

		private GameObject GetUIRoot()
		{
			if (m_UIRoot == null)
			{
				m_UIRoot = GameObject.Find("UIRoot");
			}
			return m_UIRoot;
		}

        public GameObject GetUIRootDy()
        {
            if (m_UIRootDy == null)
            {
                m_UIRootDy = GameObject.Find("UIRootDy/layer1");
            }            
            return m_UIRootDy;
        }

        private void _SetImage(GameObject oWnd, string atlas, string imgName, bool bAsync = false, bool bQueue = true)
		{
			if (oWnd == null)
				return;

			Image oImage = oWnd.GetComponent<Image>();
			if (oImage == null)
			{
				Debug.LogWarning(oWnd.name + " donot contain a Component Image");
				return;
			}

			//Check Is Same Image or not
			string assetName = imgName;
			string assetPath = ms_sUITexture + atlas;
			string sprKeyName = Path.GetFileNameWithoutExtension(atlas) + ms_sAtlasFlag + assetName;
			if (null != oImage.sprite)
			{
				string sImgSprName = oImage.sprite.name;
				if (null != oImage.sprite.texture)
					sImgSprName = oImage.sprite.texture.name + ms_sAtlasFlag + sImgSprName;
				if (sImgSprName.Equals(sprKeyName))
					return;
			}

			UnityAction<UnityEngine.Object> acall = (spr) =>
			{
				Sprite oImgSp = (Sprite)spr;
				if (null != oImgSp)
				{
					ResourceMgr.UnLoadAsset(oImage.sprite);
					oImage.sprite = oImgSp;
				}
			};
			if (bAsync)
				ResourceMgr.LoadAssetAsync(assetPath, assetName, typeof(Sprite), acall, bQueue);
			else
				acall(ResourceMgr.LoadAsset(assetPath, assetName, typeof(Sprite)));
		}

		private void _SetSprite(GameObject oWnd, string atlas, string sprName, bool bAsync = false, bool bQueue = true)
		{
			if (oWnd == null)
				return;

			SpriteRenderer oSprRender = oWnd.GetComponent<SpriteRenderer>();
			if (oSprRender == null)
			{
				Debug.LogWarning(oWnd.name + " donot contain a Component SpriteRenderer");
				return;
			}

			//Check Is Same Image or not
			string assetName = sprName;
			string assetPath = ms_sUITexture + atlas;
			string sprKeyName = Path.GetFileNameWithoutExtension(atlas) + ms_sAtlasFlag + assetName;
			if (null != oSprRender.sprite)
			{
				string sRenSprName = oSprRender.sprite.name;
				if (null != oSprRender.sprite.texture)
				    sRenSprName = oSprRender.sprite.texture.name + ms_sAtlasFlag + sRenSprName;
				if (sRenSprName.Equals(sprKeyName))
					return;
			}

			UnityAction<UnityEngine.Object> acall = (spr) =>
			{
				Sprite oRenSpr = (Sprite)spr;
				if (null != oRenSpr)
				{
					ResourceMgr.UnLoadAsset(oSprRender.sprite);
					oSprRender.sprite = oRenSpr;
				}
			};
			if (bAsync)
				ResourceMgr.LoadAssetAsync(assetPath, assetName, typeof(Sprite), acall, bQueue);
			else
				acall(ResourceMgr.LoadAsset(assetPath, assetName, typeof(Sprite)));
		}


		#region public (Export interface)
		public Rect GetUIRootRect()
		{
			GameObject uiRoot = GetUIRoot();
			if (m_RootRect.height == 0.0f && null != uiRoot) 
			{
				Camera camera = Camera.main;
				CanvasScaler canvasScaler = uiRoot.GetComponent<CanvasScaler>();
				float nWidthRate = (float)camera.pixelHeight / (float)camera.pixelWidth;
				float nHeight = canvasScaler.referenceResolution.x * nWidthRate;
				m_RootRect = new Rect(0, 0, canvasScaler.referenceResolution.x, nHeight);
			}
			return m_RootRect;
		}

        public Vector2 GetRootXYRate()
        {
            if (m_RootXYRate == Vector2.zero)
            {
               Rect rect = GetUIRootRect();
               m_RootXYRate.x = rect.width / 800;
               m_RootXYRate.y = rect.height / 480;
            }
            return m_RootXYRate;
        }

		public Camera GetUICamera()
		{
			GameObject uiRoot = GetUIRoot();
			Transform CameraTrans = uiRoot.transform.Find("UICamera");
			if (CameraTrans)
			{
				return CameraTrans.GetComponent<Camera>();
			}
			return null;
		}

		//根据具体的屏幕分辨率做摄像机的缩放适配
		public void UICameraSizeAdapt()
		{
			GameObject uiRoot = GetUIRoot();
			if (null != uiRoot)
			{
				Camera uiCamera = GetUICamera();
				//因为特效是在16:9下做的，这里根据具体的屏幕分辨率做摄像机的缩放适配
				if (uiCamera != null)
				{
					//uiCamera.orthographicSize = 500 * 16 / 9 * Camera.main.pixelHeight / Camera.main.pixelWidth;
				} 
			}
		}

		//通过相对路径获取对应gameObject
		public GameObject GetWindow(GameObject parent, string relativePath)
		{
			if (null == parent) return null;
			Transform obj = parent.transform.Find(relativePath);
			return null != obj ? obj.gameObject : null;
		}

		//pathName: Assets\Data\ui\prefabs下的相对路径
		public GameObject CreateUI(string pathName)
		{
			return CreateUI(pathName, null);
		}

		//pathName: Assets\Data\ui\prefabs下的相对路径
		public GameObject CreateUI(string pathName, GameObject parent)
		{
			if (string.IsNullOrEmpty(Path.GetExtension(pathName)))
			{
				if (!pathName.EndsWith(".prefab"))
					pathName = pathName + ".prefab";
			}
			GameObject gameobj = LoadWndAsset(ms_sUiPrefab + pathName, null, parent);
			if (null == gameobj)
				Debug.LogWarning("UIMgr.CreateUI Failed:" + pathName);
			return gameobj;
		}

		public void DestroyUI(GameObject rootWin)
		{
			if (null != rootWin)
			{
                ResourceMgr.UnLoadAsset(rootWin);
				rootWin = null;
			}
		}

		public GameObject CreateUIByCache(string pathName, GameObject parent)
		{
			GameObject goCacheUI = ResourceMgr.LoadAssetEx(ms_sUiPrefab + pathName, false);

			if (goCacheUI && parent)
			{
				goCacheUI.transform.localScale = Vector3.one;
				UIMgr.Instance.SyncRectTransform(goCacheUI, parent);
			}

			return goCacheUI;
		}

		public void MoveToFront(GameObject window)
		{
			if (window != null)
				window.transform.SetAsLastSibling();
		}

		public Vector2 GetWinScreenPos(GameObject window)
		{
			//Rect TempRect = GetUIRootRect();
			//Vector2 Vec2 = GetRootXYRate();
			Camera uiCamera = GetUICamera();
			//Vector2 TempVec2 = uiCamera.WorldToViewportPoint(window.transform.position);
			Vector3 Vec3 = uiCamera.WorldToScreenPoint(window.transform.position);
			Vector2 screenPos = new Vector2(Vec3.x, Vec3.y);
			return screenPos;
		}

        public void SetAnchorPos(GameObject window, float  anchorX, float anchorY)
        {
            if (window == null) return;
            RectTransform recTrans = window.GetComponent<RectTransform>();
            if (recTrans != null)
            {
                recTrans.anchoredPosition = new Vector2(anchorX, anchorY);
            }
        }

        public void ResizeWin(GameObject window, float width, float height)
		{
			if (window == null) return;

			RectTransform rectrans = window.GetComponent<RectTransform>();
			if (rectrans == null) return;

			if (width == -1)
				width = rectrans.rect.width;
			if (height == -1)
				height = rectrans.rect.height;
			rectrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
			rectrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
		}

		public Vector2 GetWinPixelSize(GameObject go)
		{
			Vector2 size = new Vector2(0, 0);
			Camera camera = GetUICamera();
			if (camera == null) return size;

			GameObject uiRoot = GetUIRoot();
			CanvasScaler canvasScaler = uiRoot.GetComponent<CanvasScaler>();

			float rate = camera.pixelWidth / canvasScaler.referenceResolution.x;
			if (go == null) return size;

			RectTransform trans = go.GetComponent<RectTransform>();
			float width = trans.rect.width * rate;
			float height = trans.rect.height * rate;
			size = new Vector2(width, height);
			return size;
		}

		public void SetColorGrey(GameObject oWnd)
		{
            if (oWnd == null)
            {
                return;
            }

            if (m_ColorOffMat == null)
            {
                m_ColorOffMat = GameMgr.Instance.m_ColorOffMat;
            }
            if (m_ColorOffMat == null)
            {
                return;
            }

            Image img = oWnd.GetComponent<Image>();
            if (img != null)
            {
                img.material = m_ColorOffMat;
            }
            Image[] images = oWnd.GetComponentsInChildren<Image>(true);
            for (int i = 0; i < images.Length; ++i)
            {
                images[i].material = m_ColorOffMat;
            }
        }

      
        public void DropColor(GameObject oWnd)
		{
            if (oWnd == null)
            {
                return;
            }
            if (m_DropColorMat == null)
            {
                GameObject prefab = LoadWndAsset("DropColorMatPref");
                if (prefab != null)
                {
                    Image image = prefab.GetComponent<Image>();
                    if (image != null)
                    {
                        m_DropColorMat = image.material;
                    }
                }
            }

            if (m_DropColorMat == null)
            {
                return;
            }

            Image img = oWnd.GetComponent<Image>();
            if (img != null)
            {
                img.material = m_DropColorMat;
            }
            Image[] images = oWnd.GetComponentsInChildren<Image>(true);
            for (int i = 0; i < images.Length; ++i)
            {
                images[i].material = m_DropColorMat;
            }
        }

		public void RecoverColor(GameObject oWnd)
		{
			if (oWnd == null)
			{
				return;
			}

			Image img = oWnd.GetComponent<Image>();
			if (img != null)
			{
				img.material = img.defaultMaterial;
			}
			Image[] images = oWnd.GetComponentsInChildren<Image>(true);
			for (int i = 0; i < images.Length; ++i)
			{
				if (m_DropColorMat)
				{
					if (images[i].material.name.Contains(m_DropColorMat.name))
					{
						images[i].material = images[i].defaultMaterial;
					}
				}
				if (m_ColorOffMat)
				{
					if (images[i].material.name.Contains(m_ColorOffMat.name))
					{
						images[i].material = images[i].defaultMaterial;
					}
				}
			}
		}

		public void SetAllLineRenderer(GameObject go, float LineLen)
		{
			if (go != null)
			{
				Component[] comList = go.GetComponentsInChildren<LineRenderer>();
				for (int i = 0; i < comList.Length; i++)
				{
					Component com = comList[i];
					Vector3 CurVec3 = com.transform.localScale;
					com.transform.localScale = new Vector3(CurVec3.x, CurVec3.y, LineLen);
				}
			}
		}

		public static void ApplyParentLayer(GameObject go)
		{
			if (go == null || go.transform.parent == null) return;
			SetLayer(go, go.transform.parent.gameObject.layer);
		}

		public static void SetLayer(GameObject go, int nLayer)
		{
			if (go == null) return;
			go.layer = nLayer;
			for (int i = 0; i < go.transform.childCount; i++)
			{
				Transform child = go.transform.GetChild(i);
				ApplyParentLayer(child.gameObject);
			}
		}

		public void SetText(GameObject oTextWnd, string txt)
		{
			if (oTextWnd == null) return;
			Text text = oTextWnd.GetComponent<Text>();
			if (text == null) return;
			text.text = txt;
		}

		public void SetTextColor(GameObject oTextWnd, int a, int g, int b)
		{
			if (oTextWnd == null) return;
			Text text = oTextWnd.GetComponent<Text>();
			if (text == null) return;
			text.color = new Color((float)a/255.0f, (float)g/255.0f, (float)b/255.0f);
		}

		//eg: atlas = "uipic/zhandouziti.png", imgName = "zhandouziti_0"
		public void SetImage(GameObject oWnd, string atlas, string imgName)
		{
			_SetImage(oWnd, atlas, imgName, false, false);
		}
		public void SetImageAsyn(GameObject oWnd, string atlas, string imgName)
		{
			_SetImage(oWnd, atlas, imgName, true, false);
		}
		public void ClearImage(GameObject oWnd)
		{
			if (oWnd == null)
				return;

			Image img = oWnd.GetComponent<Image>();
			if (img == null)
				return;

			if (img.sprite)
			{
				ResourceMgr.UnLoadAsset(img.sprite);
				img.sprite = null;
			}
		}

		public void SetImageColor(GameObject oWnd, float r, float g, float b, float a)
		{
			if (oWnd == null) return;
			Image img = oWnd.GetComponent<Image>();
			if (img == null) return;
			img.color = new Color(r, g, b, a);
		}

		//eg: atlas = "uipic/zhandouziti.png", imgName = "zhandouziti_0"
		public void SetSprite(GameObject oWnd, string atlas, string imgName)
		{
			_SetSprite(oWnd, atlas, imgName, false, false);
		}
		public void SetSpriteAsyn(GameObject oWnd, string atlas, string imgName)
		{
			_SetSprite(oWnd, atlas, imgName, true, false);
		}
		public void SetSpriteColor(GameObject oWnd, float r, float g, float b, float a)
		{
			if (oWnd == null) return;
			SpriteRenderer sprite = oWnd.GetComponent<SpriteRenderer>();
			if (sprite == null) return;
			sprite.color = new Color(r, g, b, a);
		}
		public void SetParentNULL(GameObject oWnd)
		{
			if (oWnd == null) return;
			oWnd.transform.SetParent(null);
		}
		public void DropdownAddOptions(GameObject oWnd, object[] args)
		{
			if (oWnd == null) return;

			Dropdown oDrop = oWnd.GetComponent<Dropdown>();
			if (oDrop == null) return;

			List<string> Options = new List<string>();
			for (int i=0;i<args.Length;i++)
				Options.Add((string)args[i]);

			oDrop.AddOptions(Options);
		}

        public void SetAnimationRate(Animation oAanimation, string sAniName, float fAnimationRate)
        {
            if(oAanimation == null)
            {
                return;
            }

            AnimationState oState = oAanimation[sAniName];
            if(oState == null)
            {
                return;
            }

            oState.normalizedTime = fAnimationRate;
        }
		#endregion
	}
}