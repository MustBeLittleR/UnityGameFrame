using UnityEngine;
using System.Collections;

namespace WCG
{
    public class FloatUIDestroy : MonoBehaviour
    {
        public GameObject m_DestroyObj = null;
        private bool m_bUpdate = false;
        private RectTransform m_DestoryObjRectTran = null;
        private Vector3 m_Vec3Position;
        private float mOffsetX = .0f;
        private float mOffsetY = .0f;


        // Use this for initialization
        void Start()
        {
            m_DestoryObjRectTran = m_DestroyObj.GetComponent<RectTransform>();
        }

        // Update is called once per frame
        public void OnDestroyCache()
        {
            m_bUpdate = false;
            ResourceMgr.PutInsGameObj(m_DestroyObj, true);
        }

        public void SetFloatPosition(Vector3 Vec3Position, float OffsetX, float OffsetY)
        {
            if (m_DestoryObjRectTran != null)
            {
                mOffsetX = OffsetX;
                mOffsetY = OffsetY;
                m_bUpdate = true;
                m_Vec3Position = Vec3Position;
				m_DestroyObj.SetActive (true);
            }
        }

        void Update()
        {
            if (!m_bUpdate)
                return;

            Camera goMainSceneCamera = SceneMgr.Instance.GetMainCamera();
            if (goMainSceneCamera != null)
            {
                Rect TempRect = UIMgr.Instance.GetUIRootRect();
                Vector2 TempVec2 = goMainSceneCamera.WorldToViewportPoint(m_Vec3Position);
				m_DestoryObjRectTran.anchoredPosition = new Vector2(TempRect.width * (TempVec2.x + mOffsetX / 800), TempRect.height * (TempVec2.y + mOffsetY / 480));
				//m_DestoryObjRectTran.anchoredPosition = new Vector2(TempRect.width * (TempVec2.x) +  mOffsetX, TempRect.height * (TempVec2.y + mOffsetY));
            }
        }
    }
}