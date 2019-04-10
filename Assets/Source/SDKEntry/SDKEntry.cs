using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using LitJson;

namespace WCG
{
    public class SDKEntry : MonoBehaviour
    {
        public static readonly string msUnityGoName = "SDKEntry";      // 接收SDK消息的GameObject名字
        public static readonly string msUnitySDKApiFun = "SDKApiResult";   // 接收SDK消息的unity函数名

        // 
        void Awake()
        {
            gameObject.name = msUnityGoName;            
        }

        // Use this for initialization
        void Start()
        {
            SDKEntryMgr.Start();
        }

        // Update is called once per frame
        void Update()
        {
            SDKEntryMgr.Update();
        }
        
        // SDK的Api执行完毕后将消息发送到此函数
        private void SDKApiResult(string result)
        {
            // 处理SDK的回调
            JsonData jd = JsonMapper.ToObject(result);
            string apiName = jd["api"].ToString();
            string retJson = jd["ret"].ToString();
            SDKEntryMgr.OnSDKApi(apiName, retJson);
        }
    }
}
