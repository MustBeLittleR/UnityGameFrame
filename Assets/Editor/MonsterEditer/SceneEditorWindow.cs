using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System;
using SceneEditor;


public class SceneEditorWindow : EditorWindow
{
    public static SceneEditorWindow _windowInstance;

    #region Menbers
    bool m_bShowRay = true;            //射线开关
    bool m_bShowChoosepng = true;      //选择生成点开关
    bool m_bShowMapInfo = true;        //是否显示地图信息开关
    bool m_bCanClickOtherObj = true;   //能否点击其他对象开关
    bool m_bShowFeature = true;        //功能按钮开关
    bool m_bShowFunNpc = false;  //选择怪物开关
    bool m_bShowNpc = false;
    bool m_bShowObj = false;
    int m_nChooseId = -1;
    bool m_bShowChooseObj = false;  //选择怪物开关

    bool m_bObsctaleMode = true;            //阻挡模式开关
    bool m_bIsLockObstacle = true;          //是否锁定阻挡

    public bool m_bCanDraw = true;            //Gui是否进行绘制的开关
    float m_iMonseRectSize = 3;   //是否开启鼠标框选  
    int m_iCreateRectSize = 0;
    bool mb_IsHit;                     //是否有碰撞产生 
    bool mb_IsHaveObStacle;            //当前位置是否有阻挡

    public float m_fMaxObstaclepos = 0;       //当前阻挡点的最高位置
    float m_fPreObstaclePos = 0;       //阻挡GUi的值
    float mf_FloorMaxheight = -10;    //地面最高
    float mf_SceneMaxheight = -10;       //场景对象最高
    int m_OneShowNum = 20;
    int m_ShowNpcIndex = 1;
    int m_ShowFunNpcIndex = 1;
    int m_ShowOBJIndex = 1;

    private Vector2 mp_MonstInfoScrollPos;     //怪物列表滑动位置
    private Vector2 mp_FunNpcInfoScrollPos;   //
    private Vector2 mp_ObjInfoScrollPos;       //OBJ列表滑动位置
    private Vector2 mp_InspectorScrollPos;     //窗口滑动位置
    private Vector3 mp_MousPos;                //鼠标位置
    private Vector3 m_v2FirstMouseposition;  //第一次点击位置
    private Vector3 m_v2SecondMouseposition;  //第一次点击位置
    
    public CreateMode m_enCreateMode = CreateMode.enObstcale;  //当前选择的生成模式
    public Projector m_oSceneEditorPreView = null;    // 用于当鼠标点下去之后的影像
    public int m_iNavigateId = 0;

    

    public GameObject RegionBorn = null; //范围刷怪点
    public GameObject PointBorn = null; //点刷怪
    public GameObject PathPointBorn = null; // 路径点
    public GameObject ScenePointBorn = null; //场景点
    public GameObject PatrolBorn = null;//怪物巡逻点
    public GameObject AllParent { get; set; }
    public GameObject m_oEditorGameObj { get; set; }
    

    public Transform RegionParent; //范围点刷怪的父节点
    public Transform PointParent;//点刷怪的父节点
    public Transform PathSetParent;//路径点父节点
    public Transform ScenePointParent; //场景点父节点

    public PathGridComponent m_csPathGrid { get; set; }
    
    
    Texture m_ObstcalePng;              //阻挡图片
    Texture m_RegionPng;           //范围刷点
    Texture m_PointPng;        //定点刷怪
    Texture m_ScenePointPng;              //场景点图片
    Texture m_PathPointPng;              //路径点图片
    Texture m_OBJPng;                   //OBj出生点图片

    Texture m_PatrolPng; //巡逻点按钮图片
    
    SceneView.OnSceneFunc _delegate;

    public string m_sObstaclePath;      //载入的阻挡的路径
    public string m_sBornListPath;      //载入的出生点的路径
    public string m_sNavigationPath;      //载入的导航的路径
    #endregion

    #region Unity函数

    //逻辑更新函数
    void Update()
    {
        if (!Application.isPlaying)
            CloseWindow();
        else
        {
            bool ClickObstacle = false;
            GameObject[] objarray = Selection.gameObjects;
            if (m_bCanClickOtherObj)
            {
                for (int i = 0; i < objarray.Length; i++)
                {
                    if (objarray[i].tag != "EditorOnly" || objarray[i] == AllParent)
                    {
                        Selection.activeGameObject = null;
                    }
                }
            }

            if (m_bIsLockObstacle && ClickObstacle)
            {
                //Selection.activeGameObject = null;
            }

 
   
        }
    }

    //简视窗口更新函数
    void OnInspectorUpdate()
    {
        if (!Application.isPlaying)
            CloseWindow();
        else
        {
            //Debug.Log("窗口面板的更新");
            //这里开启窗口的重绘，不然窗口信息不会刷新
            m_csPathGrid.InitArray();
            this.Repaint();
        }

    }

    //GUI绘制函数 Unity自带
    void OnGUI()
    {
        if (!Application.isPlaying)
            CloseWindow();
        else
        {
            if (AllParent == null || !m_bCanDraw)
                return;

            EditorGUILayout.BeginHorizontal();
            mp_InspectorScrollPos = EditorGUILayout.BeginScrollView(mp_InspectorScrollPos, false, false);
            EditorGUILayout.BeginVertical();
            Event e = Event.current;
            if (e.keyCode == KeyCode.Return && e.type == EventType.keyDown)
            {
                OnLostFocus();
            }
            DrawMapInfo();
            EditorGUILayout.Space();

            //绘制FunNpc
            DrawFunNpcInfo();
            //绘制Npc
            DrawNpcInfo();
            //绘制obj
            DrawObjInfo();

            EditorGUILayout.Space();

            
           

            EditorGUILayout.Space();
            DrawModeChoose();
            EditorGUILayout.Space();
            DrawFeatureButton();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();
        }
    }

    void DrawFunNpcInfo()
    {
        Dictionary<int, FunNpcInfo> m_FunNpcInfo = InteractionTool.GetFunNpcList();
       // List<int> m_FunNpcIdList = InteractionTool.GetFunNpcIdList();
        EditorGUILayout.BeginHorizontal();
        m_bShowFunNpc = EditorGUILayout.Foldout(m_bShowFunNpc, "选择生成功能NPC");
        if (m_bShowFunNpc)
        {
            m_bShowObj = false;
            m_bShowNpc = false;
        }
        GUILayout.Space(50);
        int funnpcid = EditorGUILayout.IntField("功能NPC ID", m_nChooseId);
        if (m_FunNpcInfo.ContainsKey(funnpcid))
        {
            m_nChooseId = funnpcid;
            m_bShowFunNpc = true;
            if (m_bShowFunNpc)
            {
                m_bShowObj = false;
                m_bShowNpc = false;
            }
            EditorGUILayout.LabelField("npc名字： " + m_FunNpcInfo[m_nChooseId].name);
        }
        
        EditorGUILayout.EndHorizontal();
        if (m_bShowFunNpc)
        {
            DrawFunNpcInfoList(ref mp_FunNpcInfoScrollPos, m_FunNpcInfo, m_nChooseId, true);
        }
    }

