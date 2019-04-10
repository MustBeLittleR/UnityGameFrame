#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System;

namespace SceneEditor
{
    public enum InfoType
    {
        Npc = 0, //战斗NPC
        FunNpc = 1, //功能NPC
        Obj = 2,         //Obj
    };

    public interface ICommonInterface
    {
        void OnCreateGameObj(GameObject obj, int id, CreateMode mode);
        void Init();
        void OnDestory();
        
    }

    public class MonsterInfo
    {

        public MonsterInfo(InfoType eType)
        {
            m_eType = eType;
            id = "";
            name = "";
            level = "";
            ischoose = false;
            TotalNumber = 0;

        }
        public void setInfo(string id, string name,  string level, string sModel)
        {            
            this.id = id;
            this.name = name;
            this.level = level;
            this.sModel = sModel;
        }

        public void SetIsChoose(bool Choose)
        {
            ischoose = Choose;
        }

        public string sModel;
        public string id;
        public string name;
        public string level;
        public bool ischoose;
        public int TotalNumber;
        public InfoType m_eType { get;set; }
    }

    public class NpcInfo: MonsterInfo
    {
        public NpcInfo():base(InfoType.Npc)
        {           
        }
    }

    public class FunNpcInfo : MonsterInfo
    {
        public FunNpcInfo() : base(InfoType.FunNpc)
        {
        }
    }
    public class ObjInfo : MonsterInfo
    {
        public ObjInfo() : base(InfoType.Obj)
        {
        }
    }
    public class RegionObjInfo
    {
        public int ObjId;
        public int Nums;
        public int Time;
        public int Dir;
        public int Per;
        public int Rate;       
    }
    public class PatrolPointInfo
    {
        public int m_WaitTime;//停留时间
        public int m_ActionId; //行为id
        public int m_GridX; 
        public int m_GridY;
    }

    public delegate void Destorydelegate(float pos, float posy);
    public delegate void WayDelDeleGate(Transform obj, float posx, float posy);
    public delegate void MonsterBornDelDeleGate(int id);
    public enum CreateMode
    {
        enObstcale, //阻挡
        enRegionObj, //普通范围内刷怪
        enPointObj, //定点刷对象        
        enScenePoint, //场景点
        enPathPoint,  //路径巡逻点     
        enPatrolPoint, //npc巡逻点  
        enInvild,
    }

    public enum CreateObjType
    {
        eMonster = 0,//怪物
        eFunNpc = 1, //funnpc
        eObj = 2, //obj
        eInvalid = 3,
    }

    public enum MapObjectType
    {
        eEmpty = 0,
        eObstcale = 1, //障碍物
        eHeroBorn,//出生点
        eNpc,//NPC
        eMonster,//怪物布点

    }
}
public class PathGridComponent : MonoBehaviour 
{
    public int m_numberOfRows = 13; //行
    public int m_numberOfColumns = 13; //列
	public static float m_cellSize = 0.5f;
	public bool m_debugShow = true;
    public bool m_bIsShowObstacle = true;       //是否显示阻挡
	public Color m_debugColor = Color.black;
    public static double m_XAngle = 0;
    public static double m_YAngle = -m_XAngle;
    Dictionary<string, Vector3> m_MapShowObjectInfo; //地图显示物件
    Dictionary<int, Dictionary<int, Dictionary<int, SceneEditor.MapObjectType>>> m_TempObjectInfo = new Dictionary<int, Dictionary<int, Dictionary<int, SceneEditor.MapObjectType>>>();

    public SceneEditor.MapObjectType[,] mia_array;                 //阻挡信息存储

    int mi_PreColoms = 0;                  //之前的列数,即X
    int mi_PreRows = 0;
    int mi_MaxNumber = 0;           //行列最大数

    
    #region Constants
    protected static readonly Vector3 kXAxis = new Vector3(1.0f, 0.0f, 0.0f); 		// points in the directon of the positive X axis
    protected static readonly Vector3 kZAxis = new Vector3(0.0f, 0.0f, 1.0f);		// points in the direction of the positive Z axis
    protected static readonly Vector3 kYAxis = new Vector3(0.0f, 1.0f, 0.0f);       // points in the direction of the positive Y axis

