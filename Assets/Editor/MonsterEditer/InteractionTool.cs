using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using SceneEditor;
using System.Xml.Linq;
public static class InteractionTool
{
    static List<ICommonInterface> ObserverList;
    public static string m_sObstacleLastName = "_layer0";
    public static string m_sMonsterBornLastName = "BornList";
    public static string m_sMapFunNpcLastName = "FunNpc";
    public static string m_sMapNpcLastName = "Npc";
    public static string m_sNavigateLastName = "Navigate";
	public static string ms_DesignerPath = Application.dataPath+ "/../../Designer/";// GameConfigMgr.ms_sLuaDesignerDir;                  // 配置的策划目录
    public static readonly string ms_sLuaEtcDir = Application.dataPath + "/../../Etc/";    // Etc目录
    public static string ms_sSaveString = "";
    public static string ms_SceneName = "";

    public static int m_iChooseMonterId;
    public static int m_iChooseFunNpcId;
    public static int m_iChooseObjId;

    static InteractionTool()
    {
        string Scenename = EditorApplication.currentScene.Replace(".unity", "");
        ms_SceneName = Scenename.Substring(Scenename.LastIndexOf("/") + 1);
        ms_sSaveString = ms_DesignerPath + "Config/Map/" + ms_SceneName + ""; //保存目录
        ObserverList = new List<ICommonInterface>();

    }

    //初始化监视者
    public static void InitObserver()
    {
        foreach (var Observer in ObserverList)
        {
            Observer.Init();
        }
    }

    //删除监视者
    public static void DestoryObserver()
    {
        foreach (var Observer in ObserverList)
        {
            Observer.OnDestory();
        }
    }

    //添加监视者
    public static void AddObserver(ICommonInterface Observer)
    {
        if(ObserverList.Contains(Observer))
        {
            return;
        }
        ObserverList.Add(Observer);
    }

    //发送出生点被创建的消息给所有监视者
    public static void SendGameObjCreateMessage(GameObject obj, int id, CreateMode mode)
    {
        foreach (var Observer in ObserverList)
        {
            Observer.OnCreateGameObj(obj, id, mode);
        }
    }

    //发送出生点信息改变的消息给所有出生者
    public static void SendFunNpcGameObjChange(int id, Dictionary<int, FunNpcInfo> ChooseDict)
    {

        if (m_iChooseFunNpcId > 0)
        {
            ChooseDict[m_iChooseFunNpcId].SetIsChoose(false);
        }

        m_iChooseFunNpcId = id;
        
        ChooseDict[id].SetIsChoose(true);
    }

    public static void SendNpcGameObjChange(int id, Dictionary<int, NpcInfo> ChooseDic)
    {
        if (m_iChooseMonterId > 0)
        {
            ChooseDic[m_iChooseMonterId].SetIsChoose(false);
        }

        m_iChooseMonterId = id;

        ChooseDic[id].SetIsChoose(true);
    }

    public static void SendObjGameObjChange(int id, Dictionary<int, ObjInfo> ChooseDic)
    {
        if (m_iChooseObjId > 0)
        {
            ChooseDic[m_iChooseObjId].SetIsChoose(false);
        }

        m_iChooseObjId = id;

        ChooseDic[id].SetIsChoose(true);
    }


    //检查路径
    static void CheckDirectory()
    {
        if (!Directory.Exists(ms_sSaveString))
            Directory.CreateDirectory(ms_sSaveString);
    }