    void DrawNpcInfo()
    {
        Dictionary<int, NpcInfo> m_NpcInfo = InteractionTool.GetNpcList();
        // List<int> m_FunNpcIdList = InteractionTool.GetFunNpcIdList();
        EditorGUILayout.BeginHorizontal();
        m_bShowNpc = EditorGUILayout.Foldout(m_bShowNpc, "选择生成NPC");
        if (m_bShowNpc)
        {
            m_bShowObj = false;
            m_bShowFunNpc = false;
        }
        GUILayout.Space(50);
        int npcid = EditorGUILayout.IntField("NPC ID", m_nChooseId);
        if (m_NpcInfo.ContainsKey(npcid))
        {
            m_nChooseId = npcid;
            m_bShowNpc = true;
            if (m_bShowNpc)
            {
                m_bShowObj = false;
                m_bShowFunNpc = false;
            }
            EditorGUILayout.LabelField("怪物名字： " + m_NpcInfo[m_nChooseId].name);
        }

        EditorGUILayout.EndHorizontal();
        if (m_bShowNpc)
        {
            DrawNpcInfoList(ref mp_MonstInfoScrollPos, m_NpcInfo, m_nChooseId, true);
        }
    }

    void DrawObjInfo()
    {
        Dictionary<int, ObjInfo> m_ObjInfo = InteractionTool.GetObjList();
        // List<int> m_FunNpcIdList = InteractionTool.GetFunNpcIdList();
        EditorGUILayout.BeginHorizontal();
        m_bShowObj = EditorGUILayout.Foldout(m_bShowObj, "选择生成Obj");
        if (m_bShowObj)
        {
            m_bShowNpc = false;
            m_bShowFunNpc = false;
        }
        GUILayout.Space(50);
        int objid = EditorGUILayout.IntField("OBJ ID", m_nChooseId);
        if (m_ObjInfo.ContainsKey(objid))
        {
            m_nChooseId = objid;
            m_bShowObj = true;
            if (m_bShowObj)
            {
                m_bShowNpc = false;
                m_bShowFunNpc = false;
            }
            EditorGUILayout.LabelField("OBJ名字： " + m_ObjInfo[m_nChooseId].name);
        }

        EditorGUILayout.EndHorizontal();
        if (m_bShowObj)
        {
            DrawObjInfoList(ref mp_ObjInfoScrollPos, m_ObjInfo, m_nChooseId, true);
        }
    }
    void OnDrawGizmos()
    {

    }

    #endregion

    #region    辅助函数
    //弹出框
    public static bool DialogReturn(string title, string text, bool defuate = false)
    {
        bool temp;
        if (defuate)
            temp = EditorUtility.DisplayDialog(
           title,
           text,
           "确定");
        else
            temp = EditorUtility.DisplayDialog(
            title,
            text,
            "确定",
            "取消");
        return temp;
    }

    //根据地图位置获取3D位置
    public  Vector2 GetMapPosWith3DPos(float x, float y)
    {
        
        x = x - _windowInstance.AllParent.transform.position.x;
        y = y - _windowInstance.AllParent.transform.position.y;

        return m_csPathGrid.GetMapPosByWordPos(x, y);
    }

    public Vector2 GetWorldPosWithMapPos(float x, float y)
    {
        return m_csPathGrid.GetWorldPosByMapPos(x, y);
    }

    //根据大小交换
    public static void SwapMaxMin(ref float max, ref float min)
    {
        float temp = min;
        if (temp > max)
        {
            min = max;
            max = temp;
        }
    }

    //获取当前选中目标的类型
    public static CreateMode GetChooseMode()
    {
        if (!_windowInstance)
        {
            _windowInstance.m_oEditorGameObj = null;
            return CreateMode.enInvild;
        }
        if (Selection.activeGameObject)
        {
            GameObject[] objarrayElse = Selection.gameObjects;
            BornInfo borninfo = Selection.activeGameObject.GetComponent<BornInfo>();
            if (objarrayElse.Length != 1 || borninfo == null)
            {
                _windowInstance.m_oEditorGameObj = null;
                return CreateMode.enInvild;
            }
            _windowInstance.m_oEditorGameObj = Selection.gameObjects[0];
            return borninfo.me_Type;
        }
        //if (_windowInstance.m_enCreateMode == CreateMode.enDaoHang)
        //{
        //    _windowInstance.m_oEditorGameObj = _windowInstance.AllParent;
        //    return _windowInstance.m_enCreateMode;
        //}

        _windowInstance.m_oEditorGameObj = null;
        return CreateMode.enInvild;
    }

    //获取2点之间的角度
    public static int GetAngelWithtowPos(float x1, float y1, float x2, float y2)
    {
        double jiaodu = (Math.Atan(((x1 - x2) / (y1 - y2)))) * 180 / Math.PI;
        if (y1 >= y2)
        {
            jiaodu += 180;
        }

        if (jiaodu >= 360)
        {
            jiaodu -= 360;
        }
        if (jiaodu < 0)
        {
            jiaodu += 360;
        }
        return (int)jiaodu;
    }

    //根据X,Y获取地图下标
    public static int GetMapIndexWithPos(int x, int y)
    {
        int MapIndex = -1;
        MapIndex = x + y * _windowInstance.m_csPathGrid.m_numberOfColumns;
        return MapIndex;
    }

    //根据地图下标获取位置
    public static void GetPosWithIndex(int Index, ref int X, ref int Y)
    {
        int numberOfColumns = _windowInstance.m_csPathGrid.m_numberOfColumns;
        X = Index % numberOfColumns;
        Y = (Index - X) / numberOfColumns;
    }

