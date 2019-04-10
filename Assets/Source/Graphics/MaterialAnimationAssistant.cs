using UnityEngine;
//[ExecuteInEditMode]
//[AddComponentMenu("")]
namespace WCG
{
    public class MaterialAnimationAssistant : MonoBehaviour
    {
        SkinnedMeshRenderer m_Renderer = null;
        SkinnedMeshRenderer m_ExtraRenderer = null;
        public float HighLight = 0;
        public Color HighLightColor = Color.white;
        public Color FadeoutColor = Color.clear;
        public Color OutlineColor = Color.red;
        public Color MainBlendColor = Color.red;
        public float OutlineWidth = 0.02f;
        public Vector3 ShakeOffset = new Vector3(1, 1, 1);
        public float ShakeAmp = 0.25f;
        Color m_CurrentColor;
        public Shader m_HighlightShader = GameMgr.Instance.m_HighlightShader;
        public Shader m_OutlineShader = GameMgr.Instance.m_CharacterOutLightShader;
        public Shader m_FadeoutShader = GameMgr.Instance.m_FadeoutShader;
        public Shader m_FossilisedShader = GameMgr.Instance.m_FossilisedShader;
        public Texture m_FossilisedMap = GameMgr.Instance.m_FossilisedMap;
        Material[] m_OriginMaterial;
        Material[] TempMaterial;
        Material[] m_ExtraOriginMaterial;
        Material[] ExtraTempMaterial;

        public float HighLightDuration = 0.15f;
        float m_HighLightRestTime = -1;
        public float MaxHighLight = 0.1f;
        public float FadeoutDuration = 0.67f;
        float m_FadeoutRestTime = -1;
        bool FossilisedDuration = false;

		bool FrozenDuration = false;
		public Shader m_FrozenShader = GameMgr.Instance.m_FrozenShader;
		public Texture m_FrozenTexture = GameMgr.Instance.m_FrozenTexture;

        public AfterImageEffect afterImageEffect = GameMgr.Instance.afterImageEffect;
        AfterImageEffect copy_afterImageEffect;
        public AfterImageWithScaleEffect afterImageWithScaleEffect = GameMgr.Instance.afterImageWithScaleEffect;
        public bool isEmit = false;
        public float EmitInterval = 1;
        float EmitTicker = 0;
        bool isOutLine = false;
        Vector3 m_OriginalPosition;

        // Use this for initialization
        void Awake()
        {
			SkinnedMeshRenderer[] arrRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
			for (int i=0;i< arrRenderers.Length; i++)
			{
				SkinnedMeshRenderer renderer = arrRenderers[i];
				string sNodeName = renderer.gameObject.name;
				if (renderer && renderer.gameObject.GetComponent<Renderer>() && !sNodeName.Contains("shadow") && !sNodeName.Contains("outline"))
				{
                    if (m_Renderer == null) {
                        m_Renderer = renderer;
                    }else if(m_ExtraRenderer == null){
                        m_ExtraRenderer = renderer;
                    }                    
				}
			}
                        

            if (GetComponent<CharacterTransparentEffect>())
            {
                m_HighlightShader = GameMgr.Instance.m_TrasparentHighlightShader;
            }
            if (m_Renderer)
            {
                m_OriginalPosition = m_Renderer.transform.localPosition;
            }
            
        }

        public SkinnedMeshRenderer GetSkinnedMeshRenderer()
        {
            return m_Renderer;
        }

        void Update()
        {
            ProcessHighLight();

            if (!isEmit)
            {
                if (copy_afterImageEffect)
                {
                    copy_afterImageEffect = (AfterImageEffect)GameObject.Instantiate(afterImageEffect, m_Renderer.transform.position, m_Renderer.transform.rotation);
                    copy_afterImageEffect.initSkinnedMeshAfterImage(m_Renderer, 1, true);
                    copy_afterImageEffect = null;
                }
                return;
            }

            if (EmitTicker > EmitInterval)
            {
                copy_afterImageEffect = (AfterImageEffect)GameObject.Instantiate(afterImageEffect, m_Renderer.transform.position, m_Renderer.transform.rotation);
                copy_afterImageEffect.initSkinnedMeshAfterImage(m_Renderer, 1);
                EmitTicker -= EmitInterval;
            }
            EmitTicker += Time.deltaTime;
        }

		void LateUpdate()
		{
			if (m_OriginMaterial==null || FossilisedDuration || FrozenDuration)
			{
				return;
			}
			float freqAmp = HighLight * 3;
			if (freqAmp >= 1)
			{
				m_Renderer.transform.localPosition = m_OriginalPosition;
				return;
			}
			m_Renderer.transform.localPosition = m_OriginalPosition + new Vector3(freqAmp, 1 - freqAmp, freqAmp) * ShakeAmp;
			ShakeAmp = -ShakeAmp;
		}

