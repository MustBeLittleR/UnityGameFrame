using UnityEngine;
using System.Collections.Generic;
using LuaInterface;
using System.Collections;
using System.IO;
using System;
using WCG;
#if UNITY_5_4_OR_NEWER
using UnityEngine.SceneManagement;
#endif

public class LuaScriptMgr
{
	public static readonly LuaScriptMgr Instance = new LuaScriptMgr();
	protected LuaState luaState = null;
	protected LuaLooperEx loop = null;
	protected LuaFunction levelLoaded = null;
	bool m_bInit = false;

	protected virtual LuaFileUtils InitLoader()
	{
		Debug.Assert(m_bInit == false);
		m_bInit = true;
		new LuaResLoaderEx();
		return LuaFileUtils.Instance;
	}

	public void Init()
	{
		InitLoader();
		luaState = new LuaState();
		OpenLibs();
		luaState.LuaSetTop(0);
		Bind();
		LoadLuaFiles();

#if UNITY_5_4_OR_NEWER
		SceneManager.sceneLoaded += OnSceneLoaded;
#endif
	}

	protected virtual void OpenLibs()
	{
		luaState.OpenLibs(LuaDLL.luaopen_pb);
		luaState.OpenLibs(LuaDLL.luaopen_struct);
		luaState.OpenLibs(LuaDLL.luaopen_lpeg);
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        luaState.OpenLibs(LuaDLL.luaopen_bit);
#endif
		if (LuaConst.openLuaSocket)
			OpenLuaSocket();

		if (LuaConst.openLuaDebugger)
			OpenZbsDebugger();
	}

	protected void OpenLuaSocket()
	{
		LuaConst.openLuaSocket = true;

		luaState.BeginPreLoad();
		luaState.RegFunction("socket.core", LuaOpen_Socket_Core);
		luaState.RegFunction("mime.core", LuaOpen_Mime_Core);
		luaState.EndPreLoad();
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int LuaOpen_Socket_Core(IntPtr L)
	{
		return LuaDLL.luaopen_socket_core(L);
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int LuaOpen_Mime_Core(IntPtr L)
	{
		return LuaDLL.luaopen_mime_core(L);
	}

	public void OpenZbsDebugger(string ip = "localhost")
	{
		if (!Directory.Exists(LuaConst.zbsDir))
		{
			Debugger.LogWarning("ZeroBraneStudio not install or LuaConst.zbsDir not right");
			return;
		}

		if (!LuaConst.openLuaSocket)
			OpenLuaSocket();

		if (!string.IsNullOrEmpty(LuaConst.zbsDir))
			luaState.AddSearchPath(LuaConst.zbsDir);

		luaState.LuaDoString(string.Format("DebugServerIp = '{0}'", ip));
	}

	protected virtual void Bind()
	{
		LuaBinder.Bind(luaState);
		DelegateFactory.Init();
		LuaCoroutine.Register(luaState, GameMgr.Instance);
	}

	protected virtual void LoadLuaFiles()
	{
		OnLoadFinished();
	}

	protected virtual void OnLoadFinished()
	{
		luaState.Start();
		StartLooper();
		StartMain();
	}

	protected void StartLooper()
	{
		if (null == loop)
		{
			loop = new LuaLooperEx();
			loop.Init(luaState);
		}
	}

	protected virtual void StartMain()
	{
		luaState.DoFile("Gac/GameMgr.lua");
		GameMgr.Instance.InitLuaCallBackFunc();
		CsRpcWrap.RegisterCSLuaFunc();
		//levelLoaded = luaState.GetFunction("OnLevelWasLoaded");
		CallMain();
	}

	protected virtual void CallMain()
	{
		LuaFunction main = luaState.GetFunction("gGameMgr.Main");
		main.Call();
		main.Dispose();
		main = null;
	}

#if UNITY_5_4_OR_NEWER
	void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		OnLevelLoaded(scene.buildIndex);
	}
#else
    protected void OnLevelWasLoaded(int level)
    {
        OnLevelLoaded(level);
    }
#endif

	void OnLevelLoaded(int level)
	{
		if (levelLoaded != null)
		{
			levelLoaded.BeginPCall();
			levelLoaded.Push(level);
			levelLoaded.PCall();
			levelLoaded.EndPCall();
		}

		if (luaState != null)
		{
			luaState.RefreshDelegateMap();
		}
	}

	public void Destroy()
	{
		if (luaState != null)
		{
#if UNITY_5_4_OR_NEWER
			SceneManager.sceneLoaded -= OnSceneLoaded;
#endif
			LuaState state = luaState;
			luaState = null;

			if (levelLoaded != null)
			{
				levelLoaded.Dispose();
				levelLoaded = null;
			}

			if (loop != null)
			{
				loop.Destroy();
				loop = null;
			}

			CsRpcWrap.DisposeCSLuaFunc();
			state.Dispose();
		}
	}

	//Add_Ex 必须保证 loop 和 luaState 的有效性
	public void Update()
	{
		loop.Update();
	}
	public void LateUpdate()
	{
		loop.LateUpdate();
	}
	public void FixedUpdate()
	{
		loop.FixedUpdate();
	}
	public void PrintLuaUsedMem()
	{
		luaState.DoString("print(\"lua use mem :\" .. collectgarbage(\"count\")/1024 .. \" mb\")", "LuaScriptMgr.cs");
	}
	public string GetStackString()
	{
		int oldTop = luaState.LuaGetTop();
		luaState.LuaGetGlobal("debug");
		luaState.LuaPushString("traceback");
		luaState.LuaRawGet(-2);
		luaState.LuaCall(0, 1);
		string sRet = luaState.LuaToString(-1);
		luaState.LuaSetTop(oldTop);
		return sRet;
	}
	public LuaFunction GetLuaFunction(string name, bool beLogMiss = true)
	{
		return luaState.GetFunction(name, beLogMiss);
	}
	public void LuaGC(params string[] param)
	{
        luaState.LuaGC(LuaGCOptions.LUA_GCRESTART, 0);
        luaState.LuaGC(LuaGCOptions.LUA_GCCOLLECT, 0);
        luaState.Collect();
        luaState.LuaGC(LuaGCOptions.LUA_GCSTOP, 0);
    }
	public void DoString(string chunk)
	{
		luaState.DoString(chunk);
	}
	public T DoString<T>(string chunk)
	{
		return luaState.DoString<T>(chunk);
	}
	
}