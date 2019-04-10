using UnityEngine;
using System.Collections;

namespace WCG {
    public class DestroyForTime : MonoBehaviour
    {
        public GameObject m_DestroyObj = null;
        void Start()
        {
            if (m_DestroyObj == null)
                m_DestroyObj = gameObject;   
        }

        public void OnDestroyObj()
        {
			ResourceMgr.PutInsGameObj(m_DestroyObj);
		}
    }
}

