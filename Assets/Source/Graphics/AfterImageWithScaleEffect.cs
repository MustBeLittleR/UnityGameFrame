using UnityEngine;

namespace WCG
{
	public class AfterImageWithScaleEffect : MonoBehaviour
	{
		MeshFilter m_MeshFilter;
		MeshRenderer m_MeshRenderer;
		Material m_Material;
		SkinnedMeshRenderer m_SkinnedMeshRenderer = null;
        float m_Elapsed = 0;
		float m_Duration = 1;

		float m_EnlargeTime = 1;
		float m_ReduceTime = 1;

		float m_MaxScale = 2;

		Vector3 m_OriginalLocalScale;

		public void initSkinnedMeshAfterImageScale(SkinnedMeshRenderer skinnedmeshrenderer, float duration, float enlarge, float reduce, float maxScale, float delayTime = 0)
		{
			if (enlarge + reduce <= 0)
			{
				Debug.LogError("enlarge + reduce Time Must Larger Than 0!");
				DestroyImmediate(this.gameObject);
			}

			m_SkinnedMeshRenderer = skinnedmeshrenderer;
            m_MeshFilter = GetComponent<MeshFilter>();
			m_MeshRenderer = GetComponent<MeshRenderer>();
			m_Material = m_MeshRenderer.material;
			m_MeshFilter.mesh = new Mesh();
			m_Material.SetTexture("_MainTex", m_SkinnedMeshRenderer.sharedMaterial.mainTexture);
			m_Material.SetFloat("_Cutoff", m_SkinnedMeshRenderer.sharedMaterial.GetFloat("_Cutoff"));
			m_Duration = duration;
			m_EnlargeTime = enlarge;
			m_OriginalLocalScale = this.transform.localScale;

			float avragetime = duration / (enlarge + reduce);
			m_EnlargeTime = enlarge * avragetime;
			m_ReduceTime = reduce * avragetime;
			m_MaxScale = maxScale;

			if (delayTime > 0)
			{
				this.gameObject.SetActive(false);
				Invoke("ReActive", delayTime);
			}
			else
			{
				m_SkinnedMeshRenderer.BakeMesh(m_MeshFilter.mesh);
			}
		}

		void ReActive()
		{
			m_SkinnedMeshRenderer.BakeMesh(m_MeshFilter.mesh);
            this.gameObject.SetActive(true);
		}

		// Update is called once per frame
		void Update()
		{
			if (m_Elapsed > m_Duration)
			{
				Destroy(this.gameObject);
				return;
			}

			if (m_Elapsed <= m_EnlargeTime)
			{
				float scale = m_Elapsed / m_EnlargeTime;
                this.transform.localScale = m_OriginalLocalScale * (1 - scale) + m_OriginalLocalScale * m_MaxScale * scale;
			}
			else
			{
				float scale = (m_Elapsed - m_EnlargeTime) / m_ReduceTime;
				this.transform.localScale = m_OriginalLocalScale * m_MaxScale * (1 - scale) + m_OriginalLocalScale * scale;
			}

			m_Elapsed += Time.deltaTime;
		}
	}
}