    //protected static readonly Vector3 kLogicXAxis = new Vector3(m_cellSize * (float)Math.Cos(Math.PI*m_XAngle/180.0), m_cellSize * (float)Math.Sin(Math.PI*m_XAngle/180.0), 0.0f); 		// points in the directon of the positive X axis
    //protected static readonly Vector3 kLogicYAxis = new Vector3(m_cellSize * (float)Math.Cos(Math.PI * m_YAngle /180.0), m_cellSize * (float)Math.Sin(Math.PI * m_YAngle / 180.0), 0.0f);       // points in the direction of the positive Y axis


    protected static readonly Vector3 kLogicXAxis = new Vector3(m_cellSize * 1.0f, 0.0f, 0.0f); 		// points in the directon of the positive X axis
    protected static readonly Vector3 kLogicYAxis = new Vector3(0.0f, m_cellSize * 1.0f, 0.0f);       // points in the direction of the positive Y axis

    #endregion

    void Awake()
	{
        
	}

    void Start()
    {

    }

    public Dictionary<int, Dictionary<int, Dictionary<int, SceneEditor.MapObjectType>>> GetMapObjectInfo()
    {
        return m_TempObjectInfo;
    }

    public void InitArray(bool ismove = false)
    {
        if (ismove)
        {
            int x = 0;
            if(transform.position.x >= 0 )
            {
                x = (int)transform.position.x;
            }
            else
            {
                x = (int)transform.position.x - 1;
            }
            int z = 0;
            if (transform.position.z >= 0)
            {
                z = (int)transform.position.z;
            }
            else
            {
                z = (int)transform.position.z - 1;
            }
           
            gameObject.transform.position = new Vector3(x, transform.position.y, z);
        }
            
        if (mia_array == null)
        {
            //int realwidth = Mathf.CeilToInt(0.5f * (m_numberOfRows / m_cellSize) / (float)Math.Sin(Math.PI * Math.Abs(m_XAngle) / 180.0) + 0.5f * (m_numberOfColumns / m_cellSize) / (float)Math.Cos(Math.PI * Math.Abs(m_XAngle) / 180.0));
            //int realHeight = Mathf.CeilToInt(0.5f * (m_numberOfRows / m_cellSize) / (float)Math.Sin(Math.PI * m_YAngle / 180.0) + 0.5f * (m_numberOfColumns / m_cellSize) / (float)Math.Cos(Math.PI * m_YAngle / 180.0));
            int realwidth = Mathf.CeilToInt(m_numberOfColumns / m_cellSize);
            int realHeight = Mathf.CeilToInt(m_numberOfRows / m_cellSize);
            mia_array = new SceneEditor.MapObjectType[realwidth, realHeight];
            mi_PreColoms = m_numberOfColumns;
            mi_PreRows = m_numberOfRows;
        }
        if (mi_PreColoms != m_numberOfColumns || mi_PreRows != m_numberOfRows)
        {
           // int realwidth = Mathf.CeilToInt(0.5f * (m_numberOfRows / m_cellSize) / (float)Math.Sin(Math.PI * Math.Abs(m_XAngle) / 180.0) + 0.5f * (m_numberOfColumns / m_cellSize) / (float)Math.Cos(Math.PI * Math.Abs(m_XAngle) / 180.0));
           // int realHeight = Mathf.CeilToInt(0.5f * (m_numberOfRows / m_cellSize) / (float)Math.Sin(Math.PI * m_YAngle / 180.0) + 0.5f * (m_numberOfColumns / m_cellSize) / (float)Math.Cos(Math.PI * m_YAngle / 180.0));

            int realwidth = Mathf.CeilToInt(m_numberOfColumns / m_cellSize);
            int realHeight = Mathf.CeilToInt(m_numberOfRows / m_cellSize);

            //int preWidth = Mathf.CeilToInt(0.5f * (mi_PreRows / m_cellSize) / (float)Math.Sin(Math.PI * Math.Abs(m_XAngle) / 180.0) + 0.5f * (mi_PreColoms / m_cellSize) / (float)Math.Cos(Math.PI * Math.Abs(m_XAngle) / 180.0));
           // int preHeight = Mathf.CeilToInt(0.5f * (mi_PreRows / m_cellSize) / (float)Math.Sin(Math.PI * m_YAngle / 180.0) + 0.5f * (mi_PreColoms / m_cellSize) / (float)Math.Cos(Math.PI * m_YAngle / 180.0));

            int preWidth = Mathf.CeilToInt(mi_PreColoms / m_cellSize);
            int preHeight = Mathf.CeilToInt(mi_PreRows / m_cellSize);

            int minx = Math.Min(preWidth, realwidth);
            int miny = Math.Min(preHeight, realHeight);

            SceneEditor.MapObjectType[,] temparray = new SceneEditor.MapObjectType[realwidth, realHeight];
            for (int i = 0; i < minx; i++)
            {
                for (int j = 0; j < miny; j++)
                {
                    temparray[i, j] = mia_array[i, j];
                }
            }
            mia_array = temparray;
            mi_PreColoms = m_numberOfColumns;
            mi_PreRows = m_numberOfRows;
            mi_MaxNumber = Mathf.Max(mi_PreColoms, mi_PreRows);
        }

        for (int i = 0; i < mia_array.GetLength(0); i++)
        {
            for (int j = 0; j < mia_array.GetLength(1); j++)
            {
                Vector2 worldPos = GetWorldPosByMapPos(i, j);
                if (worldPos.x < 0 || worldPos.x >= m_numberOfColumns)
                {
                    mia_array[i, j] = SceneEditor.MapObjectType.eObstcale;
                }
                if (worldPos.y < 0 || worldPos.y >= m_numberOfRows)
                {
                    mia_array[i, j] = SceneEditor.MapObjectType.eObstcale;
                }
            }
        }

    }

