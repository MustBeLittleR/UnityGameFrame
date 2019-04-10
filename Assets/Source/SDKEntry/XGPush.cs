#if ENTRY_XG

using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

namespace WCG
{
    public class XGPush
    {
#if UNITY_IPHONE
        [DllImport("__Internal")]
        private static extern void _IOSLocalPush(int num,int time,string info,int repType);
        [DllImport("__Internal")]
        private static extern void _IOSCancelAssignLocalPush(int num);
        [DllImport("__Internal")]
        private static extern void _XGRegisterDevice(string info);
        [DllImport("__Internal")]
        private static extern void _XGSetTag(string info);
        [DllImport("__Internal")]
        private static extern void _XGDelTag(string info);
#endif

        //=====================================信鸽相关服务器推送↓=================================================

#if UNITY_ANDROID
        //获取当前currentActivity
        static AndroidJavaClass GetCurrentActivity()
        {
            AndroidJavaClass jc = new AndroidJavaClass("com.wangch.Push.PushMessageReceiver");
            if (jc == null)
            {
                Debug.Log("when XGRegisterPush the AndroidJavaClass: com.wangch.Push.PushMessageReceiver is null");
                return null;
            }

            return jc;
        }
#endif

        //注册信鸽推送相关
        public static void RegisterXGPush(string account)
        {
#if UNITY_ANDROID
            AndroidJavaClass jc = GetCurrentActivity();
            if (jc == null)
                return;

            //调用方法  
            jc.CallStatic("RegisterXGPush", account);
#elif UNITY_IPHONE
            _XGRegisterDevice(account);
#endif
        }

        //设置标签
        public static void SetXGTag(string tagName)
        {
#if UNITY_ANDROID
            AndroidJavaClass jc = GetCurrentActivity();
            if (jc == null)
                return;

            //调用方法  
            jc.CallStatic("SetXGTag", tagName);
#elif UNITY_IPHONE
            _XGSetTag(tagName);
#endif
        }

        //删除标签
        public static void DeleteXGTag(string tagName)
        {
#if UNITY_ANDROID
            AndroidJavaClass jc = GetCurrentActivity();
            if (jc == null)
                return;

            //调用方法  
            jc.CallStatic("DeleteXGTag", tagName);
#elif UNITY_IPHONE
            _XGDelTag(tagName);
#endif
        }

        //================================================客户端原生推送↓========================================================

        //客户端发送推送该信息
        //num 标记
        //stime 定时多少秒之后推送
        //contexttext 推送的内容
        //ifAlltime == 1,一直推送
        //ifAlltime == 2,在游戏才会推送
        //ifAlltime == 3,不在游戏才会推送
        //repTime 重复推送时间间隔(秒)，0为不重复
        public static void ClientPush(int num,int stime, string contexttext, int repTime)
        {
#if UNITY_ANDROID
            AndroidJavaClass jc = GetCurrentActivity();
            if (jc == null)
                return;

            //调用方法  
            jc.CallStatic("RegistLocalNotificationAndroid", num, stime, contexttext, repTime);
#elif UNITY_IPHONE
            switch(repTime)
            {
                case 0:
                    _IOSLocalPush(num, stime, contexttext, 0);
                    break;
                case 24 * 3600:
                    _IOSLocalPush(num, stime, contexttext, 4);//按天循环
                    break;
            } 
#endif
        }

        public static void ClearPushInfo(int num)
        {
#if UNITY_ANDROID
            AndroidJavaClass jc = GetCurrentActivity();
            if (jc == null)
                return;

            //调用方法  
            jc.CallStatic("ClearAlarmmanager", num);
#elif UNITY_IPHONE
            _IOSCancelAssignLocalPush(num);
#endif
        }
    }
}
#endif