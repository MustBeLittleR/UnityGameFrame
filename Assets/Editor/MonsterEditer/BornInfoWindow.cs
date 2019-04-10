using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System;
using SceneEditor;



public class BornInfoWindow : EditorWindow
{ 
    enum enDirection
    {
        enInviled,
        enBottom,
        enBottomRight,
        enRight,
        enTopRight,
        enTop,
        enTopLeft,        
        enLeft,                      
        enBottomLeft,               
    }
    public static BornInfoWindow _Instance = null;

    bool mb_FixMonsterBorn = false;    //是否显示定点刷怪分组
    bool mb_MonsterBorn = false;       //是否显示刷怪点分组
    bool mb_ObjBorn = false;           //是否显示OBJ分组列表
    bool mb_TotalInfo = false;         //是否显示统计信息

    bool mb_MonsterTotalInfo = false;  //显示场景点统计信息
    bool mb_FixMonsterTotalInfo = false; //显示定点统计信息
    bool mb_OBjTotalInfo = false;  //显示场景点统计信息

    GameObject mo_ChooseObj = null;
    BornInfo mcs_Borninfo = null;

    private Vector3 mp_ScroPos = Vector3.zero;

    Vector3 mp_MonsterBornPos = Vector3.zero;   //怪物列表记录滑动位置
    Vector3 mp_FixBornPos = Vector3.zero;       //定点列表记录滑动位置
    Vector3 mp_ObjBornPos = Vector3.zero;       //OBJ列表记录滑动位置

    bool mb_FoldMonsterInfo = true;
    int mi_ChooseId = int.MaxValue;
    enDirection me_Direction = enDirection.enTop;
    int m_GraphDir = 0;
    // string[] m_saDirection = null;
    //string[] m_saPathState = null;
    // static GUISkin mgs_ButtonSkin = null;
    int m_groupId = 0;
    int m_minPower = 0; //最低强度
    int m_maxPower = 0; //最高强度
    int m_nRebornTime = 0;
    int m_nTrapSceneId = 0; //传送场景ID
    int m_nPosX = 0; //传送X
    int m_nPosY = 0; //传送Y
    int m_SumonNum; //范围刷怪的数量
    int m_nWidth; //刷怪范围
    int m_nHeight;//刷怪范围
    int m_nLiveTime;//存活时间？
    int m_nDelayBornTime; //延迟出生时间
    int m_nRoundType;
    int m_nRoundNum;
    int m_nMoveMode;
    int m_nMoveRadius; //移动半径
    public List<PatrolPointInfo> m_PatrolInfo; //巡逻点

    string[] m_saDirection = new string[]{
                "无方向",
                "下,180度",
                "右下,135度",
                "右,90度",
                "右上,45度",     
                "上,0度",
                "左上,315度",
                "左,270度",
                "左下,225度",                                               
                };

    string[] m_saPathState = new string[]{
                    "无状态",
                    "循环",
                    "来回",
                };
    string[] m_sRoundType = new string[] {
        "来回",
        "环行",
    };
    int[] m_RoundType = new int[]
    {
        0,
        1,
    };

    string[] m_sMoveMode = new string[] {
        "站立",
        "随机移动",
    };
    int[] m_MoveMode = new int[]
    {
        0,
        1,
    };

    static GUISkin mgs_ButtonSkin = ScriptableObject.CreateInstance(typeof(GUISkin)) as GUISkin;
        //mgs_ButtonSkin.button.normal.background = null;
        //mgs_ButtonSkin.button.normal.textColor = Color.white;

    static Dictionary<int, bool> m_DCBornDictArray = new Dictionary<int, bool>();     //出生点信息
    static Dictionary<int, bool> m_DCFixBornDictArray = new Dictionary<int, bool>();     //出生点信息

    #region 接口函数


    public void Init()
    {
        minSize = new Vector2(237, 0);
       
    }

    public void OnDestory()
    {
       // CloseWindow();
    }

    #endregion

    #region Unity函数

    [MenuItem("Window/场景编辑器/打开信息窗口")]
    public static void OpenWindow()
    {
        if (Application.isPlaying)
        {
            if (!SceneEditorWindow._windowInstance)
            {
                Debug.LogWarning("请先打开编辑窗口!");
                return;
            }
            if (!_Instance)
            {
                _Instance = EditorWindow.GetWindow<BornInfoWindow>("怪物信息窗口");
                _Instance.Init();
            }
            else
            {
                _Instance.Focus();
            }
        }
        else
        {
            Debug.LogWarning("请先运行游戏");
        }
    }

