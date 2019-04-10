using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using LuaInterface;
using System.Text;
using System.Threading;
using System.Net;
namespace WCG
{
	public enum EMsgState
	{
		eOk = 0,
		eNetErr,
		eOnConnected,
		eOnConnectFailed,
		eOnDisConnected,
	};

	public static class CallServer
    {
		public static LuaFunction m_OnLuaRecv = null;     //二级协议调lua
		private static LuaFunction m_OnLuaRecvCs = null;   //C++二级协调lua
		private static LuaFunction m_OnRecvStateMsg = null;   //捕获状态处理

		private static byte[] m_bufSendData = null;
		private static string m_sIp;
		private static int m_nPort;
        private static string m_sOpenId;
        private static string m_sToken;        
		private static Timer m_timer = null;

		static CallServer()
		{
			m_bufSendData = new byte[256];
			m_sIp = "";
			m_nPort = 0;            
		}        

		//logic协议注册
		public static void AddPackRpc(UInt32 uID, string szName, string szParamInfo)
		{
			Dispatcher.RegistPackProtocols(true, uID, szName, szParamInfo);
		}
        public static void AddUnPackRpc(UInt32 uID, string szName, string szParamInfo)
        {
			Dispatcher.RegistUnPackProtocols(true, uID, szName, szParamInfo);
        }

		//Clt2Cns协议注册
		public static void AddClt2CnsPackRpc(UInt32 uID, string szName, string szParamInfo)
		{
			Dispatcher.RegistPackProtocols(false, uID, szName, szParamInfo);
		}
		public static void AddCns2CltUnPackRpc(UInt32 uID, string szName, string szParamInfo)
		{
			Dispatcher.RegistUnPackProtocols(false, uID, szName, szParamInfo);
		}

		//保证 Lua All Require 完之后调用
		public static void InitLuaFunc() {
			if (null == m_OnLuaRecv)
				m_OnLuaRecv = GameMgr.ms_luaMgr.GetLuaFunction ("gCallServer.RpcRecv");
			/*if (null == m_OnLuaRecvCs)
				m_OnLuaRecvCs = GameMgr.ms_luaMgr.GetLuaFunction("gCallServer.CsRpcRecv");*/
			if (null == m_OnRecvStateMsg)
				m_OnRecvStateMsg = GameMgr.ms_luaMgr.GetLuaFunction("gCallServer.OnRecvStateMsg");
			if (string.IsNullOrEmpty(m_sIp) && !string.IsNullOrEmpty(GameConfigMgr.ms_sGasPort))
			{
				m_sIp = GameConfigMgr.ms_sGasIp;
				m_nPort = Convert.ToInt32(GameConfigMgr.ms_sGasPort);
			}
		}

		public static void Connect2Server(string ip, int port, string openId, string token)
		{            
            CallServer.InitLuaFunc();
			if (!string.IsNullOrEmpty(ip))
			{
				m_sIp = ip;
				m_nPort = port;
			}
            
            //IPHostEntry hostInfo = Dns.GetHostEntry(m_sIp);
            //IPAddress ipAddress = hostInfo.AddressList[0];
            //m_sIp = ipAddress.ToString();            

            m_sOpenId = openId;
            m_sToken = token;
            
            Debug.LogFormat("Connect to Server:{0},{1}", m_sIp, m_nPort);
            ConnectionMgr.instance.Connect(m_sIp, m_nPort, new AsyncCallback(OnConnected),
			   new AsyncCallback(OnConnectFailed), new AsyncCallback(OnDisConnected));
		}
        private static C2CS_GameUserLogin GenerateStLogin()
        {            
            C2CS_GameUserLogin gul = new C2CS_GameUserLogin();
            gul.bCLLogin = true;
            gul.uRcpId = 10;
            gul.nRcpCode = 10;
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
            int timeStamp = (int)(DateTime.Now - startTime).TotalSeconds; // 相差秒数
            gul.uRcpTime = timeStamp;
            gul.nUserId = 10;
            gul.uChannelId = GameMgr.Instance.GetChannel();
            gul.szPassword = new Byte[64];
            Array.Clear(gul.szPassword, 0, 64);
            byte[] bytes = Encoding.Default.GetBytes(m_sToken);
            System.Buffer.BlockCopy(bytes, 0, gul.szPassword, 0, bytes.Length);

            gul.szIp = new Byte[16];
            Array.Clear(gul.szIp, 0, 16);
            bytes = Encoding.Default.GetBytes("127.0.0.1");
            System.Buffer.BlockCopy(bytes, 0, gul.szIp, 0, bytes.Length);

            gul.uMachineId = 3;
            gul.uMachineIdHash = 10;
            gul.nVersionLength = 16;
            gul.uPassportLength = 128;

            gul.sVersion = new Byte[16];
            Array.Clear(gul.sVersion, 0, 16);
            string sversion = GameMgr.Instance.GetGameVersion();
            bytes = Encoding.Default.GetBytes(sversion);
            System.Buffer.BlockCopy(bytes, 0, gul.sVersion, 0, bytes.Length);

            gul.sPassport = new Byte[128];
            Array.Clear(gul.sPassport, 0, 128);
            
            bytes = Encoding.Default.GetBytes(m_sOpenId);
            System.Buffer.BlockCopy(bytes, 0, gul.sPassport, 0, bytes.Length);

            return gul;
        }

