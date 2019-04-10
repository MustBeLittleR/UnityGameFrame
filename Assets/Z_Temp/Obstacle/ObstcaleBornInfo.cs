#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using SceneEditor;
public class ObstcaleBornInfo : MonoBehaviour
{
    
    Destorydelegate mfc_destory;
    public void SetDestoryDel(Destorydelegate destorydec)
    {
        mfc_destory = destorydec;
    }
    void OnDestroy()
    {
        if (mfc_destory != null)
            mfc_destory(transform.localPosition.x, transform.localPosition.z );
    }
}
#endif