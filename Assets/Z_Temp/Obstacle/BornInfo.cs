#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using SceneEditor;

public class BornInfo : MonoBehaviour {

    public string ms_Name {get; set;}
    public int m_eObjType { get; set; } //对象类型
    public CreateMode me_Type { get; set; }  //刷点类型

    public int mi_Id { get; set; } //自增长id

    public int mi_ObjId { get; set; } //配置表里的obj id

    public int m_maxPower { get; set; } //最低强度
    public int m_minPower { get; set; } //最高强度
    
    //public string mi_Lv { get; set; } //怪物等级

    public int mi_Group { get; set; }

    public int m_nTrapSceneId { get; set; }
    public int m_nPosX { get; set; }
    public int m_nPosY { get; set; }

    public int m_SumonNum { get; set; }//call num
    public List<RegionObjInfo> m_RegionNpcInfo;
    public int m_nWidth { get; set; }//范围刷怪宽度
    public int m_nHeight { get; set; }//范围刷怪高度

    //public int mi_UpdataTime { get; set; }
    public int m_nRebornTime { get; set; } //复活刷新时间        
    public int m_nLiveTime { get; set; }
    public int m_nDelayBornTime { get; set; }
    public int m_RoundType { get; set; }
    public int m_nMoveMode { get; set; }
    public int m_nMoveRadius { get; set; }
    public List<PatrolPointInfo> m_PatrolInfo; //巡逻点
    public int rotation { get; set; }
    public int m_nRate { get; set; } //刷出来的几率
    public int graphDir { get; set; } //npc怪物图形层的旋转角度
    BornInfo()
    {
        m_eObjType = -1;
        mi_Id = -1;
        mi_ObjId = -1;
        m_nRebornTime = 5;
        ms_Name = "";        
        mi_Group = 0;        
        m_nTrapSceneId = 0;
        m_nPosX = 0;
        m_nPosY = 0;
        m_minPower = 1;
        m_maxPower = 999;
        m_RegionNpcInfo = new List<RegionObjInfo>();
        m_PatrolInfo = new List<PatrolPointInfo>();
        m_nLiveTime = 0;
        m_nDelayBornTime = 0;
        m_RoundType = 0;
        m_nMoveMode = 0;
        m_nMoveRadius = 0;
        m_nRate = 0;
        graphDir = 0;
    }

    public void ReSizePatrolPoint(int nSize)
    {
        if(nSize < 0)
        {
            nSize = 0;
        }
        if(nSize > m_PatrolInfo.Count)
        {
            while(nSize>m_PatrolInfo.Count)
            {
                m_PatrolInfo.Add(new PatrolPointInfo());
            }
            
        }
        if(nSize < m_PatrolInfo.Count)
        {
            while(nSize<m_PatrolInfo.Count)
            {
                m_PatrolInfo.RemoveAt(m_PatrolInfo.Count - 1);
            }
        }
    }
    public bool CheckPatrolPoint(int x, int y)
    {
        for(int i=0; i<m_PatrolInfo.Count; i++)
        {
            PatrolPointInfo point = m_PatrolInfo[i];
            if(point.m_GridX==x&& point.m_GridY==y)
            {
                return false;
            }
        }
        return true;
    }

    public void RemovePatrolPoint(int x, int y)
    {
        int nIndex = GetPatrolPointIndex(x, y);
        if(nIndex>=0)
        {
            m_PatrolInfo.RemoveAt(nIndex);
        }
        
    }
    public int GetPatrolPointIndex(int x, int y)
    {
        for (int i = 0; i < m_PatrolInfo.Count; i++)
        {
            PatrolPointInfo point = m_PatrolInfo[i];
            if (point.m_GridX == x && point.m_GridY == y)
            {
                return i;
            }
        }
        return -1;
    }
    public void SetPatrolPoint(int nIndex, int actionid, int x, int y, int waittime)
    {
        PatrolPointInfo partolpoint = m_PatrolInfo[nIndex];
        partolpoint.m_ActionId = actionid;
        partolpoint.m_GridX = x;
        partolpoint.m_GridY = y;
        partolpoint.m_WaitTime = waittime;        
    }



    public void ReSizeRegionObj(int nSize)
    {
        if (nSize < 0)
        {
            nSize = 0;
        }
        if (nSize > m_RegionNpcInfo.Count)
        {
            while (nSize > m_RegionNpcInfo.Count)
            {
                m_RegionNpcInfo.Add(new RegionObjInfo());
            }

        }
        if (nSize < m_RegionNpcInfo.Count)
        {
            while (nSize < m_RegionNpcInfo.Count)
            {
                m_RegionNpcInfo.RemoveAt(m_RegionNpcInfo.Count - 1);
            }
        }
    }
    public void SetRegionObj(int nIndex, int Id, int nNum, int Time, int dir)
    {
        RegionObjInfo obj = m_RegionNpcInfo[nIndex];
        
        obj.Nums = nNum;
        obj.ObjId = Id;
        obj.Time = Time;
        obj.Per = 0;
        obj.Dir = dir;                
    }

    public void RemoveRegionObj(int nIndex)
    {
        m_RegionNpcInfo.RemoveAt(nIndex);
        
    }

    public int GetPosX()
    {
        return (int)(gameObject.transform.localPosition.x - 0.5);
    }

    public int GetPosY()
    {
        return (int)(gameObject.transform.localPosition.y - 0.5);
    }

    public void SetPosX(int posx)
    {
        gameObject.transform.localPosition = new Vector3(posx + 0.5f, gameObject.transform.localPosition.y, gameObject.transform.localPosition.z);
    }

    public void SetPosY(int posy)
    {
        gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, gameObject.transform.localPosition.y, posy + 0.5f);
    }

    public int GetDiretionAngel()
    {
        return (int)gameObject.transform.localRotation.eulerAngles.y;
    }


    public void SetDiretionAngel(int angel)
    {
        gameObject.transform.localRotation = Quaternion.Euler(gameObject.transform.localRotation.x, angel, gameObject.transform.localRotation.z);
    }
    

}
#endif
