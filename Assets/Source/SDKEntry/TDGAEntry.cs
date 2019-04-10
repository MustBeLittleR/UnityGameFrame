// SDK Version: 3.1.50

#if ENTRY_TDGA

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using System.ComponentModel;
using System.Threading;

namespace WCG
{
    public class TalkingDataGA
    {
#if UNITY_IPHONE
	    [DllImport ("__Internal")]
	    private static extern void _tdgaOnStart(string appId, string channelId);
	
	    [DllImport ("__Internal")]
	    private static extern void _tdgaOnEvent(string eventId, string []keys, string []stringValues, double []intValues, int count);
	
	    [DllImport ("__Internal")]
	    private static extern string _tdgaGetDeviceId();
	
	    [DllImport ("__Internal")]
	    private static extern void _tdgaSetSdkType(int type);
	
	    [DllImport ("__Internal")]
	    private static extern void _tdgaSetVerboseLogDisabled();

	    [DllImport ("__Internal")]
	    private static extern void _tdgaSetDeviceToken(string deviceToken);

	    [DllImport ("__Internal")]
	    private static extern void _tdgaHandlePushMessage(string message);

	    private static bool hasTokenBeenObtained = false;
#elif UNITY_ANDROID
        //init static class --save memory/space
        private static AndroidJavaClass agent;
	    private static AndroidJavaClass unityClass;
	
	    private static string JAVA_CLASS = "com.tendcloud.tenddata.TalkingDataGA";
	    private static string UNTIFY_CLASS = "com.unity3d.player.UnityPlayer";

	    public static void AttachCurrentThread() {
		    AndroidJNI.AttachCurrentThread();
	    }
	
	    public static void DetachCurrentThread() {
		    AndroidJNI.DetachCurrentThread();
	    }
#endif

        public static string GetDeviceId()
        {
            //if the platform is real device
            string ret = null;
            if (Application.platform != RuntimePlatform.OSXEditor && Application.platform != RuntimePlatform.WindowsEditor)
            {
#if UNITY_IPHONE
			ret = _tdgaGetDeviceId();
#elif UNITY_ANDROID
			if (agent != null) {
				AndroidJavaObject activity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
				ret = agent.CallStatic<string>("getDeviceId", activity);
			}
#elif UNITY_WP8
			ret = TalkingDataGAWP.TalkingDataGA.getDeviceID();
#endif
            }
            return ret;
        }

        public static void OnStart(string appID, string channelId)
        {
            //if the platform is real device
            if (Application.platform != RuntimePlatform.OSXEditor && Application.platform != RuntimePlatform.WindowsEditor)
            {
#if UNITY_IPHONE
			_tdgaSetSdkType(2);
			_tdgaOnStart(appID, channelId);
#elif UNITY_ANDROID
			if (agent == null) {
				agent = new AndroidJavaClass(JAVA_CLASS);
			}
			agent.SetStatic<int>("sPlatformType", 2);
			unityClass = new AndroidJavaClass(UNTIFY_CLASS);
			AndroidJavaObject activity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
			agent.CallStatic("init", activity, appID, channelId);
			agent.CallStatic("onResume", activity);
#elif UNITY_WP8
			TalkingDataGAWP.TalkingDataGA.init(appID, channelId);
#endif
            }
        }

        public static void OnEnd()
        {
            if (Application.platform != RuntimePlatform.OSXEditor && Application.platform != RuntimePlatform.WindowsEditor)
            {
#if UNITY_ANDROID
			if (agent != null) {
				AndroidJavaObject activity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
				agent.CallStatic("onPause", activity);
				agent = null;
				unityClass = null;
			}
#endif
            }
        }

        public static void OnKill()
        {
            if (Application.platform != RuntimePlatform.OSXEditor && Application.platform != RuntimePlatform.WindowsEditor)
            {
#if UNITY_ANDROID
			if (agent != null) {
				AndroidJavaObject activity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
				agent.CallStatic("onKill", activity);
				agent = null;
				unityClass = null;
			}
#endif
            }
        }

