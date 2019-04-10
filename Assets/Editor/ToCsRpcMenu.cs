using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Threading;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using WCG;


[InitializeOnLoad]
public static class ToCsRpc
{
	static StringBuilder sb = null;
	static HashSet<string> usingList = new HashSet<string>();

	static int size_Byte = sizeof(Byte);
	//static int size_Boolean = sizeof(Boolean);
	static int size_Int16 = sizeof(Int16);
	static int size_UInt16 = sizeof(UInt16);
	static int size_Int32 = sizeof(Int32);
	static int size_UInt32 = sizeof(UInt32);
	static int size_Int64 = sizeof(Int64);
	static int size_UInt64 = sizeof(UInt64);
	static int size_Double = sizeof(Double);
	static int size_Single = sizeof(Single);
	static int size_EquipItem = Marshal.SizeOf(typeof(EquipItem));

	static Type type_Byte = typeof(Byte);
	static Type type_Boolean = typeof(Boolean);
	static Type type_Int16 = typeof(Int16);
	static Type type_UInt16 = typeof(UInt16);
	static Type type_Int32 = typeof(Int32);
	static Type type_UInt32 = typeof(UInt32);
	static Type type_Int64 = typeof(Int64);
	static Type type_UInt64 = typeof(UInt64);
	static Type type_Double = typeof(Double);
	static Type type_Single = typeof(Single);
	static Type type_EquipItem = typeof(EquipItem);
	static Type type_ByteArry = typeof(Byte[]);

	static Type[] Types_Nesting = new Type[] { type_EquipItem };


	[MenuItem("WCTool/Gen_Cs2Rpc", false, 1)]
	public static void GenerateCsRpc()
	{
		if (EditorApplication.isCompiling)
		{
			EditorUtility.DisplayDialog("警告", "请等待编辑器完成编译再执行此功能", "确定");
			return;
		}

		usingList.Clear();
		usingList.Add("System");
		//usingList.Add("System.Diagnostics");
		usingList.Add("System.Collections.Generic");
		usingList.Add("Debug = UnityEngine.Debug");
		//usingList.Add("System.Runtime.InteropServices");
		usingList.Add("LuaInterface");
		usingList.Add("WCG");

		sb = new StringBuilder();
		sb.AppendLine("\npublic static class CsRpcWrap");
		sb.AppendLine("{");

		RegisteredRcvAndSend();

		Add_Call_Bytes2Handler_To_Lua();

		//1导出嵌套协议
		for (int i = 0; i < Types_Nesting.Length; i++)
		{
			ConvertCsRpcToByteFunc(Types_Nesting[i]);
			ConvertCsRpcByteToFunc(Types_Nesting[i]);
		}

		//2导出接收协议
		DispatcherC<byte> tempDispatcher = new DispatcherC<byte>();
		Type[] cs2RpcRcv = tempDispatcher.HandlerRcvType;
		for (int i = 0; i < cs2RpcRcv.Length; i++)
		{
			//ConvertCsRpcToByteFunc(cs2RpcRcv[i]);
			ConvertCsRpcByteToFunc(cs2RpcRcv[i]);
		}

		//3导出发送协议
		Type[] cs2RpcSend = tempDispatcher.HandlerSendType;
		for (int i = 0; i < cs2RpcSend.Length; i++)
		{
			ConvertCsRpcToByteFunc(cs2RpcSend[i]);
			//ConvertCsRpcByteToFunc(cs2RpcSend[i]);
		}

		sb.AppendLine("\r\n");
		EndCodeGen(Application.dataPath + "/Source/RpcCore/");

		Debug.Log("GenerateCsRpc Over");      
		AssetDatabase.Refresh();
	}

