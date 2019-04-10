using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;

namespace WCG
{
    public class ConnectionMgr
    {
        struct stCallBack
        {
            public Type         tParam;
            public MethodInfo   mtCallBack;
            public MethodInfo   mtGetTotalLen;
        }

        public const int uHeadSize = 1;
        private string clientID = null;
        public static readonly ConnectionMgr instance = new ConnectionMgr();
        private stCallBack[] processor = new stCallBack[(int)ECns2CltProtocol.eCS2C_ProtocolCount];
        private string m_tmpOpenId = "";

		private TCPClient m_TcpClient = null;

        private ConnectionMgr()
        {
            RegisterProcessor();
        }
        
        //注册消息处理函数
        private void RegisterProcessor()
        {
            processor[(int)ECns2CltProtocol.eCS2C_Ping].tParam = typeof(CS2CPing);
            processor[(int)ECns2CltProtocol.eCS2C_Ping].mtCallBack = typeof(Dispatcher).GetMethod(typeof(CS2CPing).Name);
            processor[(int)ECns2CltProtocol.eCS2C_Ping].mtGetTotalLen = typeof(CS2CPing).GetMethod("GetTotalLen");

            processor[(int)ECns2CltProtocol.eCS2C_GameLogicCPtr].tParam = typeof(CS2CGameLogicCPtr);
            processor[(int)ECns2CltProtocol.eCS2C_GameLogicCPtr].mtCallBack = typeof(Dispatcher).GetMethod(typeof(CS2CGameLogicCPtr).Name);
            processor[(int)ECns2CltProtocol.eCS2C_GameLogicCPtr].mtGetTotalLen = typeof(CS2CGameLogicCPtr).GetMethod("GetTotalLen");

            processor[(int)ECns2CltProtocol.eCS2C_GameLogicLuaPtr].tParam = typeof(CCS2CGameLogicLuaPtr);
            processor[(int)ECns2CltProtocol.eCS2C_GameLogicLuaPtr].mtCallBack = typeof(Dispatcher).GetMethod(typeof(CCS2CGameLogicLuaPtr).Name);
            processor[(int)ECns2CltProtocol.eCS2C_GameLogicLuaPtr].mtGetTotalLen = typeof(CCS2CGameLogicLuaPtr).GetMethod("GetTotalLen");

            processor[(int)ECns2CltProtocol.eCS2C_CnsLoginLuaPtr].tParam = typeof(CCS2CCnsLoginLuaPtr);
            processor[(int)ECns2CltProtocol.eCS2C_CnsLoginLuaPtr].mtCallBack = typeof(Dispatcher).GetMethod(typeof(CCS2CCnsLoginLuaPtr).Name);
            processor[(int)ECns2CltProtocol.eCS2C_CnsLoginLuaPtr].mtGetTotalLen = typeof(CCS2CCnsLoginLuaPtr).GetMethod("GetTotalLen");

            processor[(int)ECns2CltProtocol.eCs2C_NpCheck].tParam = typeof(CCS2CNpCheck);
            processor[(int)ECns2CltProtocol.eCs2C_NpCheck].mtCallBack = typeof(Dispatcher).GetMethod(typeof(CCS2CNpCheck).Name);
            processor[(int)ECns2CltProtocol.eCs2C_NpCheck].mtGetTotalLen = typeof(CCS2CNpCheck).GetMethod("GetTotalLen");

            processor[(int)ECns2CltProtocol.eCs2C_NpCheckFail].tParam = typeof(CCS2CNpCheckFail);
            processor[(int)ECns2CltProtocol.eCs2C_NpCheckFail].mtCallBack = typeof(Dispatcher).GetMethod(typeof(CCS2CNpCheckFail).Name);
            processor[(int)ECns2CltProtocol.eCs2C_NpCheckFail].mtGetTotalLen = typeof(CCS2CNpCheckFail).GetMethod("GetTotalLen");

        }

        public void SetClientID(string id) {
            clientID = id;
        }

        public void Connect(string sIP, int nPort, AsyncCallback CbConnected, AsyncCallback CbConnectFailed, AsyncCallback cbDisConnected)
        {
			if (m_TcpClient == null) {
				m_TcpClient = new TCPClient();
			}
            
            m_TcpClient.Connect(sIP, nPort, CbConnected, CbConnectFailed, cbDisConnected);
        }
        



		public bool SendMsg(byte[] bytes)
        {
            if (bytes.Length > 0)
            {
                return m_TcpClient.Send(bytes);
            }
			return false;
        }
        
        public void LoopDispatchRecvData()
        {
            if (null == m_TcpClient)
                return;

            if (m_TcpClient != null)
                m_TcpClient.DispatchEvent();

            float fTotalTime = 0.0f;
            while (true)
            {
                float fOnceBegin = Profile.ProfileBegin();
                int nDispatchedDataSize = OnceDispatchRecvData();
                fTotalTime += Profile.ProfileBegin() - fOnceBegin;

                if (0 == nDispatchedDataSize) break;
                if (null == m_TcpClient ) break;

                m_TcpClient.PopRecvData(nDispatchedDataSize); // 处理了的数据要pop掉

                if (fTotalTime >= 0.02f) break; // 处理网络包超过20ms就必须要退出了，需要留时间给渲染
            }
        }


        public int OnceDispatchRecvData()
        {
			int nRecvDataSize = 0;
			if (m_TcpClient != null)
				nRecvDataSize = m_TcpClient.GetRecvDataSize();
            if (nRecvDataSize <= uHeadSize)
                return 0; //数据包头没有收齐

			byte[] recvData = null;
			if (m_TcpClient != null)
				recvData = m_TcpClient.GetRecvData();


            int msgType = recvData[0];
            if (msgType >= (int)ECns2CltProtocol.eCS2C_ProtocolCount){
                return 0;//协议不对
            }

            stCallBack cb = processor[msgType];
            int nSize = uHeadSize + Marshal.SizeOf(cb.tParam);

            //适配IOS上 CS2CPing 的计算
            if (msgType == 0) nSize = 1;

            if (nRecvDataSize < nSize)
            {
                return 0; //数据包头没有收齐
            }

            System.Object msgObj = ConverterMgr.instance.BytesToStruct(ref recvData, cb.tParam, 1);
            object[] dispatch_parameters = new object[] { };
            var value = cb.mtGetTotalLen.Invoke(msgObj, dispatch_parameters);
            nSize += (int)value;


            //Debug.Log("收到nRecvDataSize:" + nRecvDataSize + "    need:" + nSize + "  value:" + (int)value + "  name:" + callback_t.Name);
            if (nRecvDataSize < nSize)
            {
                return 0;//数据没收齐
            }

            dispatch_parameters = new object[] { msgObj, recvData };
			cb.mtCallBack.Invoke(null, dispatch_parameters);

            return nSize;

        }

		public void Close()
		{
			if (m_TcpClient != null)
			{
				m_TcpClient.Close();
				m_TcpClient = null;
			}
		}
    }
}
