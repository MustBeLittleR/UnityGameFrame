﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class WCG_OnHeadWrap
{
	public static void Register(LuaState L)
	{
		L.BeginClass(typeof(WCG.OnHead), typeof(UnityEngine.MonoBehaviour));
		L.RegFunction("SetGraphicChar", SetGraphicChar);
		L.RegFunction("SetNameShow", SetNameShow);
		L.RegFunction("SetBloodShow", SetBloodShow);
		L.RegFunction("SetBloodTxtShow", SetBloodTxtShow);
		L.RegFunction("SetProShow", SetProShow);
		L.RegFunction("SetPopMsgShow", SetPopMsgShow);
		L.RegFunction("SetMsgAniShow", SetMsgAniShow);
		L.RegFunction("SetDietShow", SetDietShow);
		L.RegFunction("SetCampShow", SetCampShow);
		L.RegFunction("SetFightShow", SetFightShow);
		L.RegFunction("SetHeadShow", SetHeadShow);
		L.RegFunction("SetName", SetName);
		L.RegFunction("SetNameColor", SetNameColor);
		L.RegFunction("SetBloodRate", SetBloodRate);
		L.RegFunction("SetBloodTxt", SetBloodTxt);
		L.RegFunction("SetProRate", SetProRate);
		L.RegFunction("SetProTime", SetProTime);
		L.RegFunction("UpdateProRate", UpdateProRate);
		L.RegFunction("SetPopTxt", SetPopTxt);
		L.RegFunction("Release", Release);
		L.RegFunction("UpdateUIPos", UpdateUIPos);
		L.RegFunction("__eq", op_Equality);
		L.RegFunction("__tostring", ToLua.op_ToString);
		L.RegVar("m_RectTrans", get_m_RectTrans, set_m_RectTrans);
		L.RegVar("m_goName", get_m_goName, set_m_goName);
		L.RegVar("m_NameTxt", get_m_NameTxt, set_m_NameTxt);
		L.RegVar("m_goBlood", get_m_goBlood, set_m_goBlood);
		L.RegVar("m_BloodSlider", get_m_BloodSlider, set_m_BloodSlider);
		L.RegVar("m_BloodText", get_m_BloodText, set_m_BloodText);
		L.RegVar("m_goBloodText", get_m_goBloodText, set_m_goBloodText);
		L.RegVar("m_goProgress", get_m_goProgress, set_m_goProgress);
		L.RegVar("m_ProgressSlider", get_m_ProgressSlider, set_m_ProgressSlider);
		L.RegVar("m_goPopMsg", get_m_goPopMsg, set_m_goPopMsg);
		L.RegVar("m_PopTxt", get_m_PopTxt, set_m_PopTxt);
		L.RegVar("m_goMsgAni", get_m_goMsgAni, set_m_goMsgAni);
		L.RegVar("m_goDiet", get_m_goDiet, set_m_goDiet);
		L.RegVar("m_goCamp", get_m_goCamp, set_m_goCamp);
		L.RegVar("m_goFight", get_m_goFight, set_m_goFight);
		L.EndClass();
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetGraphicChar(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			WCG.OnHead obj = (WCG.OnHead)ToLua.CheckObject<WCG.OnHead>(L, 1);
			WCG.RenderObj arg0 = (WCG.RenderObj)ToLua.CheckObject<WCG.RenderObj>(L, 2);
			obj.SetGraphicChar(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetNameShow(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			WCG.OnHead obj = (WCG.OnHead)ToLua.CheckObject<WCG.OnHead>(L, 1);
			bool arg0 = LuaDLL.luaL_checkboolean(L, 2);
			obj.SetNameShow(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetBloodShow(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			WCG.OnHead obj = (WCG.OnHead)ToLua.CheckObject<WCG.OnHead>(L, 1);
			bool arg0 = LuaDLL.luaL_checkboolean(L, 2);
			obj.SetBloodShow(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetBloodTxtShow(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			WCG.OnHead obj = (WCG.OnHead)ToLua.CheckObject<WCG.OnHead>(L, 1);
			bool arg0 = LuaDLL.luaL_checkboolean(L, 2);
			obj.SetBloodTxtShow(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetProShow(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			WCG.OnHead obj = (WCG.OnHead)ToLua.CheckObject<WCG.OnHead>(L, 1);
			bool arg0 = LuaDLL.luaL_checkboolean(L, 2);
			obj.SetProShow(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetPopMsgShow(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			WCG.OnHead obj = (WCG.OnHead)ToLua.CheckObject<WCG.OnHead>(L, 1);
			bool arg0 = LuaDLL.luaL_checkboolean(L, 2);
			obj.SetPopMsgShow(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetMsgAniShow(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			WCG.OnHead obj = (WCG.OnHead)ToLua.CheckObject<WCG.OnHead>(L, 1);
			bool arg0 = LuaDLL.luaL_checkboolean(L, 2);
			obj.SetMsgAniShow(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetDietShow(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			WCG.OnHead obj = (WCG.OnHead)ToLua.CheckObject<WCG.OnHead>(L, 1);
			bool arg0 = LuaDLL.luaL_checkboolean(L, 2);
			obj.SetDietShow(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetCampShow(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			WCG.OnHead obj = (WCG.OnHead)ToLua.CheckObject<WCG.OnHead>(L, 1);
			bool arg0 = LuaDLL.luaL_checkboolean(L, 2);
			obj.SetCampShow(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetFightShow(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			WCG.OnHead obj = (WCG.OnHead)ToLua.CheckObject<WCG.OnHead>(L, 1);
			bool arg0 = LuaDLL.luaL_checkboolean(L, 2);
			obj.SetFightShow(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetHeadShow(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			WCG.OnHead obj = (WCG.OnHead)ToLua.CheckObject<WCG.OnHead>(L, 1);
			bool arg0 = LuaDLL.luaL_checkboolean(L, 2);
			obj.SetHeadShow(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetName(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			WCG.OnHead obj = (WCG.OnHead)ToLua.CheckObject<WCG.OnHead>(L, 1);
			string arg0 = ToLua.CheckString(L, 2);
			obj.SetName(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetNameColor(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 4);
			WCG.OnHead obj = (WCG.OnHead)ToLua.CheckObject<WCG.OnHead>(L, 1);
			int arg0 = (int)LuaDLL.luaL_checknumber(L, 2);
			int arg1 = (int)LuaDLL.luaL_checknumber(L, 3);
			int arg2 = (int)LuaDLL.luaL_checknumber(L, 4);
			obj.SetNameColor(arg0, arg1, arg2);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetBloodRate(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			WCG.OnHead obj = (WCG.OnHead)ToLua.CheckObject<WCG.OnHead>(L, 1);
			float arg0 = (float)LuaDLL.luaL_checknumber(L, 2);
			obj.SetBloodRate(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetBloodTxt(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			WCG.OnHead obj = (WCG.OnHead)ToLua.CheckObject<WCG.OnHead>(L, 1);
			string arg0 = ToLua.CheckString(L, 2);
			obj.SetBloodTxt(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetProRate(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			WCG.OnHead obj = (WCG.OnHead)ToLua.CheckObject<WCG.OnHead>(L, 1);
			float arg0 = (float)LuaDLL.luaL_checknumber(L, 2);
			obj.SetProRate(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetProTime(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			WCG.OnHead obj = (WCG.OnHead)ToLua.CheckObject<WCG.OnHead>(L, 1);
			float arg0 = (float)LuaDLL.luaL_checknumber(L, 2);
			obj.SetProTime(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int UpdateProRate(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			WCG.OnHead obj = (WCG.OnHead)ToLua.CheckObject<WCG.OnHead>(L, 1);
			obj.UpdateProRate();
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetPopTxt(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			WCG.OnHead obj = (WCG.OnHead)ToLua.CheckObject<WCG.OnHead>(L, 1);
			string arg0 = ToLua.CheckString(L, 2);
			obj.SetPopTxt(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Release(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			WCG.OnHead obj = (WCG.OnHead)ToLua.CheckObject<WCG.OnHead>(L, 1);
			obj.Release();
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int UpdateUIPos(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			WCG.OnHead obj = (WCG.OnHead)ToLua.CheckObject<WCG.OnHead>(L, 1);
			obj.UpdateUIPos();
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
	static int get_m_RectTrans(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			WCG.OnHead obj = (WCG.OnHead)o;
			UnityEngine.RectTransform ret = obj.m_RectTrans;
			ToLua.PushSealed(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index m_RectTrans on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_m_goName(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			WCG.OnHead obj = (WCG.OnHead)o;
			UnityEngine.GameObject ret = obj.m_goName;
			ToLua.PushSealed(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index m_goName on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_m_NameTxt(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			WCG.OnHead obj = (WCG.OnHead)o;
			UnityEngine.UI.Text ret = obj.m_NameTxt;
			ToLua.Push(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index m_NameTxt on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_m_goBlood(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			WCG.OnHead obj = (WCG.OnHead)o;
			UnityEngine.GameObject ret = obj.m_goBlood;
			ToLua.PushSealed(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index m_goBlood on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_m_BloodSlider(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			WCG.OnHead obj = (WCG.OnHead)o;
			UnityEngine.UI.Slider ret = obj.m_BloodSlider;
			ToLua.Push(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index m_BloodSlider on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_m_BloodText(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			WCG.OnHead obj = (WCG.OnHead)o;
			UnityEngine.UI.Text ret = obj.m_BloodText;
			ToLua.Push(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index m_BloodText on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_m_goBloodText(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			WCG.OnHead obj = (WCG.OnHead)o;
			UnityEngine.GameObject ret = obj.m_goBloodText;
			ToLua.PushSealed(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index m_goBloodText on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_m_goProgress(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			WCG.OnHead obj = (WCG.OnHead)o;
			UnityEngine.GameObject ret = obj.m_goProgress;
			ToLua.PushSealed(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index m_goProgress on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_m_ProgressSlider(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			WCG.OnHead obj = (WCG.OnHead)o;
			UnityEngine.UI.Slider ret = obj.m_ProgressSlider;
			ToLua.Push(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index m_ProgressSlider on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_m_goPopMsg(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			WCG.OnHead obj = (WCG.OnHead)o;
			UnityEngine.GameObject ret = obj.m_goPopMsg;
			ToLua.PushSealed(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index m_goPopMsg on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_m_PopTxt(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			WCG.OnHead obj = (WCG.OnHead)o;
			UnityEngine.UI.Text ret = obj.m_PopTxt;
			ToLua.Push(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index m_PopTxt on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_m_goMsgAni(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			WCG.OnHead obj = (WCG.OnHead)o;
			UnityEngine.GameObject ret = obj.m_goMsgAni;
			ToLua.PushSealed(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index m_goMsgAni on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_m_goDiet(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			WCG.OnHead obj = (WCG.OnHead)o;
			UnityEngine.GameObject ret = obj.m_goDiet;
			ToLua.PushSealed(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index m_goDiet on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_m_goCamp(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			WCG.OnHead obj = (WCG.OnHead)o;
			UnityEngine.GameObject ret = obj.m_goCamp;
			ToLua.PushSealed(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index m_goCamp on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_m_goFight(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			WCG.OnHead obj = (WCG.OnHead)o;
			UnityEngine.GameObject ret = obj.m_goFight;
			ToLua.PushSealed(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index m_goFight on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_m_RectTrans(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			WCG.OnHead obj = (WCG.OnHead)o;
			UnityEngine.RectTransform arg0 = (UnityEngine.RectTransform)ToLua.CheckObject(L, 2, typeof(UnityEngine.RectTransform));
			obj.m_RectTrans = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index m_RectTrans on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_m_goName(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			WCG.OnHead obj = (WCG.OnHead)o;
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 2, typeof(UnityEngine.GameObject));
			obj.m_goName = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index m_goName on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_m_NameTxt(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			WCG.OnHead obj = (WCG.OnHead)o;
			UnityEngine.UI.Text arg0 = (UnityEngine.UI.Text)ToLua.CheckObject<UnityEngine.UI.Text>(L, 2);
			obj.m_NameTxt = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index m_NameTxt on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_m_goBlood(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			WCG.OnHead obj = (WCG.OnHead)o;
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 2, typeof(UnityEngine.GameObject));
			obj.m_goBlood = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index m_goBlood on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_m_BloodSlider(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			WCG.OnHead obj = (WCG.OnHead)o;
			UnityEngine.UI.Slider arg0 = (UnityEngine.UI.Slider)ToLua.CheckObject<UnityEngine.UI.Slider>(L, 2);
			obj.m_BloodSlider = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index m_BloodSlider on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_m_BloodText(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			WCG.OnHead obj = (WCG.OnHead)o;
			UnityEngine.UI.Text arg0 = (UnityEngine.UI.Text)ToLua.CheckObject<UnityEngine.UI.Text>(L, 2);
			obj.m_BloodText = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index m_BloodText on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_m_goBloodText(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			WCG.OnHead obj = (WCG.OnHead)o;
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 2, typeof(UnityEngine.GameObject));
			obj.m_goBloodText = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index m_goBloodText on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_m_goProgress(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			WCG.OnHead obj = (WCG.OnHead)o;
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 2, typeof(UnityEngine.GameObject));
			obj.m_goProgress = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index m_goProgress on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_m_ProgressSlider(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			WCG.OnHead obj = (WCG.OnHead)o;
			UnityEngine.UI.Slider arg0 = (UnityEngine.UI.Slider)ToLua.CheckObject<UnityEngine.UI.Slider>(L, 2);
			obj.m_ProgressSlider = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index m_ProgressSlider on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_m_goPopMsg(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			WCG.OnHead obj = (WCG.OnHead)o;
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 2, typeof(UnityEngine.GameObject));
			obj.m_goPopMsg = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index m_goPopMsg on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_m_PopTxt(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			WCG.OnHead obj = (WCG.OnHead)o;
			UnityEngine.UI.Text arg0 = (UnityEngine.UI.Text)ToLua.CheckObject<UnityEngine.UI.Text>(L, 2);
			obj.m_PopTxt = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index m_PopTxt on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_m_goMsgAni(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			WCG.OnHead obj = (WCG.OnHead)o;
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 2, typeof(UnityEngine.GameObject));
			obj.m_goMsgAni = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index m_goMsgAni on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_m_goDiet(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			WCG.OnHead obj = (WCG.OnHead)o;
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 2, typeof(UnityEngine.GameObject));
			obj.m_goDiet = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index m_goDiet on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_m_goCamp(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			WCG.OnHead obj = (WCG.OnHead)o;
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 2, typeof(UnityEngine.GameObject));
			obj.m_goCamp = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index m_goCamp on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_m_goFight(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			WCG.OnHead obj = (WCG.OnHead)o;
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 2, typeof(UnityEngine.GameObject));
			obj.m_goFight = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index m_goFight on a nil value");
		}
	}
}

