using UnityEngine;
using System.Collections;
using System;

public class Logo : MonoBehaviour {

    private float m_fAlphaRate;
    private GameObject m_goLogo;
    public GameObject m_goLogo1;
    public GameObject m_goLogo2;
    public GameObject m_goLauncher;

    // 初始化自身
    void Awake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.targetFrameRate = 30;       // 帧率调整

        m_fAlphaRate = 0.01f;
        m_goLogo = gameObject;
        /*
        m_fAlphaRate = 0.01f;
        m_goLogo = gameObject; //GameObject.Find("Canvas/Logo");
        m_goLogo1 = m_goLogo.transform.FindChild("ImgLogo1").gameObject; // GameObject.Find("Canvas /Logo/ImgLogo1");
        m_goLogo2 = m_goLogo.transform.FindChild("ImgLogo2").gameObject; // GameObject.Find("Canvas/Logo/ImgLogo2");
        m_goLauncher = GameObject.Find("Canvas/PreLoading");
        //*/
    }

    // Use this for initialization
    void Start()
    {
        PlayImgAni();
    }

    // 开始播放logo图片动画
    private void PlayImgAni()
    {
        m_goLogo.SetActive(true);
        m_goLogo1.SetActive(false);
        m_goLogo2.SetActive(false);
        m_goLauncher.SetActive(false);
         Action aShow = () =>
        {
            //InitLauncher();
            m_goLogo.SetActive(false);
            m_goLauncher.SetActive(true);
        };
        Action aLogo2 = () =>
        {
            StartCoroutine(ImgAni(m_goLogo2, aShow, 2));
        };

        StartCoroutine(ImgAni(m_goLogo1, aLogo2));
    }
    // 动画，图片渐隐渐现
    private IEnumerator ImgAni(GameObject go, Action act, int rate = 1)
    {
        if (go != null)
        {
            go.SetActive(true);
            UnityEngine.UI.Image img = go.GetComponent<UnityEngine.UI.Image>();
            img.color = new Color(1, 1, 1, 0);
            float fAlpha = 0.0f;
            while (fAlpha < 1.0f)
            {
                img.color = new Color(1, 1, 1, fAlpha + 2 * m_fAlphaRate * rate);
                fAlpha = img.color.a;
                yield return null;
            }
            img.color = new Color(1, 1, 1, 0);
            fAlpha = 1.0f;
            while (fAlpha > 0.0f)
            {
                img.color = new Color(1, 1, 1, fAlpha - m_fAlphaRate * rate);
                fAlpha = img.color.a;
                yield return null;
            }
            go.SetActive(false);
        }

        yield return null;
        if (act != null)
            act();
    }

    // 初始化 Launcher
    private void InitLauncher()
    {
        string name = "PreLoading";
        GameObject oLauncher = GameObject.Find(name);
        if (oLauncher == null)
        {
            GameObject prefab = Resources.Load(name) as GameObject;
            oLauncher = Instantiate(prefab) as GameObject;
            oLauncher.name = name;

            RectTransform rectransform = oLauncher.GetComponent<RectTransform>();
            if (rectransform != null)
            {
                rectransform.SetParent(m_goLogo.transform.parent);
                RectTransform prefabTrans = prefab.transform as RectTransform;
                rectransform.localScale = prefabTrans.localScale;
                rectransform.localPosition = prefabTrans.localPosition;
                rectransform.localRotation = prefabTrans.localRotation;
                rectransform.anchoredPosition = prefabTrans.anchoredPosition;
                rectransform.anchoredPosition3D = prefabTrans.anchoredPosition3D;
                rectransform.offsetMin = prefabTrans.offsetMin;
                rectransform.offsetMax = prefabTrans.offsetMax;
            }
        }

        // 如果GameManager没有初始化成功，就报错退出
        if (oLauncher == null)
        {
            Debug.Log("Error! oLauncher is null!");
            // 游戏退出
            ExitGame();
        }
    }

    // 退出游戏
    public static void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
		    Application.Quit();
#endif
    }
}