    [MenuItem("Window/场景编辑器/关闭信息窗口")]
    public static void CloseWindow()
    {
        if (_Instance != null)
        {
            _Instance.Close();
        }
    }

    //窗口刷新函数,Unity函数
    void OnInspectorUpdate()
    {
        
        //这里开启窗口的重绘，不然窗口信息不会刷新
        this.Repaint();
        
        
    }

    //GUI绘制函数, Unity函数
    void OnGUI()
    {
        if (!Application.isPlaying)
            CloseWindow();
        else
        {
            EditorGUILayout.BeginHorizontal();
            mp_ScroPos = EditorGUILayout.BeginScrollView(mp_ScroPos, false, false);
            EditorGUILayout.BeginVertical();

            mb_FoldMonsterInfo = EditorGUILayout.Foldout(mb_FoldMonsterInfo, "怪物信息");
            if (mb_FoldMonsterInfo)
            {
                SceneEditorWindow.StartSpace(1, 15);
                DrawBornInfo();
                SceneEditorWindow.EndSpace();
            }
            if (mb_MonsterBorn)
            {
                //DrawFixMonstBorninfo(ref mb_MonsterBorn, "范围刷怪点列表", InteractionTool.GetBornParent(), ref mp_MonsterBornPos, m_DCBornDictArray);
            }

            if (mb_FixMonsterBorn)
            {
               // DrawFixMonstBorninfo(ref mb_FixMonsterBorn, "定点刷怪列表", InteractionTool.GetFixBornParent(), ref mp_FixBornPos, m_DCFixBornDictArray);
            }

            if (mb_ObjBorn)
            {
              //  DrawFixMonstBorninfo(ref mb_ObjBorn, "OBJ列表", InteractionTool.GetObjParent(), ref mp_ObjBornPos, null);
            }
            

            DrawTotalInfo();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();
        }
    }

    //总更新函数,Unity函数
    void Update()
    {
        GameObject[] selectedObjs = Selection.gameObjects;
        if (selectedObjs.Length == 1)
        {
            GameObject obj = selectedObjs[0];
            SetInfo(obj);
        }

    }

    //设置信息
    public void SetInfo(GameObject obj)
    {
        mo_ChooseObj = null;
        if(obj == null)
        {
            return;
        }
           
        mcs_Borninfo = obj.GetComponent<BornInfo>();
        if(mcs_Borninfo == null)
        {
            return;
        }
        mo_ChooseObj = obj;
        me_Direction = GetDirectionWithAngel(mcs_Borninfo.GetDiretionAngel());
        mi_ChooseId = obj.GetInstanceID();
        if (me_Direction == enDirection.enInviled)
        {
            me_Direction = enDirection.enTop;
            mo_ChooseObj.transform.rotation = Quaternion.Euler(new Vector3(mo_ChooseObj.transform.rotation.x, 0, mo_ChooseObj.transform.rotation.z));
        }
    }


    #endregion

    //设置怪物该点怪物的选择开关是否被打开
    public void SetMonstBornInDict(int id, bool inValue)
    {
        if (m_DCBornDictArray.ContainsKey(id))
        {
            m_DCBornDictArray[id] = inValue;
        }
    }

    //加入怪物显示信息到列表
    public void AddMonstBornToDict(GameObject obj)
    {
        m_DCBornDictArray.Add(obj.GetInstanceID(), false);
    }

    //恢复GUI皮肤
    void RestorGuiSkin()
    {
        GUI.backgroundColor = new Color(1f, 1f, 1f, 1f);
        GUI.color = new Color(1f, 1f, 1f, 1f);
        GUI.skin.textField.fontSize = 0;
        GUI.skin.label.fontSize = 0;
        GUI.skin.box.fontSize = 0;
        EditorStyles.popup.fontSize = 0;
        GUI.skin.label.fontSize = 0;
    }


