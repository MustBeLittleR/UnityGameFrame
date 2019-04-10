using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;

namespace WCG
{
	public class CameraShotInterface
	{
        
        public static readonly CameraShotInterface Instance = new CameraShotInterface();
		static Camera m_oShotCamera = null;

		public CameraShotInterface()
		{}

        public static void RenderToTexture(GameObject oRiginalCamera, GameObject goImg, float fPosX, float fPosY, float fCamerSize, int nTexW, int nTexH)
        {
			if (m_oShotCamera == null)
			{
				Camera oRiCamera = oRiginalCamera.GetComponent<Camera>();
				m_oShotCamera = GameObject.Instantiate(oRiCamera) as Camera;
				m_oShotCamera.CopyFrom(oRiCamera);
				AudioListener audiolistener = m_oShotCamera.GetComponent<AudioListener>();
				audiolistener.enabled = false;
			}

			if (!m_oShotCamera)
				return;

			m_oShotCamera.targetTexture = new RenderTexture(nTexW, nTexH, 0);
			m_oShotCamera.transform.position = new Vector3(fPosX, fPosY, m_oShotCamera.transform.position.z);
			m_oShotCamera.orthographicSize = fCamerSize;

			UnityAction<Texture2D> aComplete = (texture) =>
			{
                RawImage img = goImg.GetComponent<RawImage>();
				img.texture = texture;
			};

            GameMgr.Instance.StartCoroutine(GenerateScreeShot(aComplete));
		}

		static IEnumerator GenerateScreeShot(UnityAction<Texture2D> aComplete = null)
		{
            //yield return new WaitForEndOfFrame();
            int width = m_oShotCamera.targetTexture.width;
			int height = m_oShotCamera.targetTexture.height;
			Texture2D screenShot = new Texture2D(width, height,TextureFormat.RGB24,false);
			m_oShotCamera.Render();
			RenderTexture.active = m_oShotCamera.targetTexture;
			screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
			screenShot.Apply();
			RenderTexture.active = null;
			yield return new WaitForEndOfFrame();
			Object.Destroy(m_oShotCamera.targetTexture);
			m_oShotCamera.targetTexture = null;
			Object.Destroy(m_oShotCamera.gameObject);
			m_oShotCamera = null;
			if (aComplete != null)
			{
				aComplete(screenShot);
			}
		}
    }
}

