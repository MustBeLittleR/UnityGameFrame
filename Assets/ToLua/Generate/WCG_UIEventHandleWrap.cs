﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class WCG_UIEventHandleWrap
{
	public static void Register(LuaState L)
	{
		L.BeginClass(typeof(WCG.UIEventHandle), typeof(UnityEngine.MonoBehaviour));
		L.RegFunction("Get", Get);
		L.RegFunction("EventDelegate", EventDelegate);
		L.RegFunction("IntDelegate", IntDelegate);
		L.RegFunction("StringDelegate", StringDelegate);
		L.RegFunction("FloatDelegate", FloatDelegate);
		L.RegFunction("BoolDelegate", BoolDelegate);
		L.RegFunction("Vec2Delegate", Vec2Delegate);
		L.RegFunction("BaseEventDelegate", BaseEventDelegate);
		L.RegFunction("AddUIEvent", AddUIEvent);
		L.RegFunction("AddButtonClick", AddButtonClick);
		L.RegFunction("AddInputFieldEndEdit", AddInputFieldEndEdit);
		L.RegFunction("AddInputFieldValueChange", AddInputFieldValueChange);
		L.RegFunction("AddScrollbarValueChange", AddScrollbarValueChange);
		L.RegFunction("AddScrollRectValueChange", AddScrollRectValueChange);
		L.RegFunction("AddSliderValueChange", AddSliderValueChange);
		L.RegFunction("AddToggleValueChange", AddToggleValueChange);
		L.RegFunction("AddDropdownValueChange", AddDropdownValueChange);
		L.RegFunction("__eq", op_Equality);
		L.RegFunction("__tostring", ToLua.op_ToString);
		L.RegVar("parameter", get_parameter, set_parameter);
		L.EndClass();
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Get(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 1, typeof(UnityEngine.GameObject));
			WCG.UIEventHandle o = WCG.UIEventHandle.Get(arg0);
			ToLua.Push(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int EventDelegate(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			LuaFunction arg0 = ToLua.CheckLuaFunction(L, 1);
			UnityEngine.Events.UnityAction o = WCG.UIEventHandle.EventDelegate(arg0);
			ToLua.Push(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int IntDelegate(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			LuaFunction arg0 = ToLua.CheckLuaFunction(L, 1);
			UnityEngine.Events.UnityAction<int> o = WCG.UIEventHandle.IntDelegate(arg0);
			ToLua.Push(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int StringDelegate(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			LuaFunction arg0 = ToLua.CheckLuaFunction(L, 1);
			UnityEngine.Events.UnityAction<string> o = WCG.UIEventHandle.StringDelegate(arg0);
			ToLua.Push(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int FloatDelegate(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			LuaFunction arg0 = ToLua.CheckLuaFunction(L, 1);
			UnityEngine.Events.UnityAction<float> o = WCG.UIEventHandle.FloatDelegate(arg0);
			ToLua.Push(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int BoolDelegate(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			LuaFunction arg0 = ToLua.CheckLuaFunction(L, 1);
			UnityEngine.Events.UnityAction<bool> o = WCG.UIEventHandle.BoolDelegate(arg0);
			ToLua.Push(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Vec2Delegate(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			LuaFunction arg0 = ToLua.CheckLuaFunction(L, 1);
			UnityEngine.Events.UnityAction<UnityEngine.Vector2> o = WCG.UIEventHandle.Vec2Delegate(arg0);
			ToLua.Push(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int BaseEventDelegate(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			LuaFunction arg0 = ToLua.CheckLuaFunction(L, 1);
			UnityEngine.Events.UnityAction<UnityEngine.EventSystems.BaseEventData> o = WCG.UIEventHandle.BaseEventDelegate(arg0);
			ToLua.Push(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int AddUIEvent(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 3);
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 1, typeof(UnityEngine.GameObject));
			int arg1 = (int)LuaDLL.luaL_checknumber(L, 2);
			LuaFunction arg2 = ToLua.CheckLuaFunction(L, 3);
			WCG.UIEventHandle.AddUIEvent(arg0, arg1, arg2);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int AddButtonClick(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 1, typeof(UnityEngine.GameObject));
			LuaFunction arg1 = ToLua.CheckLuaFunction(L, 2);
			WCG.UIEventHandle.AddButtonClick(arg0, arg1);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int AddInputFieldEndEdit(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 1, typeof(UnityEngine.GameObject));
			LuaFunction arg1 = ToLua.CheckLuaFunction(L, 2);
			WCG.UIEventHandle.AddInputFieldEndEdit(arg0, arg1);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int AddInputFieldValueChange(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 1, typeof(UnityEngine.GameObject));
			LuaFunction arg1 = ToLua.CheckLuaFunction(L, 2);
			WCG.UIEventHandle.AddInputFieldValueChange(arg0, arg1);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int AddScrollbarValueChange(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 1, typeof(UnityEngine.GameObject));
			LuaFunction arg1 = ToLua.CheckLuaFunction(L, 2);
			WCG.UIEventHandle.AddScrollbarValueChange(arg0, arg1);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int AddScrollRectValueChange(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 1, typeof(UnityEngine.GameObject));
			LuaFunction arg1 = ToLua.CheckLuaFunction(L, 2);
			WCG.UIEventHandle.AddScrollRectValueChange(arg0, arg1);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int AddSliderValueChange(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 1, typeof(UnityEngine.GameObject));
			LuaFunction arg1 = ToLua.CheckLuaFunction(L, 2);
			WCG.UIEventHandle.AddSliderValueChange(arg0, arg1);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int AddToggleValueChange(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 1, typeof(UnityEngine.GameObject));
			LuaFunction arg1 = ToLua.CheckLuaFunction(L, 2);
			WCG.UIEventHandle.AddToggleValueChange(arg0, arg1);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int AddDropdownValueChange(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 1, typeof(UnityEngine.GameObject));
			LuaFunction arg1 = ToLua.CheckLuaFunction(L, 2);
			WCG.UIEventHandle.AddDropdownValueChange(arg0, arg1);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int op_Equality(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			UnityEngine.Object arg0 = (UnityEngine.Object)ToLua.ToObject(L, 1);
			UnityEngine.Object arg1 = (UnityEngine.Object)ToLua.ToObject(L, 2);
			bool o = arg0 == arg1;
			LuaDLL.lua_pushboolean(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_parameter(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			WCG.UIEventHandle obj = (WCG.UIEventHandle)o;
			object ret = obj.parameter;
			ToLua.Push(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index parameter on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_parameter(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			WCG.UIEventHandle obj = (WCG.UIEventHandle)o;
			object arg0 = ToLua.ToVarObject(L, 2);
			obj.parameter = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index parameter on a nil value");
		}
	}
}