    //导出阻挡
    static public void Exportobstacle()
    {
        CheckDirectory();
        string path = ms_sSaveString + "/" + ms_SceneName+ m_sObstacleLastName+".dat";
        string clientpath = ms_sSaveString + "/" + ms_SceneName  + ".lua";
        string blkPath = ms_sSaveString +"/"+ ms_SceneName+".blk";
        bool right = false;
        //while (!right)
        //{
        //    path = EditorUtility.SaveFilePanel("导出场景阻挡文件", ms_sSaveString, ms_SceneName + m_sObstacleLastName, "dat");
        //    if (path == "")
        //        return;
        //    if (path.IndexOf(m_sObstacleLastName) != -1)
        //    {
        //        break;
        //    }
        //    SceneEditorWindow.DialogReturn("文件命名错误", "阻挡文件应该包含  (" + m_sObstacleLastName + " ) ,请重新进行命名操作!", true);

        //}
        EditerDataClass._Instance.ExportMapInfo(path, MapObjectType.eObstcale);
        EditerDataClass._Instance.ExportClientMapInfo(clientpath, MapObjectType.eObstcale);
        //导出blk
        File.Delete(blkPath);
        try
        {
            XDocument xDoc = new XDocument();
            XElement xRoot = new XElement("GRIDDATA");
            xDoc.Add(xRoot);
            XAttribute xVersion = new XAttribute("version", "1");
            XAttribute xlayercount = new XAttribute("layercount", "1");
            xRoot.Add(xVersion);
            xRoot.Add(xlayercount);

            XElement xLayers = new XElement("LAYERS");

            XElement layer0 = new XElement("LAYER0");
            string datafile = ms_SceneName + m_sObstacleLastName + ".dat";
            XAttribute xdatafile = new XAttribute("datafile", datafile);
            layer0.Add(xdatafile);
            xLayers.Add(layer0);

            xRoot.Add(xLayers);

            xDoc.Save(blkPath);
        }
        catch (IOException ex)
        {
            Debug.LogError("阻挡导出失败: " + ex.ToString());
            return;
        }
        Debug.Log(ms_SceneName + " 阻挡导出成功");
    }
    



    //导入Npc布怪信息
    static public void ImportNpcMapInfo()
    {
        CheckDirectory();
        string path = ms_sSaveString + "/" + ms_SceneName + m_sMapFunNpcLastName + "2Client.lua";
        //检查文件是否存在, 不存在直接断掉
        if (!File.Exists(path))
        {
            
            return;
        }

        EditerDataClass._Instance.ImportNpcMapInfo(path);
    }

    //导出FunNpc信息
    static public void ExportFunNpcMapInfo()
    {
        CheckDirectory();
        string clientpath = ms_sSaveString + "/" + ms_SceneName + m_sMapFunNpcLastName + "2Client.lua";
        string serverpath = ms_sSaveString + "/" + ms_SceneName + m_sMapFunNpcLastName + ".lua";


        //while (!right)
        //{
        //    path = EditorUtility.SaveFilePanel("导出场景阻挡文件", ms_sSaveString, ms_SceneName + m_sObstacleLastName, "dat");
        //    if (path == "")
        //        return;
        //    if (path.IndexOf(m_sObstacleLastName) != -1)
        //    {
        //        break;
        //    }
        //    SceneEditorWindow.DialogReturn("文件命名错误", "阻挡文件应该包含  (" + m_sObstacleLastName + " ) ,请重新进行命名操作!", true);

        //}
        EditerDataClass._Instance.ExportMapFunNpcInfo(serverpath);
        EditerDataClass._Instance.ExportMapFunNpcInfo2Client(clientpath);
    }    
    

    


    //导入阻挡
    static public void Importobstacle(bool idDefault = false)
    {
        string path = "";
        CheckDirectory();
        if (idDefault == true)
        {
            path = ms_sSaveString + "/" + ms_SceneName + m_sObstacleLastName + ".dat";
        }
        else
        {
            path = EditorUtility.OpenFilePanel("导入场景阻挡文件", ms_sSaveString, "dat");
            if (path == "")
                return;
        }
        if (path.IndexOf(m_sObstacleLastName) == -1)
        {
            SceneEditorWindow.DialogReturn("文件选择错误", "文件错误,阻挡文件应该包含  (" + m_sObstacleLastName + ".dat ) ,请重新进行导入操作!", true);
            return;
        }

        //检查文件是否存在, 不存在直接断掉
        if (!File.Exists(path))
        {
            return;
        }
        EditerDataClass._Instance.Importobstacle(path);

    }

    
    //获取怪物信息表