        public static void OnEvent(string actionId, Dictionary<string, object> parameters)
        {
            if (Application.platform != RuntimePlatform.OSXEditor && Application.platform != RuntimePlatform.WindowsEditor)
            {
                if (parameters != null && parameters.Count > 0 && parameters.Count <= 10)
                {
#if UNITY_IPHONE
				int count = parameters.Count;
				string []keys = new string[count];
				string []stringValues = new string[count];
				double []numberValues = new double[count];
				int index = 0;
				foreach (KeyValuePair<string, object> kvp in parameters) {
					if (kvp.Value is string) {
						keys[index] = kvp.Key;
						stringValues[index] = (string)kvp.Value;
					} else {
						try {
							double tmp = System.Convert.ToDouble(kvp.Value);
							numberValues[index] = tmp;
							keys[index] = kvp.Key;
						} catch(System.Exception) {
							count--;
							continue;
						}
					}
					index++;
				}
				
				_tdgaOnEvent(actionId, keys, stringValues, numberValues, count);
#elif UNITY_ANDROID
				int count = parameters.Count;
				AndroidJavaObject map = new AndroidJavaObject("java.util.HashMap", count);
				IntPtr method_Put = AndroidJNIHelper.GetMethodID(map.GetRawClass(), 
						"put", "(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;");
				object[] args = new object[2];
				foreach (KeyValuePair<string, object> kvp in parameters) {
					args[0] = new AndroidJavaObject("java.lang.String", kvp.Key);
					if (typeof(System.String).IsInstanceOfType(kvp.Value)) {
						args[1] = new AndroidJavaObject("java.lang.String", kvp.Value);
					} else {
						args[1] = new AndroidJavaObject("java.lang.Double", ""+kvp.Value);
					}
					AndroidJNI.CallObjectMethod(map.GetRawObject(), method_Put, AndroidJNIHelper.CreateJNIArgArray(args));
				}
				
				if (agent != null) {
					AndroidJavaObject activity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
					agent.CallStatic("onEvent", activity, actionId, map);
				}
#elif UNITY_WP8
				TalkingDataGAWP.TalkingDataGA.onEvent(actionId, parameters);
#endif
                }
            }
        }

#if UNITY_IPHONE
	public static void SetDeviceToken() {
		if (!hasTokenBeenObtained) {
			byte[] byteToken = NotificationServices.deviceToken;
			if(byteToken != null) {
				string deviceToken = System.BitConverter.ToString(byteToken).Replace("-","");
				_tdgaSetDeviceToken(deviceToken);
				hasTokenBeenObtained = true;
			}
		}
	}

	public static void HandleTDGAPushMessage() {
		RemoteNotification[] notifications = NotificationServices.remoteNotifications;
		if (notifications != null) {
			NotificationServices.ClearRemoteNotifications();
			foreach (RemoteNotification rn in notifications) {
				foreach (DictionaryEntry de in rn.userInfo) {
					if (de.Key.ToString().Equals("sign")) {
						string sign = de.Value.ToString();
						_tdgaHandlePushMessage(sign);
					}
				}
			}
		}
	}
#endif

        public static void SetVerboseLogDisabled()
        {
            if (Application.platform != RuntimePlatform.OSXEditor && Application.platform != RuntimePlatform.WindowsEditor)
            {
#if UNITY_IPHONE
			_tdgaSetVerboseLogDisabled();
#elif UNITY_ANDROID
			if (agent == null) {
				agent = new AndroidJavaClass(JAVA_CLASS);
			}
			agent.CallStatic("setVerboseLogDisabled");
#elif UNITY_WP8
#endif
            }
        }
    }

    public enum Gender
    {
        UNKNOW = 0,
        MALE = 1,
        FEMALE = 2
    }

    public enum AccountType
    {
        ANONYMOUS = 0,
        REGISTERED = 1,
        SINA_WEIBO = 2,
        QQ = 3,
        QQ_WEIBO = 4,
        ND91 = 5,
        TYPE1 = 11,
        TYPE2 = 12,
        TYPE3 = 13,
        TYPE4 = 14,
        TYPE5 = 15,
        TYPE6 = 16,
        TYPE7 = 17,
        TYPE8 = 18,
        TYPE9 = 19,
        TYPE10 = 20
    }

    public class TDGAAccount
    {
#if UNITY_IPHONE
	[DllImport ("__Internal")]
	private static extern void _tdgaSetAccount(string accountId);
	
	[DllImport ("__Internal")]   
	private static extern void _tdgaSetAccountName(string accountName);
	
	[DllImport ("__Internal")]   
	private static extern void _tdgaSetAccountType(int accountType);
	
