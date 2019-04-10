using UnityEngine;
using UnityEngine.UI;

namespace WCG
{
    public class UIRootSortingManager : MonoBehaviour
    {
        private int m_SortDelta = 10;
        UISortingNode[] m_SortNode;
        private bool m_NeedSort = false;
        // Use this for initialization
        //void Start()
        //{
        //    //ReSortingUI();
        //}

        // Update is called once per frame
        void Update()
        {
            m_SortDelta--;
            if (m_SortDelta < 0)
            {
                m_SortDelta = 10;
                if (m_NeedSort)
                {
                    SortUI();
                    m_NeedSort = false;
                }                
            }            
        }

        public void SetNeedSort(bool bNeedSort)
        {
            m_NeedSort = bNeedSort;
        }

        public void SortUI()
        {
           //ReSortingUI();
           ResetPos();
        }
        void ResetPos()
        {
            float thickness = 0;
            m_SortNode = gameObject.GetComponentsInChildren<UISortingNode>();
            for (int i = 0; i < m_SortNode.Length; i++)
            {
                UISortingNode oSortNode = m_SortNode[i];                              
                RectTransform rectrans = oSortNode.gameObject.transform as RectTransform;                
                rectrans.localPosition = new Vector3(rectrans.localPosition.x, rectrans.localPosition.y, -thickness);
				float thicknessAdd = oSortNode.GetThickness();    
                thickness += thicknessAdd;        
            }
        }

        void ReSortingUI()
        {
			int ii = 1;
			bool hassroting = false;
			int childCount = this.transform.childCount;
			for (int i = 0;i < childCount;i++)
			{
				Transform childtrans = this.transform.GetChild(i);
                ProcessChildren(childtrans, ref ii, ref hassroting);              				
			}            
        }

        void ProcessChildren(Transform trans, ref int index, ref bool sortingRest)
		{
			int childCount = trans.childCount;

			Renderer renderers = trans.gameObject.GetComponent<Renderer>();
			if (renderers)
			{
				renderers.sortingOrder = index++;
				sortingRest = true;
			}
			else if (sortingRest)
			{
                //CanvasRenderer canvasrenderer = trans.gameObject.GetComponent<CanvasRenderer>();
                //if (canvasrenderer)
                //{
					Canvas canvas = trans.gameObject.GetComponent<Canvas>();
					if (!canvas)
					{
						//canvas = trans.gameObject.gameObject.AddComponent<Canvas>();
					}
					else
					{
						if (!canvas.overrideSorting)
						{
							canvas.overrideSorting = true;
						}
						else
						{
							canvas.sortingOrder = index++;
						}
					}
					GraphicRaycaster graphicraycaster = trans.gameObject.GetComponent<GraphicRaycaster>();
					if (!graphicraycaster)
					{
						//graphicraycaster = trans.gameObject.gameObject.AddComponent<GraphicRaycaster>();
					}
				//}
			}
			else
			{
                //GraphicRaycaster graphicraycaster = trans.gameObject.GetComponent<GraphicRaycaster>();
                //if (graphicraycaster)
                //{
                //    Destroy(graphicraycaster);
                //}
                //Canvas canvas = trans.gameObject.GetComponent<Canvas>();
                //if (canvas)
                //{
                //    Destroy(canvas);
                //}
			}

			bool hassroting = false;
            for (int i = 0; i < childCount; i++)
            {
                Transform childtrans = trans.GetChild(i);
                ProcessChildren(childtrans, ref index, ref hassroting);                
            }

			if (hassroting)
			{
				sortingRest = true;
            }
		}

    }
}