        public static string GetCurOpenId()
        {
            return m_sOpenId;
        }

        public static string GetCurToken()
        {
            return m_sToken;
        }
        public static void OnConnected(IAsyncResult connected)
        {
			C2CS_GameUserLogin lg = GenerateStLogin();            
            RpcSendHander.Send_GameUserLogin(lg);

			if (m_timer == null)
				m_timer = new Timer(OnTick, null, 0, 20000);

			OnRecvStateMsg(EMsgState.eOnConnected, string.Format("OnConnected->{0}:{1}", m_sIp, m_nPort));
		}

		private static void OnTick(object obj)
		{
			RpcSendHander.Send_Ping();
		}

		public static void OnRpcRecv(string funnane, object[] args)
        {
            if (null != m_OnLuaRecv)
				m_OnLuaRecv.LazyCallEx(args);
        }

		public static void OnRpcRecvCs(string funnane, object[] args)
		{
			if (null != m_OnLuaRecvCs)
				m_OnLuaRecvCs.LazyCallEx(args);
		}

		public static void OnRecvStateMsg(EMsgState eState, string sMsg)
		{
			if (null != m_OnRecvStateMsg)
				m_OnRecvStateMsg.Call((int)eState, sMsg);
			else
				Debug.LogWarningFormat("OnRecvStateMsg -> state:{0} msg:{1} ", eState, sMsg);
		}

		public static void OnConnectFailed(IAsyncResult connectFailed)
        {
			OnRecvStateMsg(EMsgState.eOnConnectFailed, "OnConnectFailed");
		}

		public static void OnDisConnected(IAsyncResult disConnected)
        {
			OnRecvStateMsg(EMsgState.eOnDisConnected, "OnDisConnected");
		}

        public static void CloseConnection()
		{
			if (null != m_timer)
			{
				m_timer.Dispose();
				m_timer = null;
			}
			ConnectionMgr.instance.Close();
		}
        

        public static void GacCallGas(int iFuncId, object[] args)
		{
			int packLen = 0;
			Array.Clear(m_bufSendData, 0, 256);

			Dispatcher.LogicRpc.PackProtocol(iFuncId, args, ref m_bufSendData, ref packLen);
			if (packLen > 0) {
				bool ret = RpcSendHander.Send_GameLogicLua_WithBytes (ref m_bufSendData, packLen);
				if (!ret) {
					Debug.LogWarning("Send_GameLogicLua_WithBytes: "+ iFuncId);
				}
			}
		}

		public static void CltCallCns(int iFuncId, object[] args)
		{
			int packLen = 0;
			Array.Clear(m_bufSendData, 0, 256);

			Dispatcher.Cns2CltRpc.PackProtocol(iFuncId, args, ref m_bufSendData, ref packLen);
			if (packLen > 0) {
				bool ret = RpcSendHander.CLuaSendHeadlerClt2Cns_SendRpcData(ref m_bufSendData, packLen);
				if (!ret) {
					Debug.LogWarning("CLuaSendHeadlerClt2Cns_SendRpcData: "+ iFuncId);
				}
			}
		}

		public static void GacCsCallGas(int iFuncId, object[] args)
		{
			Dispatcher.dispatcherC.LuaCallSendApi (iFuncId, args);
		}
	}
}