	[DllImport ("__Internal")]   
	private static extern void _tdgaSetLevel(int level) ;
	
	[DllImport ("__Internal")]   
	private static extern void _tdgaSetGender(int gender);
	
	[DllImport ("__Internal")]   
	private static extern void _tdgaSetAge(int age);
	
	[DllImport ("__Internal")]   
	private static extern void _tdgaSetGameServer(string ameServer);
#elif UNITY_ANDROID
	//init static class --save memory/space
	private static string JAVA_CLASS = "com.tendcloud.tenddata.TDGAAccount";
	static AndroidJavaClass agent = new AndroidJavaClass(JAVA_CLASS);
	
	private AndroidJavaObject mAccount;
	public void setAccountObject(AndroidJavaObject account) {
		mAccount = account;
	}
#elif UNITY_WP8
	private TalkingDataGAWP.TDGAAccount mAccount;
	public void setAccountObject(TalkingDataGAWP.TDGAAccount account) {
		mAccount = account;
	}
#endif

        public static TDGAAccount SetAccount(string accountId)
        {
            TDGAAccount account = new TDGAAccount();
            //if the platform is real device
            if (Application.platform != RuntimePlatform.OSXEditor && Application.platform != RuntimePlatform.WindowsEditor)
            {
#if UNITY_IPHONE
			_tdgaSetAccount(accountId);
#elif UNITY_ANDROID
			AndroidJavaObject jobj = agent.CallStatic<AndroidJavaObject>("setAccount", accountId);
			account.setAccountObject(jobj);
#elif UNITY_WP8
			TalkingDataGAWP.TDGAAccount csObj = TalkingDataGAWP.TDGAAccount.setAccount(accountId);
			account.setAccountObject(csObj);
#endif
            }

            return account;
        }

        public TDGAAccount()
        {
        }

        public void SetAccountName(string accountName)
        {
            //if the platform is real device
            if (Application.platform != RuntimePlatform.OSXEditor && Application.platform != RuntimePlatform.WindowsEditor)
            {
#if UNITY_IPHONE
			_tdgaSetAccountName(accountName);
#elif UNITY_ANDROID
			if (mAccount != null) {
				mAccount.Call("setAccountName", accountName);
			}
#elif UNITY_WP8
			if (mAccount != null) {
				mAccount.setAccountName(accountName);
			}
#endif
            }
        }

        public void SetAccountType(AccountType type)
        {
            //if the platform is real device
            if (Application.platform != RuntimePlatform.OSXEditor && Application.platform != RuntimePlatform.WindowsEditor)
            {
#if UNITY_IPHONE
			_tdgaSetAccountType((int)type);
#elif UNITY_ANDROID
			if (mAccount != null) {
				AndroidJavaClass enumClass = new AndroidJavaClass("com.tendcloud.tenddata.TDGAAccount$AccountType");
				AndroidJavaObject obj = enumClass.CallStatic<AndroidJavaObject>("valueOf", type.ToString());
				mAccount.Call("setAccountType", obj);
			}
#elif UNITY_WP8
			if (mAccount != null) {
				mAccount.setAccountType((TalkingDataGAWP.TDGAAccount.AccountType)type);
			}
#endif
            }
        }

        public void SetLevel(int level)
        {
            //if the platform is real device
            if (Application.platform != RuntimePlatform.OSXEditor && Application.platform != RuntimePlatform.WindowsEditor)
            {
#if UNITY_IPHONE
			_tdgaSetLevel(level);
#elif UNITY_ANDROID
			if (mAccount != null) {
				mAccount.Call("setLevel", level);
			}
#elif UNITY_WP8
			if (mAccount != null) {
				mAccount.setLevel(level);
			}
#endif
            }
        }

        public void SetAge(int age)
        {
            //if the platform is real device
            if (Application.platform != RuntimePlatform.OSXEditor && Application.platform != RuntimePlatform.WindowsEditor)
            {
#if UNITY_IPHONE
			_tdgaSetAge(age);
#elif UNITY_ANDROID
			if (mAccount != null) {
				mAccount.Call("setAge", age);
			}
#elif UNITY_WP8
			if (mAccount != null) {
				mAccount.setAge(age);
			}
#endif
            }
        }

