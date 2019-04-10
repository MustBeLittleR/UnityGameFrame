using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Runtime.InteropServices;


namespace WCG
{
	public class ConverterMgr
	{
        private static Int32 nBuffSize = 128;
        private static IntPtr mBuffer = Marshal.AllocHGlobal(nBuffSize);

        public static ConverterMgr instance = new ConverterMgr();
        public Byte[] StructToBytes(System.Object structure)
		{
            Int32 size = Marshal.SizeOf(structure);
            if (size > nBuffSize)
            {
                mBuffer = Marshal.ReAllocHGlobal(mBuffer, (IntPtr)size);
                nBuffSize = size;
            }

            //Console.WriteLine(size);
            //IntPtr buffer = Marshal.AllocHGlobal(size);
            try
			{
				Marshal.StructureToPtr(structure, mBuffer, false);
				Byte[] bytes = new Byte[size];
				Marshal.Copy(mBuffer, bytes, 0, size);
				return bytes;
			}
			finally
			{
				//Marshal.FreeHGlobal(buffer);
			}
		}

        public System.Object BytesToStruct(ref Byte[] bytes, Type strcutType, int offset)
		{
			Int32 size = Marshal.SizeOf(strcutType);

            if(size > nBuffSize)
            {
                mBuffer = Marshal.ReAllocHGlobal(mBuffer, (IntPtr)size);
                nBuffSize = size;
            }
			//IntPtr buffer = Marshal.AllocHGlobal(size);
			try
			{
				Marshal.Copy(bytes, offset, mBuffer, size);
				return Marshal.PtrToStructure(mBuffer, strcutType);
			}
			finally
			{
				//Marshal.FreeHGlobal(buffer);
			}
		}

        public byte[] ConvertEnumToByteArray(Int16 iType)
        {
            Byte[] bytes = new Byte[1];
            //bytes[0] = (byte)(iType & 0xFF);
            bytes[0] = Convert.ToByte(iType);
            return bytes;
        }
    }
}