		void ProcessHighLight()
        {
            if (!m_Renderer)
            {
                return;
            }

            if (m_FadeoutRestTime >= 0)
            {
                HighLight = (FadeoutDuration - m_FadeoutRestTime) / FadeoutDuration;
                if (TempMaterial == null || TempMaterial.Length == 0)
                {
                    TempMaterial = m_Renderer.materials;
                }
                for (int i = 0; i < TempMaterial.Length; i++)
                {
                    TempMaterial[i].SetColor("_HighLightColor", m_CurrentColor);
                    TempMaterial[i].SetFloat("_HighLight", HighLight);
                }
                m_FadeoutRestTime -= Time.deltaTime;
                if (m_FadeoutRestTime <= 0)
                {
                    m_FadeoutRestTime = -1;
                }
                return;
            }

            if (m_OriginMaterial==null)
            {
                return;
            }

            if (FossilisedDuration || FrozenDuration)
            {
                return;
            }
            else if (m_HighLightRestTime >= 0)
            {
                HighLight = m_HighLightRestTime / HighLightDuration;
                m_Renderer.transform.localPosition = m_OriginalPosition + new Vector3(HighLight, 1 - HighLight, HighLight) * ShakeAmp;
                ShakeAmp = -ShakeAmp;
                if (TempMaterial == null || TempMaterial.Length == 0)
                {
                    TempMaterial = m_Renderer.materials;
                }
                for (int i = 0; i < TempMaterial.Length; i++)
                {
                    TempMaterial[i].SetColor("_HighLightColor", m_CurrentColor);
                    TempMaterial[i].SetFloat("_HighLight", Mathf.Clamp01(HighLight * 1.2f));
                }
                m_HighLightRestTime -= Time.deltaTime;
                if (m_HighLightRestTime <= 0)
                {
                    m_HighLightRestTime = -1;
                }
            }
            else
            {
                resetShader();
            }
        }

		public void playFadeout()
        {
            if (!m_Renderer)
            {
                return;
            }
            if (m_OriginMaterial==null)
            {
				m_Renderer.transform.localPosition = m_OriginalPosition;
            }

            if (TempMaterial != null)
            {
                for (int i = 0; i < TempMaterial.Length; i++)
                {
                    if (TempMaterial[i].shader != m_FadeoutShader)
                    {
                        TempMaterial[i].shader = m_FadeoutShader;
                    }
                }
            }
            else
            {
                TempMaterial = m_Renderer.materials;
                for (int i = 0; i < TempMaterial.Length; i++)
                {
                    TempMaterial[i].shader = m_FadeoutShader;
                }
            }

            m_HighLightRestTime = -1;
            m_CurrentColor = FadeoutColor;
            m_FadeoutRestTime = FadeoutDuration;
        }

        public void playHighlight()
        {
            if (FossilisedDuration || FrozenDuration || m_FadeoutRestTime >= 0 || !m_Renderer)
            {
                return;
            }

            if (GetComponent<CharacterTransparentEffect>())
            {
                m_HighlightShader = GameMgr.Instance.m_TrasparentHighlightShader;
            }

            if (TempMaterial != null)
            {
                for (int i = 0; i < TempMaterial.Length; i++)
                {
                    if (TempMaterial[i].shader != m_HighlightShader)
                    {
                        TempMaterial[i].shader = m_HighlightShader;
                    }
                }
            }
            else
            {
				if (m_OriginMaterial != m_Renderer.sharedMaterials)
				{
					m_OriginMaterial = m_Renderer.sharedMaterials;
				}
				TempMaterial = m_Renderer.materials;
                for (int i = 0; i < TempMaterial.Length; i++)
                {
                    TempMaterial[i].shader = m_HighlightShader;
                }
            }

            m_CurrentColor = HighLightColor;
            m_HighLightRestTime = HighLightDuration;
            m_OriginalPosition = m_Renderer.transform.localPosition;
        }

        public void playFossilisedEffect()
        {
            if (FossilisedDuration || FrozenDuration || m_FadeoutRestTime >= 0 || !m_Renderer)
            {
                return;
            }
            m_HighLightRestTime = -1;

            if (TempMaterial != null)
            {
                for (int i = 0; i < TempMaterial.Length; i++)
                {
                    if (TempMaterial[i].shader != m_FossilisedShader)
                    {
                        TempMaterial[i].shader = m_FossilisedShader;
                    }
                    TempMaterial[i].SetTexture("_FossilisedTex", m_FossilisedMap);
                }
            }
            else
			{
				if (m_OriginMaterial != m_Renderer.sharedMaterials)
				{
					m_OriginMaterial = m_Renderer.sharedMaterials;
				}
				TempMaterial = m_Renderer.materials;
                for (int i = 0; i < TempMaterial.Length; i++)
                {
                    TempMaterial[i].shader = m_FossilisedShader;
                    TempMaterial[i].SetTexture("_FossilisedTex", m_FossilisedMap);
                }
            }

            FossilisedDuration = true;
        }