        public void SetGender(Gender type)
        {
            //if the platform is real device
            if (Application.platform != RuntimePlatform.OSXEditor && Application.platform != RuntimePlatform.WindowsEditor)
            {
#if UNITY_IPHONE
			_tdgaSetGender((int)type);
#elif UNITY_ANDROID
			if (mAccount != null) {
				AndroidJavaClass enumClass = new AndroidJavaClass("com.tendcloud.tenddata.TDGAAccount$Gender");
				AndroidJavaObject obj = enumClass.CallStatic<AndroidJavaObject>("valueOf", type.ToString());
				mAccount.Call("setGender", obj);
			}
#elif UNITY_WP8
			if (mAccount != null) {
				mAccount.setGender((TalkingDataGAWP.TDGAAccount.Gender)type);
			}
#endif
            }
        }

        public void SetGameServer(string gameServer)
        {
            //if the platform is real device
            if (Application.platform != RuntimePlatform.OSXEditor && Application.platform != RuntimePlatform.WindowsEditor)
            {
#if UNITY_IPHONE
			_tdgaSetGameServer(gameServer);
#elif UNITY_ANDROID
			if (mAccount != null) {
				mAccount.Call("setGameServer", gameServer);
			}
#elif UNITY_WP8
			if (mAccount != null) {
				mAccount.setGameServer(gameServer);
			}
#endif
            }
        }
    }

    public class TDGAItem
    {
#if UNITY_IPHONE
	[DllImport ("__Internal")]
	private static extern void _tdgaOnPurchase(string item, int itemNumber, double priceInVirtualCurrency);
	
	[DllImport ("__Internal")]
	private static extern void _tdgaOnUse(string item, int itemNumber);
#elif UNITY_ANDROID
	private static string JAVA_CLASS = "com.tendcloud.tenddata.TDGAItem";
	static AndroidJavaClass agent = new AndroidJavaClass(JAVA_CLASS);
#endif

        public static void OnPurchase(string item, int itemNumber, double priceInVirtualCurrency)
        {
            //if the platform is real device
            if (Application.platform != RuntimePlatform.OSXEditor && Application.platform != RuntimePlatform.WindowsEditor)
            {
#if UNITY_IPHONE
			_tdgaOnPurchase(item, itemNumber, priceInVirtualCurrency);
#elif UNITY_ANDROID
			agent.CallStatic("onPurchase", item, itemNumber, priceInVirtualCurrency);
#elif UNITY_WP8
			TalkingDataGAWP.TDGAItem.onPurchase(item, itemNumber, priceInVirtualCurrency);
#endif
            }
        }

        public static void OnUse(string item, int itemNumber)
        {
            //if the platform is real device
            if (Application.platform != RuntimePlatform.OSXEditor && Application.platform != RuntimePlatform.WindowsEditor)
            {
#if UNITY_IPHONE
			_tdgaOnUse(item, itemNumber);
#elif UNITY_ANDROID
			agent.CallStatic("onUse", item, itemNumber);
#elif UNITY_WP8
			TalkingDataGAWP.TDGAItem.onUse(item, itemNumber);
#endif
            }
        }
    }

    public class TDGAMission
    {
#if UNITY_IPHONE
	[DllImport ("__Internal")]
	private static extern void _tdgaOnBegin(string missionId);
	
	[DllImport ("__Internal")]
	private static extern void _tdgaOnCompleted(string missionId);
	
	[DllImport ("__Internal")]
	private static extern void _tdgaOnFailed(string missionId, string failedCause);
#elif UNITY_ANDROID
	private static string JAVA_CLASS = "com.tendcloud.tenddata.TDGAMission";
	static AndroidJavaClass agent = new AndroidJavaClass(JAVA_CLASS);
#endif

        public static void OnBegin(string missionId)
        {
            //if the platform is real device
            if (Application.platform != RuntimePlatform.OSXEditor && Application.platform != RuntimePlatform.WindowsEditor)
            {
#if UNITY_IPHONE
			_tdgaOnBegin(missionId);
#elif UNITY_ANDROID
			agent.CallStatic("onBegin", missionId);
#elif UNITY_WP8
			TalkingDataGAWP.TDGAMission.onBegin(missionId);
#endif
            }
        }

        public static void OnCompleted(string missionId)
        {
            //if the platform is real device
            if (Application.platform != RuntimePlatform.OSXEditor && Application.platform != RuntimePlatform.WindowsEditor)
            {
#if UNITY_IPHONE
			_tdgaOnCompleted(missionId);
#elif UNITY_ANDROID
			agent.CallStatic("onCompleted", missionId);
#elif UNITY_WP8
			TalkingDataGAWP.TDGAMission.onCompleted(missionId);
#endif
            }
        }

