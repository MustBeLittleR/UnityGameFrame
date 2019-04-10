
using UnityEngine;
using System.Collections;
using WCG;

namespace WCG
{
	public class EffectTime : MonoBehaviour
	{
		public float continuetime = -1;

		void OnEnable()
		{
			if (continuetime > .0f)
				Invoke("AutoDestroy", continuetime);
		}

		void OnDisable()
		{
			if (this.IsInvoking())
				this.CancelInvoke();
		}

		void AutoDestroy()
		{
			SceneMgr.Instance.OnObjAutoDestroy(gameObject);
			ResourceMgr.PutInsGameObj(gameObject);
		}
	}
}