    //根据角度获取方向
    enDirection GetDirectionWithAngel(int Angel)
    {   
        enDirection direciton;
        switch (Angel)
        {
            case 0:
                {
                    direciton = enDirection.enInviled;
                    break;
                }
            case 1:
                {
                    direciton = enDirection.enBottom;
                    break;
                }
            case 2:
                {
                    direciton = enDirection.enBottomRight;
                    break;
                }
            case 3:
                {
                    direciton = enDirection.enRight;
                    break;
                }
            case 4:
                {
                    direciton = enDirection.enTopRight;
                    break;
                }
            case 5:
                {
                    direciton = enDirection.enTop;
                    break;
                }
            case 6:
                {
                    direciton = enDirection.enTopLeft;
                    break;
                }
            case 7:
                {
                    direciton = enDirection.enLeft;
                    break;
                }
            case 8:
                {
                    direciton = enDirection.enBottomLeft;
                    break;
                }
            default:
                Debug.Log(Angel);
                direciton = enDirection.enInviled;
                break;
        }
        return direciton;
    }

    //根据方向获取角度
    int GetAngelWithDireciton(enDirection direction)
    {
        int angel = 0;
        switch (direction)
        {
            case enDirection.enInviled:
                angel = 0;
                break;
            case enDirection.enBottom:
                angel = 1;
                break;
            case enDirection.enBottomRight:
                angel = 2;
                break;
            case enDirection.enRight:
                angel = 3;
                break;
            case enDirection.enTopRight:
                angel = 4;
                break;
            case enDirection.enTop:
                angel = 5;
                break;
            case enDirection.enTopLeft:
                angel = 6;
                break;
            case enDirection.enLeft:
                angel = 7;
                break;
            case enDirection.enBottomLeft:
                angel = 8;
                break;  
            default:
                Debug.Log("错误");
                break;
        }
        return angel;
    }
    