	public static bool RegisteredRcvAndSend()
	{
		sb.AppendLine("\tstatic int iBufSize = 1;");
		sb.AppendLine("\tstatic Byte[] byteBuf = new Byte[iBufSize];");
		sb.AppendLine("\tstatic void BufReset()");
		sb.AppendLine("\t{");
		sb.AppendLine("\t\tArray.Clear(byteBuf, 0, iBufSize);");
		sb.AppendLine("\t}");

		sb.AppendLine("");
		sb.AppendLine("\tdelegate bool Bytes_To_Handler(ref Byte[] pData, uint iOffset, ref uint uProcessedOnce, ref LuaFunction luafunc);");
		DispatcherC<byte> tempDispatcher = new DispatcherC<byte>();

		//3导出接收协议
		sb.AppendLine("\tstatic Bytes_To_Handler[] bytes_to_pHander = new Bytes_To_Handler[]");
		sb.AppendLine("\t{");
		Type[] cs2RpcSend = tempDispatcher.HandlerRcvType;
		for (int i = 0; i < cs2RpcSend.Length; i++)
		{
			sb.AppendFormat("\t\t{0}_Bytes_To,\n", cs2RpcSend[i].Name);
		}
		sb.AppendLine("\t};");
		sb.AppendLine("");


		//2导出接收协议
		sb.AppendLine("\tpublic delegate Byte[] To_Bytes_Handler(ref object[] valObjs, uint iOffset);");
		sb.AppendLine("\tpublic static To_Bytes_Handler[] to_bytes_pHander = new To_Bytes_Handler[]");
		sb.AppendLine("\t{");
		Type[] cs2RpcRcv = tempDispatcher.HandlerSendType;
		for (int i = 0; i < cs2RpcRcv.Length; i++)
		{
			sb.AppendFormat("\t\t{0}_To_Bytes,\n", cs2RpcRcv[i].Name);
		}
		sb.AppendLine("\t};");
		sb.AppendLine("");

		//3导出注册和释放CSLua的二级协议接口函数
		sb.AppendFormat("\tstatic LuaFunction[] csRpc_to_LuaFunc = new LuaFunction[{0}];\n", cs2RpcSend.Length);
		sb.AppendLine("\tpublic static void RegisterCSLuaFunc()");
		sb.AppendLine("\t{");
		sb.AppendLine("\t\tDispatcherC<byte> tempDispatcher = new DispatcherC<byte>();");
		sb.AppendLine("\t\tType[] cs2RpcSend = tempDispatcher.HandlerRcvType;");
		sb.AppendLine("\t\tfor (int i = 0; i < cs2RpcSend.Length; i++)");
		sb.AppendLine("\t\t{");
		sb.AppendLine("\t\t\tDebug.Assert(csRpc_to_LuaFunc[i] == null);");
		sb.AppendLine("\t\t\tstring sLuaFunc = cs2RpcSend[i].Name.Replace(\"CS2CG\", \"Gas2GacCs.\");");
		sb.AppendLine("\t\t\tLuaFunction luafunc = GameMgr.ms_luaMgr.GetLuaFunction(sLuaFunc, false);");
		sb.AppendLine("\t\t\tDebug.AssertFormat(luafunc!=null, \"RegisterCSLuaFunc Error: {0}\", sLuaFunc);");
		sb.AppendLine("\t\t\tcsRpc_to_LuaFunc[i] = luafunc;");
		sb.AppendLine("\t\t}");
		sb.AppendLine("\t}");
		sb.AppendLine("");
		sb.AppendLine("\tpublic static void DisposeCSLuaFunc()");
		sb.AppendLine("\t{");
		sb.AppendLine("\t\tfor (int i = 0; i < csRpc_to_LuaFunc.Length; i++)");
		sb.AppendLine("\t\t{");
		sb.AppendLine("\t\t\tLuaFunction luafunc = csRpc_to_LuaFunc[i];");
		sb.AppendLine("\t\t\tif (luafunc != null)");
		sb.AppendLine("\t\t\t{");
		sb.AppendLine("\t\t\t\tluafunc.Dispose();");
		sb.AppendLine("\t\t\t\tluafunc = null;");
		sb.AppendLine("\t\t\t\tcsRpc_to_LuaFunc[i] = null;");
		sb.AppendLine("\t\t\t}");
		sb.AppendLine("\t\t}");
		sb.AppendLine("\t}");

		return true;
	}

