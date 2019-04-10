using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Runtime.InteropServices;

/// <summary>
/// add by zhongcy
/// 协议处理器初始化和注册
/// </summary>
namespace WCG
{
    public class Dispatcher
    {
		public static LuaRpc<UInt16> LogicRpc = null;  	//Gas2Gac  服务器到客户端协议处理
		public static LuaRpc<UInt16> Cns2CltRpc = null; //Cns2Clt  cns到客户端协议处理
		
		public static DispatcherC<byte>  dispatcherC = null ;

		public static void Init()
		{
			if (LogicRpc == null) {
				LogicRpc = new LuaRpc<UInt16> ("Gas2Gac");
			}

			if (Cns2CltRpc == null) {
				Cns2CltRpc = new LuaRpc<UInt16> ("Cns2Clt");
			}

			if (dispatcherC == null) {
				dispatcherC = new DispatcherC<byte>();
			}
		}

		//注册打包协议
		public static void RegistPackProtocols(Boolean isLogic, UInt32 uID, string szName, string szParamInfo) 
		{
			Init ();
			
			if (isLogic)
				LogicRpc.RegistLogicPackProtocols (uID, szName, szParamInfo);
			else 
				Cns2CltRpc.RegistLogicPackProtocols (uID, szName, szParamInfo);
		}

		//注册解包协议
		public static void RegistUnPackProtocols(Boolean isLogic, UInt32 uID, string szName, string szParamInfo) 
		{
			Init ();

			if (isLogic)
				LogicRpc.RegistLogicUnPackProtocols (uID, szName, szParamInfo);
			else 
				Cns2CltRpc.RegistLogicUnPackProtocols (uID, szName, szParamInfo);
		}


		public static int CS2CPing(CS2CPing msg, ref byte[] recvData)
        {
			int first_size = Marshal.SizeOf(msg) + ConnectionMgr.uHeadSize;
			return first_size;
        }

		public static int CS2CGameLogicCPtr(CS2CGameLogicCPtr msg, ref byte[] recvData)
        {
			int first_size = Marshal.SizeOf(msg) + ConnectionMgr.uHeadSize;
			int second_size = msg.GetTotalLen();
			byte[] bodyData = new byte[second_size];
			System.Buffer.BlockCopy(recvData, first_size, bodyData, 0, second_size);

			int processTotalByte = dispatcherC.LoopDispatch (ref bodyData, (uint)bodyData.Length);

            /*Byte[] tempArr = new Byte[128];
            Array.Copy(recvData, tempArr, 128);
            Debug.LogFormat("On_CS2CGameLogicCPtr---1---:{0},{1},{2}\n{3}", first_size, second_size, processTotalByte, BitConverter.ToString(tempArr));*/

            return processTotalByte + first_size;
        }

		public static int CCS2CGameLogicLuaPtr(CCS2CGameLogicLuaPtr msg, ref byte[] recvData)
        {
			int first_size = Marshal.SizeOf(msg) + ConnectionMgr.uHeadSize;
			int second_size = msg.GetTotalLen();
			byte[] bodyData = new byte[second_size];
			System.Buffer.BlockCopy(recvData, first_size, bodyData, 0, second_size);

			int processTotalByte = (int)LogicRpc.LoopUnPackProtocol (0, ref bodyData, (uint)second_size);

            /*Byte[] tempArr = new Byte[128];
            Array.Copy(recvData, tempArr, 128);
            Debug.LogFormat("On_CCS2CGameLogicLuaPtr---2---:{0},{1},{2}\n{3}", first_size, second_size, processTotalByte, BitConverter.ToString(tempArr));*/

            return processTotalByte+first_size;

        }
		public static int CCS2CCnsLoginLuaPtr(CCS2CCnsLoginLuaPtr msg, ref byte[] recvData)
        {
			int first_size = Marshal.SizeOf(msg) + ConnectionMgr.uHeadSize;
			int second_size = msg.GetTotalLen();
			byte[] bodyData = new byte[second_size];
			System.Buffer.BlockCopy(recvData, first_size, bodyData, 0, second_size);

			int processTotalByte = (int)Cns2CltRpc.LoopUnPackProtocol (0, ref bodyData, (uint)second_size);

            /*Byte[] tempArr = new Byte[128];
            Array.Copy(recvData, tempArr, 128);
            Debug.LogFormat("On_CCS2CCnsLoginLuaPtr---3---:{0},{1},{2}\n{3}", first_size, second_size, processTotalByte, BitConverter.ToString(tempArr));*/

            return processTotalByte+first_size;
        }

		public static int CCS2CNpCheck(CCS2CNpCheck msg, ref byte[] recvData)
        {
			//Debug.Log("### On_CCS2CNpCheck");
			//int first_size = Marshal.SizeOf(msg) + ConnectionMgr.uHeadSize;
			return 0;
        }

		public static int CCS2CNpCheckFail(CCS2CNpCheckFail msg, ref byte[] recvData)
        {
			//Debug.Log("### On_CCS2CNpCheckFail");
			int first_size = Marshal.SizeOf(msg) + ConnectionMgr.uHeadSize;
			return first_size;
        }
    }
}

