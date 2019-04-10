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


//public delegate void InceptData(byte[] getBytes, int getType);
//public delegate void SocketOfConnect(int type);

namespace WCG
{
    public class TCPClient
    {
        public enum SocketState
        {
            Init = 0,
            Connecting = 1,
            Connectted_Callback = 2,
            Connectted = 3,
            ConnectFailed_Callback = 4,
            ConnectFailed = 5,
            SocketError = 6,
        };

        private int m_SocketState = (int)SocketState.Init;
        private string m_SocketErr = "";
        private bool m_bReConnect = false;
        private string m_Ip;
        private int m_nPort;

        private const int m_nBufferSize = 1024 * 1024 * 2;
        private byte[] m_aryRecvBuf = new byte[m_nBufferSize];
        private byte[] m_aryDataBuf = new byte[m_nBufferSize];
        private int m_nDataLen = 0;
        private int m_nRecvDataOnce = 0;

        private bool m_bIsConnected = false;

        public Socket m_Socket = null;

        AsyncCallback m_CbConnected = null;
        AsyncCallback m_CbConnectFailed = null;
        AsyncCallback m_CbDisConnected = null;

        public TCPClient()
        {
        }

        public void Connect(string sIP, int nPort, AsyncCallback CbConnected, AsyncCallback CbConnectFailed, AsyncCallback cbDisConnected)
        {
            if (m_SocketState != (int)SocketState.Init
                && m_SocketState != (int)SocketState.ConnectFailed
                && m_SocketState != (int)SocketState.SocketError)
            {
                //Debug.LogError("socket in use!!!!!!");
                if (m_Socket != null) {
                    m_Socket.Close();
                }
                return;
            }

            

            Interlocked.Exchange(ref m_SocketState, (int)SocketState.Connecting);
            Debug.Log("Socket Connecting : " + sIP + " : " + nPort);
            m_Ip = sIP;
            m_nPort = nPort;

            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(sIP), nPort);
            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_CbConnected = CbConnected;
            m_CbConnectFailed = CbConnectFailed;
            m_CbDisConnected = cbDisConnected;

            IAsyncResult result = m_Socket.BeginConnect(ipe, new AsyncCallback(connectCallback), null);
        }


        /// 判断connected的方法，但未检测对端网线断开或ungraceful的情况
		/// https://stackoverflow.com/questions/2661764/how-to-check-if-a-socket-is-connected-disconnected-in-c
        static bool IsSocketConnected(Socket s)
        {
			bool part1 = s.Poll(1000, SelectMode.SelectRead);
			bool part2 = (s.Available == 0);
			if ((part1 && part2) || !s.Connected)
				return false;
			else
				return true;
        }

        public void DispatchEvent()
        {
            switch (m_SocketState)
            {
                case (int)SocketState.Connectted_Callback:
                    if (m_Socket.Connected)
                    {
                        Interlocked.Exchange(ref m_SocketState, (int)SocketState.Connectted);
                        //Debug.Log("connect Success :" + m_Ip + " : " + m_nPort);
                        //与socket建立连接成功，开启线程接受服务端数据。
                        receive();
                        m_CbConnected(null);
                        m_bIsConnected = true;
                    }
                    else
                    {
                        Interlocked.Exchange(ref m_SocketState, (int)SocketState.ConnectFailed);
                        Debug.Log("connect failed :" + m_Ip + " : " + m_nPort + "error code :");
                        m_CbConnectFailed(null);
                    }
                    break;
                case (int)SocketState.ConnectFailed_Callback:
                    Interlocked.Exchange(ref m_SocketState, (int)SocketState.ConnectFailed);
                    //Debug.Log("connect failed :" + m_Ip + " : " + m_nPort);
                    m_CbConnectFailed(null);
                    break;
                case (int)SocketState.SocketError:
                    Close();
                    m_CbDisConnected(null);
                    break;
                case (int)SocketState.Connectted:
                    int nReadSize = Interlocked.Exchange(ref m_nRecvDataOnce, 0);
                    if (nReadSize > 0)
                    {
                        //Debug.Log("recv data size = " + nReadSize);
                        if (m_nDataLen + nReadSize > m_nBufferSize)
                        {
                            //超过缓冲区大小，直接shutdown
                            Close();
                            m_CbDisConnected(null);
                        }
                        else
                        {
                            Array.Copy(m_aryRecvBuf, 0, m_aryDataBuf, m_nDataLen, nReadSize);
                            m_nDataLen += nReadSize;
                            //Debug.Log("recv data totoal size = " + m_nDataLen);

                            /*Byte[] tempArr = new Byte[128];
                            Array.Copy(m_aryRecvBuf, tempArr, 128);
                            Byte[] tempArrEx = new Byte[128];
                            Array.Copy(m_aryDataBuf, tempArrEx, 128);
                            Debug.LogFormat("DispatchEvent-------:{0},{1}\n{2}\n{3} ", nReadSize, m_nDataLen, BitConverter.ToString(tempArr), BitConverter.ToString(tempArrEx));*/
                        }

                        receive();
                    }
                    else
					{
                        //bool isConnected = IsSocketConnected(m_Socket);
                        //if (m_bIsConnected && !isConnected)
						if (!m_Socket.Connected)
						{
                            Close();
                            m_CbDisConnected(null);
							m_bIsConnected = false;
                        }
                    }
                    break;
                default:
                    break;
            }
            //call back)
        }