        public static void OnFailed(string missionId, string failedCause)
        {
            //if the platform is real device
            if (Application.platform != RuntimePlatform.OSXEditor && Application.platform != RuntimePlatform.WindowsEditor)
            {
#if UNITY_IPHONE
			_tdgaOnFailed(missionId, failedCause);
#elif UNITY_ANDROID
			agent.CallStatic("onFailed", missionId, failedCause);
#elif UNITY_WP8
			TalkingDataGAWP.TDGAMission.onFailed(missionId, failedCause);
#endif
            }
        }
    }

    public class TDGAVirtualCurrency
    {
#if UNITY_IPHONE
	[DllImport ("__Internal")]
	private static extern void _tdgaOnChargeRequst(string orderId, string iapId, double currencyAmount, 
			string currencyType, double virtualCurrencyAmount, string paymentType);
	
	[DllImport ("__Internal")]
	private static extern void _tdgaOnChargSuccess(string orderId);
	
	[DllImport ("__Internal")]
	private static extern void _tdgaOnReward(double virtualCurrencyAmount, string reason);
#elif UNITY_ANDROID
	private static string JAVA_CLASS = "com.tendcloud.tenddata.TDGAVirtualCurrency";
	static AndroidJavaClass agent = new AndroidJavaClass(JAVA_CLASS);
#endif

        public static void OnChargeRequest(string orderId, string iapId, double currencyAmount,
                string currencyType, double virtualCurrencyAmount, string paymentType)
        {
            //if the platform is real device
            if (Application.platform != RuntimePlatform.OSXEditor && Application.platform != RuntimePlatform.WindowsEditor)
            {
#if UNITY_IPHONE
			_tdgaOnChargeRequst(orderId, iapId, currencyAmount, currencyType, virtualCurrencyAmount, paymentType);
#elif UNITY_ANDROID
			agent.CallStatic("onChargeRequest", orderId, iapId, currencyAmount, 
					currencyType, virtualCurrencyAmount, paymentType);
#elif UNITY_WP8
			TalkingDataGAWP.TDGAVirtualCurrency.onChargeRequest(orderId, iapId, currencyAmount, 
					currencyType, virtualCurrencyAmount, paymentType);
#endif
            }
        }

        public static void OnChargeSuccess(string orderId)
        {
            //if the platform is real device
            if (Application.platform != RuntimePlatform.OSXEditor && Application.platform != RuntimePlatform.WindowsEditor)
            {
#if UNITY_IPHONE
			_tdgaOnChargSuccess(orderId);
#elif UNITY_ANDROID
			agent.CallStatic("onChargeSuccess", orderId);
#elif UNITY_WP8
			TalkingDataGAWP.TDGAVirtualCurrency.onChargeSuccess(orderId);
#endif
            }
        }

        public static void OnReward(double virtualCurrencyAmount, string reason)
        {
            //if the platform is real device
            if (Application.platform != RuntimePlatform.OSXEditor && Application.platform != RuntimePlatform.WindowsEditor)
            {
#if UNITY_IPHONE
			_tdgaOnReward(virtualCurrencyAmount, reason);
#elif UNITY_ANDROID
			agent.CallStatic("onReward", virtualCurrencyAmount, reason);
#elif UNITY_WP8
			TalkingDataGAWP.TDGAVirtualCurrency.onReward(virtualCurrencyAmount, reason);
#endif
            }
        }
    }

    public class TDGAEntry
    {
        private static bool bInit = false;
        // TalkingData相关
        //private static string msAppName4TDGA = "nfxy";
        //private static string msAppID4TDGA = "D8257ADF848C0AD10A16AD6435E88EFE";
        //private static string msAppName4TDGA = "妖王孙悟空";
        //private static string msAppID4TDGA = "5E97B96350AD1B7DA56AE61713F1303C";
        //private static string msAppName4TDGA = "斗战三界"; 
        private static string msAppID4TDGA = "9CBCD364F16427823DB6E543400A70E7";

        // 当前账号
        private static TDGAAccount msAccount = null;

