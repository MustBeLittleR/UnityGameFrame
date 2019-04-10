using UnityEngine;

namespace WCG
{
	public class CharacterTransparentEffect : MonoBehaviour
	{
		SkinnedMeshRenderer[] m_Renderer = null;
		Material[] m_DefaultMaterial = null;
		Material[] m_TmpMaterial = null;
        Color m_TintColor = new Color(0,0,0,1);
		float m_Alpha = 1.0f;
		// Use this for initialization
		void OnEnable()
		{
			m_Renderer = GetComponentsInChildren<SkinnedMeshRenderer>();
		}

		public void setTintColor(Color color, float alpha)
		{
			if (null == m_Renderer || m_Renderer.Length == 0)
			{
				return;
			}

			m_TintColor = color;
			m_Alpha = alpha;

			SkinnedMeshRenderer oTemMesh = null;
			if (null == m_DefaultMaterial)
			{
				m_DefaultMaterial = new Material[m_Renderer.Length];
				for (int i = 0; i < m_Renderer.Length; i++)
				{
					oTemMesh = m_Renderer[i];
					if (oTemMesh)
						m_DefaultMaterial[i] = oTemMesh.sharedMaterial;
					else
						m_DefaultMaterial[i] = null;
				}
			}

			if (null == m_TmpMaterial)
			{
				m_TmpMaterial = new Material[m_Renderer.Length];
				for (int i = 0; i < m_Renderer.Length; i++)
				{
					oTemMesh = m_Renderer[i];
					if (oTemMesh)
					{
						m_TmpMaterial[i] = oTemMesh.material;
						m_TmpMaterial[i].shader = GameMgr.Instance.m_TintColorShader;

						m_TmpMaterial[i].SetColor("_TintColor", m_TintColor);
						m_TmpMaterial[i].SetFloat("_Alpha", m_Alpha);
					}
					else
					{
						m_TmpMaterial[i] = null;
					}
				}
			}
		}

		public void RevertEffect()
		{
			SkinnedMeshRenderer oTemMesh = null;
			for (int i = 0; i < m_Renderer.Length; i++)
			{
				oTemMesh = m_Renderer[i];
				if (oTemMesh && m_DefaultMaterial[i])
				{
					oTemMesh.sharedMaterial = m_DefaultMaterial[i];
					m_DefaultMaterial[i] = null;
					if (null != m_TmpMaterial && m_TmpMaterial[i])
						m_TmpMaterial[i] = null;
				}
				else
				{
					if (null != m_DefaultMaterial && m_DefaultMaterial[i])
						m_DefaultMaterial[i] = null;
					if (null != m_TmpMaterial && m_TmpMaterial[i])
						m_TmpMaterial[i] = null;
				}
			}

			m_DefaultMaterial = null;
			m_TmpMaterial = null;
		}
	}
}