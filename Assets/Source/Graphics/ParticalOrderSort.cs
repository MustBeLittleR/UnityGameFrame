using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace WCG
{
    //为该节点下所有特效设置为自己的order
    public class ParticalOrderSort : MonoBehaviour
    {
        private int m_nParentOrder = 0;
        private Canvas m_oParentCanvas;
        Renderer[] m_tParticalSystem;
        // Use this for initialization
        void Awake()
        {            
            m_oParentCanvas = gameObject.GetComponent<Canvas>();
            m_tParticalSystem = gameObject.GetComponentsInChildren<Renderer>(true);            
        }

        // Update is called once per frame
        void Update()
        {
            if (m_tParticalSystem.Length == 0)
            {
                m_tParticalSystem = gameObject.GetComponentsInChildren<Renderer>(true);            
            }
            if (m_oParentCanvas && m_oParentCanvas.sortingOrder != m_nParentOrder)
            {
                m_nParentOrder = m_oParentCanvas.sortingOrder;
                SetParticalOrder();
            }
        }

        void SetParticalOrder()
        {            
            for (int i = 0; i < m_tParticalSystem.Length; i++)
            {
                Renderer oParticle = m_tParticalSystem[i];
                if (oParticle)
                {
                    oParticle.sortingOrder = m_nParentOrder;
                }
            }
        }
    }
}