	public static bool Add_Call_Bytes2Handler_To_Lua()
	{
		sb.AppendLine("");
		sb.AppendLine("\tpublic static bool Call_Bytes2Handler_To_Lua(int iType, ref Byte[] pData, uint iRpcOffset, ref uint uProcessedOnce)");
		sb.AppendLine("\t{");
		sb.AppendLine("\t\ttry");
		sb.AppendLine("\t\t{");
		sb.AppendLine("\t\t\tBytes_To_Handler pBy2Handler = bytes_to_pHander[iType];");
		sb.AppendLine("\t\t\tLuaFunction pLuaFunc = csRpc_to_LuaFunc[iType];");
		sb.AppendLine("\t\t\tif (null == pBy2Handler || null == pLuaFunc)");
		sb.AppendLine("\t\t\t\tDebug.AssertFormat(false, \"Call_Bytes2Handler_To_Lua(iType Error):{0}\", iType);");
		sb.AppendLine("\t\t\tpLuaFunc.BeginPCall();");
		sb.AppendLine("\t\t\tbool bCheckVal = pBy2Handler(ref pData, iRpcOffset, ref uProcessedOnce, ref pLuaFunc);");
		sb.AppendLine("\t\t\tif (!bCheckVal)");
		sb.AppendLine("\t\t\t\tDebug.AssertFormat(false, \"Call_Bytes2Handler_To_Lua(pBy2Handler Error):{0},{1}\", iType, iRpcOffset);");
		sb.AppendLine("\t\t\tpLuaFunc.PCall();");
		sb.AppendLine("\t\t\tpLuaFunc.EndPCall();");
		sb.AppendLine("\t\t\treturn true;");
		sb.AppendLine("\t\t}");
		sb.AppendLine("\t\tcatch (Exception ex)");
		sb.AppendLine("\t\t{");
		sb.AppendLine("\t\t\tDebug.LogErrorFormat(\"Call_Bytes2Handler_To_Lua(Exception):{0}\", ex.ToString());");
		sb.AppendLine("\t\t}");
		sb.AppendLine("\t\treturn false;");
		sb.AppendLine("\t}");
		return true;
	}