        //向服务端发送数据包
        public bool Send(byte[] aryData)
        {
            if (null == m_Socket)
                return false;

            if (!m_Socket.Connected)
            {
				return false;
            }
            try
            {
                //向服务端异步发送这个字节数组
                IAsyncResult asyncSend = m_Socket.BeginSend(aryData, 0, aryData.Length, SocketFlags.None, new AsyncCallback(sendCallback), m_Socket);
                //int value = Convert.ToInt32(asyncSend.AsyncState);
                //Debug.Log("############### Send: " + asyncSend.AsyncState.ToString());
            }
            catch (Exception e)
            {
                CloseOnException("BeginSend", e);
                return false;
            }

            return true;
        }


        public void receive()
        {
            try
            {
                m_aryRecvBuf.Initialize();
                m_Socket.BeginReceive(m_aryRecvBuf, 0, m_nBufferSize, 0, new AsyncCallback(receivedCallback), null);
            }
            catch (Exception e)
            {
                CloseOnException("BeginReceive", e);
            }
        }

        public void PopRecvData(int nSize)
        {
            /*Byte[] tempArr = new Byte[128];
            Array.Copy(m_aryDataBuf, tempArr, 128);
            Debug.LogFormat("PopRecvData---1---:{0},{1}\n{2} ", m_nDataLen, nSize, BitConverter.ToString(tempArr));*/

                            if (m_nDataLen == nSize)
            {
                Array.Clear(m_aryDataBuf, 0, m_nBufferSize);
                m_nDataLen = 0;
            }
            else if (nSize < m_nDataLen)
            {
                for (int n = 0; n < m_nDataLen - nSize; n++)
                {
                    m_aryDataBuf[n] = m_aryDataBuf[nSize + n];
                }
                m_nDataLen -= nSize;
            }

            /*tempArr.Initialize();
            Array.Copy(m_aryDataBuf, tempArr, 128);
            Debug.LogFormat("PopRecvData---2---:{0},{1}\n{2} ", m_nDataLen, nSize, BitConverter.ToString(tempArr));*/
        }

        public byte[] GetRecvData()
        {
            return m_aryDataBuf;
        }

        public int GetRecvDataSize()
        {
            return m_nDataLen;
        }

        private void connectCallback(IAsyncResult asyncConnect)
        {
            if (m_Socket.Connected)
            {
                Interlocked.Exchange(ref m_SocketState, (int)SocketState.Connectted_Callback);
            }
            else
            {
                Interlocked.Exchange(ref m_SocketState, (int)SocketState.ConnectFailed_Callback);
            }
        }

        private void sendCallback(IAsyncResult ar)
        {
			/*
            try
            {
                // Retrieve the socket from the state object.
                //Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                //int bytesSent = client.EndSend(ar);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            */
            try
            {
                Socket client = (Socket)ar.AsyncState;
                int nSendByte = client.EndSend(ar);
                if (nSendByte == 0)
                {
                    Interlocked.Exchange(ref m_SocketState, (int)SocketState.ConnectFailed_Callback);
                }
            }
            catch (Exception e)
            {
                CloseOnException("sendCallback", e);
            }


        }

        public void receivedCallback(IAsyncResult ar)
        {
            try
            {
                if (!m_Socket.Connected)
                {
                    return;
                }

                int nReadSize = m_Socket.EndReceive(ar);
                Interlocked.Exchange(ref m_nRecvDataOnce, nReadSize);
                //Debug.Assert(nOldSize == 0);
            }
            catch (Exception e)
            {
                CloseOnException("receivedCallback", e);
            }
        }

        private void CloseOnException(string errinfo, Exception e)
        {
            Interlocked.Exchange(ref m_SocketState, (int)SocketState.SocketError);
            m_SocketErr = errinfo + ":" + e.ToString();
        }

        //关闭Socket
        public void Close()
        {
            Interlocked.Exchange(ref m_SocketState, (int)SocketState.Init);
            m_SocketErr = "";
            m_nDataLen = 0;
            m_nRecvDataOnce = 0;
            m_aryRecvBuf.Initialize();
            m_aryDataBuf.Initialize();

            try
            {
                if (m_Socket != null)
                {
                    if (m_Socket.Connected)
                    {
                        m_Socket.Shutdown(SocketShutdown.Both);
                    }
                    m_Socket.Close();
                }
                m_Socket = null;
            }
            catch (Exception e)
            {
				m_Socket.Close();
                m_Socket = null;
			}
		}
    }
}