#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolInfo : MonoBehaviour
{
    private BornInfo parentBornInfo;
    public int m_GridX;
    public int m_GridY;
    // Use this for initialization
    void Start()
    {
        parentBornInfo = this.transform.parent.GetComponent<BornInfo>();
    }

    void OnDestroy()
    {
        parentBornInfo.RemovePatrolPoint(m_GridX, m_GridY);
    }
}
#endif