	public static bool ConvertCsRpcToByteFunc(Type structType)
	{
		sb.AppendLine("");
		sb.AppendFormat("\t//************** {0} **************\n",structType.Name);
		sb.AppendFormat("\tpublic static Byte[] {0}_To_Bytes(ref object[] valObjs, uint iOffset)\n", structType.Name);
		sb.AppendLine("\t{");
		sb.AppendLine("\t\ttry");
		sb.AppendLine("\t\t{");

		FieldInfo[] fields = structType.GetFields();
		int iAddValNum = 0;
		//最多嵌套一层
		for (int i = 0; i < fields.Length; i++)
		{
			if (fields[i].FieldType==type_EquipItem)
				iAddValNum += type_EquipItem.GetFields().Length-1;
		}
		sb.AppendFormat("\t\t\tint iParaNum = {0};\n",fields.Length+iAddValNum);
		sb.AppendFormat("\t\t\tint iStructSize = {0};\n",Marshal.SizeOf(structType));
		sb.AppendLine("\t\t\tif (valObjs.Length < iParaNum + iOffset)");
		sb.AppendLine("\t\t\t{");
		sb.AppendFormat("\t\t\t\tDebug.LogWarningFormat(\"{0}_To_Bytes(parameters wrong):{{0}},{{1}}\",valObjs.Length,iParaNum);\n", structType.Name);
		sb.AppendLine("\t\t\t\treturn null;");
		sb.AppendLine("\t\t\t}");
		sb.AppendLine("\t\t\tByte[] bytes = new Byte[iStructSize];");
		sb.AppendLine("\t\t\tuint idx = iOffset;");
		sb.AppendLine("\t\t\tint uProcessed = 0;");
		for (int i = 0; i < fields.Length; i++)
		{
			FieldInfo info = fields[i];
			MarshalAsAttribute marAttr = (MarshalAsAttribute)MarshalAsAttribute.GetCustomAttribute(info as MemberInfo, typeof(MarshalAsAttribute));
			if (null != marAttr)
			{
				if (marAttr.Value == UnmanagedType.ByValArray && info.FieldType == type_ByteArry)
				{
					sb.AppendLine("");
					sb.AppendFormat("\t\t\tByte[] by{0} = new Byte[{1}];\n",info.Name,marAttr.SizeConst);
					sb.AppendFormat("\t\t\tByte[] valby{0} = System.Text.Encoding.UTF8.GetBytes(Convert.ToString(valObjs[idx++]));\n",info.Name);
					sb.AppendFormat("\t\t\tArray.Copy(valby{0}, 0, by{0}, 0, valby{0}.Length);\n",info.Name);
					sb.AppendFormat("\t\t\tArray.Copy(by{0}, 0, bytes, uProcessed, {1}); uProcessed+={1};\n",info.Name,marAttr.SizeConst);
				}
				else if (marAttr.Value == UnmanagedType.I1 && info.FieldType == type_Boolean)
				{
					sb.AppendFormat("\t\t\tArray.Copy(System.BitConverter.GetBytes(Convert.ToBoolean(valObjs[idx++])), 0, bytes, uProcessed, {0}); uProcessed+={0};\n",size_Byte);
				}
				else
				{
					Debug.Assert(false, "ConvertCsRpcByteToFunc(marAttr) Error:" + structType.Name + "," + marAttr.Value.ToString());
				}
			}
			else
			{
				if (info.FieldType == type_Byte)
				{
					sb.AppendFormat("\t\t\tArray.Copy(System.BitConverter.GetBytes(Convert.ToByte(valObjs[idx++])), 0, bytes, uProcessed, {0}); uProcessed+={0};\n",size_Byte);
				}
				else if (info.FieldType == type_Int16)
				{
					sb.AppendFormat("\t\t\tArray.Copy(System.BitConverter.GetBytes(Convert.ToInt16(valObjs[idx++])), 0, bytes, uProcessed, {0}); uProcessed+={0};\n",size_Int16);
				}
				else if (info.FieldType == type_UInt16)
				{
					sb.AppendFormat("\t\t\tArray.Copy(System.BitConverter.GetBytes(Convert.ToUInt16(valObjs[idx++])), 0, bytes, uProcessed, {0}); uProcessed+={0};\n",size_UInt16);
				}
				else if (info.FieldType == type_Int32)
				{
					sb.AppendFormat("\t\t\tArray.Copy(System.BitConverter.GetBytes(Convert.ToInt32(valObjs[idx++])), 0, bytes, uProcessed, {0}); uProcessed+={0};\n",size_Int32);
				}
				else if (info.FieldType == type_UInt32)
				{
					sb.AppendFormat("\t\t\tArray.Copy(System.BitConverter.GetBytes(Convert.ToUInt32(valObjs[idx++])), 0, bytes, uProcessed, {0}); uProcessed+={0};\n",size_UInt32);
				}
				else if (info.FieldType == type_Int64)
				{
					sb.AppendFormat("\t\t\tArray.Copy(System.BitConverter.GetBytes(Convert.ToInt64(valObjs[idx++])), 0, bytes, uProcessed, {0}); uProcessed+={0};\n",size_Int64);
				}
				else if (info.FieldType == type_UInt64)
				{
					sb.AppendFormat("\t\t\tArray.Copy(System.BitConverter.GetBytes(Convert.ToUInt64(valObjs[idx++])), 0, bytes, uProcessed, {0}); uProcessed+={0};\n",size_UInt64);
				}
				else if (info.FieldType == type_Double)
				{
					sb.AppendFormat("\t\t\tArray.Copy(System.BitConverter.GetBytes(Convert.ToDouble(valObjs[idx++])), 0, bytes, uProcessed, {0}); uProcessed+={0};\n",size_Double);
				}
				else if (info.FieldType == type_Single)
				{
					sb.AppendFormat("\t\t\tArray.Copy(System.BitConverter.GetBytes(Convert.ToSingle(valObjs[idx++])), 0, bytes, uProcessed, {0}); uProcessed+={0};\n",size_Single);
				}
				else if (info.FieldType == type_EquipItem)
				{
					sb.AppendLine("");
					sb.AppendFormat("\t\t\tByte[] by{0} = EquipItem_To_Bytes(ref valObjs, idx); idx+={1};\n",info.Name,type_EquipItem.GetFields().Length);
					sb.AppendFormat("\t\t\tArray.Copy(by{0}, 0, bytes, uProcessed, {1}); uProcessed+={1};\n",info.Name,size_EquipItem);
				}
				else
				{
					Debug.Assert(false, "ConvertCsRpcByteToFunc Error:" + structType.Name+"," + info.FieldType.Name);
				}
			}
		}
		sb.AppendLine("\t\t\treturn bytes;");
		sb.AppendLine("\t\t}");
		sb.AppendLine("\t\tcatch(Exception ex)");
		sb.AppendLine("\t\t{");
		sb.AppendFormat("\t\t\tDebug.LogWarningFormat(\"{0}_To_Bytes(Exception):{{0}}\",ex.ToString());\n",structType.Name);
		sb.AppendLine("\t\t}");
		sb.AppendLine("\t\treturn null;");
		sb.AppendLine("\t}");
		return true;
	}

