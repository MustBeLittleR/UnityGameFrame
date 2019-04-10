using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System;
using SceneEditor;
using LuaInterface;
namespace SceneEditor
{
    [Serializable]
    struct CGridDataLayer
    {
        public uint m_iWidth, m_iHeight;   // logic size of this scene layer
        public int m_iGridSpacingPower;            // for real world with graphic representation, 1 logic unit means GridSpacing real world unit
        public int m_iTypeSize;                  // the data type size, 1=8bit, 2=16bit, 4=32bit, 8=64bit

       public /*char**/int m_Data;                //原本是一个char*，因c#没有char*，用int代替
    }
    
}
public class EditerDataClass 
{
    public static EditerDataClass _Instance = new EditerDataClass();
    bool m_bOnImport = false; //是否正在导出


    //NPC列表
    Dictionary<int, NpcInfo> m_NpcInfoList = new Dictionary<int, NpcInfo>();

    //FunNPC列表
    Dictionary<int, FunNpcInfo> m_FunNpcInfoList = new Dictionary<int, FunNpcInfo>();

    //Obj列表
    Dictionary<int, ObjInfo> m_ObjInfoList = new Dictionary<int, ObjInfo>();




    LuaState m_oLuamgr = null;
    LuaFunction m_fLoadNpcFun = null;
    LuaFunction m_fLoadFunNpcFun = null;
    LuaFunction m_fLoadObjFun = null;
   PathGridComponent m_csPathGrid { get; set; }

    #region 接口函数
    //public void OnCreateGameObj(GameObject obj, int id, CreateMode mode)
    //{
    //    SetBornInfo(ref obj, id);
    //    BornInfo info = obj.GetComponent<BornInfo>();
    //    info.mi_Id = id;
    //    info.me_Type = mode;
    //    switch (mode)
    //    {
    //        case CreateMode.enMonstBorn:

    //           // info.mfc_destory = MonsterBornDestorydelegate;
    //            break;
    //        case CreateMode.enfixMonsterBorn:
    //           // AddBornToTotalDic(obj, id, ref m_dcBornTotalNumber);
    //            break;
    //        case CreateMode.enObj:
    //           // AddBornToTotalDic(obj, id, ref m_dcOBjTotalNumber);

    //           // info.mfc_destory = SetObjIdFree;
    //            break;
    //        default:
    //            break;
    //    }


    //}

    public void Init()
    {
       

        InitLuaInfo();
        m_csPathGrid = InteractionTool.GetPathGridComponent();


    }

    public void OnDestory()
    {
        m_oLuamgr.Dispose();
    }


    #endregion

    //初始化Lua信息
    void InitLuaInfo()
    {              
        m_oLuamgr =  new LuaState();
        m_oLuamgr.Start();
        string filelua = File.ReadAllText(Application.dataPath + "/Z_Temp/Obstacle/LuaTool.lua");
        m_oLuamgr.DoString(filelua);

                
        m_fLoadFunNpcFun = m_oLuamgr.GetFunction("LoadNpcMapInfo");        

        object[] npcInfo = m_oLuamgr.GetFunction("InitNpcInfo").LazyCall();
        InitNpcInfo(npcInfo);
        object[] tFunNpc = m_oLuamgr.GetFunction("InitFunNpcInfo").LazyCall();
        InitFunNpcInfo(tFunNpc);
        object[] objInfo = m_oLuamgr.GetFunction("InitOjbInfo").LazyCall();
        InitObjInfo(objInfo);
    }


    private List<string> GetInfoWithId(Dictionary<int, List<string>> ValueList, Dictionary<string, int> IdList, string Id)
    {
        List<string> Value;
        int nIndex = -1;
        if (IdList.TryGetValue(Id, out nIndex))
        {
            if(ValueList.TryGetValue(nIndex, out Value))
            {
                return Value;
            }
        }
        return null;
    }


    public void InitNpcInfo(object[] npcInfo)
    {
        //m_NpcInfoList;
        string[] sIdInfo = npcInfo[0].ToString().Split(',');
        LuaTable tInfo = (LuaTable)npcInfo[1];
        for (int i = 0; i < sIdInfo.Length; i++)
        {
            string sId = sIdInfo[i];
            int npcId = int.Parse(sId);
            LuaTable tNpcInfo = (LuaTable)tInfo[npcId];

            NpcInfo npc = new NpcInfo();
            if (tNpcInfo!=null)
            {
                npc.setInfo(tNpcInfo["npcid"].ToString(), tNpcInfo["name"].ToString(), tNpcInfo["npclevel"].ToString(), tNpcInfo["npcresource"].ToString());
                m_NpcInfoList.Add(npcId, npc);
            }
            

            
        }
    }