    void DrawBornInfo()
    {
        if (mo_ChooseObj == null)
            return;
        GUI.backgroundColor = Color.red;
        GUI.color = Color.yellow;
        GUI.skin.textField.fontSize = 15;
        GUI.skin.label.fontSize = 12;
        GUI.skin.box.fontSize = 15;
        EditorStyles.popup.fontSize = 12;
        
       
        EditorGUILayout.BeginVertical();
        DrawBoxWithText("类型", GetTypeNameByObjectType((CreateObjType)mcs_Borninfo.m_eObjType));
        if (name.Length > 0)
        {
            DrawBoxWithText("名字", name);
        }
        
        
        DrawBoxWithText("ID", mcs_Borninfo.mi_ObjId.ToString());               

        if(mcs_Borninfo.m_eObjType != 1)//不是功能NPC，就一定有强度
        {
            m_minPower = DrawBoxWithInt("怪物限定最低强度:", mcs_Borninfo.m_minPower);
            mcs_Borninfo.m_minPower = m_minPower;

            m_maxPower = DrawBoxWithInt("怪物限定最低强度:", mcs_Borninfo.m_maxPower);
            mcs_Borninfo.m_maxPower = m_maxPower;
            m_groupId = DrawBoxWithInt("怪物组id:", mcs_Borninfo.mi_Group);
            mcs_Borninfo.mi_Group = m_groupId;
        }
        

        m_nRebornTime = DrawBoxWithInt("OBJ复活时间:", mcs_Borninfo.m_nRebornTime);
        mcs_Borninfo.m_nRebornTime = m_nRebornTime;

        Transform tran = mo_ChooseObj.transform;        
        Vector2 pos = SceneEditorWindow._windowInstance.GetMapPosWith3DPos(tran.position.x, tran.position.y);
        DrawBoxWithInt("X坐标", (int)pos.x);
        DrawBoxWithInt("Y坐标", (int)pos.y);

        //tran.localPosition = new Vector3(posx, DrawBoxWithInt("Y坐标", posy) + 0.5f, tran.localPosition.z);

        int angel = GetAngelWithDireciton((enDirection)DrawBoxWithPop("朝向", (int)GetDirectionWithAngel(mcs_Borninfo.rotation), ref m_saDirection));
        mcs_Borninfo.rotation = angel;

        if (mcs_Borninfo.m_eObjType == 1)
        {
            int graphDir = DrawBoxWithInt("图形朝向:", mcs_Borninfo.graphDir);
            mcs_Borninfo.graphDir = graphDir;
        }
        

        if (mcs_Borninfo.me_Type == CreateMode.enRegionObj)
        {
            m_nWidth = DrawBoxWithInt("随机范围Width:", mcs_Borninfo.m_nWidth);
            mcs_Borninfo.m_nWidth = m_nWidth;

            m_nHeight = DrawBoxWithInt("随机范围Height:", mcs_Borninfo.m_nHeight);
            mcs_Borninfo.m_nHeight = m_nHeight;

            m_SumonNum = DrawBoxWithInt("随机怪组数:", mcs_Borninfo.m_RegionNpcInfo.Count);
            //mcs_Borninfo.m_SumonNum = m_SumonNum;
            mcs_Borninfo.ReSizeRegionObj(m_SumonNum);
            
            for(int regionindex = 0; regionindex<mcs_Borninfo.m_RegionNpcInfo.Count; regionindex++)
            {
                int showindex = regionindex + 1;
                RegionObjInfo regionobj = mcs_Borninfo.m_RegionNpcInfo[regionindex];
                int nID = DrawBoxWithInt("随机怪物"+ showindex + "ID:", regionobj.ObjId);
                int nNum = DrawBoxWithInt("随机怪物"+ showindex + "数量:", regionobj.Nums);
                int Time = DrawBoxWithInt("随机怪物"+ showindex + "时间:", regionobj.Time);
                int Dir = DrawBoxWithInt("随机怪物"+ showindex + "朝向:", regionobj.Dir);
                int nRate = DrawBoxWithInt("随机怪物" + showindex + "几率:", regionobj.Rate);
                regionobj.ObjId = nID;
                regionobj.Nums = nNum;
                regionobj.Time = Time;
                regionobj.Dir = Dir;
                regionobj.Rate = nRate;
            }

           
        }
        else
        {
            if(mcs_Borninfo.m_eObjType == 0)
            {
                m_nRebornTime = DrawBoxWithInt("重生时间", mcs_Borninfo.m_nRebornTime); 
                mcs_Borninfo.m_nRebornTime = m_nRebornTime;
                m_nLiveTime = DrawBoxWithInt("生存时间", mcs_Borninfo.m_nLiveTime);
                mcs_Borninfo.m_nLiveTime = m_nLiveTime;
                int nRate = DrawBoxWithInt("几率:", mcs_Borninfo.m_nRate);
                mcs_Borninfo.m_nRate = nRate;
                m_nRoundType = DrawBoxWithIntPop("巡逻类型", mcs_Borninfo.m_RoundType, m_RoundType, m_sRoundType);
                mcs_Borninfo.m_RoundType = m_nRoundType;

                m_nRoundNum = DrawBoxWithInt("巡逻点数量", mcs_Borninfo.m_PatrolInfo.Count);
                mcs_Borninfo.ReSizePatrolPoint(m_nRoundNum);
                //巡逻点
                for (int i = 0; i < mcs_Borninfo.m_PatrolInfo.Count; i++)
                {
                    PatrolPointInfo point = mcs_Borninfo.m_PatrolInfo[i];
                    int WaitTime = DrawBoxWithInt("     等待时间",point.m_WaitTime);
                    int ActionId = DrawBoxWithInt("     行为Id", point.m_ActionId);
                    int gridx = DrawBoxWithInt("        X坐标", point.m_GridX);
                    int gridy = DrawBoxWithInt("        Y坐标", point.m_GridY);
                    point.m_WaitTime = WaitTime;
                    point.m_ActionId = ActionId;
                    //point.m_GridX = gridx;
                    //point.m_GridY = gridy;
                }
            }
            if(mcs_Borninfo.m_eObjType == 1)
            {
                
                m_nMoveMode = DrawBoxWithIntPop("移动方式", mcs_Borninfo.m_nMoveMode, m_MoveMode, m_sMoveMode);                
                mcs_Borninfo.m_nMoveMode = m_nMoveMode;

                m_nMoveRadius = DrawBoxWithInt("移动半径:", mcs_Borninfo.m_nMoveRadius);
                mcs_Borninfo.m_nMoveRadius = m_nMoveRadius;

				m_nRoundType = DrawBoxWithIntPop("巡逻类型", mcs_Borninfo.m_RoundType, m_RoundType, m_sRoundType);
				mcs_Borninfo.m_RoundType = m_nRoundType;

				m_nRoundNum = DrawBoxWithInt("巡逻点数量", mcs_Borninfo.m_PatrolInfo.Count);
				mcs_Borninfo.ReSizePatrolPoint(m_nRoundNum);
				//巡逻点
				for (int i = 0; i < mcs_Borninfo.m_PatrolInfo.Count; i++)
				{
					PatrolPointInfo point = mcs_Borninfo.m_PatrolInfo[i];
					int WaitTime = DrawBoxWithInt("     等待时间",point.m_WaitTime);
					int ActionId = DrawBoxWithInt("     行为Id", point.m_ActionId);
					int gridx = DrawBoxWithInt("        X坐标", point.m_GridX);
					int gridy = DrawBoxWithInt("        Y坐标", point.m_GridY);
					point.m_WaitTime = WaitTime;
					point.m_ActionId = ActionId;
					point.m_GridX = gridx;
					point.m_GridY = gridy;
				}
            }

            if(mcs_Borninfo.m_eObjType == 2)
            {
                int nRate = DrawBoxWithInt("几率:", mcs_Borninfo.m_nRate);
                mcs_Borninfo.m_nRate = nRate;
                m_nDelayBornTime = DrawBoxWithInt("延迟出生时间", mcs_Borninfo.m_nDelayBornTime);
                mcs_Borninfo.m_nDelayBornTime = m_nDelayBornTime;

                m_nTrapSceneId = DrawBoxWithInt("OBJ传送场景id:", mcs_Borninfo.m_nTrapSceneId);
                mcs_Borninfo.m_nTrapSceneId = m_nTrapSceneId;
                m_nPosX = DrawBoxWithInt("OBJ传送位置X：", mcs_Borninfo.m_nPosX);
                mcs_Borninfo.m_nPosX = m_nPosX;
                m_nPosY = DrawBoxWithInt("OBJ传送位置Y：", mcs_Borninfo.m_nPosY);
                mcs_Borninfo.m_nPosY = m_nPosY;
            }



            

                     
        }
               


        EditorGUILayout.EndVertical();
        RestorGuiSkin();
    }