	public static bool ConvertCsRpcByteToFunc(Type structType)
	{
		sb.AppendLine("");
		sb.AppendFormat("\t//************** {0} **************\n", structType.Name);
		sb.AppendFormat("\tpublic static bool {0}_Bytes_To(ref Byte[] pData, uint iOffset, ref uint uProcessedOnce, ref LuaFunction luafunc)\n", structType.Name);
		sb.AppendLine("\t{");
		sb.AppendLine("\t\ttry");
		sb.AppendLine("\t\t{");

		FieldInfo[] fields = structType.GetFields();
		sb.AppendFormat("\t\t\tint iStructSize = {0};\n",Marshal.SizeOf(structType));
		sb.AppendLine("\t\t\tif (pData.Length < (iOffset + iStructSize))");
		sb.AppendLine("\t\t\t{");
		sb.AppendFormat("\t\t\t\tDebug.LogWarningFormat(\"{0}_Bytes_To(parameters wrong):{{0}},{{1}},{{2}}\",pData.Length,iOffset,iStructSize);\n", structType.Name);
		sb.AppendLine("\t\t\t\treturn false;");
		sb.AppendLine("\t\t\t}");
		sb.AppendLine("\t\t\tuint uProcessed = iOffset;");
		sb.AppendLine("\t\t\tuint uProcessedLen = 0;");
		for (int i = 0; i < fields.Length; i++)
		{
			FieldInfo info = fields[i];
			MarshalAsAttribute marAttr = (MarshalAsAttribute)MarshalAsAttribute.GetCustomAttribute(info as MemberInfo, typeof(MarshalAsAttribute));
			if (null != marAttr)
			{
				if (marAttr.Value == UnmanagedType.ByValArray && info.FieldType == type_ByteArry)
				{
					sb.AppendFormat("\t\t\tByte[] {0} = new Byte[{1}]; Array.Copy(pData, uProcessed, {0}, 0, {1}); luafunc.PushGeneric(System.Text.Encoding.UTF8.GetString({0})); uProcessed += {1}; uProcessedLen += {1};\n", info.Name,marAttr.SizeConst);
				}
				else if (marAttr.Value == UnmanagedType.I1 && info.FieldType == type_Boolean)
				{
					//sb.AppendFormat("\t\t\tByte[] {0} = new Byte[{1}]; Array.Copy(pData, uProcessed, {0}, 0, 1); luafunc.PushGeneric(Convert.ToBoolean({0}[0])); uProcessed += {1}; uProcessedLen += {1};\n", info.Name,size_Byte);
					sb.AppendFormat("\t\t\tBufReset(); Array.Copy(pData, uProcessed, byteBuf, 0, 1); luafunc.PushGeneric(Convert.ToBoolean(byteBuf[0])); uProcessed += {1}; uProcessedLen += {1};\n", info.Name, size_Byte);
				}
				else
				{
					Debug.Assert(false, "ConvertCsRpcByteToFunc(marAttr) Error:" + structType.Name + "," + marAttr.Value.ToString());
				}
			}
			else
			{
				if (info.FieldType == type_Byte)
				{
                    //sb.AppendFormat("\t\t\tByte[] by{0} = new Byte[1]; Array.Copy(pData, uProcessed, by{0}, 0, 1); UInt16 {0} = Convert.ToUInt16(by{0}[0]); luafunc.PushGeneric({0}); uProcessed += 1; uProcessedLen += {1};\n", info.Name,size_Byte);
                    sb.AppendFormat("\t\t\tBufReset(); Array.Copy(pData, uProcessed, byteBuf, 0, 1); UInt16 {0} = Convert.ToUInt16(byteBuf[0]); luafunc.PushGeneric({0}); uProcessed += 1; uProcessedLen += {1};\n", info.Name, size_Byte);
                }
				else if (info.FieldType == type_Int16)
				{
					sb.AppendFormat("\t\t\tluafunc.PushGeneric(BitConverter.ToInt16(pData, (int)uProcessed)); uProcessed += {1}; uProcessedLen += {1};\n", info.Name,size_Int16);
				}
				else if (info.FieldType == type_UInt16)
				{
					sb.AppendFormat("\t\t\tluafunc.PushGeneric(BitConverter.ToUInt16(pData, (int)uProcessed)); uProcessed += {1}; uProcessedLen += {1};\n", info.Name,size_UInt16);
				}
				else if (info.FieldType == type_Int32)
				{
					sb.AppendFormat("\t\t\tluafunc.PushGeneric(BitConverter.ToInt32(pData, (int)uProcessed)); uProcessed += {1}; uProcessedLen += {1};\n", info.Name,size_Int32);
				}
				else if (info.FieldType == type_UInt32)
				{
					sb.AppendFormat("\t\t\tluafunc.PushGeneric(BitConverter.ToUInt32(pData, (int)uProcessed)); uProcessed += {1}; uProcessedLen += {1};\n", info.Name,size_UInt32);
				}
				else if (info.FieldType == type_Int64)
				{
					sb.AppendFormat("\t\t\tluafunc.PushGeneric(BitConverter.ToInt64(pData, (int)uProcessed)); uProcessed += {1}; uProcessedLen += {1};\n", info.Name,size_Int64);
				}
				else if (info.FieldType == type_UInt64)
				{
					sb.AppendFormat("\t\t\tluafunc.PushGeneric(BitConverter.ToUInt64(pData, (int)uProcessed)); uProcessed += {1}; uProcessedLen += {1};\n", info.Name,size_UInt64);
				}
				else if (info.FieldType == type_Double)
				{
					sb.AppendFormat("\t\t\tluafunc.PushGeneric(BitConverter.ToDouble(pData, (int)uProcessed)); uProcessed += {1}; uProcessedLen += {1};\n", info.Name,size_Double);
				}
				else if (info.FieldType == type_Single)
				{
					sb.AppendFormat("\t\t\tluafunc.PushGeneric(BitConverter.ToSingle(pData, (int)uProcessed)); uProcessed += {1}; uProcessedLen += {1};\n", info.Name,size_Single);
				}
				else if (info.FieldType == type_EquipItem)
				{
					sb.AppendFormat("\t\t\tEquipItem_Bytes_To(ref pData, uProcessed, ref uProcessedOnce, ref luafunc); uProcessed += {1}; uProcessedLen += {1};\n", info.Name,size_EquipItem);
				}
				else
				{
					Debug.Assert(false, "ConvertCsRpcByteToFunc Error:" + structType.Name+"," + info.FieldType.Name);
				}
			}
		}
		sb.AppendLine("\t\t\tif (uProcessedLen != iStructSize)");
		sb.AppendLine("\t\t\t{");
		sb.AppendFormat("\t\t\t\tDebug.LogWarningFormat(\"{0}_Bytes_To(uProcessedOnce Error):{{0}},{{1}}\",uProcessedOnce,iStructSize);\n",structType.Name);
		sb.AppendLine("\t\t\t\treturn false;");
		sb.AppendLine("\t\t\t}");
		sb.AppendLine("\t\t\tuProcessedOnce += uProcessedLen;");

		SpecialRpcAdd(structType);

		sb.AppendLine("\t\t\treturn true;");
		sb.AppendLine("\t\t}");
		sb.AppendLine("\t\tcatch(Exception ex)");
		sb.AppendLine("\t\t{");
		sb.AppendFormat("\t\t\tDebug.LogWarningFormat(\"{0}_Bytes_To(Exception):{{0}}\",ex.ToString());\n",structType.Name);
		sb.AppendLine("\t\t}");
		sb.AppendLine("\t\treturn false;");
		sb.AppendLine("\t}");
		return true;
	}