        // TDGA
        public static void AttachCurrentThread()
        {
#if UNITY_ANDROID
            if (bInit)
                TalkingDataGA.AttachCurrentThread();
#endif
        }
        public static void DetachCurrentThread()
        {
#if UNITY_ANDROID
            if (bInit)
                TalkingDataGA.DetachCurrentThread();
#endif
        }
        public static string GetDeviceId()
        {
            string id = "";
            if (bInit)
                id = TalkingDataGA.GetDeviceId();
            return id;
        }
        public static bool isInit()
        {
            return bInit;
        }
        public static void OnStart()
        {
            string appID = msAppID4TDGA;
            string channelID = "" + SDKEntryMgr.GetChannel();
            TalkingDataGA.OnStart(appID, channelID);
            bInit = true;
        }
        public static void OnUpdate()
        {
#if UNITY_IPHONE
        if (bInit)
        {
		    TalkingDataGA.SetDeviceToken();
		    TalkingDataGA.HandleTDGAPushMessage();
        }
#endif
        }
        public static void OnDestroy()
        {
            if (bInit)
                TalkingDataGA.OnEnd();
        }
        public static void OnKill()
        {
            if (bInit)
                TalkingDataGA.OnKill();
            bInit = false;
        }
        public static void OnEvent(string actionId, Dictionary<string, object> parameters)
        {
            if (bInit)
                TalkingDataGA.OnEvent(actionId, parameters);
        }
        public static void SetVerboseLogDisabled()
        {
            if (bInit)
                TalkingDataGA.SetVerboseLogDisabled();
        }

	    public static void SetDeviceToken()
        {
#if UNITY_IPHONE
            if (bInit)
                TalkingDataGA.SetDeviceToken();
#endif
        }

        public static void HandleTDGAPushMessage()
        {
#if UNITY_IPHONE
            if (bInit)
                TalkingDataGA.HandleTDGAPushMessage();
#endif
        }

        // 账号
        public static void SetAccount(string accountId)
        {
            if (bInit)
                msAccount = TDGAAccount.SetAccount(accountId);
        }
        public static void SetAccountName(string accountName)
        {
            if (bInit && msAccount != null)
                msAccount.SetAccountName(accountName);
        }
        public static void SetAccountType(AccountType type)
        {
            if (bInit && msAccount != null)
                msAccount.SetAccountType(type);
        }
        public static void SetLevel(int level)
        {
            if (bInit && msAccount != null)
                msAccount.SetLevel(level);
        }
        public static void SetAge(int age)
        {
            if (bInit && msAccount != null)
                msAccount.SetAge(age);
        }
        public static void SetGender(Gender type)
        {
            if (bInit && msAccount != null)
                msAccount.SetGender(type);
        }
        public static void SetGameServer(string gameServer)
        {
            if (bInit && msAccount != null)
                msAccount.SetGameServer(gameServer);
        }


        // 虚拟币
        public static void OnChargeRequest(string orderId, string iapId, double currencyAmount,
                string currencyType, double virtualCurrencyAmount, string paymentType)
        {
            if (bInit)
                TDGAVirtualCurrency.OnChargeRequest(orderId, iapId, currencyAmount, currencyType, virtualCurrencyAmount, paymentType);
        }
        public static void OnChargeSuccess(string orderId)
        {
            if (bInit)
                TDGAVirtualCurrency.OnChargeSuccess(orderId);
        }
        public static void OnReward(double virtualCurrencyAmount, string reason)
        {
            if (bInit)
                TDGAVirtualCurrency.OnReward(virtualCurrencyAmount, reason);
        }


        // 消费点
        public static void OnPurchase(string item, int itemNumber, double priceInVirtualCurrency)
        {
            if (bInit)
                TDGAItem.OnPurchase(item, itemNumber, priceInVirtualCurrency);
        }
        public static void OnUse(string item, int itemNumber)
        {
            if (bInit)
                TDGAItem.OnUse(item, itemNumber);
        }


        // 任务关卡、副本
        public static void OnMissionBegin(string missionId)
        {
            if (bInit)
                TDGAMission.OnBegin(missionId);
        }
        public static void OnMissionCompleted(string missionId)
        {
            if (bInit)
                TDGAMission.OnCompleted(missionId);
        }
        public static void OnMissionFailed(string missionId, string failedCause)
        {
            if (bInit)
                TDGAMission.OnFailed(missionId, failedCause);
        }
    }
}
#endif
