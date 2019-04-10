/*
Copyright (c) 2015-2017 topameng(topameng@qq.com)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
//优先读取persistentDataPath/系统/Lua 目录下的文件（默认下载目录）
//未找到文件怎读取 Resources/Lua 目录下文件（仍没有使用LuaFileUtil读取）
using UnityEngine;
using LuaInterface;
using System.IO;
using System.Text;
using WCG;

public class LuaResLoaderEx : LuaFileUtils
{
	public static RunMode m_eRunMode = GameConfigMgr.ms_eArchive;
	public LuaResLoaderEx()
	{
		instance = this;
		beZip = false;
	}

	public override string FindFile(string fileName)
	{
		if (m_eRunMode == RunMode.eRelease)
			return Util.GetLuaPath(fileName);

		return base.FindFile(fileName);
	}

	public override byte[] ReadFile(string fileName)
	{
		if (m_eRunMode == RunMode.eRelease)
			return Util.GetLuaBytes(FindFile(fileName));

		return base.ReadFile(fileName);
	}
}
