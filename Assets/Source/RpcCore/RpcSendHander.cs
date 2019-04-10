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
    public class RpcSendHander
    {
        public static void Send_Ping()
        {
            Byte[] bytes = ConverterMgr.instance.ConvertEnumToByteArray(Convert.ToInt16(EClt2CnsProtocol.eC2CS_Ping));
            ConnectionMgr.instance.SendMsg(bytes);
        }

        public static void Send_GameUserLogin(System.Object obj)
        {
            Byte[] header = ConverterMgr.instance.ConvertEnumToByteArray(Convert.ToInt16(EClt2CnsProtocol.eC2CS_GameUserLogin));
            Byte[] data = ConverterMgr.instance.StructToBytes(obj);
            Byte[] send_bytes = new Byte[data.Length + header.Length];

            System.Buffer.BlockCopy(header, 0, send_bytes, 0, header.Length);
            System.Buffer.BlockCopy(data, 0, send_bytes, header.Length, data.Length);
            ConnectionMgr.instance.SendMsg(send_bytes);
        }

		public static void SendLogicCPrt(ref Byte[] constData, int constDataSize)
        {
            //header
            Byte[] header = ConverterMgr.instance.ConvertEnumToByteArray(Convert.ToInt16(EClt2CnsProtocol.eC2CS_GameLogicCPtr));

            //data len
            C2CS_GameLogicCPtr logic;
			logic.uDataLength = Convert.ToByte(constDataSize);
            Byte[] first = ConverterMgr.instance.StructToBytes(logic);

            //data
            //Byte[] data = ConverterMgr.instance.StructToBytes(obj);
			Byte[] send_bytes = new Byte[constDataSize + first.Length + header.Length];

            System.Buffer.BlockCopy(header, 0, send_bytes, 0, header.Length);
            System.Buffer.BlockCopy(first, 0, send_bytes, header.Length, first.Length);
			System.Buffer.BlockCopy(constData, 0, send_bytes, header.Length + first.Length, constDataSize);

            ConnectionMgr.instance.SendMsg(send_bytes);
        }


		public static bool Send_GameLogicLua_WithBytes(ref Byte[] constData, int constDataSize)
		{
			//header
			Byte[] header = ConverterMgr.instance.ConvertEnumToByteArray(Convert.ToInt16(EClt2CnsProtocol.eC2CS_GameLogicLuaPtr));

			//data len
			C2CS_GameLogicLuaPtr logic;
			logic.uDataLength =  (Int16)constDataSize;
			Byte[] first = ConverterMgr.instance.StructToBytes(logic);

			//data
			//Byte[] data = ConverterMgr.instance.StructToBytes(obj);
			Byte[] send_bytes = new Byte[constDataSize + first.Length + header.Length];

			System.Buffer.BlockCopy(header, 0, send_bytes, 0, header.Length);
			System.Buffer.BlockCopy(first, 0, send_bytes, header.Length, first.Length);
			System.Buffer.BlockCopy(constData, 0, send_bytes, header.Length + first.Length, constDataSize);

			bool ret = ConnectionMgr.instance.SendMsg(send_bytes);
			return ret;
		}

		public static bool CLuaSendHeadlerClt2Cns_SendRpcData(ref Byte[] constData, int constDataSize)
		{
			//header
			Byte[] header = ConverterMgr.instance.ConvertEnumToByteArray(Convert.ToInt16(EClt2CnsProtocol.eC2CS_GameLoginLuaPtr));

			//data len
			C2CS_GameLoginLuaPtr logic;
			logic.uDataLength = (Int16)constDataSize;
			Byte[] first = ConverterMgr.instance.StructToBytes(logic);

			//data
			//Byte[] data = ConverterMgr.instance.StructToBytes(obj);
			Byte[] send_bytes = new Byte[constDataSize + first.Length + header.Length];

			System.Buffer.BlockCopy(header, 0, send_bytes, 0, header.Length);
			System.Buffer.BlockCopy(first, 0, send_bytes, header.Length, first.Length);
			System.Buffer.BlockCopy(constData, 0, send_bytes, header.Length + first.Length, constDataSize);

			bool ret = ConnectionMgr.instance.SendMsg(send_bytes);
			return ret;
		}
    }
}