        public void endFossilisedEffect()
        {
			if (FossilisedDuration)
			{
				FossilisedDuration = false;
				resetShader();
			}
        }

		public void playFrozenEffect()
		{
			if (FrozenDuration || FossilisedDuration || m_FadeoutRestTime >= 0 || !m_Renderer)
			{
				return;
			}

			m_HighLightRestTime = -1;

			if (TempMaterial != null)
			{
				for (int i = 0; i < TempMaterial.Length; i++)
				{
					if (TempMaterial[i].shader != m_FrozenShader)
					{
						TempMaterial[i].shader = m_FrozenShader;
					}
					TempMaterial[i].SetTexture("_Reftex", m_FrozenTexture);
				}
			}
			else
			{
				if (m_OriginMaterial != m_Renderer.sharedMaterials)
				{
					m_OriginMaterial = m_Renderer.sharedMaterials;
				}
				TempMaterial = m_Renderer.materials;
				for (int i = 0; i < TempMaterial.Length; i++)
				{
					TempMaterial[i].shader = m_FrozenShader;
					TempMaterial[i].SetTexture("_Reftex", m_FrozenTexture);
				}
			}
			FrozenDuration = true;
		}

		public void endFrozenEffect()
		{
			if (FrozenDuration)
			{
				FrozenDuration = false;
				resetShader();
			}
		}

        public void PlayOutLineEffect()
        {
            if (FossilisedDuration || m_FadeoutRestTime >= 0 || !m_Renderer)
            {
                return;
            }
            m_HighLightRestTime = -1;
            isOutLine = true;

            if (TempMaterial != null)
            {
                for (int i = 0; i < TempMaterial.Length; i++)
                {
                    if (TempMaterial[i].shader != m_OutlineShader)
                    {
                        TempMaterial[i].shader = m_OutlineShader;
                    }
                    TempMaterial[i].SetColor("_OutlineColor", OutlineColor);
                    TempMaterial[i].SetFloat("_OutlineWidth", OutlineWidth);
                    TempMaterial[i].SetColor("_MainBlend", MainBlendColor);
                    
                }
            }
            else
            {
                if (m_OriginMaterial != m_Renderer.sharedMaterials)
                {
                    m_OriginMaterial = m_Renderer.sharedMaterials;                    
                }
                TempMaterial = m_Renderer.materials;
                for (int i = 0; i < TempMaterial.Length; i++)
                {
                    TempMaterial[i].shader = m_OutlineShader;
                    TempMaterial[i].SetColor("_OutlineColor", OutlineColor);
                    TempMaterial[i].SetFloat("_OutlineWidth", OutlineWidth);
                    TempMaterial[i].SetColor("_MainBlend", MainBlendColor);
                }
            }

            if (ExtraTempMaterial != null)
            {
                for (int i = 0; i < ExtraTempMaterial.Length; i++)
                {
                    if (ExtraTempMaterial[i].shader != m_OutlineShader)
                    {
                        ExtraTempMaterial[i].shader = m_OutlineShader;
                    }
                    ExtraTempMaterial[i].SetColor("_OutlineColor", OutlineColor);
                    ExtraTempMaterial[i].SetFloat("_OutlineWidth", OutlineWidth);
                    ExtraTempMaterial[i].SetColor("_MainBlend", MainBlendColor);

                }
            }
            else
            {
                if (m_ExtraRenderer == null)
                {
                    return;
                }
                if (m_ExtraOriginMaterial != m_ExtraRenderer.sharedMaterials)
                {
                    m_ExtraOriginMaterial = m_ExtraRenderer.sharedMaterials;
                }
                ExtraTempMaterial = m_ExtraRenderer.materials;
                for (int i = 0; i < ExtraTempMaterial.Length; i++)
                {
                    ExtraTempMaterial[i].shader = m_OutlineShader;
                    ExtraTempMaterial[i].SetColor("_OutlineColor", OutlineColor);
                    ExtraTempMaterial[i].SetFloat("_OutlineWidth", OutlineWidth);
                    ExtraTempMaterial[i].SetColor("_MainBlend", MainBlendColor);
                }
            }
        }

        public void resetShader(bool isCloseOutline = false)
        {
            if (m_OriginMaterial==null || !m_Renderer)
            {
                return;
            }
            if (isOutLine && !isCloseOutline)
            {
                PlayOutLineEffect();
                return;
            }
            isOutLine = false;
            m_Renderer.transform.localPosition = m_OriginalPosition;
			m_Renderer.sharedMaterials = m_OriginMaterial;            
            TempMaterial = null;
            m_OriginMaterial = null;

            if (m_ExtraOriginMaterial == null || !m_ExtraRenderer)
            {
                return;
            }
            if ( m_ExtraRenderer)
            {
                m_ExtraRenderer.sharedMaterials = m_ExtraOriginMaterial;
                ExtraTempMaterial = null;
                m_ExtraOriginMaterial = null;
            }
        }
    }
}