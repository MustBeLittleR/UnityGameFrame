using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

/// <summary>
/// add by zhongcy
/// LuaRpc协议处理
/// </summary>
namespace WCG
{
	public class LuaRpc<T>
	{
        public struct ParamInfo
	    {
		    public UInt32	uID;
		    public string 	szName;
            public char[] 	szParamInfo;
	    }

		private Dictionary<UInt32, ParamInfo> m_PackProtocols = null;
		private Dictionary<UInt32, ParamInfo> m_UnPackProtocols = null;

		private UInt32 TYPE_SIZE;
		private string m_LuaModuleName; //Gas2Gac Cns2Clt

		//unpack  基本数据类型缓存
		private byte[] byteBuff;
		private byte[] int16Buff;
		private byte[] int32Buff;
		private byte[] doubleBuff;
		private byte[] shortStringBuff;
		private byte[] longStringBuff;


		public LuaRpc(string moduleName)
		{
			if (m_PackProtocols == null) {
				m_PackProtocols = new Dictionary<UInt32, ParamInfo>();
			}

			if (m_UnPackProtocols == null) {
				m_UnPackProtocols = new Dictionary<UInt32, ParamInfo> ();
			}

			//获取类型长度
			TYPE_SIZE = (UInt32)Marshal.SizeOf(default(T).GetType());

			//设置lua模块名称
			m_LuaModuleName = moduleName;

			//unpack数据类型缓存
			byteBuff = new byte[1];
			int16Buff = new byte[sizeof(Int16)];
			int32Buff = new byte[sizeof(Int32)];
			doubleBuff = new byte[sizeof(Double)];
			shortStringBuff = new byte[256];
			longStringBuff = new byte[65536];
		}

		//注册打包协议
		public void RegistLogicPackProtocols(UInt32 uID, string szName, string szParamInfo) 
		{
			ParamInfo info = new ParamInfo();
			info.uID =  uID;
			info.szName =  szName;
            info.szParamInfo = szParamInfo.ToCharArray();

			if (!m_PackProtocols.ContainsKey(uID))
				m_PackProtocols.Add(uID, info);
			else
				m_PackProtocols[uID] = info;
			//Debug.LogWarning ("RegistPackProtocols: " + uID + " " + szName + " " + szParamInfo);
		}

		//注册解包协议
		public void RegistLogicUnPackProtocols(UInt32 uID, string szName, string szParamInfo) 
		{
			ParamInfo info = new ParamInfo();
			info.uID =  uID;
			info.szName =  szName;
			info.szParamInfo =  szParamInfo.ToCharArray();

            if (!m_UnPackProtocols.ContainsKey(uID))
				m_UnPackProtocols.Add(uID, info);
			else
				m_UnPackProtocols[uID] = info;
			//Debug.LogWarning ("RegistUnPackProtocols: " + uID + " " + szName + " " + szParamInfo);
		}
			
		//解包主循环
		public UInt32 LoopUnPackProtocol(UInt32 uConnId, ref byte[] pConstBuf, UInt32 uBufLen)
		{
			UInt32 nProcessTotalByte=0;
			UInt32 uLeftSrcBufLen = uBufLen;

			for(;;)
			{
				UInt32 nProcessByte = OnceUnPackProtocol(uConnId, ref pConstBuf, uLeftSrcBufLen, nProcessTotalByte);
				if(nProcessByte<=0)
				{
					break;
				}
				else
				{
					nProcessTotalByte += nProcessByte;

					Debug.Assert(uLeftSrcBufLen>=nProcessByte);
					uLeftSrcBufLen -= nProcessByte;

					//Debug.LogWarning ("RegistUnPackProtocols: " + uBufLen + " " + nProcessTotalByte + " " + nProcessByte +" " + uLeftSrcBufLen);
					if (nProcessTotalByte == uBufLen) {
						break;
					}
				}
			}
			Debug.Assert(nProcessTotalByte <= uBufLen);
			return nProcessTotalByte;
		}

