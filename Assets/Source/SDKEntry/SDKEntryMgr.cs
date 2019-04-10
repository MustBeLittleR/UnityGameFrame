#define ENTRY_SDK
using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using LuaInterface;

namespace WCG
{    
    public delegate void SDKApiCallBack(string apiName, string retJson);

    public class SDKEntryMgr
    {
        private static int msChannel = 0; // 渠道ID
        private static int msChanTag = 0; // 若是奥飞的融合SDK，会有一个二级的渠道ID；否则 msChanTag = msChannel
        private static SDKApiCallBack msSDKApiCB = null;  // SDKApi函数的回调
        //public static readonly string msUnityGoName = "SDKEntry";      // 接收SDK消息的GameObject名字
        //public static readonly string msUnitySDKApiFun = "SDKApiResult";   // 接收SDK消息的unity函数名
        private static LuaFunction m_sdkCallFun = null; 
        public static readonly string msJavaClass = "com.wangch.dgqb.SDKEntry";   // 接收Unity消息的Java类名

#if ENTRY_SDK && UNITY_IPHONE
        [DllImport("__Internal")]
        private static extern string _SDKApi(string apiName, string apiArgJson);
        [DllImport("__Internal")]
        private static extern int _InitEntry(string unityGoName, string unitySDKApiFun);
#elif ENTRY_SDK && UNITY_ANDROID
        private static string _SDKApi(string apiName, string apiArgJson)
        {
            AndroidJavaClass agent = new AndroidJavaClass(msJavaClass);
            return agent.CallStatic<string>("_SDKApi", apiName, apiArgJson);
        }
        private static int _InitEntry( string unityGoName, string unitySDKApiFun)
        {
            AndroidJavaClass agent = new AndroidJavaClass(msJavaClass);
            int nChannel = agent.CallStatic<int>("_InitEntry", unityGoName, unitySDKApiFun);
            return nChannel;
        }
              
#else
        private static string _SDKApi(string apiName, string apiArgJson)
        {
            return null;
        }
        private static int _InitEntry(string unityGoName, string unitySDKApiFun)
        {
            return 0;
        }
        private static void _InitSDK(string appid, string appkey)
        {

        }

#endif

        // 获取渠道ID
        public static int GetChannel()
        {
            return msChannel;
        }

        // 奥飞的融合SDK，会有一个二级的渠道ID；否则 msChanTag = msChannel
        public static int GetChanTag()
        {
            return msChanTag;
        }

        // 初始化Entry，并非初始化SDK，将unity的响应 对象以及函数 传给SDK，并从SDK中获取渠道ID
        public static int InitEntry()
        {
            //Debug.Log("InitEntry");
            msChannel = ExtInfo.GetChannel();
            msChanTag = msChannel;
            Debug.Log("InitEntry:" + msChannel);
            // 非编辑器模式且是真机模式下才能进行
            if (msChannel != 0 && Application.platform != RuntimePlatform.OSXEditor && Application.platform != RuntimePlatform.WindowsEditor)
            {
                // SDKEntry
                msChanTag = _InitEntry(SDKEntry.msUnityGoName, SDKEntry.msUnitySDKApiFun);
                msSDKApiCB = (apiName, retJson) =>
                {
                    m_sdkCallFun.LazyCall(apiName, retJson);                                        
                };
            }

            return msChannel;
        }
        

        // 调用SDK的Api
        public static string SDKApi(string apiName, string apiArgJson = null)
        {
            if (msChannel == 0)
            {
                Debug.Log("SDKApi: msChannel = 0!");
                return null;
            }
            Debug.Log("SDKApi: " + apiName + " -> " + apiArgJson);
            if (string.IsNullOrEmpty(apiName))
            {
                Debug.LogError("SDKApi: apiName is null!");
                return null;
            }

            // 非编辑器模式下，就是真机模式下才能进行
            if (Application.platform != RuntimePlatform.OSXEditor && Application.platform != RuntimePlatform.WindowsEditor)
            {
                if (string.IsNullOrEmpty(apiArgJson))
                    apiArgJson = "{}";
                return _SDKApi(apiName, apiArgJson);
            }
            return null;
        }

        // SDK的Api执行完毕后的回调
        public static void OnSDKApi(string apiName, string retJson)
        {
            if (msChannel == 0)
            {
                Debug.Log("OnSDKApi: msChannel = 0!");
                return;
            }
            Debug.Log("OnSDKApi: " + apiName + " -> " + retJson);
            if (msSDKApiCB == null)
            {
                Debug.LogError("OnSDKApi: msApiListener is null");
                return;
            }
            if (string.IsNullOrEmpty(apiName))
            {
                Debug.LogError("OnSDKApi: apiName is null!");
                return;
            }

            // 非编辑器模式下，就是真机模式下才能进行
            if (Application.platform != RuntimePlatform.OSXEditor && Application.platform != RuntimePlatform.WindowsEditor)
            {
                if (string.IsNullOrEmpty(retJson))
                    retJson = "{}";
                msSDKApiCB(apiName, retJson);
            }
        }

        // 
        public static void Start()
        {
            InitEntry();
        }

        // 
        public static void Update()
        {
            if(m_sdkCallFun == null)
            {
                m_sdkCallFun = GameMgr.ms_luaMgr.GetLuaFunction("SDKEntryMgr.OnSDKApi");
            }
        }

    }
}
