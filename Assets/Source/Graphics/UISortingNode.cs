using UnityEngine;
using UnityEngine.UI;

namespace WCG
{
    //[RequireComponent(typeof(CanvasRenderer))]
    //[RequireComponent(typeof(Canvas))]
    //[RequireComponent(typeof(GraphicRaycaster))]
    public class UISortingNode : MonoBehaviour
    {
        //厚度
        public float m_thickness = 201;

        //获取
        public float GetThickness()
        {
            return m_thickness;
        }

    }
}