		//解析一个协议包
		public UInt32 OnceUnPackProtocol(UInt32 uConnId, ref byte[] pConstBuf, UInt32 uBufLen, UInt32 nProcessTotalByte)
		{
			if (uBufLen < TYPE_SIZE) {
				Debug.LogWarning ("OnceUnPackProtocol not enougth: " + uBufLen + " " + TYPE_SIZE);
				return 0;
			}

			UInt16 PtlID = 0;
			if (TYPE_SIZE == 1) {
				PtlID = (UInt16)pConstBuf[nProcessTotalByte];
			} else if (TYPE_SIZE == sizeof(Int16)) {
				Array.Clear(int16Buff, 0, sizeof(Int16));
				Array.Copy(pConstBuf, nProcessTotalByte, int16Buff, 0, sizeof(Int16));
				PtlID = System.BitConverter.ToUInt16(int16Buff, 0);
			}

            UInt32 DecodeBytelen = 0;
			DecodeBytelen += TYPE_SIZE;

			if(PtlID>=0 && PtlID < m_UnPackProtocols.Count)
		    {
				//获取对应lua函数
				ParamInfo paramInfo = m_UnPackProtocols[PtlID];
                //string szDisParamInfo = szParamInfo;

				UInt32 stProcessByte = (UInt32)uBufLen - TYPE_SIZE;
				UInt32 stProcessParamNum = 0;

                CallServer.m_OnLuaRecv.BeginPCallEx();
                CallServer.m_OnLuaRecv.Push(m_LuaModuleName);   //lua模块
                CallServer.m_OnLuaRecv.Push(paramInfo.szName);  //lua方法名

                char[] arr = paramInfo.szParamInfo;
                int stParamNum = arr.Length;
				bool bBreak=true;
				for(int i=0; i<stParamNum; i++)
			    {
					if (stProcessParamNum == stParamNum) 
						break;

				    if(!bBreak)
					    break;
					
					char type = arr[i];
				    switch(type)
				    {
				    case 'i':
					    {
                        	if(stProcessByte < sizeof(Int32))
                            {
	                            bBreak = false;
	                            break;
                            }

							Array.Clear(int32Buff, 0, sizeof(Int32));
							Array.Copy(pConstBuf, nProcessTotalByte + DecodeBytelen, int32Buff, 0, sizeof(Int32));
							Int32 Value = System.BitConverter.ToInt32(int32Buff, 0);
                            CallServer.m_OnLuaRecv.Push(Value);

                            DecodeBytelen += sizeof(Int32);
                            stProcessByte -= sizeof(Int32);
                            ++stProcessParamNum;
					    }break;
				    case 'I':
					    {
                            if(stProcessByte < sizeof(UInt32))
                            {
	                            bBreak = false;
	                            break;
                            }

							Array.Clear(int32Buff, 0, sizeof(Int32));
							Array.Copy(pConstBuf, nProcessTotalByte + DecodeBytelen, int32Buff, 0, sizeof(Int32));

							UInt32 Value = (UInt32)System.BitConverter.ToUInt32(int32Buff, 0);
                            CallServer.m_OnLuaRecv.Push(Value);

                            DecodeBytelen += sizeof(UInt32);
                            stProcessByte -= sizeof(UInt32);
                            ++stProcessParamNum;
					    }break;
				    case 'c':
					    {
                            if(stProcessByte < sizeof(byte))
                            {
	                            bBreak = false;
	                            break;
                            }

							byte Value = pConstBuf[nProcessTotalByte + DecodeBytelen];
                            CallServer.m_OnLuaRecv.Push(Value);

                            DecodeBytelen += sizeof(byte);
                            stProcessByte -= sizeof(byte);
                            ++stProcessParamNum;
					    }break;
				    case 'C':
					    {
                            if(stProcessByte < sizeof(byte))
                            {
	                            bBreak = false;
	                            break;
                            }

							byte Value = pConstBuf[nProcessTotalByte + DecodeBytelen];
                            CallServer.m_OnLuaRecv.Push(Value);

                            DecodeBytelen += sizeof(byte);
                            stProcessByte -= sizeof(byte);
                            ++stProcessParamNum;
					    }break;
				    case 'h':
					    {
                            if(stProcessByte < sizeof(Int16))
                            {
	                            bBreak = false;
	                            break;
                            }

							Array.Clear(int16Buff, 0, sizeof(Int16));
							Array.Copy(pConstBuf, nProcessTotalByte + DecodeBytelen, int16Buff, 0, sizeof(Int16));

							Int16 Value = System.BitConverter.ToInt16(int16Buff, 0);
                            CallServer.m_OnLuaRecv.Push(Value);

                            DecodeBytelen += sizeof(Int16);
                            stProcessByte -= sizeof(Int16);
                            ++stProcessParamNum;
					    }break;
				    case 'H':
					    {
                            if(stProcessByte < sizeof(UInt16))
                            {
	                            bBreak = false;
	                            break;
                            }

							Array.Clear(int16Buff, 0, sizeof(UInt16));
							Array.Copy(pConstBuf, nProcessTotalByte + DecodeBytelen, int16Buff, 0, sizeof(UInt16));

							UInt16 Value = System.BitConverter.ToUInt16(int16Buff, 0);
                            CallServer.m_OnLuaRecv.Push(Value);

                            DecodeBytelen += sizeof(UInt16);
                            stProcessByte -= sizeof(UInt16);
                            ++stProcessParamNum;
					    }break;
				    //case 'f':
				    //	{
				    //		CALL_RPC_FUNC_PUSH_NUMBER_PARAM(float);
				    //	}break;
				    case 'd':
					    {
                            if(stProcessByte < sizeof(Double))
                            {
	                            bBreak = false;
	                            break;
                            }

							Array.Clear(doubleBuff, 0, sizeof(Double));
							Array.Copy(pConstBuf, nProcessTotalByte + DecodeBytelen, doubleBuff, 0, sizeof(Double));

							Double Value = System.BitConverter.ToDouble(doubleBuff, 0);
                            CallServer.m_OnLuaRecv.Push(Value);

                            DecodeBytelen += sizeof(Double);
                            stProcessByte -= sizeof(Double);
                            ++stProcessParamNum;

					    }break;
				    case 'b':
					    {
						    if(stProcessByte < sizeof(Boolean))
						    {
							    bBreak = false;
							    break;
						    }

							byteBuff[0] = pConstBuf[nProcessTotalByte + DecodeBytelen];
							Boolean bValue = System.BitConverter.ToBoolean(byteBuff, 0);
                            CallServer.m_OnLuaRecv.Push(bValue);

                            DecodeBytelen +=sizeof(bool);
						    stProcessByte-=sizeof(bool);
						    ++stProcessParamNum;
					    }break;
				    case 's':
					    {
                        	if(stProcessByte < sizeof(byte))
                            {
	                            bBreak = false;
	                            break;
                            }

							byte uLen = pConstBuf[nProcessTotalByte + DecodeBytelen];

							DecodeBytelen+=sizeof(byte);
                            stProcessByte-=sizeof(byte);

                            if(stProcessByte<uLen)
                            {
	                            bBreak = false;
	                            break;
                            }
								
							Array.Clear(shortStringBuff, 0, 256);
							Array.Copy(pConstBuf, nProcessTotalByte + DecodeBytelen, shortStringBuff, 0, uLen);
                            CallServer.m_OnLuaRecv.PushByteBuffer(shortStringBuff, uLen);


                            DecodeBytelen +=uLen;
                            stProcessByte-=uLen;
                            ++stProcessParamNum;

					    }break;
				    case 'S':
					    {
                        	if(stProcessByte < sizeof(UInt16))
                            {
	                            bBreak = false;
	                            break;
                            }

							Array.Clear(int16Buff, 0, sizeof(UInt16));
							Array.Copy(pConstBuf, nProcessTotalByte + DecodeBytelen, int16Buff, 0, sizeof(UInt16));
							UInt16 uLen = System.BitConverter.ToUInt16(int16Buff, 0);

							DecodeBytelen+=sizeof(UInt16);
                            stProcessByte-=sizeof(UInt16);

                            if(stProcessByte<uLen)
                            {
	                            bBreak = false;
	                            break;
                            }

							Array.Clear(longStringBuff, 0, 65536);
							Array.Copy(pConstBuf, nProcessTotalByte + DecodeBytelen, longStringBuff, 0, uLen);
                            CallServer.m_OnLuaRecv.PushByteBuffer(longStringBuff, uLen);

                            DecodeBytelen +=uLen;
                            stProcessByte-=uLen;
	                        ++stProcessParamNum;

					    }break;
				    default:
					    {
							Debug.LogError ("RPC Function error :" + type+" PtlID=" + PtlID + " " +paramInfo.szName);
						    break;
					    }
				    }
			    }

                // RPC包没有收完就恢复现场
                if (stProcessParamNum < stParamNum) {
					Debug.LogWarning ("OnceUnPackProtocol not enougth: PtlID=" + PtlID + " " +paramInfo.szName+" "+ stProcessParamNum + " " + stParamNum);
                    CallServer.m_OnLuaRecv.EndPCall();

                    return 0;
				} else {

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                    string sMark = string.Format("CallLuaRpc {0}", paramInfo.szName);
                    Profile.BenchmarkBegin(sMark);
#endif

                    CallServer.m_OnLuaRecv.PCall();
                    CallServer.m_OnLuaRecv.EndPCall();

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                    Profile.BenchmarkEnd(sMark);
#endif

                    return DecodeBytelen;
				}
		    }
		    else // 严重error,ID不对
		    {
				Debug.LogError ("OnceUnPackProtocol err: PtlID=" + PtlID + " " + m_UnPackProtocols.Count);
			    return 0; // 链接错误，让外面断掉链接
		    }
        }