    //UI排版使用,和EndSpace配套使用
    public static void StartSpace(int cengji, int LSpace = 40)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(cengji * LSpace);
        GUILayout.BeginVertical();
    }

    //UI排版使用,和StartSpace配套使用
    public static void EndSpace()
    {
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }
    #endregion

    #region 操作相关
    //打开窗口
    [MenuItem("Window/场景编辑器/打开场景编辑器")]
    static void OpenWindow()
    {
        if(Application.isPlaying)
        {
            if (_windowInstance == null)
            {
                _windowInstance = EditorWindow.GetWindow(typeof(SceneEditorWindow)) as SceneEditorWindow;
                _windowInstance.Init();
            }
            else
            {
                _windowInstance.Focus();
            }
        }
        else
        {
            Debug.LogWarning("请先启动游戏");
        }
    }

    [MenuItem("Window/测试")]
    static void Test()
    {
    }

    void GetMapMaxAndMin()
    {
        GameObject[] objs = GameObject.FindObjectsOfType<GameObject>();
        Vector2 MaxPos = Vector2.zero;
        Vector2 MinPos = Vector2.zero;
        bool IsInit = false;
        for (int i = 0; i < objs.Length; i++)
        {
            if (objs[i].transform.parent != null)
                continue;
            MeshRenderer render = objs[i].GetComponent<MeshRenderer>();

            if(render)
            {
                if (!IsInit)
                {
                    MaxPos = new Vector2(render.bounds.max.x, render.bounds.max.z);
                    MinPos = new Vector2(render.bounds.min.x, render.bounds.min.z);
                    IsInit = true;
                }
                else
                {
                    Vector2 TempMaxPos = new Vector2(render.bounds.max.x, render.bounds.max.z);
                    Vector2 TempMinPos = new Vector2(render.bounds.min.x, render.bounds.min.z);
                    if (TempMaxPos.x - MaxPos.x > 0)
                    {
                        MaxPos.x = TempMaxPos.x;
                    }
                    if (TempMaxPos.y - MaxPos.y > 0)
                    {
                        MaxPos.y = TempMaxPos.y;
                    }
                    if (TempMinPos.x - MinPos.x < 0)
                    {
                        MinPos.x = TempMinPos.x;
                    }
                    if (TempMinPos.y - MinPos.y < 0)
                    {
                        MinPos.y = TempMinPos.y;
                    }
                }
            }
            
        }
        m_csPathGrid.transform.position = new Vector3((int)(MinPos.x - 5), m_fMaxObstaclepos, (int)(MinPos.y - 5));
        m_csPathGrid.m_numberOfColumns = (int)(MaxPos.x - MinPos.x + 15);
        m_csPathGrid.m_numberOfRows = (int)(MaxPos.y - MinPos.y + 15);

        m_csPathGrid.InitArray(true); 
    }

    //关闭窗口
    [MenuItem("Window/场景编辑器/关闭场景编辑器")]
    public static void CloseWindow()
    {
        
        if (_windowInstance != null )
        {
            if(Application.isPlaying == true)
            {
                string title = "关闭窗口";
                string text = "关闭窗口会导致当前阻挡和出生点全部失效?是否确定?";
                if (!DialogReturn(title, text))
                {
                    return;
                }
            }
            _windowInstance.Close();
            InteractionTool.DestoryObserver();
        }
    }
    #endregion

    #region 初始化

       

    //移动所有阻挡到同一平面
    void MoveObsctleToHigh(float y)
    {
        if (m_fMaxObstaclepos != y)
        {
            m_fMaxObstaclepos = y;
            m_fPreObstaclePos = y;
            AllParent.transform.position = new Vector3(AllParent.transform.position.x, y, AllParent.transform.position.z);
        }

    }

    //用于查找节点
    void FindGameObj(ref Transform tran, string name , string path)
    {
        if (!_windowInstance || tran)
            return;
        GameObject obj = GameObject.Find(name);
        if(obj == null)
        {
            obj = CreateParntWithPath(path);
        }
        if (obj == null)
            return;
        tran = obj.transform;
    }

    //检测父节点是否存在,不存在则创建
    public void CheekParent()
    {
        FindGameObj(ref RegionParent, "RegionParent", "Assets/Z_Temp/Obstacle/SceneEditorPerfab/RegionParent.prefab");
        FindGameObj(ref PointParent, "PointParent", "Assets/Z_Temp/Obstacle/SceneEditorPerfab/PointParent.prefab");
        FindGameObj(ref PathSetParent, "PathSetParent", "Assets/Z_Temp/Obstacle/SceneEditorPerfab/PathSetParent.prefab");
        FindGameObj(ref ScenePointParent, "ScenePointParent", "Assets/Z_Temp/Obstacle/SceneEditorPerfab/ScenePointParent.prefab");               
    }

    //用于创建父节点
    GameObject CreateParntWithPath(string path)
    {
        if (AllParent == null)
            return null;
        GameObject objprefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
        GameObject parent = Instantiate(objprefab) as GameObject;
        Transform tran = parent.transform;
        tran.parent = AllParent.transform;
        tran = parent.transform;
        tran.localPosition = new Vector3(0, 0, 0);
        tran.name = tran.name.Replace("(Clone)", "");
        return parent;
    }

    //初始化函数
    void Init()
    {
        minSize = new Vector2(600, 0);
        RenderSettings.fog = false;
        _delegate = new SceneView.OnSceneFunc(OnSceneFunc);
        SceneView.onSceneGUIDelegate += _delegate;
        m_sBornListPath = "";
        m_sObstaclePath = "";
        m_sNavigationPath = "";
        //加载选择图片
        ChangeTexture(ref  _windowInstance.m_ObstcalePng);

        //加载prefab
		string sPrefabPath = "Assets/Z_Temp/Obstacle/SceneEditorPerfab/";
		//ObsPrefabs = AssetDatabase.LoadAssetAtPath( sPrefabPath + "Obstacle.prefab", typeof(GameObject)) as GameObject; ;
        RegionBorn = AssetDatabase.LoadAssetAtPath(sPrefabPath + "RegionBorn.prefab", typeof(GameObject)) as GameObject; ;
        PointBorn = AssetDatabase.LoadAssetAtPath(sPrefabPath + "PointBorn.prefab", typeof(GameObject)) as GameObject; ;
        ScenePointBorn = AssetDatabase.LoadAssetAtPath(sPrefabPath + "ScenePointBorn.prefab", typeof(GameObject)) as GameObject; ;
        PathPointBorn = AssetDatabase.LoadAssetAtPath(sPrefabPath + "PathPointBorn.prefab", typeof(GameObject)) as GameObject;
        PatrolBorn = AssetDatabase.LoadAssetAtPath(sPrefabPath + "PatrolBorn.prefab", typeof(GameObject)) as GameObject;

        //生成各个父节点
        AllParent = GameObject.Find("Parent");
        if (AllParent == null)
        {
			GameObject parent = AssetDatabase.LoadAssetAtPath(sPrefabPath +"Parent.prefab", typeof(GameObject)) as GameObject;
            AllParent = Instantiate(parent) as GameObject;
            AllParent.transform.position = new Vector3(0, 0, 0);
            AllParent.name = AllParent.name.Replace("(Clone)", "");
        }
        m_csPathGrid = AllParent.GetComponent<PathGridComponent>();
        m_csPathGrid.m_debugShow = true;
        CheekParent();
        m_csPathGrid.InitArray(false); 
        GetFloorHeight();

        _windowInstance.position = new Rect(400, 90, 150, 940);
        BornInfoWindow.OpenWindow();
        EditerDataClass._Instance.Init();
        //InteractionTool.AddObserver(EditerDataClass._Instance);
        //InteractionTool.AddObserver(BornInfoWindow._Instance);
       // InteractionTool.InitObserver();
        //自动导入阻挡文件
        InteractionTool.Importobstacle(true);
    }

    //析构
    void OnDestroy()
    {
        
        if (_delegate != null)
            SceneView.onSceneGUIDelegate -= _delegate;
        DestroyImmediate(AllParent);
    }
    
    //获取地面高度
    void GetFloorHeight()
    {
       GameObject[] objarry = GameObject.FindObjectsOfType<GameObject>();
       for (int i = 0; i < objarry.Length; i++)
       {
           if (objarry[i].transform.parent != null || objarry[i].name == "Main Camera")
           {
               continue;
           }
           float y = objarry[i].transform.position.y;
           if(objarry[i].tag == "floor")
           {
               if(mf_FloorMaxheight < y)
               {
                   mf_FloorMaxheight = y;
               }
               
           }
           else
           {
               if(mf_SceneMaxheight < y)
               {
                   mf_SceneMaxheight = y;
               }
               
           }
       }
       mf_FloorMaxheight += 0.1f;
       mf_SceneMaxheight += 0.2f;
    }

    //切换按钮图片
    void ChangeTexture(ref Texture changetexure)
    {

        m_ObstcalePng = AssetDatabase.LoadAssetAtPath("Assets/Z_Temp/Obstacle/Meta/zudang.png", typeof(Texture)) as Texture;
        m_RegionPng = AssetDatabase.LoadAssetAtPath("Assets/Z_Temp/Obstacle/Meta/regionpoint.png", typeof(Texture)) as Texture;
        m_PointPng = AssetDatabase.LoadAssetAtPath("Assets/Z_Temp/Obstacle/Meta/dingdian.png", typeof(Texture)) as Texture;
        m_ScenePointPng = AssetDatabase.LoadAssetAtPath("Assets/Z_Temp/Obstacle/Meta/scenepoint.png", typeof(Texture)) as Texture;
        m_PathPointPng = AssetDatabase.LoadAssetAtPath("Assets/Z_Temp/Obstacle/Meta/pathpoint2.png", typeof(Texture)) as Texture;
        m_OBJPng = AssetDatabase.LoadAssetAtPath("Assets/Z_Temp/Obstacle/Meta/obj.png", typeof(Texture)) as Texture;        
        m_PatrolPng = AssetDatabase.LoadAssetAtPath("Assets/Z_Temp/Obstacle/Meta/pathpoint.png", typeof(Texture)) as Texture;
        changetexure = AssetDatabase.LoadAssetAtPath("Assets/Z_Temp/Obstacle/Meta/" + changetexure.name + "anxia.png", typeof(Texture)) as Texture;
        

    }
    #endregion

    #region  绘制场景中的信息
    //绘制场景坐标
    void DrawScenePos()
    {
        Vector2 posv2 = GetMapPosWith3DPos(mp_MousPos.x, mp_MousPos.y);
        
        if (!mb_IsHit )
        {
            GUILayout.Label("X坐标: " + "错误的坐标" + "  " + "Y坐标: " + "错误的坐标");
        }
        else
        {
            string ishave = "";
            if (mb_IsHaveObStacle)
            {
                ishave = "有阻挡";
            }
            else
            {
                ishave = "无";
            }

            //GUILayout.Label("X坐标: " + posv2.x + "  " + "Y坐标: " + posv2.y + "   阻挡: " + ishave+ " Index:" + GetMapIndexWithPos((int)posv2.x,(int)posv2.y).ToString());
            GUILayout.Label("X坐标: " + posv2.x + "  " + "Y坐标: " + posv2.y + "   阻挡: " + ishave + " OldPos:" + mp_MousPos.x.ToString() + ", " + mp_MousPos.y.ToString());
        }
        

    }

    //绘制场景中信息
    void DrawSceneGui()
    {
        GUI.color = Color.green;
        Handles.BeginGUI();

        //GUILayout.BeginArea(new Rect(10, 10, 190, 43), GUI.skin.button);
        GUILayout.BeginArea(new Rect(10, 10,500, 110), GUI.skin.button);
        DrawScenePos();
        DrawButton();
        DrawPath();
        GUILayout.EndArea();
        Handles.EndGUI();
    }

    //绘制载入路径
    void DrawPath()
    {
        string obstaclestr = "未载入";
        string BornListstr = "未载入";
        string navigationstr = "未载入";
        if (m_sObstaclePath != "")
        {
            obstaclestr = m_sObstaclePath;
        }
        if (m_sBornListPath != "")
        {
            BornListstr = m_sBornListPath;
        }
        if (m_sNavigationPath != "")
        {
            navigationstr = m_sNavigationPath;
        }
        GUILayout.Label("阻挡文件: " + obstaclestr );
        GUILayout.Label("出生点文件: " + BornListstr);
        GUILayout.Label("导航文件: " + navigationstr);
    }

    //绘制功能按钮
    void DrawButton()
    {
        GUILayout.BeginHorizontal();
        GUIContent Buttongui = new GUIContent("最高");
        if (GUILayout.Button(Buttongui, GUILayout.Width(50)))
        {
            MoveObsctleToHigh(mf_SceneMaxheight);
        }
        Buttongui = new GUIContent("地面");
        if (GUILayout.Button(Buttongui, GUILayout.Width(50)))
        {
            MoveObsctleToHigh(mf_FloorMaxheight);
        }
        //Buttongui = new GUIContent("路径点");
        //if (GUILayout.Button(Buttongui, GUILayout.Width(70)))
        //{
        //    m_enCreateMode = CreateMode.enWayPoint;
        //}
        GUILayout.EndHorizontal();
    }

    
    #endregion

    #region 绘制函数
    

    //绘制功能按钮
    void DrawFeatureButton()
    {
        m_bShowFeature = EditorGUILayout.Foldout(m_bShowFeature, "功能相关");
        if (m_bShowFeature)
        {

            StartSpace(1);
            if (GUILayout.Button("导出场景阻挡文件"))
            {
                InteractionTool.Exportobstacle();
            }

            if (GUILayout.Button("导出场景布怪/Npc/Obj文件"))
            {
                InteractionTool.ExportFunNpcMapInfo();
            }           

            if (GUILayout.Button("导出场景导航文件"))
            {
                //InteractionTool.ExportNavigate();
            }

            if (GUILayout.Button("导入场景阻挡文件"))
            {
                InteractionTool.Importobstacle();
            }

            if (GUILayout.Button("导入场景布怪/NPC/Obj文件"))
            {
                InteractionTool.ImportNpcMapInfo();
            }

            if (GUILayout.Button("导入导航文件"))
            {
               // InteractionTool.ImportNavigate();
            }


            EndSpace();
        }
        
    }


    //绘制地图信息
    void DrawMapInfo()
    {
        m_bShowMapInfo = EditorGUILayout.Foldout(m_bShowMapInfo, "地图相关信息");
        if (m_bShowMapInfo)
        {
            StartSpace(1);
            m_csPathGrid.m_debugShow = EditorGUILayout.Toggle("是否显示地图格子", m_csPathGrid.m_debugShow);
            m_bShowRay = EditorGUILayout.Toggle("是否显示地图射线", m_bShowRay);
            m_bCanClickOtherObj = EditorGUILayout.Toggle("是否锁定其他对象", m_bCanClickOtherObj);
            m_bObsctaleMode = EditorGUILayout.Toggle("删除阻挡模式", m_bObsctaleMode);
            m_csPathGrid.m_debugColor = EditorGUILayout.ColorField("地图格子颜色", m_csPathGrid.m_debugColor);

            m_csPathGrid.m_bIsShowObstacle = EditorGUILayout.Toggle("隐藏阻挡", m_csPathGrid.m_bIsShowObstacle);
            m_bIsLockObstacle = EditorGUILayout.Toggle("锁定阻挡", m_bIsLockObstacle);

            Vector2 mapvect2 = EditorGUILayout.Vector2Field("地图大小", new Vector2((int)m_csPathGrid.m_numberOfColumns, (int)m_csPathGrid.m_numberOfRows));
            m_csPathGrid.m_numberOfColumns = (int)mapvect2.x;
            m_csPathGrid.m_numberOfRows = (int)mapvect2.y;
            
            m_iCreateRectSize = EditorGUILayout.IntSlider(m_iCreateRectSize, 1, 40);//格子数

            m_iMonseRectSize = (float)m_iCreateRectSize*0.2f / (float)2;
            m_fPreObstaclePos = EditorGUILayout.FloatField("地图整体高度", m_fPreObstaclePos);
            BoxCollider pathgridcollider = AllParent.GetComponent<BoxCollider>();
            pathgridcollider.center = new Vector3(((float)m_csPathGrid.m_numberOfColumns) / (float)2, ((float)m_csPathGrid.m_numberOfRows) / (float)2, 0 );
            pathgridcollider.size = new Vector3(((int)mapvect2.x), ((int)mapvect2.y), 0);

            if (GUILayout.Button("匹配地图大小"))
            {
                GetMapMaxAndMin();
            }

            if (GUILayout.Button("修正零点坐标"))
            {
                m_csPathGrid.InitArray(true);
            }

            EndSpace();
        }
    }

    void OnLostFocus()
    {
        if (m_fPreObstaclePos != m_fMaxObstaclepos)
        {
            MoveObsctleToHigh(m_fPreObstaclePos);
        }
    }

    //绘制模式选择按钮
    void DrawModeChoose()
    {
        m_bShowChoosepng = EditorGUILayout.Foldout(m_bShowChoosepng,"选择生成点");
        switch (m_enCreateMode)
        {
            case CreateMode.enObstcale:
                ChangeTexture(ref m_ObstcalePng);
                break;
            case CreateMode.enScenePoint:
                ChangeTexture(ref m_ScenePointPng);
                break;
            case CreateMode.enRegionObj:
                ChangeTexture(ref m_RegionPng);
                break;
            case CreateMode.enPointObj:
                ChangeTexture(ref m_PointPng);
                break;
            case CreateMode.enPathPoint:
                ChangeTexture(ref m_PathPointPng);
                break;
            case CreateMode.enPatrolPoint:
                ChangeTexture(ref m_PatrolPng);
                break;          
            default:
                break;
        }
        if(m_bShowChoosepng)
        {
            StartSpace(1);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(m_ObstcalePng, GUILayout.Width(80), GUILayout.Height(80)))
            {
                m_enCreateMode = CreateMode.enObstcale;
            }
            if (GUILayout.Button(m_RegionPng, GUILayout.Width(80), GUILayout.Height(80)))
            {
                m_enCreateMode = CreateMode.enRegionObj;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(m_PointPng, GUILayout.Width(80), GUILayout.Height(80)))
            {
                m_enCreateMode = CreateMode.enPointObj;
            }
            if (GUILayout.Button(m_ScenePointPng, GUILayout.Width(80), GUILayout.Height(80)))
            {
                m_enCreateMode = CreateMode.enScenePoint;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(m_PathPointPng, GUILayout.Width(80), GUILayout.Height(80)))
            {
               m_enCreateMode = CreateMode.enPathPoint;
            }

            if (GUILayout.Button(m_PatrolPng, GUILayout.Width(80), GUILayout.Height(80)))
            {
                m_enCreateMode = CreateMode.enPatrolPoint;
            }

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
        
            EndSpace();
            GUILayout.EndHorizontal();
        }
    }

    //绘制怪物信息
    void DrawFunNpcInfoList(ref Vector2 ScrollPos, Dictionary<int, FunNpcInfo> MonsterInfoList, int nChooseID, bool isChar = false)
    {
        StartSpace(1);
        float height = 30;
        int ScrollViewheight = m_OneShowNum * 30 + 20;
        if (ScrollViewheight > 280) 
        {
            ScrollViewheight = 280;
        }
        GUILayout.BeginHorizontal();
        ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos, false, false, GUILayout.Height(ScrollViewheight));
        GUILayout.BeginVertical();

        int nCurIndex = m_ShowFunNpcIndex;
        List<int> tKeys = new List<int>(MonsterInfoList.Keys);
        tKeys.Sort();
        int nStart = (nCurIndex - 1) * m_OneShowNum;
        int nEnd = Mathf.Min(nCurIndex * m_OneShowNum, tKeys.Count);
        int nTotal = (int) Mathf.Ceil(tKeys.Count / (float)m_OneShowNum);
        for (int i = nStart; i < nEnd; i++)
        {
            FunNpcInfo info = null;
            int id = tKeys[i];
            MonsterInfoList.TryGetValue(id, out info);
            string Buttonshow = "名字:\t" + info.name + "\t" + "ID:\t" + info.id;
            if (id != nChooseID)
            {
                height = 30;
            }
            else
            {
                height = 60;
                Buttonshow = "当  前  选  中\n\n" + Buttonshow;
            }
            GUIContent b = new GUIContent(Buttonshow);
            if (GUILayout.Button(b, GUILayout.Height(height)))
            {
                m_nChooseId = id;
            }
        }
        GUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
        GUILayout.EndHorizontal();

        //EditorGUILayout.Space();
        nCurIndex = EditorGUILayout.IntSlider(nCurIndex, 1, nTotal);
        m_ShowFunNpcIndex = nCurIndex;

        EndSpace();
    }


    void DrawNpcInfoList(ref Vector2 ScrollPos, Dictionary<int, NpcInfo> MonsterInfoList, int nChooseID, bool isChar = false)
    {
        StartSpace(1);
        float height = 30;
        int ScrollViewheight = m_OneShowNum * 30 + 20;
        if (ScrollViewheight > 280)
        {
            ScrollViewheight = 280;
        }
        GUILayout.BeginHorizontal();
        ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos, false, false, GUILayout.Height(ScrollViewheight));
        GUILayout.BeginVertical();

        int nCurIndex = m_ShowNpcIndex;
        List<int> tKeys = new List<int>(MonsterInfoList.Keys);
        tKeys.Sort();
        int nStart = (nCurIndex - 1) * m_OneShowNum;
        int nEnd = Mathf.Min(nCurIndex * m_OneShowNum, tKeys.Count);
        int nTotal = (int)Mathf.Ceil(tKeys.Count / (float)m_OneShowNum);
        for (int i = nStart; i < nEnd; i++)
        {
            NpcInfo info = null;
            int id = tKeys[i];
            MonsterInfoList.TryGetValue(id, out info);
            string Buttonshow = "名字:\t" + info.name + "\t" + "ID:\t" + info.id;
            if (id != nChooseID)
            {
                height = 30;
            }
            else
            {
                height = 60;
                Buttonshow = "当  前  选  中\n\n" + Buttonshow;
            }
            GUIContent b = new GUIContent(Buttonshow);
            if (GUILayout.Button(b, GUILayout.Height(height)))
            {
                m_nChooseId = id;
            }
        }
        GUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
        GUILayout.EndHorizontal();

        //EditorGUILayout.Space();
        nCurIndex = EditorGUILayout.IntSlider(nCurIndex, 1, nTotal);
        m_ShowNpcIndex = nCurIndex;

        EndSpace();
    }

    void DrawObjInfoList(ref Vector2 ScrollPos, Dictionary<int, ObjInfo> ObjInfoList, int nChooseID, bool isChar = false)
    {
        StartSpace(1);
        float height = 30;
        int ScrollViewheight = m_OneShowNum * 30 + 20;
        if (ScrollViewheight > 280)
        {
            ScrollViewheight = 280;
        }
        GUILayout.BeginHorizontal();
        ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos, false, false, GUILayout.Height(ScrollViewheight));
        GUILayout.BeginVertical();

        int nCurIndex = m_ShowOBJIndex;
        List<int> tKeys = new List<int>(ObjInfoList.Keys);
        tKeys.Sort();
        int nStart = (nCurIndex - 1) * m_OneShowNum;
        int nEnd = Mathf.Min(nCurIndex * m_OneShowNum, tKeys.Count);
        int nTotal = (int)Mathf.Ceil(tKeys.Count / (float)m_OneShowNum);
        for (int i = nStart; i < nEnd; i++)
        {
            ObjInfo info = null;
            int id = tKeys[i];
            ObjInfoList.TryGetValue(id, out info);
            string Buttonshow = "名字:\t" + info.name + "\t" + "ID:\t" + info.id;
            if (id != nChooseID)
            {
                height = 30;
            }
            else
            {
                height = 60;
                Buttonshow = "当  前  选  中\n\n" + Buttonshow;
            }
            GUIContent b = new GUIContent(Buttonshow);
            b.text.PadRight(300);
            if (GUILayout.Button(b, GUILayout.Height(height)))
            {
                m_nChooseId = id;
            }
        }
        GUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
        GUILayout.EndHorizontal();

        //EditorGUILayout.Space();
        nCurIndex = EditorGUILayout.IntSlider(nCurIndex, 1, nTotal);
        m_ShowOBJIndex = nCurIndex;

        EndSpace();
    }
    #endregion

    #region 逻辑相关




    //场景回调函数
    public void OnSceneFunc(SceneView sceneView)
    {
        if (!m_bCanDraw || Application.isPlaying == false)
            return;

       Event e =  Event.current;
        CustomSceneGUI(sceneView);
        if (m_enCreateMode == CreateMode.enObstcale && !m_bIsLockObstacle)
        {

            Handles.RectangleCap(0, mp_MousPos, Quaternion.Euler(90, 0, 0), m_iMonseRectSize);
            HandleUtility.AddDefaultControl(0);
            if (e.isMouse && e.button == 0 && e.type == EventType.MouseDown)
            {
                MouseRect(e.mousePosition);
            }
            if (e.isMouse && e.button == 0 && e.type == EventType.MouseDrag)
            {
                MouseRect(e.mousePosition);
            }
        }
        

        DrawSceneGui();
        
    }

    //点击生成矩形位置阻挡
    void MouseRect(Vector2 Pos)
    {
        bool hit = GetRayHitPoint(Pos, out m_v2FirstMouseposition);
        if (hit)
        {
            if (!m_bObsctaleMode)
            {
                for (float x = m_v2FirstMouseposition.x - m_iMonseRectSize; x <= m_v2FirstMouseposition.x + m_iMonseRectSize; x += 0.1f)
                {
                    for (float y = m_v2FirstMouseposition.y - 0.1f; y <= m_v2FirstMouseposition.y + 0.1f; y += 0.1f)
                    {

                        if (GetHaveObstcle(x, y) == 2)
                        {
                            AddPosToObstacle(x, y, m_fMaxObstaclepos);
                        }
                    }
                }

            }
            else
            {
                for (float x = m_v2FirstMouseposition.x - m_iMonseRectSize; x <= m_v2FirstMouseposition.x + m_iMonseRectSize; x += 0.1f)
                {
                    for (float y = m_v2FirstMouseposition.y - 0.1f; y <= m_v2FirstMouseposition.y + 0.1f; y += 0.1f)
                    {

                        if (GetHaveObstcle(x, y) == 1)
                        {
                            PosToDelObstacle(x, y, m_fMaxObstaclepos);
                        }
                    }
                }
            }
        }
        

        
       
       
    }


    bool GetRayHitPoint(Vector2 Pos, out Vector3 outPos)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Pos);
        RaycastHit[] hitarray = Physics.RaycastAll(ray);


        for (int i = 0; i < hitarray.Length; i++)
        {
            if (hitarray[i].collider.gameObject.name == "Parent")
            {
                 outPos = hitarray[i].point;
                 return true;
            }
        }
        outPos = Vector3.zero;
        return false;
       
    }

    //绘制射线
    void DrawRayLine()
    {

        RaycastHit _hitInfo;
        Vector3 origin = mp_MousPos;
        origin.y += 100;
        if (Physics.Raycast(origin, Vector3.down, out _hitInfo))
        {
            Handles.color = Color.yellow;
            Handles.DrawLine(_hitInfo.point, origin);
            float arrowSize = 1;
            Vector3 pos = _hitInfo.point;
            Quaternion quat;
            Handles.color = Color.red;
            quat = Quaternion.LookRotation(Vector3.right, Vector3.up);
            Handles.ArrowCap(0, pos, quat, arrowSize);
            Handles.color = Color.blue;
            quat = Quaternion.LookRotation(Vector3.forward, Vector3.up);
            Handles.ArrowCap(0, pos, quat, arrowSize);
        }
    }


    //场景绘制函数与逻辑主函数
    void CustomSceneGUI(SceneView sceneView)
    {
        CreateMode mode = GetChooseMode();
        Event e = Event.current;
        mb_IsHaveObStacle = false;


        mb_IsHit = GetRayHitPoint(e.mousePosition, out mp_MousPos);

        if (GetHaveObstcle(mp_MousPos.x, mp_MousPos.y) == 1)
        {
            mb_IsHaveObStacle = true;
        }

        if (e.character == 'g' && e.type == EventType.keyDown)
        {
            OnKeyGDown();
        }
  

            
        if (e.character == 'h' && m_oEditorGameObj != null && e.type == EventType.keyDown) 
        {
            OnKeyHDown();
            e.Use();
        } 
        SceneView.RepaintAll();
    }

    //生成键G按下
    void OnKeyGDown()
    {
        if (mb_IsHaveObStacle)
            return;
        GameObject obj = null;
        CreateObjType eType = CreateObjType.eInvalid;
        
        if (m_bShowFunNpc)
        {
            eType = CreateObjType.eFunNpc;
        }
        if(m_bShowNpc)
        {
            eType = CreateObjType.eMonster;
        }
        if(m_bShowObj)
        {
            eType = CreateObjType.eObj;
        }
        switch (m_enCreateMode)
        {
            case CreateMode.enObstcale:
                obj = AddPosToObstacle(mp_MousPos.x, mp_MousPos.y, mp_MousPos.z);
                break;
            case CreateMode.enScenePoint: //场景点(可能是随机刷怪点)
                Vector2 ScenePointPos = GetMapPosWith3DPos(mp_MousPos.x, mp_MousPos.y);
                //obj = AddScenePointToMap(ScenePointPos.x, ScenePointPos.y, -1);
                break;
            case CreateMode.enPathPoint://路径点
                //Vector2 pathPointPos = GetMapPosWith3DPos(mp_MousPos.x, mp_MousPos.y);
                //obj = AddPathPointToMap(pathPointPos.x, pathPointPos.y, -1);
                break;
            case CreateMode.enRegionObj://范围怪
                Vector2 regionNpcPos = GetMapPosWith3DPos(mp_MousPos.x, mp_MousPos.y);
                obj = AddReginNpc(regionNpcPos.x, regionNpcPos.y, -1, m_nChooseId, eType, 0);
                break;
            case CreateMode.enPointObj://定点怪
                Vector2 pointNpcPos = GetMapPosWith3DPos(mp_MousPos.x, mp_MousPos.y);
                obj = AddPointNpc(pointNpcPos.x, pointNpcPos.y, -1, m_nChooseId, eType, 0);
                break;
            
            case CreateMode.enPatrolPoint://npc巡逻点                
                Vector2 objPos = GetMapPosWith3DPos(mp_MousPos.x, mp_MousPos.y);
                if(Selection.gameObjects.Length == 1)
                {
                    BornInfo info = Selection.gameObjects[0].GetComponent<BornInfo>();
                    if(info == null)
                    {
                        return;
                    }
                    obj = AddNpcPatrolPoint(objPos.x, objPos.y, -1, Selection.gameObjects[0]);
                }
                
                break;                           
            default:
                break;
        }


        if (obj != null)
        {
            Undo.RegisterCreatedObjectUndo(obj, "InstantiatePrefabWithpos");
        }
    }

    //删除路径键H按下
    void OnKeyHDown()
    {
        GameObject Chooseobj = m_oEditorGameObj;
        BornInfo info = Chooseobj.GetComponent<BornInfo>();
        
    }

    //获取该位置是否有阻挡
    int GetHaveObstcle(float posx, float posy)
    {
        Vector2 mapPos = m_csPathGrid.GetMapPosByWordPos(posx, posy);

        if (!m_csPathGrid.IsMapPosValid((int)mapPos.x, (int)mapPos.y))
        {
            return -1;
        }

        if(m_csPathGrid.IsObstcale((int)mapPos.x, (int)mapPos.y))
        {
            return 1;
        }else
        {
            return 2;
        }
    }

    int GetHaveObstcleByMapPos(float posx, float posy)
    {
        if (!m_csPathGrid.IsMapPosValid((int)posx, (int)posy))
        {
            return -1;
        }

        if (m_csPathGrid.IsObstcale((int)posx, (int)posy))
        {
            return 1;
        }
        else
        {
            return 2;
        }
    }
    //点击当前选中怪物
    void OnClickChooseMonster()
    {
        //HACK: 预留接口
    }



    #endregion

    #region 创建prefab用
    //创建prefab
    GameObject InstantiatePrefabWithpos(float posx, float posy, float posz, ref GameObject prefab)
    {
        if((AllParent.transform.position.x - (int)(AllParent.transform.position.x)) != 0 || (AllParent.transform.position.x - (AllParent.transform.position.x)) != 0)
        {
            DialogReturn("零点错误","零点不为整数值,请修正谢谢!",true);
            return null;
        }
        if (GetHaveObstcleByMapPos(posx, posy) != 2) {
            Debug.LogError("怪物在阻挡中：" + posx + "," + posy);
            //return null;
        }
        

        //Vector2 v2 = GetMapPosWith3DPos(posx, posy);
        GameObject obj = Instantiate(prefab) as GameObject;
        Vector2 worldPos = GetWorldPosWithMapPos(posx, posy);
        obj.transform.position = new Vector3(worldPos.x, worldPos.y, posz);
        obj.transform.SetPositionAndRotation(new Vector3(worldPos.x, worldPos.y, posz), new Quaternion(0, 0, 0, 0));
        return obj;
    }

    //加入到阻挡
    public GameObject AddPosToObstacle(float posx, float posy, float posz)
    {
        Vector2 v2 = GetMapPosWith3DPos(posx, posy);
        m_csPathGrid.SetPosObstcale((int)v2.x, (int)v2.y, true);
        return null;
    }

    public void PosToDelObstacle(float posx, float posy, float posz)
    {
        Vector2 v2 = GetMapPosWith3DPos(posx, posy);
        m_csPathGrid.SetPosObstcale((int)v2.x, (int)v2.y, false);
    }


    public GameObject AddReginNpc(float posx, float posy, float posz, int id, CreateObjType eType, int rotation)
    {
        GameObject obj = InstantiatePrefabWithpos(posx, posy, posz, ref RegionBorn);
        obj.transform.SetParent(RegionParent);
        obj.name = "范围刷怪点";

        if(eType == CreateObjType.eFunNpc)
        {
            FunNpcInfo funnpcinfo;
            if (EditerDataClass._Instance.GetFunNpcInfo().TryGetValue(id, out funnpcinfo))
            {
                BornInfo bornInfo = obj.GetComponent<BornInfo>();
                if (bornInfo != null)
                {
                    bornInfo.m_eObjType = 1;
                    bornInfo.me_Type = CreateMode.enRegionObj;                    
                    bornInfo.mi_ObjId = id;
                    bornInfo.ms_Name = funnpcinfo.name;
                    bornInfo.rotation = rotation;
                }
            }
        }

        if (eType == CreateObjType.eMonster)
        {
            NpcInfo npcinfo;
            if (EditerDataClass._Instance.GetNpcInfo().TryGetValue(id, out npcinfo))
            {
                BornInfo bornInfo = obj.GetComponent<BornInfo>();
                if (bornInfo != null)
                {
                    bornInfo.m_eObjType = 0;
                    bornInfo.me_Type = CreateMode.enRegionObj;
                    bornInfo.mi_ObjId = id;
                    bornInfo.ms_Name = npcinfo.name;
                    bornInfo.rotation = rotation;
                }
            }
        }

        if (eType == CreateObjType.eObj)
        {
            ObjInfo objinfo;
            if (EditerDataClass._Instance.GetObjInfo().TryGetValue(id, out objinfo))
            {
                BornInfo bornInfo = obj.GetComponent<BornInfo>();
                if (bornInfo != null)
                {
                    bornInfo.m_eObjType = 2;
                    bornInfo.me_Type = CreateMode.enRegionObj;
                    bornInfo.mi_ObjId = id;
                    bornInfo.ms_Name = objinfo.name;
                    bornInfo.rotation = rotation;
                }
            }
        }

        BornInfo born = obj.GetComponent<BornInfo>();
        if (born != null)
        {
            RegionObjInfo info = new RegionObjInfo();
            info.Nums = 1;
            info.ObjId = id;
            info.Time = 0;
            info.Dir = 0;
            info.Per = 0;
            born.m_RegionNpcInfo.Clear();
            born.m_RegionNpcInfo.Add(info);
        }
        obj.transform.localScale = new Vector3(0.2f, 0.2f, 0.01f);
        return obj;
    }

    public GameObject AddPointNpc(float posx, float posy, float posz, int id , CreateObjType eType, int rotation)
    {
        GameObject obj = InstantiatePrefabWithpos(posx, posy, posz, ref PointBorn);
        obj.transform.SetParent(PointParent);
        obj.name = "定点刷怪点";

        if (eType == CreateObjType.eFunNpc)
        {
            FunNpcInfo funnpcinfo;
            if (EditerDataClass._Instance.GetFunNpcInfo().TryGetValue(id, out funnpcinfo))
            {
                BornInfo bornInfo = obj.GetComponent<BornInfo>();
                if (bornInfo != null)
                {
                    bornInfo.m_eObjType = 1;
                    bornInfo.me_Type = CreateMode.enPointObj;
                    bornInfo.mi_ObjId = id;
                    bornInfo.ms_Name = funnpcinfo.name;
                    bornInfo.rotation = rotation;
                }
            }
        }

        if (eType == CreateObjType.eMonster)
        {
            NpcInfo npcinfo;
            if (EditerDataClass._Instance.GetNpcInfo().TryGetValue(id, out npcinfo))
            {
                BornInfo bornInfo = obj.GetComponent<BornInfo>();
                if (bornInfo != null)
                {
                    bornInfo.m_eObjType = 0;
                    bornInfo.me_Type = CreateMode.enPointObj;
                    bornInfo.mi_ObjId = id;
                    bornInfo.ms_Name = npcinfo.name;
                    bornInfo.rotation = rotation;
                }
            }
        }

        if (eType == CreateObjType.eObj)
        {
            ObjInfo objinfo;
            if (EditerDataClass._Instance.GetObjInfo().TryGetValue(id, out objinfo))
            {
                BornInfo bornInfo = obj.GetComponent<BornInfo>();
                if (bornInfo != null)
                {
                    bornInfo.m_eObjType = 2;                    
                    bornInfo.me_Type = CreateMode.enPointObj;
                    bornInfo.mi_ObjId = id;
                    bornInfo.ms_Name = objinfo.name;
                    bornInfo.rotation = rotation;
                }
            }
        }


        obj.transform.localScale = new Vector3(0.2f, 0.2f, 0.01f);
        return obj;
    }

   
    public GameObject AddNpcPatrolPoint(float posx, float posy, float posz, GameObject obj)
    {
        BornInfo borninfo = obj.GetComponent<BornInfo>();
        if(borninfo == null)
        {
            return null;
        }
        if(!borninfo.CheckPatrolPoint((int)posx, (int)posy))
        {
            return null;
        }
        GameObject patrolpoint = InstantiatePrefabWithpos(posx, posy, posz, ref PatrolBorn);
        patrolpoint.transform.SetParent(obj.transform);
        patrolpoint.transform.localScale = new Vector3(0.4f, 0.4f, 0.01f);
        patrolpoint.name = "巡逻点";
        PatrolPointInfo info = new PatrolPointInfo();
        info.m_GridX = (int)posx;
        info.m_GridY = (int)posy;
        borninfo.m_PatrolInfo.Add(info);

        PatrolInfo patrol = patrolpoint.GetComponent<PatrolInfo>();
        if(patrol != null)
        {
            patrol.m_GridX = info.m_GridX;
            patrol.m_GridY = info.m_GridY;
        }
        return patrolpoint;
    }
    public GameObject AddScenePointToMap(float posx, float posy, float posz, int id = -1, int rotation = 0)
    {       
        return null;
    }

    public GameObject AddPathPointToMap(float posx, float posy, float posz, int id = -1, int rotation = 0)
    {       
        return null;
    }


    public GameObject AddPatrolPoint(GameObject parent , float posx, float posy, float posz)
    {
        BornInfo borninfo = parent.GetComponent<BornInfo>();

        return null;
    }

   
    #endregion

    public Transform GetRegionParent()
    {
        return RegionParent;
    }

    public Transform GetPointParent()
    {
        return PointParent;
    }
}