    public void InitFunNpcInfo(object[] funNpcInfo)
    {
        //m_FunNpcInfoList;
        string[] sIdInfo = funNpcInfo[0].ToString().Split(',');
        LuaTable tInfo = (LuaTable)funNpcInfo[1];
        for (int i = 0; i < sIdInfo.Length; i++)
        {
            string sId = sIdInfo[i];
            int npcId = int.Parse(sId);
            LuaTable tFunNpcInfo = (LuaTable)tInfo[npcId];

            FunNpcInfo funnpc = new FunNpcInfo();
            if (tFunNpcInfo != null)
            {
                funnpc.setInfo(tFunNpcInfo["npcid"].ToString(), tFunNpcInfo["name"].ToString(), "1", tFunNpcInfo["npcresource"].ToString());
                m_FunNpcInfoList.Add(npcId, funnpc);
            }
            

            
        }


    }

    public void InitObjInfo(object[] ObjInfo)
    {
        //m_ObjInfoList;
        string[] sIdInfo = ObjInfo[0].ToString().Split(',');
        LuaTable tInfo = (LuaTable)ObjInfo[1];
        for (int i = 0; i < sIdInfo.Length; i++)
        {
            string sId = sIdInfo[i];
            int npcId = int.Parse(sId);
            LuaTable tObjInfo = (LuaTable)tInfo[npcId];

            ObjInfo obj = new ObjInfo();
            if(tObjInfo != null)
            {
                obj.setInfo(sId, tObjInfo["ObjName"].ToString(), "1", tObjInfo["ModelName"].ToString());

                m_ObjInfoList.Add(npcId, obj);
            }
            
        }
    }
    
   
    public  Dictionary<int, NpcInfo> GetNpcInfo()
    {
        return m_NpcInfoList;
    }

    public NpcInfo GetNpcInfo(int id)
    {
        return m_NpcInfoList[id];
    }

    public Dictionary<int, FunNpcInfo> GetFunNpcInfo()
    {
        return m_FunNpcInfoList;
    }

    public FunNpcInfo GetFunNpcInfo(int id)
    {
        return m_FunNpcInfoList[id];
    }

    //获取OBJ信息
    public Dictionary<int, ObjInfo> GetObjInfo()
    {
        return m_ObjInfoList;
    }
    public ObjInfo GetObjInfo(int id)
    {
        return m_ObjInfoList[id];
    }
   

    //导出地图信息(传入导出内容类型 0阻挡)
    public void ExportMapInfo(string sFileName, SceneEditor.MapObjectType eType)
    {
        File.Delete(sFileName);
        string SceneName = InteractionTool.ms_SceneName;
        string ObstacleLastName = InteractionTool.m_sObstacleLastName;
        try
        {
            using (FileStream file = new FileStream(sFileName, FileMode.CreateNew))
            {
                BinaryWriter sw = new BinaryWriter(file, System.Text.Encoding.UTF8);
                CGridDataLayer data;
                data.m_iWidth = (uint)m_csPathGrid.mia_array.GetLength(0);
                data.m_iHeight = (uint)m_csPathGrid.mia_array.GetLength(1);
                data.m_iGridSpacingPower = 0;
                data.m_iTypeSize = 2;
                data.m_Data = m_csPathGrid.m_numberOfRows*1000 + m_csPathGrid.m_numberOfColumns;
                sw.Write(data.m_iWidth);
                sw.Write(data.m_iHeight);
                sw.Write(data.m_iGridSpacingPower);
                sw.Write(data.m_iTypeSize);
                sw.Write(data.m_Data);

                for (int j = 0; j < m_csPathGrid.mia_array.GetLength(1); j++)
                {
                    for (int i = 0; i < m_csPathGrid.mia_array.GetLength(0); i++)
                    {
                        sw.Write((Int16)m_csPathGrid.mia_array[i, j]);
                    }
                }
                //sw.WriteLine("Id,Type,Posx,Posy,Posz");
                //sw.WriteLine("int,String,int,int,int");
                ////地图size
                //sw.WriteLine("0,MapSize," + m_csPathGrid.m_numberOfColumns.ToString() + "," + m_csPathGrid.m_numberOfRows.ToString() + ",");
                
                //Transform AllParent = InteractionTool.GetAllParent();
                ////初始点位置
                //sw.WriteLine("1,StartPos," + AllParent.position.x.ToString() + "," + AllParent.position.z.ToString() + ",");

                //Dictionary<int, Dictionary<int, Dictionary<int, SceneEditor.MapObjectType>>> ObjInfo = m_csPathGrid.GetMapObjectInfo();
                //int nBeginLine = 2;

                //foreach(int xKey in ObjInfo[0].Keys)
                //{
                //    Dictionary<int, SceneEditor.MapObjectType> Values = ObjInfo[0][xKey];
                //    foreach(int yKey in Values.Keys)
                //    {
                //        if(Values[yKey] == SceneEditor.MapObjectType.eObstcale)
                //        {
                //            sw.WriteLine(nBeginLine.ToString() + "," + eType.ToString() + "," + xKey.ToString() + "," + yKey.ToString() + "," + "0");
                //            nBeginLine++;
                //        }
                //    }

                //}
             
                sw.Close();
                file.Close();
            }
        }

        catch (IOException ex)
        {
            Debug.LogError(SceneName + "阻挡导出失败: " + ex.ToString());
            return;
        }
        
        Debug.Log(SceneName + " 阻挡写入成功");
    }