    //绘制BOX
    void DrowBox(string title)
    {
        EditorGUILayout.BeginHorizontal();
        GUIContent titleContent = new GUIContent(title);
                
        GUILayout.BeginHorizontal();
        GUILayout.Label(titleContent, GUILayout.Width(100), GUILayout.Height(22));
        GUILayout.EndHorizontal();
    }

    //绘制BOX 文本框
    void DrawBoxWithText(string title,  string text)
    {
        DrowBox(title);
        GUILayout.BeginVertical();
        GUILayout.Space(5);
        GUILayout.Label(text);
        GUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();        
    }

    string DrawBoxWithTextField(string title, string content)
    {
        DrowBox(title);
        GUILayout.BeginVertical();
        GUILayout.Space(5);
        string sContent = GUILayout.TextField(content);
        GUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        return sContent;
    }
    //绘制BOX Int框
    int DrawBoxWithInt(string title, int number)
    {

        DrowBox(title);
        GUILayout.BeginVertical();
        GUILayout.Space(5);
        int nWidth = 110;
        
        int getnumber = EditorGUILayout.IntField(number, GUILayout.Width(nWidth), GUILayout.Height(18));
        GUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        return getnumber;

    }

    //绘制BOX  弹出框
    int DrawBoxWithPop(string title, int direction, ref string[] showstring)
    {
        DrowBox(title);
        GUILayout.BeginVertical();
        GUILayout.Space(5);
        enDirection getdirection = (enDirection)EditorGUILayout.Popup(direction, showstring, GUILayout.Width(100));
        GUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        return (int)getdirection;

    }

    int DrawBoxWithIntPop(string title , int value, int[] values, string[] showValues)
    {
       // DrowBox(title);
        GUILayout.BeginVertical();
        GUILayout.Space(5);
        value = EditorGUILayout.IntPopup( title, value, showValues, values);
        GUILayout.EndVertical();
        //EditorGUILayout.EndHorizontal();
        return value;
    }

    //绘制BOX  开关
    void DrawBoxWithToggle(string title, ref bool can)
    {
        DrowBox(title);
        GUILayout.BeginVertical();
        can = EditorGUILayout.Toggle(can);
        GUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

    }

    //绘制BOX  标题
    void DrawBoxForTitle(string title, int With, float space)
    {
        GUILayout.Space(space);
        GUILayout.Box(title, GUILayout.Width(With));
    }
   
    string GetTypeNameByObjectType(CreateObjType eType)
    {
        string sName = "";
        switch(eType)
        {
            case CreateObjType.eMonster:
                sName = "普通怪物";
                break;
            case CreateObjType.eFunNpc:
                sName = "功能Npc";
                break;
            case CreateObjType.eObj:
                sName = "OBJ";
                break;
            default:
                break;
        }
        return sName;
    }

