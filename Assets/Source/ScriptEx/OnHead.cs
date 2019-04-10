using UnityEngine;
using UnityEngine.UI;

namespace WCG
{
    public class OnHead : MonoBehaviour
    {
        public RectTransform m_RectTrans = null;
        bool m_bHeadShow = true;

        //名字
        public GameObject m_goName = null;
        public Text m_NameTxt = null;
        bool m_bNameShow = false;
        string m_sName = "";

        //血条
        public GameObject m_goBlood = null;
        public Slider m_BloodSlider = null;
        public Text m_BloodText = null;
        public GameObject m_goBloodText = null;
        bool m_bBloodShow = false;
        bool m_bBloodTextShow = false;
        float m_fBloodRate = 0;
        string m_sBloodTxt = "";

        //操作进度
        public GameObject m_goProgress = null;
        public Slider m_ProgressSlider = null;
        bool m_bProgressShow = false;
        float m_fProgressRate = 0;
        float m_fProTime = 0;             //初始时间
        float m_fProLostTime = 0;         //流失时间

        //说话类容
        public GameObject m_goPopMsg = null;
        bool m_bPopMsgShow = false;
        public Text m_PopTxt = null;
        string m_sPopTxt = "";

        //气泡动画提示
        public GameObject m_goMsgAni = null;
        bool m_bMsgAniShow = false;

        public GameObject m_goDiet = null;
        bool m_bDietShow = false;

        Transform m_trDummy = null;
        bool m_bChangeVal = false;
        bool m_bChangeShow = false;

		public GameObject m_goCamp = null;
		bool m_bCampShow = false;


		public GameObject m_goFight = null;
		bool m_bFightShow = false;

		void Awake()
		{
			m_bNameShow = m_goName.activeSelf;
			m_bBloodShow = m_goBlood.activeSelf;
			m_bProgressShow = m_goProgress.activeSelf;
			m_bPopMsgShow = m_goPopMsg.activeSelf;
			m_bMsgAniShow = m_goMsgAni.activeSelf;
			m_bDietShow = m_goDiet.activeSelf;
            m_bBloodTextShow = m_goBloodText.activeSelf;

			m_bCampShow = m_goCamp.activeSelf;
			m_bFightShow = m_goFight.activeSelf;
		}

        // Use this for initialization
        /*void Start()
        {
        }*/

        void OnEnable()
        {
            m_bNameShow = m_goName.activeSelf;
            m_bBloodShow = m_goBlood.activeSelf;
            m_bProgressShow = m_goProgress.activeSelf;
            m_bPopMsgShow = m_goPopMsg.activeSelf;
            m_bMsgAniShow = m_goMsgAni.activeSelf;
            m_bDietShow = m_goDiet.activeSelf;
            m_bBloodTextShow = m_goBloodText.activeSelf;

			m_bCampShow = m_goCamp.activeSelf;
			m_bFightShow = m_goFight.activeSelf;
        }

        public void SetGraphicChar(RenderObj oGraphicChar)
        {
            if (oGraphicChar != null)
            {
                enabled = true;
                m_trDummy = oGraphicChar.GetDummyFromCache("Dummy_top2");
                if (m_trDummy == null)
                    m_trDummy = oGraphicChar.transform;
                UpdateUIPos();
            }
            else
            {
                enabled = false;
                m_trDummy = null;
            }
        }

        public void SetNameShow(bool bShow)
        {
			m_bChangeShow = m_goName.activeSelf != bShow || m_bChangeShow;
            m_bNameShow = bShow;
        }
        public void SetBloodShow(bool bShow)
        {
			m_bChangeShow = m_goBlood.activeSelf != bShow || m_bChangeShow;
            m_bBloodShow = bShow;
        }
        public void SetBloodTxtShow(bool bShow)
        {
            m_bChangeShow = m_goBloodText.activeSelf != bShow || m_bChangeShow;
            m_bBloodTextShow = bShow;
        }
        public void SetProShow(bool bShow)
        {
			m_bChangeShow = m_goProgress.activeSelf != bShow || m_bChangeShow;
            m_bProgressShow = bShow;
        }
        public void SetPopMsgShow(bool bShow)
        {
			m_bChangeShow = m_goPopMsg.activeSelf != bShow || m_bChangeShow;
            m_bPopMsgShow = bShow;
        }
        public void SetMsgAniShow(bool bShow)
        {
			m_bChangeShow = m_goMsgAni.activeSelf != bShow || m_bChangeShow;
            m_bMsgAniShow = bShow;
        }
        public void SetDietShow(bool bShow)
        {
			m_bChangeShow = m_goDiet.activeSelf != bShow || m_bChangeShow;
            m_bDietShow = bShow;
        }

		public void SetCampShow(bool bShow)
		{
			m_bChangeShow = m_goCamp.activeSelf != bShow || m_bChangeShow;
			m_bCampShow = bShow;
		}

