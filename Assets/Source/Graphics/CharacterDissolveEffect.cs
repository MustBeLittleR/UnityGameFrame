using UnityEngine;

namespace WCG
{
    public class CharacterDissolveEffect : MonoBehaviour
    {
        public Shader m_DissolveShader;
        public Shader m_CharacterHighlightShader;
        public Texture m_DissolveTexture;
        public float Duration = 1.0f;
        float m_ElapsedTime = 0.0f;
        SkinnedMeshRenderer m_SkinnedMeshRenderer;
        MeshFilter m_MeshFilter = null;

        ParticleSystem[] m_ParticleSystem;

        Material m_OriginMat = null;
        Material m_OriginMat1 = null;

        Color m_highLightColor;

        public bool PlayingDissolve = false;
        public bool PlayingDisDissolve = false;

        Material[] m_SkinnedMeshMaterials; //保存获取到的material防止修改属性拷贝

        // Use this for initialization
        void Awake()
        {
            m_SkinnedMeshRenderer = GetComponentInChildren<MaterialAnimationAssistant>().GetSkinnedMeshRenderer();

            //保存共享材质
            m_OriginMat = m_SkinnedMeshRenderer.sharedMaterial;
            if (m_SkinnedMeshRenderer.sharedMaterials.Length != 1)
            {
                m_OriginMat1 = m_SkinnedMeshRenderer.sharedMaterials[1];
            }

            //材质保存，用于修改材质属性
            m_SkinnedMeshMaterials = m_SkinnedMeshRenderer.materials;

            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).name.Contains("shadow"))
                {
                    m_MeshFilter = transform.GetChild(i).GetComponentInChildren<MeshFilter>();
                    if (m_MeshFilter)
                    {
                        break;
                    }
                }
            }

            m_ParticleSystem = GetComponentsInChildren<ParticleSystem>();

            m_DissolveShader = GameMgr.Instance.m_DissolveShader;
            m_CharacterHighlightShader = GameMgr.Instance.m_CharacterHighlightShader;
            m_DissolveTexture = GameMgr.Instance.m_DissolveTexture;
        }

        public void startDissolve()
        {
            if (m_SkinnedMeshRenderer)
            {
                for (int i = 0; i < m_SkinnedMeshMaterials.Length; i++)
                {
                    m_SkinnedMeshMaterials[i].shader = m_DissolveShader;
                    m_SkinnedMeshMaterials[i].SetTexture("_DissolveSrc", m_DissolveTexture);
                    //m_SkinnedMeshMaterials[i] = m_SkinnedMeshRenderer.materials[i];
                }
                m_ElapsedTime = 0.0f;
                PlayingDissolve = true;
                PlayingDisDissolve = false;
            }
        }

        public void DisDissolve()
        {
            if (m_SkinnedMeshRenderer)
            {
                for (int i = 0; i < m_SkinnedMeshMaterials.Length; i++)
                {
                    m_SkinnedMeshMaterials[i].shader = m_DissolveShader;
                    m_SkinnedMeshMaterials[i].SetTexture("_DissolveSrc", m_DissolveTexture);
                    //m_SkinnedMeshMaterials[i] = m_SkinnedMeshRenderer.materials[i];
                }
                m_ElapsedTime = 0.0f;
                PlayingDisDissolve = true;
                PlayingDissolve = false;

                if (m_MeshFilter)
                    m_MeshFilter.gameObject.SetActive(true);

                for (int i = 0; i < m_ParticleSystem.Length; i++)
                {
                    m_ParticleSystem[i].Play(false);
                }
            }
        }

        public void startOutLine()
        {
            if (m_SkinnedMeshRenderer)
            {
                for (int i = 0; i < m_SkinnedMeshMaterials.Length; i++)
                {
                    m_SkinnedMeshMaterials[i].shader = m_CharacterHighlightShader;
                }
            }
        }

        public void setOutLineColor(float r, float g, float b, float a)
        {
            m_highLightColor = new Color(r, g, b, a);
            if (m_SkinnedMeshRenderer)
            {
                for (int i = 0; i < m_SkinnedMeshMaterials.Length; i++)
                {
                    m_SkinnedMeshMaterials[i].SetColor("_HighLightColor", m_highLightColor);
                }
            }
        }

        public void revert()
        {
            if (m_SkinnedMeshRenderer)
            {
                m_ElapsedTime = 0.0f;
                PlayingDissolve = false;
                PlayingDisDissolve = false;
                m_SkinnedMeshRenderer.sharedMaterial = m_OriginMat;
                if (m_OriginMat1)
                {
                    m_SkinnedMeshRenderer.sharedMaterials[1] = m_OriginMat1;
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (PlayingDissolve || PlayingDisDissolve)
            {
                if (PlayingDissolve)
                {
                    for (int i = 0; i < m_SkinnedMeshMaterials.Length; i++)
                    {
                        m_SkinnedMeshMaterials[i].SetFloat("_Amount", m_ElapsedTime / Duration);
                    }
                }
                else
                {
                    for (int i = 0; i < m_SkinnedMeshMaterials.Length; i++)
                    {
                        m_SkinnedMeshMaterials[i].SetFloat("_Amount", 1.0f - m_ElapsedTime / Duration);
                    }
                }
                m_ElapsedTime += Time.deltaTime;
                if (m_ElapsedTime > Duration)
                {
                    m_ElapsedTime = Duration;
                    if (PlayingDisDissolve)
                    {
                        //revert();
                        startOutLine();
                        setOutLineColor(m_highLightColor.r, m_highLightColor.g, m_highLightColor.b, m_highLightColor.a);
                    }
                    else
                    {
                        if (m_MeshFilter)
                            m_MeshFilter.gameObject.SetActive(false);
                        for (int i = 0; i < m_ParticleSystem.Length; i++)
                        {
                            m_ParticleSystem[i].Stop(false);
                        }
                    }
                }
            }
        }
    }
}