    void DrawName(string name, int width, float space)
    {
        DrawBoxForTitle(name, width, space);
    }

    void DrawId(string id, int width, float space)
    {
        DrawBoxForTitle(id, width, space);
    }

    void DrawLv(string Lv, int width, float space)
    {
        DrawBoxForTitle(Lv, width, space);
    }

    void DrawInfo(string name, int namewith, float space, string id, int idwith, float idspace, string lv, int lvwith, float lvspace)
    {
        EditorGUILayout.BeginHorizontal();
        if (name != "")
            DrawName(name, namewith, space);
        if (id != "")
            DrawId(id, idwith, idspace);
        if (lv != "")
            DrawLv(lv, lvwith, lvspace);
        EditorGUILayout.EndHorizontal();
    }

    string getSpaceWithNameLenght(int lenghth)
    {
        string tempstring;
        switch (lenghth)
        {
            case 0:
                {
                    tempstring = "                      ";
                    break;
                }
            case 1:
                {
                    tempstring = "                    ";
                    break;
                }
            case 2:
                {
                    tempstring = "                 ";
                    break;
                }
            case 3:
                {
                    tempstring = "              ";
                    break;
                }
            case 4:
                {
                    tempstring = "           ";
                    break;
                }
            case 5:
                {
                    tempstring = "        ";
                    break;
                }
            case 6:
                {
                    tempstring = "     ";
                    break;
                }
            case 7:
                {
                    tempstring = "      ";
                    break;
                }
            default:
                tempstring = "";
                break;
        }
        return tempstring;
    }

    string getSpaceWithNumberLenght(int lenghth)
    {
        string tempstring;
        switch (lenghth)
        {
            case 0:
                {
                    tempstring = "             ";
                    break;
                }
            case 1:
                {
                    tempstring = "           ";
                    break;
                }
            case 2:
                {
                    tempstring = "        ";
                    break;
                }
            case 3:
                {
                    tempstring = "      ";
                    break;
                }
            case 4:
                {
                    tempstring = "   ";
                    break;
                }
            default:
                tempstring = "";
                break;
        }
        return tempstring;
    }


    bool DrawShowInfo(Transform childtran, bool foldout, ref Dictionary<int, bool> bornDic, int childCount, ref BornInfo info, int index)
    {
        int id = childtran.gameObject.GetInstanceID();

        string ShowString = GetShowInfo(childtran, ref info, index);
        GUILayout.BeginHorizontal();
        int offset = 3;
        if (childCount != 0)
        {
            offset = index * 3 + 3;
        }
        if (bornDic != null)
        {
            bornDic.TryGetValue(id, out foldout);
            foldout = EditorGUI.Foldout(new Rect(2, 17f * index + childCount * 18f + offset, 20, 20), foldout, GUIContent.none);
        }
        else
        {
            GUILayout.Space(5);
        }

        GUILayout.Space(20);
        GUILayout.BeginVertical();
        GUILayout.Space(4);

        if (mi_ChooseId != int.MaxValue && mi_ChooseId == id)
        {
            mgs_ButtonSkin.button.normal.textColor = Color.red;
            if (GUILayout.Button(ShowString, mgs_ButtonSkin.button))
            {
                ChangeChooseObj(id);
            }
            mgs_ButtonSkin.button.normal.textColor = Color.white;
        }
        else
        {
            if (GUILayout.Button(ShowString, mgs_ButtonSkin.button))
            {
                ChangeChooseObj(id);
            }
        }
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        SetMonstBornInDict(id, foldout);
        return foldout;
    }

    string GetShowInfo(Transform childtran, ref BornInfo info, int index)
    {
        string returnstring = "";

        //if (childtran.parent == InteractionTool.GetFixBornParent())
        //{
        //    string tempstring = childtran.name + getSpaceWithNameLenght(childtran.name.Length);
        //    string numberstring = info.mi_Lv + getSpaceWithNumberLenght(info.mi_Lv.Length);
        //    string numberstring2 = info.mi_Id.ToString() + getSpaceWithNumberLenght(info.mi_Id.ToString().Length);
        //    returnstring = tempstring + numberstring + numberstring2;
        //}
        //else if (childtran.parent == InteractionTool.GetBornParent())
        //{
        //    string tempstring = "场景刷怪点" + getSpaceWithNameLenght("场景刷怪点".Length);
        //    string numberstring = (info.mi_Id).ToString() + getSpaceWithNumberLenght((info.mi_Id).ToString().Length);
        //    returnstring = tempstring + "     " + numberstring;
        //}
        //else if (childtran.parent == InteractionTool.GetObjParent())
        //{
        //    string tempstring = childtran.name + getSpaceWithNumberLenght(childtran.name.Length);
        //    string numberstring2 = info.mi_Id.ToString() + getSpaceWithNumberLenght(info.mi_Id.ToString().Length);
        //    returnstring = tempstring + "     " + numberstring2;

        //}
        return returnstring;
    }