    void OnGUI()
    {

    }

    void OnDrawGizmos()
    {
        Vector3 origin = transform.position;
        int numRows = m_numberOfRows; //行 y
        int numCols = m_numberOfColumns; //列 x
        float cellSize = m_cellSize;

        Color color = m_debugColor;
        Gizmos.color = m_debugColor;

        if (m_debugShow)
        {
            //float realwidth = Mathf.CeilToInt( 0.5f * (numRows / cellSize) / (float)Math.Sin(Math.PI * Math.Abs(m_XAngle) / 180.0) + 0.5f*(numCols/cellSize)/(float)Math.Cos(Math.PI*Math.Abs(m_XAngle)/180.0));
            //float realHeight = Mathf.CeilToInt(0.5f * (numRows / cellSize) / (float)Math.Sin(Math.PI * m_YAngle / 180.0) + 0.5f * (numCols / cellSize) / (float)Math.Cos(Math.PI * m_YAngle / 180.0));

            int realwidth = Mathf.CeilToInt(m_numberOfColumns / m_cellSize);
            int realHeight = Mathf.CeilToInt(m_numberOfRows / m_cellSize);

            //Vector3 originalPos = new Vector3(-0.5f * numRows / (float)Math.Sin(Math.PI * Math.Abs(m_XAngle) / 180.0), 0.5f * numRows, 0);
            Vector3 originalPos = new Vector3(0, 0,-10);
            //横线
            for (int i=0; i<= realwidth; i++)
            {
                Vector3 startPos = originalPos + i * kLogicXAxis;
                Vector3 endPos = startPos + realHeight * kLogicYAxis;
                Debug.DrawLine(startPos, endPos, color);
            }

            //竖线
            for(int j=0; j<=realHeight; j++)
            {
                Vector3 startPos = originalPos + j * kLogicYAxis;
                Vector3 endPos = startPos + realwidth * kLogicXAxis;
                Debug.DrawLine(startPos, endPos, color);
            }
            

                              
        }
        if (m_bIsShowObstacle)
        {
            Gizmos.color = new Color(1, 0, 0, .4f);
            //if (!m_TempObjectInfo.ContainsKey(0))
            //{
            //    return;
            //}
            //foreach (int xkey in m_TempObjectInfo[0].Keys)
            //{
            //    Dictionary<int, SceneEditor.MapObjectType> ObjInfo = m_TempObjectInfo[0][xkey];

            //    foreach ( int ykey in ObjInfo.Keys)
            //    {

            //        if (ObjInfo[ykey] == SceneEditor.MapObjectType.eObstcale)
            //        {
            //Vector2 worldPos = GetWorldPosByMapPos(xkey, ykey);
            // //Gizmos.DrawCube(new Vector3(gameObject.transform.position.x + worldPos.x, gameObject.transform.position.y + worldPos.y , transform.position.z - 0.5f), new Vector3(0.4f, 0.4f, 0));
            // Gizmos.DrawSphere(new Vector3(gameObject.transform.position.x + worldPos.x, gameObject.transform.position.y + worldPos.y, transform.position.z - 0.5f), 0.5f);
            //        }

            //    }    
            //}
            
            for(int i= 0; i < mia_array.GetLength(0); i++)
            {
                for (int j = 0; j < mia_array.GetLength(1); j++)
                {
                    Vector2 worldPos = GetWorldPosByMapPos(i, j);                                     

                    if (mia_array[i, j] == SceneEditor.MapObjectType.eObstcale)
                   {
                        
                        Gizmos.DrawSphere(new Vector3(gameObject.transform.position.x + worldPos.x, gameObject.transform.position.y + worldPos.y, transform.position.z - 10.5f), 0.3f);
                   }
                    //Gizmos.DrawCube(new Vector3(gameObject.transform.position.x + worldPos.x, gameObject.transform.position.y + worldPos.y , transform.position.z - 0.5f), new Vector3(0.4f, 0.4f, 0));
                    
                }
            }
        }
    }
    //根据地图坐标获取世界坐标
    public  Vector2 GetWorldPosByMapPos(float x , float y)
    {
        Vector3 mapPos = new Vector3(x, y, 0);
        x = x + 0.5f;
        y = y + 0.5f;
        //float newX1 = m_cellSize* y* (float)Math.Cos(Math.PI * m_YAngle / 180.0);
        //float newY1 = m_cellSize * y * (float)Math.Sin(Math.PI * m_YAngle / 180.0);

        //Vector2 mapPosX = new Vector2(newX1, newY1);

        //float newX2 = m_cellSize * x * (float)Math.Cos(Math.PI * m_XAngle / 180.0);
        //float newY2 = m_cellSize * x * (float)Math.Sin(Math.PI *  m_XAngle / 180.0);

        //Vector2 mapPosY = new Vector2(newX2, newY2);

        //Vector3 originalPos = new Vector3(-0.5f * m_numberOfRows / (float)Math.Sin(Math.PI * Math.Abs(m_XAngle) / 180.0), 0.5f * m_numberOfRows, 0);
        //return new Vector2(newX1 + newX2+originalPos.x, newY1 + newY2+originalPos.y);

        return new Vector2(x * m_cellSize, y * m_cellSize);
    }

