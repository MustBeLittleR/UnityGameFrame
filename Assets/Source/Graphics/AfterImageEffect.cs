using UnityEngine;

namespace WCG
{
	public class AfterImageEffect : MonoBehaviour
	{
		MeshFilter m_MeshFilter;
		MeshRenderer m_MeshRenderer;
		Material m_Material;
		float m_Elapsed = 0;
		float m_Duration = 1;
		bool m_IsLatest = false;

		public void initSkinnedMeshAfterImage(SkinnedMeshRenderer skinnedmeshrenderer, float duration, bool isLatest = false)
		{
			m_MeshFilter = GetComponent<MeshFilter>();
			m_MeshRenderer = GetComponent<MeshRenderer>();
			m_Material = m_MeshRenderer.material;
			m_MeshFilter.mesh = new Mesh();
			skinnedmeshrenderer.BakeMesh(m_MeshFilter.mesh);
			m_Material.SetTexture("_MainTex", skinnedmeshrenderer.sharedMaterial.mainTexture);
			m_Material.SetFloat("_Cutoff", skinnedmeshrenderer.sharedMaterial.GetFloat("_Cutoff"));
			m_Duration = duration;
			m_IsLatest = isLatest;
		}

		// Update is called once per frame
		void Update()
		{
			if (m_Elapsed > m_Duration)
			{
				Destroy(this.gameObject);
			}

			float alpha = m_Elapsed / m_Duration;
			if (m_Elapsed == 0)
			{
				alpha = 1;
			}

			m_Elapsed += Time.deltaTime;
			m_Material.SetFloat("_Alpha", 1 - alpha);

			if (m_IsLatest)
			{
				float scale = (1 + alpha * 3);
				this.transform.localScale = new Vector3(scale, scale, scale);
			}
		}
	}
}