    void DrawWayPoint(ref int childCount, LinkedList<Vector2> WayPointarray)
    {
        SceneEditorWindow.StartSpace(2, 15);
        childCount = childCount + WayPointarray.Count;
        int index = 1;
        foreach (var item in WayPointarray)
        {
            GUILayout.Label("路径点: " + (index++).ToString() + "  X: " + item.x.ToString() + "  Y: " + item.y.ToString());
        }
        SceneEditorWindow.EndSpace();
    }

    int GetScroviewHeightWithCount(int count)
    {
        int height = (int)(count * 17);
        return (height > 240) ? 240 : height;
    }

    void ChangeChooseObj(int id)
    {
        mi_ChooseId = id;
        Selection.activeInstanceID = id;
        Vector3 pos = Selection.activeTransform.position;
        SceneView.lastActiveSceneView.pivot = pos;
        SceneView.lastActiveSceneView.Repaint();
    }

    void DrawTotalInfo()
    {
        mb_TotalInfo = EditorGUILayout.Foldout(mb_TotalInfo, "统计信息");
        if (mb_TotalInfo)
        {
            SceneEditorWindow.StartSpace(1, 15);
            mb_MonsterTotalInfo = EditorGUILayout.Foldout(mb_MonsterTotalInfo, "场景点统计信息");
            if (mb_MonsterTotalInfo)
            {
                SceneEditorWindow.StartSpace(1, 20);
                GUILayout.Space(2);
                //DrawTotalMonsterBorn(InteractionTool.GetBornParent());
                SceneEditorWindow.EndSpace();
            }

            mb_FixMonsterTotalInfo = EditorGUILayout.Foldout(mb_FixMonsterTotalInfo, "定点统计信息");
            if (mb_FixMonsterTotalInfo)
            {
                SceneEditorWindow.StartSpace(1, 15);
                EditorGUILayout.BeginHorizontal();
                DrawId("I  D", 50, 0);
                DrawName("名  字", 90, -5);
                DrawLv("数  量", 55, -5);
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(2);
                DrawTotalBorn(InteractionTool.GetDCBornTotalNumber());
                SceneEditorWindow.EndSpace();
            }


            mb_OBjTotalInfo = EditorGUILayout.Foldout(mb_OBjTotalInfo, "OBJ统计信息");
            if (mb_OBjTotalInfo)
            {
                SceneEditorWindow.StartSpace(1, 15);
                EditorGUILayout.BeginHorizontal();
                DrawId("I  D", 50, 0);
                DrawName("名  字", 90, -5);
                DrawLv("数  量", 55, -5);
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(2);
                DrawTotalBorn(InteractionTool.GetDCObjTotalNumber());
                SceneEditorWindow.EndSpace();
            }

            GUILayout.Label("场景所有对象总数:" + " " + (InteractionTool.GetRegionParent().childCount + InteractionTool.GetPointParent().childCount).ToString());
            SceneEditorWindow.EndSpace();
        }
    }

    void DrawTotalBorn(Dictionary<int, MonsterInfo> fixmonsterdc)
    {
        foreach (var item in fixmonsterdc)
        {
            string tempstring = item.Key.ToString() + getSpaceWithNumberLenght(item.Key.ToString().Length);
            string numberstring = item.Value.name + getSpaceWithNameLenght(item.Value.name.Length);
            string numberstring2 = item.Value.TotalNumber.ToString() + getSpaceWithNumberLenght(item.Value.TotalNumber.ToString().Length);
            GUILayout.Space(2);
            GUILayout.Label(tempstring + numberstring + numberstring2);
        }
        GUILayout.Label("场景定点刷怪点" + "\t" + "总数: " + InteractionTool.GetPointParent().childCount.ToString());
    }

    void DrawTotalMonsterBorn(Transform tran)
    {
        GUILayout.Label("场景刷怪点" + "\t" + "总数: " + tran.childCount.ToString());
    }
}
