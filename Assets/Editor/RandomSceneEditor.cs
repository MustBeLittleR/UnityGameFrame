using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Reflection;
using System.IO;
using System;
using LuaInterface;
public class RandomSceneEditor {
    public static string ms_DesignerPath = Application.dataPath + "/../../Designer/";
    public static GameObject m_oRootNode = null;
    public static LuaState m_oLuamgr = null;
    public static LuaFunction m_fGetBlockInfoFunc = null;

    public static void Init()
    {
        m_oLuamgr = new LuaState();
        m_oLuamgr.Start();
        string filelua = File.ReadAllText(Application.dataPath + "/Z_Temp/Obstacle/LuaTool.lua");
        m_oLuamgr.DoString(filelua);
        m_fGetBlockInfoFunc = m_oLuamgr.GetFunction("GetBlockInfo");
        
    }

    [MenuItem("WCTool/ExportRandomSceneInfo")]
    public static void BeginExport()
    {
        Init();
        GameObject[] selectedObjs = Selection.gameObjects;
        if (selectedObjs.Length == 0)
        {
            return;
        }
        m_oRootNode = selectedObjs[0];
        if(m_oRootNode.name != "scenenode")
        {
            m_oRootNode = null;
            return;
        }

        ExportBlockInfo();
    }

    public static void ExportBlockInfo()
    {
        string path = ms_DesignerPath + "Config/Map/map_randomscene/BlockInfo.txt";


        try {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            
            using (FileStream file = new FileStream(path, FileMode.CreateNew))
            {
                
                StreamWriter sw = new StreamWriter(file, new System.Text.UTF8Encoding(false));                
                int nChildCount = m_oRootNode.transform.childCount;
                string sBlockInfo = "";
                for (int i = 0; i < nChildCount; i++)
                {
                    Transform child = m_oRootNode.transform.GetChild(i);
                    string sGameObjectName = child.name;
                    if (sGameObjectName.Contains("("))
                    {
                        sGameObjectName = child.name.Split('(')[0].TrimEnd();
                    }                    
                    object[] blockinfo = m_fGetBlockInfoFunc.LazyCall(sGameObjectName);
                    if(blockinfo.Length == 0)
                    {
                        Debug.LogError("配置错误，没有找到block配置：" + sGameObjectName);
                    }
                    int nBlockId = int.Parse(blockinfo[0].ToString());
                    int nWidth = int.Parse(blockinfo[1].ToString());
                    int nHeight = int.Parse(blockinfo[2].ToString());
                    int nPixelWidth = int.Parse(blockinfo[3].ToString());
                    int nPixelHeight = int.Parse(blockinfo[4].ToString());
                    if (nBlockId == 0)
                    {
                        Debug.LogError("配置错误，没有找到block配置2：" + sGameObjectName);
                    }
                    float nPosX = child.localPosition.x;
                    float nPosY = child.localPosition.y;

                    float nLogicPosX = nPosX * 2 - nPixelWidth *0.01f;
                    float nLogicPosY = nPosY * 2 - nPixelHeight*0.01f;
                    
                    float nPosZ = -child.localPosition.z;
                    if(sBlockInfo.Length == 0)
                    {
                        sBlockInfo =  nBlockId.ToString() + "," + nLogicPosX.ToString() + "," + nLogicPosY.ToString() + "," + nPosZ.ToString();
                    }
                    else
                    {
                        sBlockInfo = sBlockInfo +";" +  nBlockId.ToString() + "," + nLogicPosX.ToString("0.00") + "," + nLogicPosY.ToString("0.00") + "," + nPosZ.ToString();
                    }
                    
                }

                sw.WriteLine(sBlockInfo);
                sw.Close();
                file.Close();
            }
        }
        catch (IOException ex)
        {
            Debug.LogError("RandomScene" + "阻挡导出失败: " + ex.ToString());
            return;
        }
    }

    
}