    //根据世界坐标获取地图坐标
    public Vector2 GetMapPosByWordPos(float x, float y)
    {


        //Vector3 originalPos = new Vector3(-0.5f * m_numberOfRows / (float)Math.Sin(Math.PI * Math.Abs(m_XAngle) / 180.0), 0.5f * m_numberOfRows, 0);
        //Vector3 worldPos = new Vector3(x, y, 0) + Vector3.zero-originalPos;

        //float newY1 = (worldPos.x /2 ) /(float) Math.Cos(Math.PI * m_YAngle / 180.0) ;
        //newY1 = newY1 / m_cellSize;
        //float newX1 = m_cellSize*(worldPos.x / (m_cellSize * 2)) / (float)Math.Cos(Math.PI * m_XAngle / 180.0);
        //newX1 = newX1 / m_cellSize;

        //Vector2 mapPosX = new Vector2(newX1, newY1);

        //float newY2 = m_cellSize * (worldPos.y / (m_cellSize * 2)) / (float)Math.Sin(Math.PI *m_YAngle / 180.0);
        //float newX2 = m_cellSize * (worldPos.y / (m_cellSize * 2)) / (float)Math.Sin(Math.PI * m_XAngle / 180.0);
        //newY2 = newY2 / m_cellSize;
        //newX2 = newX2 / m_cellSize;

        //Vector2 mapPosY = new Vector2(newX2, newY2);


        //return new Vector2(Mathf.FloorToInt(newX1 + newX2), Mathf.FloorToInt(newY1 + newY2));

        int posx = Mathf.FloorToInt( x / m_cellSize);
        int posy = Mathf.FloorToInt( y / m_cellSize);
        return new Vector2(posx, posy);
    }