    static public Dictionary<int , FunNpcInfo>GetFunNpcList()
    {
        return EditerDataClass._Instance.GetFunNpcInfo();  
    }

    static public Dictionary<int, NpcInfo>GetNpcList()
    {
        return EditerDataClass._Instance.GetNpcInfo();
    }

    //获取OBJ信息表
    static public Dictionary<int, ObjInfo> GetObjList()
    {
        return EditerDataClass._Instance.GetObjInfo();
    }

    //获取怪物ID列表
    static public List<int> GetFunNpcIdList()
    {
        return new List<int>(EditerDataClass._Instance.GetFunNpcInfo().Keys);
    }

    //获取ObjId列表
    static public List<int> GetObjIdList()
    {
        return new List<int>();
    }

    //获取当前怪物列表存储数量信息
    public static Dictionary<int, MonsterInfo> GetDCBornTotalNumber()
    {
        return new Dictionary<int, MonsterInfo>();
    }

    //获取当前怪物列表存储数量信息
    public static Dictionary<int, MonsterInfo> GetDCObjTotalNumber()
    {
        return new Dictionary<int, MonsterInfo>();
    }



    //获取英雄出生点
    public static Transform GetRegionParent()
    {
        return SceneEditorWindow._windowInstance.GetRegionParent();
    }

    //获取场景刷怪点父节点
    public static Transform GetPointParent()
    {
        return SceneEditorWindow._windowInstance.GetPointParent();
    }

    //获取地图脚本
    public static PathGridComponent GetPathGridComponent()
    {
        return SceneEditorWindow._windowInstance.m_csPathGrid;
    }

    //获取当前模式
    public static CreateMode GetNowCreateMode()
    {
        return SceneEditorWindow._windowInstance.m_enCreateMode;
    }

    //设置当前模式
    public static void SetCreateMode( CreateMode mode)
    {
        SceneEditorWindow._windowInstance.m_enCreateMode = mode;
    }

    //获取当前高度
    public static float GetMaxObstaclepos()
    {
        return SceneEditorWindow._windowInstance.m_fMaxObstaclepos;
    }

    //设置是否更新
    public static void SetCanDraw(bool Can)
    {
        SceneEditorWindow._windowInstance.m_bCanDraw = Can;
    }

   
    //创建范围刷怪点
    public static GameObject AddReginNpc(float posx, float posy, float posz, int objId, CreateObjType eType,  int rotation)
    {
        return SceneEditorWindow._windowInstance.AddReginNpc(posx, posy, posz, objId, eType, rotation);
    }

    //创建范围刷怪点
    public static GameObject AddPointNpc(float posx, float posy, float posz, int objId, CreateObjType eType, int rotation)
    {
        return SceneEditorWindow._windowInstance.AddPointNpc(posx, posy, posz, objId, eType, rotation);
    }

    public static GameObject AddNpcPatrolPoint(float posx, float  posy,  float posz, GameObject obj)
    {
        return SceneEditorWindow._windowInstance.AddNpcPatrolPoint(posx, posy, posz, obj);
    }

    //检测父节点
    public static void CheekParent()
    {
        SceneEditorWindow._windowInstance.CheekParent();
    }

    //清楚阻挡信息
    public static void ClearObscaleInfo()
    {
        SceneEditorWindow._windowInstance.m_csPathGrid.ClearAllObscaleInfo();
    }

    //重新初始化数组
    public static void InitArray()
    {
        SceneEditorWindow._windowInstance.m_csPathGrid.InitArray();
    }

    //在该位置添加阻挡
    public static void AddPosToObstacle(float posx, float posy, float posz)
    {
        SceneEditorWindow._windowInstance.AddPosToObstacle(posx, posy, posz);
    }
}