		public void SetFightShow(bool bShow)
		{
			m_bChangeShow = m_goFight.activeSelf != bShow || m_bChangeShow;
			m_bFightShow = bShow;
		}

        public void SetHeadShow(bool bShow)
        {
            if (gameObject.activeSelf != bShow)
            {
                m_bHeadShow = bShow;
                gameObject.SetActive(bShow);
            }
        }
        
        public void SetName(string sName)
        {
			m_bChangeVal = !m_sName.Equals(sName) || m_bChangeVal;
            m_sName = sName;
        }
        public void SetNameColor( int a, int g, int b)
        {    
            m_NameTxt.color = new Color((float)a/255.0f, (float)g/255.0f, (float)b/255.0f);
        }
        public void SetBloodRate(float fRate)
        {
			m_bChangeVal = m_BloodSlider.value != fRate || m_bChangeVal;
            m_fBloodRate = fRate;
        }
        public void SetBloodTxt(string sTxt)
        {
			m_bChangeVal = !m_sBloodTxt.Equals(sTxt) || m_bChangeVal;
            m_sBloodTxt = sTxt;
        }
        public void SetProRate(float fRate)
        {
			m_bChangeVal = m_ProgressSlider.value != fRate || m_bChangeVal;
            m_fProgressRate = fRate;
        }
        public void SetProTime(float fTime)
        {
            if (fTime > 0)
            {
                SetProShow(true);
                m_fProTime = fTime;
                m_fProLostTime = 0;
            }
            else 
            {
                SetProRate(0);
                m_fProTime = 0;
                m_fProLostTime = 0;
                SetProShow(false);
            }

        }
        public void UpdateProRate()
        {
			m_fProLostTime += Time.deltaTime;
			float fRate = m_fProLostTime / m_fProTime;
			if (fRate <= 1)
				SetProRate (fRate);
			else
				SetProTime (-1);
        }
        public void SetPopTxt(string sPopTxt)
        {
			m_bChangeVal = !m_sPopTxt.Equals(sPopTxt) || m_bChangeVal;
            m_sPopTxt = sPopTxt;
        }

        public void Release()
        {
            Update();
            SetGraphicChar(null);
            ResourceMgr.PutInsGameObj(gameObject, true);
            m_trDummy = null;
        }

        public void UpdateUIPos()
        {
            Camera goMainSceneCamera = SceneMgr.Instance.GetMainCamera();
            if (goMainSceneCamera != null)
            {
                Rect TempRect = UIMgr.Instance.GetUIRootRect();
                Vector2 TempVec2 = goMainSceneCamera.WorldToViewportPoint(m_trDummy.position);
                m_RectTrans.anchoredPosition = new Vector2(TempVec2.x * TempRect.width, TempVec2.y * TempRect.height);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!m_trDummy || !m_bHeadShow)
                return;

            UpdateUIPos();
            if (m_fProTime > 0)
                UpdateProRate();

            if (m_bChangeVal)
            {
                m_bChangeVal = false;

                if (!m_NameTxt.text.Equals(m_sName))
                    m_NameTxt.text = m_sName;

                if (!m_BloodText.text.Equals(m_sBloodTxt))
                    m_BloodText.text = m_sBloodTxt;

                if (m_BloodSlider.value!=m_fBloodRate)
                    m_BloodSlider.value = m_fBloodRate;

                if (m_ProgressSlider.value!=m_fProgressRate)
                    m_ProgressSlider.value = m_fProgressRate;

                if (!m_PopTxt.text.Equals(m_sPopTxt))
                    m_PopTxt.text = m_sPopTxt;
            }

            if (m_bChangeShow)
            {
                m_bChangeShow = false;

                if (m_goName.activeSelf != m_bNameShow)
                    m_goName.SetActive(m_bNameShow);

                if (m_goBlood.activeSelf != m_bBloodShow)
                    m_goBlood.SetActive(m_bBloodShow);

                if (m_goBloodText.activeSelf != m_bBloodTextShow)
                    m_goBloodText.SetActive(m_bBloodTextShow);

                if (m_goProgress.activeSelf != m_bProgressShow)
                    m_goProgress.SetActive(m_bProgressShow);

                if (m_goPopMsg.activeSelf != m_bPopMsgShow)
                    m_goPopMsg.SetActive(m_bPopMsgShow);

                if (m_goMsgAni.activeSelf != m_bMsgAniShow)
                    m_goMsgAni.SetActive(m_bMsgAniShow);

                if (m_goDiet.activeSelf != m_bDietShow)
                    m_goDiet.SetActive(m_bDietShow);

				if (m_goCamp.activeSelf != m_bCampShow)
					m_goCamp.SetActive(m_bCampShow);

				if (m_goFight.activeSelf != m_bFightShow)
					m_goFight.SetActive(m_bFightShow);

            }
        }
    }
}