    //才用全阻挡，默认都是阻挡，
    public bool IsObstcale(int x, int y)
    {
        return IsObjectTypeMatch(x, y, SceneEditor.MapObjectType.eObstcale);
    }

    public bool IsMapPosValid(int x, int y)
    {
        return true;
    }

    public bool IsObjectTypeMatch(int x, int y, SceneEditor.MapObjectType eType)
    {

        if (x < 0 || x >= mia_array.GetLength(0))
        {
            return false;
        }
        if(y<0 || y >= mia_array.GetLength(1))
        {
            return false;
        }
        if(mia_array[x,y] == eType)
        {
            return true;
        }else
        {
            return false;
        }
        //if (!m_TempObjectInfo.ContainsKey(0))
        //{
        //    return false;
        //}
        //if (m_TempObjectInfo[0].ContainsKey(x))
        //{
        //    Dictionary<int, SceneEditor.MapObjectType> ObjInfo = m_TempObjectInfo[0][x];
        //    SceneEditor.MapObjectType value;
        //    if (ObjInfo.TryGetValue(y, out value))
        //    {
        //        if (value == eType)
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        return false;
        //    }

        //}
        //return false;
    }

    public void SetPosObstcale(int posx,int posy, bool isObsctale)
    {
        //if (posx < 0 || posx > m_numberOfColumns)
        //{
        //    return;
        //}

        //if (posy < 0 || posy > m_numberOfRows)
        //{
        //    return;
        //}
        if (posx < 0 || posx >= mia_array.GetLength(0))
        {
            return;
        }
        if (posy < 0 || posy >= mia_array.GetLength(1))
        {
            return;
        }

       // mia_array[posx, posy] = isObsctale;

       //添加阻挡
        if (isObsctale)
        {
            // 已经是阻挡了
            if (IsObstcale(posx, posy))
            {//直接忽略
                return;
            }

            //if (!m_TempObjectInfo.ContainsKey(0))
            //{
            //    m_TempObjectInfo[0] = new Dictionary<int, Dictionary<int, SceneEditor.MapObjectType>>();
            //}

            //if(!m_TempObjectInfo[0].ContainsKey(posx))
            //{
            //    m_TempObjectInfo[0][posx] = new Dictionary<int, SceneEditor.MapObjectType>();

            //}


            //m_TempObjectInfo[0][posx][posy] = SceneEditor.MapObjectType.eObstcale;
            mia_array[posx, posy] = SceneEditor.MapObjectType.eObstcale;
        }
        else//删除阻挡步奏
        {
            // 不是阻挡
            if (!IsObstcale(posx, posy))
            {//直接忽略
                return;
            }

            mia_array[posx, posy] = SceneEditor.MapObjectType.eEmpty;
        }
    }

    //清除阻挡点信息
    public void ClearAllObscaleInfo()
    {
        if(!m_TempObjectInfo.ContainsKey(0))
        {
            return;
        }
       foreach (int key in m_TempObjectInfo[0].Keys)
       {
            Dictionary<int, SceneEditor.MapObjectType> ObjInfo = m_TempObjectInfo[0][key];

            List<int> removeInfo = new List<int>();
            foreach (int index in ObjInfo.Keys)
            {
                if (ObjInfo[index] == SceneEditor.MapObjectType.eObstcale)
                {
                    removeInfo.Add(index);
                }
            }

            foreach(int removeIndex in removeInfo)
            {
                m_TempObjectInfo[0][key].Remove(removeIndex);
            }

       }
       if(mia_array != null)
        {
            for(int i=0; i < mia_array.GetLength(0); i++)
            {
                for(int j=0; j<mia_array.GetLength(1); j++)
                {
                    if(mia_array[i,j] == SceneEditor.MapObjectType.eObstcale)
                    {
                        mia_array[i, j] = SceneEditor.MapObjectType.eEmpty;
                    }
                }
            }
        }
    }
    
   

}
#endif