    public void ExportClientMapInfo(string sFileName, MapObjectType eType)
    {
        File.Delete(sFileName);
        string SceneName = InteractionTool.ms_SceneName;
        string ObstacleLastName = InteractionTool.m_sObstacleLastName;
        try
        {
            using (FileStream file = new FileStream(sFileName, FileMode.CreateNew))
            {

                StreamWriter sw = new StreamWriter(file, new System.Text.UTF8Encoding(false));
                sw.WriteLine("local tPos={");    
                sw.WriteLine("  nWidth = " + m_csPathGrid.mia_array.GetLength(0) + ",");
                sw.WriteLine("  nHeight =" + m_csPathGrid.mia_array.GetLength(1) + ",");
                sw.WriteLine("  nRows =" + m_csPathGrid.m_numberOfRows + ",");
                sw.WriteLine("  nColumns = " + m_csPathGrid.m_numberOfColumns + ",");
                sw.WriteLine("  nCellSize = " + PathGridComponent.m_cellSize+",");
                for (int j = 0; j < m_csPathGrid.mia_array.GetLength(1); j++)
                {
                    string sGridInfo = "    {";
                    int index = 0;
                    for (int i = 0; i < m_csPathGrid.mia_array.GetLength(0); i++)
                    {
                        if (index == 0)
                        {
                            sGridInfo = sGridInfo + (int)m_csPathGrid.mia_array[i, j];
                        }
                        else
                        {
                            sGridInfo = sGridInfo + "," + (int)m_csPathGrid.mia_array[i, j] ;
                        }
                        index++;
                                               
                    }
                    sGridInfo = sGridInfo + "},";
                    sw.WriteLine(sGridInfo);
                }
                
                sw.WriteLine("}");
                sw.WriteLine("return tPos");


                
                //sw.WriteLine("Id,Type,Posx,Posy,Posz");
                //sw.WriteLine("int,String,int,int,int");
                ////地图size
                //sw.WriteLine("0,MapSize," + m_csPathGrid.m_numberOfColumns.ToString() + "," + m_csPathGrid.m_numberOfRows.ToString() + ",");

                //Transform AllParent = InteractionTool.GetAllParent();
                ////初始点位置
                //sw.WriteLine("1,StartPos," + AllParent.position.x.ToString() + "," + AllParent.position.z.ToString() + ",");

                //Dictionary<int, Dictionary<int, Dictionary<int, SceneEditor.MapObjectType>>> ObjInfo = m_csPathGrid.GetMapObjectInfo();
                //int nBeginLine = 2;

                //foreach(int xKey in ObjInfo[0].Keys)
                //{
                //    Dictionary<int, SceneEditor.MapObjectType> Values = ObjInfo[0][xKey];
                //    foreach(int yKey in Values.Keys)
                //    {
                //        if(Values[yKey] == SceneEditor.MapObjectType.eObstcale)
                //        {
                //            sw.WriteLine(nBeginLine.ToString() + "," + eType.ToString() + "," + xKey.ToString() + "," + yKey.ToString() + "," + "0");
                //            nBeginLine++;
                //        }
                //    }

                //}

                sw.Close();
                file.Close();
            }
        }

        catch (IOException ex)
        {
            Debug.LogError(SceneName + "阻挡导出失败: " + ex.ToString());
            return;
        }

        Debug.Log(SceneName + " 阻挡写入成功");
    }
    //导入阻挡
    public void Importobstacle(string filename)
    {
        InteractionTool.ClearObscaleInfo();
        Debug.Log("阻挡开始导入");
        
        //filename = filename.Substring(filename.IndexOf("/Scene/") + "/Scene/".Length).Replace(".csv", "");


        FileStream fs = new FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read);
               
        BinaryReader sr = new BinaryReader(fs, Encoding.UTF8);
        //开始读配置文件,生成阻挡对象

        CGridDataLayer data;
        data.m_iWidth = sr.ReadUInt32();
        data.m_iHeight = sr.ReadUInt32();
        data.m_iGridSpacingPower = sr.ReadInt32();
        data.m_iTypeSize = sr.ReadInt32();
        data.m_Data = sr.ReadInt32();        
        m_csPathGrid.m_numberOfColumns = data.m_Data % 1000;
        m_csPathGrid.m_numberOfRows = data.m_Data / 1000;
        m_csPathGrid.InitArray();

