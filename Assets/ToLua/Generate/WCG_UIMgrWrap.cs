﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class WCG_UIMgrWrap
{
	public static void Register(LuaState L)
	{
		L.BeginClass(typeof(WCG.UIMgr), typeof(System.Object));
		L.RegFunction("Init", Init);
		L.RegFunction("Update", Update);
		L.RegFunction("Destroy", Destroy);
		L.RegFunction("GetUIRootDy", GetUIRootDy);
		L.RegFunction("GetUIRootRect", GetUIRootRect);
		L.RegFunction("GetRootXYRate", GetRootXYRate);
		L.RegFunction("GetUICamera", GetUICamera);
		L.RegFunction("UICameraSizeAdapt", UICameraSizeAdapt);
		L.RegFunction("GetWindow", GetWindow);
		L.RegFunction("CreateUI", CreateUI);
		L.RegFunction("DestroyUI", DestroyUI);
		L.RegFunction("CreateUIByCache", CreateUIByCache);
		L.RegFunction("MoveToFront", MoveToFront);
		L.RegFunction("GetWinScreenPos", GetWinScreenPos);
		L.RegFunction("SetAnchorPos", SetAnchorPos);
		L.RegFunction("ResizeWin", ResizeWin);
		L.RegFunction("GetWinPixelSize", GetWinPixelSize);
		L.RegFunction("SetColorGrey", SetColorGrey);
		L.RegFunction("DropColor", DropColor);
		L.RegFunction("RecoverColor", RecoverColor);
		L.RegFunction("SetAllLineRenderer", SetAllLineRenderer);
		L.RegFunction("ApplyParentLayer", ApplyParentLayer);
		L.RegFunction("SetLayer", SetLayer);
		L.RegFunction("SetText", SetText);
		L.RegFunction("SetTextColor", SetTextColor);
		L.RegFunction("SetImage", SetImage);
		L.RegFunction("SetImageAsyn", SetImageAsyn);
		L.RegFunction("ClearImage", ClearImage);
		L.RegFunction("SetImageColor", SetImageColor);
		L.RegFunction("SetSprite", SetSprite);
		L.RegFunction("SetSpriteAsyn", SetSpriteAsyn);
		L.RegFunction("SetSpriteColor", SetSpriteColor);
		L.RegFunction("SetParentNULL", SetParentNULL);
		L.RegFunction("DropdownAddOptions", DropdownAddOptions);
		L.RegFunction("SetAnimationRate", SetAnimationRate);
		L.RegFunction("New", _CreateWCG_UIMgr);
		L.RegFunction("__tostring", ToLua.op_ToString);
		L.RegVar("Instance", get_Instance, null);
		L.EndClass();
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int _CreateWCG_UIMgr(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 0)
			{
				WCG.UIMgr obj = new WCG.UIMgr();
				ToLua.PushObject(L, obj);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to ctor method: WCG.UIMgr.New");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Init(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			WCG.UIMgr obj = (WCG.UIMgr)ToLua.CheckObject<WCG.UIMgr>(L, 1);
			obj.Init();
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Update(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			WCG.UIMgr obj = (WCG.UIMgr)ToLua.CheckObject<WCG.UIMgr>(L, 1);
			obj.Update();
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Destroy(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			WCG.UIMgr obj = (WCG.UIMgr)ToLua.CheckObject<WCG.UIMgr>(L, 1);
			obj.Destroy();
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetUIRootDy(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			WCG.UIMgr obj = (WCG.UIMgr)ToLua.CheckObject<WCG.UIMgr>(L, 1);
			UnityEngine.GameObject o = obj.GetUIRootDy();
			ToLua.PushSealed(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetUIRootRect(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			WCG.UIMgr obj = (WCG.UIMgr)ToLua.CheckObject<WCG.UIMgr>(L, 1);
			UnityEngine.Rect o = obj.GetUIRootRect();
			ToLua.PushValue(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetRootXYRate(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			WCG.UIMgr obj = (WCG.UIMgr)ToLua.CheckObject<WCG.UIMgr>(L, 1);
			UnityEngine.Vector2 o = obj.GetRootXYRate();
			ToLua.Push(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetUICamera(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			WCG.UIMgr obj = (WCG.UIMgr)ToLua.CheckObject<WCG.UIMgr>(L, 1);
			UnityEngine.Camera o = obj.GetUICamera();
			ToLua.PushSealed(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int UICameraSizeAdapt(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			WCG.UIMgr obj = (WCG.UIMgr)ToLua.CheckObject<WCG.UIMgr>(L, 1);
			obj.UICameraSizeAdapt();
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetWindow(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 3);
			WCG.UIMgr obj = (WCG.UIMgr)ToLua.CheckObject<WCG.UIMgr>(L, 1);
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 2, typeof(UnityEngine.GameObject));
			string arg1 = ToLua.CheckString(L, 3);
			UnityEngine.GameObject o = obj.GetWindow(arg0, arg1);
			ToLua.PushSealed(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int CreateUI(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 2)
			{
				WCG.UIMgr obj = (WCG.UIMgr)ToLua.CheckObject<WCG.UIMgr>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				UnityEngine.GameObject o = obj.CreateUI(arg0);
				ToLua.PushSealed(L, o);
				return 1;
			}
			else if (count == 3)
			{
				WCG.UIMgr obj = (WCG.UIMgr)ToLua.CheckObject<WCG.UIMgr>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				UnityEngine.GameObject arg1 = (UnityEngine.GameObject)ToLua.CheckObject(L, 3, typeof(UnityEngine.GameObject));
				UnityEngine.GameObject o = obj.CreateUI(arg0, arg1);
				ToLua.PushSealed(L, o);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: WCG.UIMgr.CreateUI");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int DestroyUI(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			WCG.UIMgr obj = (WCG.UIMgr)ToLua.CheckObject<WCG.UIMgr>(L, 1);
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 2, typeof(UnityEngine.GameObject));
			obj.DestroyUI(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int CreateUIByCache(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 3);
			WCG.UIMgr obj = (WCG.UIMgr)ToLua.CheckObject<WCG.UIMgr>(L, 1);
			string arg0 = ToLua.CheckString(L, 2);
			UnityEngine.GameObject arg1 = (UnityEngine.GameObject)ToLua.CheckObject(L, 3, typeof(UnityEngine.GameObject));
			UnityEngine.GameObject o = obj.CreateUIByCache(arg0, arg1);
			ToLua.PushSealed(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int MoveToFront(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			WCG.UIMgr obj = (WCG.UIMgr)ToLua.CheckObject<WCG.UIMgr>(L, 1);
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 2, typeof(UnityEngine.GameObject));
			obj.MoveToFront(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetWinScreenPos(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			WCG.UIMgr obj = (WCG.UIMgr)ToLua.CheckObject<WCG.UIMgr>(L, 1);
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 2, typeof(UnityEngine.GameObject));
			UnityEngine.Vector2 o = obj.GetWinScreenPos(arg0);
			ToLua.Push(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetAnchorPos(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 4);
			WCG.UIMgr obj = (WCG.UIMgr)ToLua.CheckObject<WCG.UIMgr>(L, 1);
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 2, typeof(UnityEngine.GameObject));
			float arg1 = (float)LuaDLL.luaL_checknumber(L, 3);
			float arg2 = (float)LuaDLL.luaL_checknumber(L, 4);
			obj.SetAnchorPos(arg0, arg1, arg2);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int ResizeWin(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 4);
			WCG.UIMgr obj = (WCG.UIMgr)ToLua.CheckObject<WCG.UIMgr>(L, 1);
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 2, typeof(UnityEngine.GameObject));
			float arg1 = (float)LuaDLL.luaL_checknumber(L, 3);
			float arg2 = (float)LuaDLL.luaL_checknumber(L, 4);
			obj.ResizeWin(arg0, arg1, arg2);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetWinPixelSize(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			WCG.UIMgr obj = (WCG.UIMgr)ToLua.CheckObject<WCG.UIMgr>(L, 1);
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 2, typeof(UnityEngine.GameObject));
			UnityEngine.Vector2 o = obj.GetWinPixelSize(arg0);
			ToLua.Push(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetColorGrey(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			WCG.UIMgr obj = (WCG.UIMgr)ToLua.CheckObject<WCG.UIMgr>(L, 1);
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 2, typeof(UnityEngine.GameObject));
			obj.SetColorGrey(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int DropColor(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			WCG.UIMgr obj = (WCG.UIMgr)ToLua.CheckObject<WCG.UIMgr>(L, 1);
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 2, typeof(UnityEngine.GameObject));
			obj.DropColor(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int RecoverColor(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			WCG.UIMgr obj = (WCG.UIMgr)ToLua.CheckObject<WCG.UIMgr>(L, 1);
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 2, typeof(UnityEngine.GameObject));
			obj.RecoverColor(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetAllLineRenderer(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 3);
			WCG.UIMgr obj = (WCG.UIMgr)ToLua.CheckObject<WCG.UIMgr>(L, 1);
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 2, typeof(UnityEngine.GameObject));
			float arg1 = (float)LuaDLL.luaL_checknumber(L, 3);
			obj.SetAllLineRenderer(arg0, arg1);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int ApplyParentLayer(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 1, typeof(UnityEngine.GameObject));
			WCG.UIMgr.ApplyParentLayer(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetLayer(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 1, typeof(UnityEngine.GameObject));
			int arg1 = (int)LuaDLL.luaL_checknumber(L, 2);
			WCG.UIMgr.SetLayer(arg0, arg1);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetText(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 3);
			WCG.UIMgr obj = (WCG.UIMgr)ToLua.CheckObject<WCG.UIMgr>(L, 1);
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 2, typeof(UnityEngine.GameObject));
			string arg1 = ToLua.CheckString(L, 3);
			obj.SetText(arg0, arg1);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetTextColor(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 5);
			WCG.UIMgr obj = (WCG.UIMgr)ToLua.CheckObject<WCG.UIMgr>(L, 1);
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 2, typeof(UnityEngine.GameObject));
			int arg1 = (int)LuaDLL.luaL_checknumber(L, 3);
			int arg2 = (int)LuaDLL.luaL_checknumber(L, 4);
			int arg3 = (int)LuaDLL.luaL_checknumber(L, 5);
			obj.SetTextColor(arg0, arg1, arg2, arg3);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetImage(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 4);
			WCG.UIMgr obj = (WCG.UIMgr)ToLua.CheckObject<WCG.UIMgr>(L, 1);
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 2, typeof(UnityEngine.GameObject));
			string arg1 = ToLua.CheckString(L, 3);
			string arg2 = ToLua.CheckString(L, 4);
			obj.SetImage(arg0, arg1, arg2);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetImageAsyn(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 4);
			WCG.UIMgr obj = (WCG.UIMgr)ToLua.CheckObject<WCG.UIMgr>(L, 1);
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 2, typeof(UnityEngine.GameObject));
			string arg1 = ToLua.CheckString(L, 3);
			string arg2 = ToLua.CheckString(L, 4);
			obj.SetImageAsyn(arg0, arg1, arg2);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int ClearImage(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			WCG.UIMgr obj = (WCG.UIMgr)ToLua.CheckObject<WCG.UIMgr>(L, 1);
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 2, typeof(UnityEngine.GameObject));
			obj.ClearImage(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetImageColor(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 6);
			WCG.UIMgr obj = (WCG.UIMgr)ToLua.CheckObject<WCG.UIMgr>(L, 1);
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 2, typeof(UnityEngine.GameObject));
			float arg1 = (float)LuaDLL.luaL_checknumber(L, 3);
			float arg2 = (float)LuaDLL.luaL_checknumber(L, 4);
			float arg3 = (float)LuaDLL.luaL_checknumber(L, 5);
			float arg4 = (float)LuaDLL.luaL_checknumber(L, 6);
			obj.SetImageColor(arg0, arg1, arg2, arg3, arg4);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetSprite(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 4);
			WCG.UIMgr obj = (WCG.UIMgr)ToLua.CheckObject<WCG.UIMgr>(L, 1);
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 2, typeof(UnityEngine.GameObject));
			string arg1 = ToLua.CheckString(L, 3);
			string arg2 = ToLua.CheckString(L, 4);
			obj.SetSprite(arg0, arg1, arg2);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetSpriteAsyn(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 4);
			WCG.UIMgr obj = (WCG.UIMgr)ToLua.CheckObject<WCG.UIMgr>(L, 1);
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 2, typeof(UnityEngine.GameObject));
			string arg1 = ToLua.CheckString(L, 3);
			string arg2 = ToLua.CheckString(L, 4);
			obj.SetSpriteAsyn(arg0, arg1, arg2);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetSpriteColor(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 6);
			WCG.UIMgr obj = (WCG.UIMgr)ToLua.CheckObject<WCG.UIMgr>(L, 1);
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 2, typeof(UnityEngine.GameObject));
			float arg1 = (float)LuaDLL.luaL_checknumber(L, 3);
			float arg2 = (float)LuaDLL.luaL_checknumber(L, 4);
			float arg3 = (float)LuaDLL.luaL_checknumber(L, 5);
			float arg4 = (float)LuaDLL.luaL_checknumber(L, 6);
			obj.SetSpriteColor(arg0, arg1, arg2, arg3, arg4);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetParentNULL(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			WCG.UIMgr obj = (WCG.UIMgr)ToLua.CheckObject<WCG.UIMgr>(L, 1);
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 2, typeof(UnityEngine.GameObject));
			obj.SetParentNULL(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int DropdownAddOptions(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 3);
			WCG.UIMgr obj = (WCG.UIMgr)ToLua.CheckObject<WCG.UIMgr>(L, 1);
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 2, typeof(UnityEngine.GameObject));
			object[] arg1 = ToLua.CheckObjectArray(L, 3);
			obj.DropdownAddOptions(arg0, arg1);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetAnimationRate(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 4);
			WCG.UIMgr obj = (WCG.UIMgr)ToLua.CheckObject<WCG.UIMgr>(L, 1);
			UnityEngine.Animation arg0 = (UnityEngine.Animation)ToLua.CheckObject(L, 2, typeof(UnityEngine.Animation));
			string arg1 = ToLua.CheckString(L, 3);
			float arg2 = (float)LuaDLL.luaL_checknumber(L, 4);
			obj.SetAnimationRate(arg0, arg1, arg2);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_Instance(IntPtr L)
	{
		try
		{
			ToLua.PushObject(L, WCG.UIMgr.Instance);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}
}
