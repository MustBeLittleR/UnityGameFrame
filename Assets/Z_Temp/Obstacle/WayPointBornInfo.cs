#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using SceneEditor;
public class WayPointBornInfo : MonoBehaviour {
    WayDelDeleGate mfc_destory;
    public void SetDestoryDel(WayDelDeleGate destorydec)
    {
        mfc_destory = destorydec;
    }
    void OnDestroy()
    {
        if (mfc_destory != null && transform.parent)
            mfc_destory(transform.parent , transform.localPosition.x, transform.localPosition.z);
    }
}
#endif