	static void AddStructToByte()
	{
		sb.AppendLine("\tpublic static Byte[] StructToBytes(object structure)");
		sb.AppendLine("\t{");
		sb.AppendLine("\t\tInt32 size = Marshal.SizeOf(structure);");
		sb.AppendLine("\t\tIntPtr buffer = Marshal.AllocHGlobal(size);");
		sb.AppendLine("\t\ttry");
		sb.AppendLine("\t\t{");
		sb.AppendLine("\t\t\tMarshal.StructureToPtr(structure, buffer, false);");
		sb.AppendLine("\t\t\tByte[] bytes = new Byte[size];");
		sb.AppendLine("\t\t\tMarshal.Copy(buffer, bytes, 0, size);");
		sb.AppendLine("\t\t\treturn bytes;");
		sb.AppendLine("\t\t}");
		sb.AppendLine("\t\tfinally");
		sb.AppendLine("\t\t{");
		sb.AppendLine("\t\t\tMarshal.FreeHGlobal(buffer);");
		sb.AppendLine("\t\t}");
		sb.AppendLine("\t}");
	}

	static void SpecialRpcAdd(Type structType)
	{
		if (structType == typeof(CS2CGSynFightBasePro))
		{
			sb.AppendLine("");
			sb.AppendFormat("\t\t\t//************ {0}(special Begin) ************\n", structType.Name);
			sb.AppendLine("\t\t\tif (uDataLength > 0)");
			sb.AppendLine("\t\t\t{");
			sb.AppendLine("\t\t\t\tByte[] extra = new byte[uDataLength];");
			sb.AppendLine("\t\t\t\tArray.Copy (pData, uProcessed, extra, 0, uDataLength);");
			sb.AppendLine("\t\t\t\tuProcessed+=uDataLength; uProcessedOnce += uDataLength;");
			sb.AppendLine("\t\t\t\tluafunc.PushGeneric(System.Text.Encoding.UTF8.GetString(extra));");
			sb.AppendLine("\t\t\t}");
			sb.AppendLine("\t\t\telse");
			sb.AppendLine("\t\t\t{");
			sb.AppendLine("\t\t\t\tluafunc.PushGeneric(\"\");");
			sb.AppendLine("\t\t\t}");
			sb.AppendFormat("\t\t\t//************ {0}(special End) ************\n", structType.Name);
		}
		else if (structType == typeof(CS2CGSyncPlayerBase))
		{
			sb.AppendLine("");
			sb.AppendFormat("\t\t\t//************ {0}(special Begin) ************\n", structType.Name);
			sb.AppendLine("\t\t\tif (uDataLength > 0)");
			sb.AppendLine("\t\t\t{");
			sb.AppendLine("\t\t\t\tByte[] extra = new byte[uDataLength];");
			sb.AppendLine("\t\t\t\tArray.Copy (pData, uProcessed, extra, 0, uDataLength);");
			sb.AppendLine("\t\t\t\tuProcessed+=uDataLength; uProcessedOnce += uDataLength;");
			sb.AppendLine("\t\t\t\tluafunc.PushGeneric(System.Text.Encoding.UTF8.GetString(extra));");
			sb.AppendLine("\t\t\t}");
			sb.AppendLine("\t\t\telse");
			sb.AppendLine("\t\t\t{");
			sb.AppendLine("\t\t\t\tluafunc.PushGeneric(\"\");");
			sb.AppendLine("\t\t\t}");
			sb.AppendLine("\t\t\tif (uTattooLength > 0)");
			sb.AppendLine("\t\t\t{");
			sb.AppendLine("\t\t\t\tByte[] extra = new byte[uTattooLength];");
			sb.AppendLine("\t\t\t\tArray.Copy (pData, uProcessed, extra, 0, uTattooLength);");
			sb.AppendLine("\t\t\t\tuProcessed+=uTattooLength; uProcessedOnce += uTattooLength;");
			sb.AppendLine("\t\t\t\tluafunc.PushGeneric(System.Text.Encoding.UTF8.GetString(extra));");
			sb.AppendLine("\t\t\t}");
			sb.AppendLine("\t\t\telse");
			sb.AppendLine("\t\t\t{");
			sb.AppendLine("\t\t\t\tluafunc.PushGeneric(\"\");");
			sb.AppendLine("\t\t\t}");
			sb.AppendFormat("\t\t\t//************ {0}(special End) ************\n", structType.Name);
		}
		else if (structType == typeof(CS2CGSynNpcData))
		{
			sb.AppendLine("");
			sb.AppendFormat("\t\t\t//************ {0}(special Begin) ************\n", structType.Name);
			sb.AppendLine("\t\t\tif (uValidPlayerCharIDLength > 0)");
			sb.AppendLine("\t\t\t{");
			sb.AppendLine("\t\t\t\tByte[] extra = new byte[uValidPlayerCharIDLength];");
			sb.AppendLine("\t\t\t\tArray.Copy (pData, uProcessed, extra, 0, uValidPlayerCharIDLength);");
			sb.AppendLine("\t\t\t\tuProcessed+=uValidPlayerCharIDLength; uProcessedOnce += uValidPlayerCharIDLength;");
			sb.AppendLine("\t\t\t\tluafunc.PushGeneric(System.Text.Encoding.UTF8.GetString(extra));");
			sb.AppendLine("\t\t\t}");
			sb.AppendLine("\t\t\telse");
			sb.AppendLine("\t\t\t{");
			sb.AppendLine("\t\t\t\tluafunc.PushGeneric(\"\");");
			sb.AppendLine("\t\t\t}");
			sb.AppendLine("\t\t\tif (uAddNameLength > 0)");
			sb.AppendLine("\t\t\t{");
			sb.AppendLine("\t\t\t\tByte[] extra = new byte[uAddNameLength];");
			sb.AppendLine("\t\t\t\tArray.Copy (pData, uProcessed, extra, 0, uAddNameLength);");
			sb.AppendLine("\t\t\t\tuProcessed+=uAddNameLength; uProcessedOnce += uAddNameLength;");
			sb.AppendLine("\t\t\t\tluafunc.PushGeneric(System.Text.Encoding.UTF8.GetString(extra));");
			sb.AppendLine("\t\t\t}");
			sb.AppendLine("\t\t\telse");
			sb.AppendLine("\t\t\t{");
			sb.AppendLine("\t\t\t\tluafunc.PushGeneric(\"\");");
			sb.AppendLine("\t\t\t}");

			sb.AppendLine("\t\t\tif (uRandomAiLength > 0)");
			sb.AppendLine("\t\t\t{");
			sb.AppendLine("\t\t\t\tByte[] extra = new byte[uRandomAiLength];");
			sb.AppendLine("\t\t\t\tArray.Copy (pData, uProcessed, extra, 0, uRandomAiLength);");
			sb.AppendLine("\t\t\t\tuProcessed+=uRandomAiLength; uProcessedOnce += uRandomAiLength;");
			sb.AppendLine("\t\t\t\tluafunc.PushGeneric(System.Text.Encoding.UTF8.GetString(extra));");
			sb.AppendLine("\t\t\t}");
			sb.AppendLine("\t\t\telse");
			sb.AppendLine("\t\t\t{");
			sb.AppendLine("\t\t\t\tluafunc.PushGeneric(\"\");");
			sb.AppendLine("\t\t\t}");

			sb.AppendFormat("\t\t\t//************ {0}(special End) ************\n", structType.Name);
		}
	}

	static void EndCodeGen(string dir)
	{
		sb.AppendLine("}\r\n");
		SaveFile(dir + "CsRpc" + "Wrap.cs");
	}

	static void SaveFile(string file)
	{
		using (StreamWriter textWriter = new StreamWriter(file, false, Encoding.UTF8))
		{
			StringBuilder usb = new StringBuilder();
			usb.AppendLine("//this source code was auto-generated by toCsRpc#, do not modify it");

			//using
			foreach (string str in usingList)
			{
				usb.AppendFormat("using {0};\r\n", str);
			}
			textWriter.Write(usb.ToString());

			//Registered && Interface
			textWriter.Write(sb.ToString());
			textWriter.Flush();
			textWriter.Close();
		}
	}
}