		//组装数据
		//packProtocolId是PackProtocols对于的ID
		public int PackProtocol(int packProtocolId, object[] args, ref byte[] strSendData, ref int packLen)
		{
			UInt16 uID = (UInt16)(packProtocolId);

			//获取对应lua函数
			ParamInfo paramInfo = m_PackProtocols[uID];
			int EncodeBytelen = 0;

			//push头
			byte[] pIDValue = System.BitConverter.GetBytes (uID);
			int len = pIDValue.Length;
			Array.Copy(pIDValue, 0, strSendData, EncodeBytelen, TYPE_SIZE);
			EncodeBytelen += (int)TYPE_SIZE;

			bool bCheck = true;
            char[] arr = paramInfo.szParamInfo;

            int stParamNum = arr.Length;
			for(int i=0; i<stParamNum; i++)
			{
				char szByte = arr[i];
				switch(szByte)
				{
				case 'i':
					{
						int value =  Convert.ToInt32(args [i]);
						byte[] b_value = System.BitConverter.GetBytes (value);
						Array.Copy(b_value, 0, strSendData, EncodeBytelen, sizeof(int));
						EncodeBytelen += sizeof(int);
					}break;
				case 'I':
					{
						UInt32 value = Convert.ToUInt32(args [i]);
						byte[] b_value = System.BitConverter.GetBytes (value);
						Array.Copy(b_value, 0, strSendData, EncodeBytelen, sizeof(UInt32));
						EncodeBytelen += sizeof(UInt32);
					}break;
				case 'h':
					{
						Int16 value = Convert.ToInt16(args [i]);
						byte[] b_value = System.BitConverter.GetBytes (value);
						int slen = b_value.Length;
						Array.Copy(b_value, 0, strSendData, EncodeBytelen, sizeof(Int16));
						EncodeBytelen += sizeof(Int16);

					}break;
				case 'H':
					{
						UInt16 value = Convert.ToUInt16(args [i]);
						byte[] b_value = System.BitConverter.GetBytes (value);
						int slen = b_value.Length;
						Array.Copy(b_value, 0, strSendData, EncodeBytelen, sizeof(UInt16));
						EncodeBytelen += sizeof(UInt16);
					}break;
				case 'c':
					{
						byte value = Convert.ToByte(args [i]);
						byte[] b_value = System.BitConverter.GetBytes (value);
						Array.Copy(b_value, 0, strSendData, EncodeBytelen, sizeof(byte));
						EncodeBytelen += sizeof(byte);

					}break;
				case 'C':
					{
						//uint8 
						sbyte value = Convert.ToSByte(args [i]);
						byte[] b_value = System.BitConverter.GetBytes (value);
						Array.Copy(b_value, 0, strSendData, EncodeBytelen, sizeof(byte));
						EncodeBytelen += sizeof(byte);
					}break;
				case 'f':
					{
						float value =  Convert.ToSingle(args [i]);
						byte[] b_value = System.BitConverter.GetBytes (value);
						int l = b_value.Length;
						Array.Copy(b_value, 0, strSendData, EncodeBytelen, sizeof(float));
						EncodeBytelen += sizeof(float);

					}break;
				case 'd':
					{
						double value =  Convert.ToDouble(args [i]);
						byte[] b_value = System.BitConverter.GetBytes (value);
						int l = b_value.Length;
						Array.Copy(b_value, 0, strSendData, EncodeBytelen, sizeof(double));
						EncodeBytelen += sizeof(double);

					}break;
				case 'b':
					{
						Boolean value = Convert.ToBoolean(args [i]);
						byte[] b_value = System.BitConverter.GetBytes (value);
						Array.Copy(b_value, 0, strSendData, EncodeBytelen, sizeof(Boolean));
						EncodeBytelen += sizeof(Boolean);
					}break;
				case 's':
					{
						//PACK_PROTOCOL_CHECK_PARAM_STRING;
						string value = Convert.ToString(args [i]);
                        byte[] b_data = Encoding.UTF8.GetBytes(value);

                        byte slen = (byte)b_data.Length;
						byte[] b_value = System.BitConverter.GetBytes (slen);
						Array.Copy(b_value, 0, strSendData, EncodeBytelen, sizeof(byte));
						EncodeBytelen += sizeof(byte);

						//b_value = System.BitConverter.GetBytes (value);
						Array.Copy(b_data, 0, strSendData, EncodeBytelen, b_data.Length);
						EncodeBytelen += b_data.Length;

					}break;
				case 'S':
					{
                        string value = Convert.ToString(args[i]);
                        byte[] b_data = Encoding.UTF8.GetBytes(value);

                        UInt16 slen = (UInt16)b_data.Length;
                        byte[] b_value = System.BitConverter.GetBytes(slen);
                        Array.Copy(b_value, 0, strSendData, EncodeBytelen, sizeof(UInt16));
                        EncodeBytelen += sizeof(UInt16);

                        //b_value = System.BitConverter.GetBytes (value);
                        Array.Copy(b_data, 0, strSendData, EncodeBytelen, b_data.Length);
                        EncodeBytelen += b_data.Length;

					}break;
				default:
					{
						Debug.LogError ("PackProtocol switch err: szByte=" + szByte);
						bCheck = false;
						return -1;
					}
				}
				++szByte;
			}

			if(bCheck)
			{
				packLen = EncodeBytelen;
				return 0;
			}

			Debug.LogError ("PackProtocol err: uID=" + uID);
			// 结束的时候必须要做的处理
			return -1;
		}
	}

}
