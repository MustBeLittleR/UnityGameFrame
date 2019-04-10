using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using LuaInterface;

namespace WCG
{
    //CD圈
    public class CDMask : MonoBehaviour
    {
        public float m_CDTime = 0.0f;
        public float m_EspTime = 0.0f;
        float m_centerX = 0.5f;
        float m_centerY = 0.5f;
        private bool m_bReverse = false;
        private Material m_NewMatrial = null;
        private bool m_bPause = false;
        Image m_oImg = null;
        RawImage m_rawImage = null;
        Text m_oCdTxt = null;
        Button m_oBtn = null;
        private LuaFunction m_fCal = null;

        void Awake()
        {
            Transform tCD = transform.Find("Cd");
            if (tCD) m_oCdTxt = tCD.GetComponent<Text>();
            if (m_oCdTxt) m_oCdTxt.gameObject.SetActive(false);
            m_oBtn = transform.GetComponent<Button>();

            m_oImg = gameObject.GetComponent<Image>();
            m_rawImage = gameObject.GetComponent<RawImage>();
            m_NewMatrial = new Material(GameMgr.Instance.m_UICDMaskMaterial);
            m_NewMatrial.SetFloat("_CenterX", m_centerX);
            m_NewMatrial.SetFloat("_CenterY", m_centerY);

            if (m_oImg != null)
                m_oImg.material = m_NewMatrial;
            else if (m_rawImage != null)
                m_rawImage.material = m_NewMatrial;
        }

        void OnDestroy()
        {
            if (m_NewMatrial != null)
            {
                Destroy(m_NewMatrial);
                m_NewMatrial = null;
            }
        }

        void Update()
        {
            if (m_CDTime <= 0)
            {
                return;
            }
            if (m_bPause)
            {
                return;
            }
            
            m_EspTime = m_EspTime - Time.deltaTime;
            if (m_EspTime > 0)
            {
                if (m_oImg != null && m_oImg.material != m_NewMatrial)
                    m_oImg.material = m_NewMatrial;
                else if (m_rawImage != null && m_rawImage.material != m_NewMatrial)
                    m_rawImage.material = m_NewMatrial;

                float fAmount = 1 - m_EspTime / m_CDTime;
                if (m_oCdTxt) m_oCdTxt.text = ((int)m_EspTime+1).ToString();
                m_NewMatrial.SetFloat("_Radian", fAmount * Mathf.PI * 2);
            }
            else
            {
                //目前只有异常显示在使用反转CD效果
                if (!m_bReverse)
				    ClearCD();
            }                        
        }

        public void SetCDTime(float nCDTime)
        {
            this.enabled = true;
			if (m_oCdTxt) m_oCdTxt.gameObject.SetActive(true);
			if (m_oBtn) m_oBtn.enabled = false;
			m_CDTime = nCDTime;
            m_EspTime = nCDTime;
            m_bPause = false;
        }

		public void SetMenPaiCDTime(float nCDTime)
		{
			if (m_EspTime > nCDTime)
				return;

			m_EspTime = nCDTime;
			if (m_oCdTxt) m_oCdTxt.gameObject.SetActive(false);
			if (m_oBtn) m_oBtn.enabled = false;
			m_CDTime = nCDTime;
			m_EspTime = nCDTime;
			m_bPause = false;
		}

		public void OnChangeCD(float fChangeRate)
        {
            float nCostCD = m_CDTime - m_EspTime;
            m_CDTime = m_CDTime * (1+fChangeRate);
            m_EspTime = m_CDTime - nCostCD;
        }

        public float GetEspTime() 
        {
            return m_EspTime;
        }

        public void ChangeEspTime(float nCDTime)
        {
            m_EspTime = m_EspTime - nCDTime;
        }

        public void SetEspTime(float nEspTime)
        {
            m_EspTime = nEspTime;
        }

		public void SetClearCDFun(LuaFunction fun)
		{
			m_fCal = fun;
		}

        public void SetPause(bool value)
        {
            m_bPause = value;
        }

        public void SetReverse(bool value)
        {
            m_bReverse = value;
            int nReverse = m_bReverse ? 0 : 1;
            if (m_NewMatrial != null)
            {
                m_NewMatrial.SetFloat("_Reverse", nReverse);
                m_NewMatrial.SetFloat("_Radian", 0);
            }            
        }

        public void ClearCD()
        {
            m_CDTime = 0.0f;
            m_EspTime = 0.0f;
            m_bPause = false;

			if (null != m_fCal)
				m_fCal.Call();

			if (m_oCdTxt) m_oCdTxt.gameObject.SetActive(false);
			if (m_oBtn) m_oBtn.enabled = true;
            if (m_NewMatrial != null)
            {
                m_NewMatrial.SetFloat("_Radian",6.5f);
            }
            this.enabled = false;
        }
    }
}