        for (int j = 0; j < data.m_iHeight; j++)
        {
            for (int i = 0; i < data.m_iWidth; i++)
            {
                Int16 eValue = sr.ReadInt16();
                if(i<m_csPathGrid.mia_array.GetLength(0) && j < m_csPathGrid.mia_array.GetLength(1))
                {
                    m_csPathGrid.mia_array[i, j] = (MapObjectType)eValue;
                }                
            }
        }
        //while ((strLine = sr.ReadLine()) != null)
        //{
        //    if(nLineCount++<2)
        //    {
        //        if(nLineCount == 2)
        //        {
        //            TypeInfo = strLine.Split(',');
        //        }
        //        continue;
        //    }
        //    LineStrInfo = strLine.Split(',');
        //    if(LineStrInfo.Length>0)
        //    {
        //        if(LineStrInfo[1] == "MapSize")
        //        {
        //            m_csPathGrid.m_numberOfRows = int.Parse(LineStrInfo[2]);
        //            m_csPathGrid.m_numberOfColumns = int.Parse(LineStrInfo[3]);
        //        }
        //        else if(LineStrInfo[1] == "StartPos")
        //        {
        //            int nStartX = int.Parse(LineStrInfo[2]);
        //            int nStartY = int.Parse(LineStrInfo[3]);
        //            InteractionTool.GetAllParent().position = new Vector3(nStartX, 0, nStartY);
        //        }
        //        else
        //        {
        //            int nPosX = int.Parse(LineStrInfo[2]);
        //            int nPosY = int.Parse(LineStrInfo[3]);
        //            m_csPathGrid.SetPosObstcale(nPosX, nPosY, true);
        //        }
        //    }
        //}
        sr.Close();
        fs.Close();
        SceneEditorWindow._windowInstance.m_sObstaclePath = filename;
        Debug.Log("阻挡导入成功");
        InteractionTool.SetCanDraw(true);
    }
    
    
    public void ImportNpcMapInfo(string filePath)
    {
        m_bOnImport = true;
       
        CreateMode mode = InteractionTool.GetNowCreateMode();
        BornInfo info;
        int preid = InteractionTool.m_iChooseFunNpcId;
       
        InteractionTool.SetCanDraw(false);         


        //fileName = fileName.Substring(filename.IndexOf("/Scene/") + "/Scene/".Length).Replace(".lua", "");
        string fileName = filePath.Substring(filePath.IndexOf("/Map/")+1).Replace(".lua", "");
        object[] objarray = m_fLoadFunNpcFun.LazyCall(fileName);
        string sRegionNpcIdInfo = objarray[0].ToString();
        LuaTable tRegionNpcInfo = (LuaTable)objarray[1];
        string sPointNpcIdInfo = objarray[2].ToString();
        LuaTable tPointNpcInfo = (LuaTable)objarray[3];
        string sPathSetInfo = objarray[4].ToString();
        LuaTable tPathSetInfo = (LuaTable)objarray[5];
        string sScenePointInfo = objarray[6].ToString();
        LuaTable tScenenPointInfo = (LuaTable)objarray[7];
        

        
        string[] npcIdList = sRegionNpcIdInfo.Split(',');
        for(int i=0; i< npcIdList.Length; i++)
        {
            int id = -1;
            if (!int.TryParse(npcIdList[i], out id))
            {
                continue;
            }
            
            LuaTable npcInfo = (LuaTable)tRegionNpcInfo[id];

            int Type = int.Parse(npcInfo["Type"].ToString());
            int ID = int.Parse(npcInfo["ID"].ToString());
            int GridX = int.Parse(npcInfo["GridX"].ToString());
            int GridY = int.Parse(npcInfo["GridY"].ToString());
            int GridW = int.Parse(npcInfo["GridW"].ToString());
            int GridH = int.Parse(npcInfo["GridH"].ToString());
            int summonNum = int.Parse(npcInfo["NpcSum"].ToString());
            List<RegionObjInfo> regionObjInfo = new List<RegionObjInfo>();
            int ObjId = 0;
            int Dir = 0;
            for (int key=0; key < summonNum; key++)
            {
                int showkey = key + 1;
                LuaTable tNpc = (LuaTable)npcInfo["npc_"+showkey];
                ObjId = int.Parse(tNpc["ObjId"].ToString());
                int Nums = int.Parse(tNpc["Nums"].ToString());
                Dir = int.Parse(tNpc["Dir"].ToString());
                int Time = int.Parse(tNpc["Time"].ToString());

                int nRate = 0;
                if (tNpc["Rate"] != null && tNpc["Rate"].ToString() != "")
                {
                    nRate = int.Parse(tNpc["Rate"].ToString());
                }

                RegionObjInfo newobj = new RegionObjInfo();
                newobj.ObjId = ObjId;
                newobj.Nums = Nums;
                newobj.Dir = Dir;
                newobj.Time = Time;
                newobj.Per = 0;
                newobj.Rate = nRate;
                regionObjInfo.Add(newobj);
            }
            
            
            int minPower = 1;
            if (npcInfo["MinPower"] != null && npcInfo["MinPower"].ToString() != "")
            {
                minPower = int.Parse(npcInfo["MinPower"].ToString());
            }
            int maxPower = 99;
            if (npcInfo["MaxPower"] != null && npcInfo["MaxPower"].ToString() != "")
            {
                maxPower = int.Parse(npcInfo["MaxPower"].ToString());
            }          

            
            GameObject obj = InteractionTool.AddReginNpc(GridX, GridY, -1, ObjId, (CreateObjType)Type, Dir);
            if (obj != null)
            {
                BornInfo bornInfo = obj.GetComponent<BornInfo>();
                bornInfo.m_minPower = minPower;
                bornInfo.m_maxPower = maxPower;
                bornInfo.m_nWidth = GridW;
                bornInfo.m_nHeight = GridH;
                bornInfo.m_RegionNpcInfo = regionObjInfo;
            }                                    
        }

        
        string[] funNpcIdList = sPointNpcIdInfo.Split(',');
        for(int i=0; i<funNpcIdList.Length; i++)
        {
            int funnpcid = -1;
            if(!int.TryParse(funNpcIdList[i], out funnpcid))
            {
                continue;
            }            
            LuaTable funNpcInfo = (LuaTable)tPointNpcInfo[funnpcid];
            int Type = int.Parse(funNpcInfo["Type"].ToString());
            int ID = int.Parse(funNpcInfo["ID"].ToString());
            int nTime = 0;
            if (funNpcInfo["Time"] != null && funNpcInfo["Time"].ToString() != "")
            {
                nTime = int.Parse(funNpcInfo["Time"].ToString());
            }

            int nLiveTime = 0;
            if (funNpcInfo["LiveTime"] != null && funNpcInfo["LiveTime"].ToString() != "")
            {
                nLiveTime = int.Parse(funNpcInfo["LiveTime"].ToString());
            }

            int nDelayBornTime = 0;
            if (funNpcInfo["DelayBornTime"] != null && funNpcInfo["DelayBornTime"].ToString() != "")
            {
                nLiveTime = int.Parse(funNpcInfo["DelayBornTime"].ToString());
            }

            int nRate = 0;
            if (funNpcInfo["Rate"] != null && funNpcInfo["Rate"].ToString() != "")
            {
                nRate = int.Parse(funNpcInfo["Rate"].ToString());
            }

            int nGraphicDir = 0;
            if (funNpcInfo["GraphicDir"] != null && funNpcInfo["GraphicDir"].ToString() != "")
            {
                nGraphicDir = int.Parse(funNpcInfo["GraphicDir"].ToString());
            }
            int ObjId = int.Parse(funNpcInfo["ObjId"].ToString());
            int GridX = int.Parse(funNpcInfo["GridX"].ToString());
            int GridY = int.Parse(funNpcInfo["GridY"].ToString());
            int Dir = int.Parse(funNpcInfo["Dir"].ToString());
            int nTrapSceneId = 0;
            int nTrapPosX = 0;
            int nTrapPosY = 0;
            if (funNpcInfo["Tranp_SceneID"]!=null && funNpcInfo["Tranp_SceneID"].ToString() != "")
            {
                nTrapSceneId = int.Parse(funNpcInfo["Tranp_SceneID"].ToString());
                nTrapPosX = int.Parse(funNpcInfo["Tranp_PosX"].ToString());
                nTrapPosY = int.Parse(funNpcInfo["Tranp_PosY"].ToString());
            }
            int minPower = 1;
            if (funNpcInfo["MinPower"] != null && funNpcInfo["MinPower"].ToString() != "")
            {
                minPower = int.Parse(funNpcInfo["MinPower"].ToString());
            }
            int maxPower = 99;
            if (funNpcInfo["MaxPower"] != null && funNpcInfo["MaxPower"].ToString() != "")
            {
                maxPower = int.Parse(funNpcInfo["MaxPower"].ToString());
            }
           
            int RoundType = 0;
            if (funNpcInfo["RoundType"] != null && funNpcInfo["RoundType"].ToString() != "")
            {
                RoundType = int.Parse(funNpcInfo["RoundType"].ToString());
            }
            int LinkID = 0;
            if (funNpcInfo["LinkID"] != null && funNpcInfo["LinkID"].ToString() != "")
            {
                LinkID = int.Parse(funNpcInfo["LinkID"].ToString());
            }
           
            
            int RoundNum = 0;
            if (funNpcInfo["RoundNum"] != null && funNpcInfo["RoundNum"].ToString() != "")
            {
                RoundNum = int.Parse(funNpcInfo["RoundNum"].ToString());
            }



            List<PatrolPointInfo> patrolInfo = new List<PatrolPointInfo>();
            int MoveMode = 0;
            int MoveRadius = 0;
            if (RoundNum>0)
            {
                for(int key=0; key<RoundNum; key++)
                {
                    LuaTable tRoundInfo = (LuaTable)funNpcInfo["Round_"+key.ToString()];
                    PatrolPointInfo patrolpoint = new PatrolPointInfo();
                    patrolpoint.m_WaitTime = int.Parse(tRoundInfo["WaitTime"].ToString());
                    patrolpoint.m_ActionId = int.Parse(tRoundInfo["ActionID"].ToString());
                    patrolpoint.m_GridX = int.Parse(tRoundInfo["GridX"].ToString());
                    patrolpoint.m_GridY = int.Parse(tRoundInfo["GridY"].ToString());
                    patrolInfo.Add(patrolpoint);
                }
            }
            else
            {
                
                if (funNpcInfo["MoveMode"] != null && funNpcInfo["MoveMode"].ToString() != "")
                {
                    MoveMode = int.Parse(funNpcInfo["MoveMode"].ToString());
                }
                
                if (funNpcInfo["MoveRadius"] != null && funNpcInfo["MoveRadius"].ToString() != "")
                {
                    MoveRadius = int.Parse(funNpcInfo["MoveRadius"].ToString());
                }
            }

            GameObject obj = InteractionTool.AddPointNpc(GridX, GridY, -1, ObjId, (CreateObjType)Type, Dir);
            if (obj != null)
            {
                BornInfo bornInfo = obj.GetComponent<BornInfo>();
                bornInfo.m_nRebornTime = nTime;
                bornInfo.m_nTrapSceneId = nTrapSceneId;
                bornInfo.m_nPosX = nTrapPosX;
                bornInfo.m_nPosY = nTrapPosY;
                
                bornInfo.m_minPower = minPower;
                bornInfo.m_maxPower = maxPower;
                bornInfo.mi_Group = LinkID;
                bornInfo.m_RoundType = RoundType;
                
                bornInfo.m_nMoveMode = MoveMode;
                bornInfo.m_nMoveRadius = MoveRadius;
                bornInfo.m_nLiveTime = nLiveTime;
                bornInfo.m_nDelayBornTime = nDelayBornTime;
                bornInfo.m_nRate = nRate;
                bornInfo.graphDir = nGraphicDir;
                for (int n =0; n<patrolInfo.Count; n++)
                {
                    PatrolPointInfo point = patrolInfo[n];
                    InteractionTool.AddNpcPatrolPoint(point.m_GridX, point.m_GridY, -1, obj);
                }
                bornInfo.m_PatrolInfo = patrolInfo;
            }                       
        }

        
        Debug.Log("导入功能NPC文件完成：" + filePath);
      
        InteractionTool.SetCanDraw(true);
        InteractionTool.SetCreateMode(mode);
        m_bOnImport = false;
        SceneEditorWindow._windowInstance.m_sBornListPath = filePath;
    }


    public void ExportMapFunNpcInfo(string fileName)
    {
        string SceneName = InteractionTool.ms_SceneName;
        string MonsterBornLastName = InteractionTool.m_sMonsterBornLastName;

        File.Delete(fileName);
        InteractionTool.CheekParent();
        try
        {            
            Transform RegionParent = InteractionTool.GetRegionParent();
            Transform Pointparent = InteractionTool.GetPointParent();            
            
            Debug.Log(SceneName + " 怪物出生点开始导出");
            using (FileStream file = new FileStream(fileName, FileMode.CreateNew))
            {

                StreamWriter sw = new StreamWriter(file, new System.Text.UTF8Encoding(false));
                string PointSet = "PointSet"; //funnpc
                string NpcDisposeSet = "NpcDisposeSet"; //npc
                string PathSet = "PathSet";
                string ScenePointSet = "ScenePointSet";
                sw.WriteLine("local Name='Map/" +SceneName + "/"+ SceneName + "FunNpc'");
                sw.WriteLine("local NpcDisposeSet = { }");
                sw.WriteLine("local PointSet = { }");
                sw.WriteLine("local PathSet = { }");
                sw.WriteLine("local ScenePointSet = { }");     
                
                //ExportMonsterBorn(ref tablename, isforEditor, HeroParent, ref sw, ref index);

                ExportRegionNpcInfo(NpcDisposeSet, RegionParent, ref sw);
                ExportPointNpcInfo(PointSet, Pointparent, ref sw);
                //ExportPathSetInfo(PathSet, BornParent, ref sw);
                //ExportScenePointInfo(ScenePointSet, BornParent, ref sw);

                sw.WriteLine("gRebornMgr.AddRegionNpcSet(Name, NpcDisposeSet)");
                sw.WriteLine("gRebornMgr.AddPointSet(Name, PointSet)");
                sw.WriteLine("gRebornMgr.AddPathSet(Name, PathSet)");
                sw.WriteLine("gRebornMgr.AddScenePointSet(Name, ScenePointSet)");                
                sw.Close();
                file.Close();
            }


        }
        catch (IOException ex)
        {
            Debug.LogError(SceneName + "怪物出生点导出失败: " + ex.ToString());
            return;
        }
        Debug.Log(SceneName + " 怪物出生点导出成功");
    }

    public void ExportMapFunNpcInfo2Client(string fileName)
    {
        string SceneName = InteractionTool.ms_SceneName;
        string MonsterBornLastName = InteractionTool.m_sMonsterBornLastName;

        File.Delete(fileName);
        InteractionTool.CheekParent();
        try
        {
            
            Transform RegionParent = InteractionTool.GetRegionParent();
            Transform Pointparent = InteractionTool.GetPointParent();
            

            Debug.Log(SceneName + " 怪物出生点开始导出");
            using (FileStream file = new FileStream(fileName, FileMode.CreateNew))
            {

                StreamWriter sw = new StreamWriter(file, new System.Text.UTF8Encoding(false));
                string PointSet = "PointSet"; //funnpc
                string NpcDisposeSet = "NpcDisposeSet"; //npc
                string PathSet = "PathSet";
                string ScenePointSet = "ScenePointSet";
                sw.WriteLine("local Name='Map/" + SceneName+"/"+ SceneName + "FunNpc'");
                sw.WriteLine("local NpcDisposeSet = { }");
                sw.WriteLine("local PointSet = { }");
                sw.WriteLine("local PathSet = { }");
                sw.WriteLine("local ScenePointSet = { }");
                
                ExportRegionNpcInfo(NpcDisposeSet, RegionParent, ref sw);
                ExportPointNpcInfo(PointSet, Pointparent, ref sw);                

                sw.WriteLine("gSceneQueryTable.AddDisposeSet(Name, NpcDisposeSet)");
                sw.WriteLine("gSceneQueryTable.AddPointSet(Name, PointSet)");
                sw.WriteLine("return {NpcDisposeSet, PointSet, PathSet, ScenePointSet}");
                sw.Close();
                file.Close();
            }


        }
        catch (IOException ex)
        {
            Debug.LogError(SceneName + "怪物出生点导出失败: " + ex.ToString());
            return;
        }
        Debug.Log(SceneName + " 怪物出生点导出成功");
    }





    


    void ExportRegionNpcInfo(string tablename, Transform parent, ref StreamWriter sw)
    {
        int index = 1;

        string key = tablename;

        for (int i = 0; i < parent.childCount; ++i)
        {

            Transform ChildTRF = parent.GetChild(i);
            BornInfo info = ChildTRF.GetComponent<BornInfo>();  
            key = tablename + "[" + (index++).ToString() + "]";            

            Vector2 mapPos = m_csPathGrid.GetMapPosByWordPos(ChildTRF.position.x, ChildTRF.position.y);
            int id = info.mi_Id;
            sw.WriteLine(key + "= {}");

            
            sw.WriteLine(key + "['ID']" + " = " + i);
            sw.WriteLine(key + "['Type']" + " = " + info.m_eObjType);
            sw.WriteLine(key + "['GridX']" + " = " + mapPos.x.ToString());
            sw.WriteLine(key + "['GridY']" + " = " + mapPos.y.ToString());
            sw.WriteLine(key + "['GridW']" + " = " + info.m_nWidth);
            sw.WriteLine(key + "['GridH']" + " = " + info.m_nHeight);
            sw.WriteLine(key + "['NpcSum']" + " = " + info.m_RegionNpcInfo.Count);
            for(int k=0; k< info.m_RegionNpcInfo.Count; k++)
            {
                RegionObjInfo objinfo = info.m_RegionNpcInfo[k];
                int showkey = k + 1;
                sw.WriteLine(key + "['npc_"+showkey+"']" + " = {}");
                sw.WriteLine(key + "['npc_"+showkey+"']['Nums']" + " = " + objinfo.Nums);
                sw.WriteLine(key + "['npc_"+showkey+"']['ObjId']" + " = " + objinfo.ObjId);                
                sw.WriteLine(key + "['npc_" + showkey + "']['Dir']" + " = " + objinfo.Dir);
                sw.WriteLine(key + "['npc_" + showkey + "']['Per']" + " = " + 0);
                sw.WriteLine(key + "['npc_" + showkey + "']['Time']" + " = " + objinfo.Time);
                sw.WriteLine(key + "['npc_" + showkey + "']['Rate']" + " = " + objinfo.Rate);
            }
            
            sw.WriteLine(key + "['MinPower']" + " = " + info.m_minPower);
            sw.WriteLine(key + "['MaxPower']" + " = " + info.m_maxPower);
        }
    }

    void ExportPointNpcInfo(string tablename , Transform parent, ref StreamWriter sw)
    {
        int index = 1;
        string key = tablename;

        for (int i = 0; i < parent.childCount; ++i)
        {

            Transform ChildTRF = parent.GetChild(i);
            BornInfo info = ChildTRF.GetComponent<BornInfo>();
           
            
            key = tablename + "[" + (index++).ToString() + "]";
            Vector2 mapPos = m_csPathGrid.GetMapPosByWordPos(ChildTRF.position.x, ChildTRF.position.y);
            int id = info.mi_Id;
            sw.WriteLine(key + "= {}");

            sw.WriteLine(key + "['ID']" + " = " + i);
            sw.WriteLine(key + "['Type']" + " = " + info.m_eObjType);

            if (info.m_eObjType != 1)
            {                
                sw.WriteLine(key + "['MinPower']" + " = " + info.m_minPower);
                sw.WriteLine(key + "['MaxPower']" + " = " + info.m_maxPower);
            }            
            
            sw.WriteLine(key + "['ObjId']" + " = " + info.mi_ObjId);
            sw.WriteLine(key + "['Time'] = " + info.m_nRebornTime);            
            sw.WriteLine(key + "['Dir']" + " = " + info.rotation);
            if (info.m_eObjType == 1)
            {
                sw.WriteLine(key + "['GraphicDir']" + " = " + info.graphDir);
            }            
            sw.WriteLine(key + "['GridX']" + " = " + mapPos.x);
            sw.WriteLine(key + "['GridY']" + " = " + mapPos.y);
            sw.WriteLine(key + "['LiveTime']  = " + info.m_nLiveTime);
            sw.WriteLine(key + "['DelayBornTime']  = " + info.m_nDelayBornTime);
            sw.WriteLine(key + "['Rate']  = " + info.m_nRate);
            sw.WriteLine(key + "['RoundType'] = " + info.m_RoundType);
            sw.WriteLine(key + "['LinkID'] = " + info.mi_Group);
            sw.WriteLine(key + "['RoundNum'] = " + info.m_PatrolInfo.Count);
            if (info.m_PatrolInfo.Count>0)
            {                
                for (int k = 0; k < info.m_PatrolInfo.Count; k++)
                {
                    int showKey = k ;
                    PatrolPointInfo patrolinfo = info.m_PatrolInfo[k];
                    sw.WriteLine(key + "['Round_"+ showKey + "'] = {}");
                    sw.WriteLine(key + "['Round_" + showKey + "']['WaitTime'] = "+ patrolinfo.m_WaitTime);
                    sw.WriteLine(key + "['Round_" + showKey + "']['ActionID'] = " + patrolinfo.m_ActionId);
                    sw.WriteLine(key + "['Round_" + showKey + "']['GridX'] = " + patrolinfo.m_GridX);
                    sw.WriteLine(key + "['Round_" + showKey + "']['GridY'] = " + patrolinfo.m_GridY);
                    
                }
            }
            else
            {
                sw.WriteLine(key + "['MoveMode'] = " + info.m_nMoveMode);
                sw.WriteLine(key + "['MoveRadius'] = " + info.m_nMoveRadius);
            }                                

            if (info.m_nTrapSceneId > 0)
            {
                sw.WriteLine(key + "['Tranp_SceneID'] = " + info.m_nTrapSceneId);
                sw.WriteLine(key + "['Tranp_PosX'] = " + info.m_nPosX);
                sw.WriteLine(key + "['Tranp_PosY'] = " + info.m_nPosY);
                sw.WriteLine(key + "['Tranp_Dir'] = " + 0);
            }

        }
    }

    

    //导出场景点(包括随机刷怪)
    void ExportScenePointInfo(string tablename, Transform parent, ref StreamWriter sw)
    {
        int index = 1;
        string key = tablename;

        for (int i = 0; i < parent.childCount; ++i)
        {

            Transform ChildTRF = parent.GetChild(i);
            BornInfo info = ChildTRF.GetComponent<BornInfo>();
           
           
            key = tablename + "[" + (index++).ToString() + "]";
            Vector2 mapPos = m_csPathGrid.GetMapPosByWordPos(ChildTRF.position.x, ChildTRF.position.y);
            int id = info.mi_Id;
            sw.WriteLine(key + "= {}");

            sw.WriteLine(key + "['ID']" + " = " + i);
            sw.WriteLine(key + "['Class']" + " = " + info.m_minPower);
            sw.WriteLine(key + "['ActionID']" + " = " + info.m_maxPower);
            sw.WriteLine(key + "['GridX']" + " = " + info.m_maxPower);
            sw.WriteLine(key + "['GridY']" + " = " + info.m_maxPower);

        }
    }

    void ExportPathSetInfo(string tablename, Transform parent, ref StreamWriter sw)
    {
        int index = 1;
        string key = tablename;

        for (int i = 0; i < parent.childCount; ++i)
        {

            Transform ChildTRF = parent.GetChild(i);
            BornInfo info = ChildTRF.GetComponent<BornInfo>();
           
            key = tablename + "[" + (index++).ToString() + "]";
            Vector2 mapPos = m_csPathGrid.GetMapPosByWordPos(ChildTRF.position.x, ChildTRF.position.y);
            int id = info.mi_Id;
            sw.WriteLine(key + "= {}");


            sw.WriteLine(key + "['ID']" + " = " + i);
            sw.WriteLine(key + "['GridX']" + " = " + info.mi_ObjId);
            sw.WriteLine(key + "['GridY'] = " + info.m_nRebornTime);            
            sw.WriteLine(key + "['ActionID']" + " = " + 0);
            sw.WriteLine(key + "['RoundType']" + " = " + mapPos.x);
            sw.WriteLine(key + "['RoundMark']" + " = " + mapPos.y);
            int nRoundNum = 0;
            sw.WriteLine(key + "['RoundNum']  = " + nRoundNum);
            for(int k=0;k<nRoundNum;k++)
            {
                sw.WriteLine(key + "['Round_"+k+"']  = {}");
                sw.WriteLine(key + "['Round_" + k + "']['ActionID'] = "+0);
                sw.WriteLine(key + "['Round_" + k + "']['GridX'] = "+0);
                sw.WriteLine(key + "['Round_" + k + "']['GridY'] = "+0);
            }            

        }
    }

    //根据Transform删除GameObj
    void DeleteGameObj(Transform tran)
    {
        if (tran != null)
        {
            GameObject.DestroyImmediate(tran.gameObject);
        }
    }



}

