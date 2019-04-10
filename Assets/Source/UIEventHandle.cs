using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;

namespace WCG
{
    public class UIEventHandle : MonoBehaviour
    {

        protected EventTrigger m_eventTrigger;

       // public delegate void BaseEventDelegate(BaseEventData eData);

        public object parameter;


        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame



        //导出给Lua用

        static public UIEventHandle Get(GameObject go)
        {
            UIEventHandle listener = go.GetComponent<UIEventHandle>();
            if (listener == null) listener = go.AddComponent<UIEventHandle>();
            return listener;
        }

        static public UnityEngine.Events.UnityAction EventDelegate(LuaFunction func)
        {
            UnityEngine.Events.UnityAction d = () =>
            {
                func.Call();
            };
            return d;
        }

		static public UnityEngine.Events.UnityAction<int> IntDelegate(LuaFunction func)
		{
			UnityEngine.Events.UnityAction<int> d = (arg0) =>
			{
				func.Call<int>(arg0);
			};
			return d;
		}

		static public UnityEngine.Events.UnityAction<String> StringDelegate(LuaFunction func)
        {
            UnityEngine.Events.UnityAction<String> d = (arg0) =>
            {
				func.Call<String>(arg0);
			};
            return d;
        }

        static public UnityEngine.Events.UnityAction<float> FloatDelegate(LuaFunction func)
        {
            UnityEngine.Events.UnityAction<float> d = (arg0) =>
            {
				func.Call<float>(arg0);
			};
            return d;
        }

        static public UnityEngine.Events.UnityAction<bool> BoolDelegate(LuaFunction func)
        {
            UnityEngine.Events.UnityAction<bool> d = (arg0) =>
            {
				func.Call<bool>(arg0);
			};
            return d;
        }

        static public UnityEngine.Events.UnityAction<Vector2> Vec2Delegate(LuaFunction func)
        {
            UnityEngine.Events.UnityAction<Vector2> d = (arg0) =>
            {
				func.Call<Vector2>(arg0);
			};
            return d;
        }

        static public UnityEngine.Events.UnityAction<BaseEventData> BaseEventDelegate(LuaFunction func)
        {
            UnityEngine.Events.UnityAction<BaseEventData> d = (arg0) =>
            {
				func.Call<BaseEventData>(arg0);
			};
            return d;
        }


        //统一添加事件接口
        static public void AddUIEvent(GameObject oWnd, int eventType, LuaFunction callback)
        {
            if (oWnd == null)
            {
                return;
            }

            //添加通用事件
            EventTrigger eventTrigger = oWnd.GetComponent<EventTrigger>();
            if (eventTrigger == null)
            {
                eventTrigger = oWnd.AddComponent<EventTrigger>();
                if (eventTrigger == null)
                {
                    Debug.LogError("GameObject not contains a EventTrigger:" + oWnd.name);
                    return;
                }

				if (eventTrigger.triggers == null)
                {
					eventTrigger.triggers = new List<EventTrigger.Entry>();
                }
            }

            EventTrigger.TriggerEvent trigger = new EventTrigger.TriggerEvent();
                   
            trigger.AddListener(BaseEventDelegate(callback));
            
            EventTrigger.Entry entry = new EventTrigger.Entry() { callback = trigger, eventID = (EventTriggerType)eventType };
			eventTrigger.triggers.Add(entry);            
        }

        //添加Button控件 onClick事件监听
        static public void AddButtonClick(GameObject oWnd, LuaFunction callback)
        {
            if (oWnd == null)
            {
                //Debug.LogError("GameObject is null:" + wndName);
                return;
            }

            Button btn = oWnd.GetComponent<Button>();
            if (btn == null)
            {
                Debug.LogError(oWnd.name + "donot contains a Button Component");
                return;
            }

            btn.onClick.AddListener(EventDelegate(callback));
        }
        //添加inputfield onendedit事件
        static public void AddInputFieldEndEdit(GameObject oWnd, LuaFunction callback)
        {
            if (oWnd == null)
            {
                return;
            }

            InputField input = oWnd.GetComponent<InputField>();
            if (input == null)
            {
                Debug.LogError(oWnd.name + "donot contains a InputField Component");
                return;
            }
            input.onEndEdit.AddListener(StringDelegate(callback));
        }

        //inputfield onValueChange
        static public void AddInputFieldValueChange(GameObject oWnd, LuaFunction callback)
        {
            if (oWnd == null)
            {
                return;
            }

            InputField input = oWnd.GetComponent<InputField>();
            if (input == null)
            {
                Debug.LogError(oWnd.name + "donot contains a InputField Component");
                return;
            }
			input.onValueChanged.AddListener(StringDelegate(callback));
        }

        //scrollbar valuechange
        static public void AddScrollbarValueChange(GameObject oWnd, LuaFunction callback)
        {
            if (oWnd == null)
            {
                return;
            }

            Scrollbar scrollbar = oWnd.GetComponent<Scrollbar>();
            if (scrollbar == null)
            {
                Debug.LogError(oWnd.name + "donot contains a Scrollbar Component");
                return;
            }
            scrollbar.onValueChanged.AddListener(FloatDelegate(callback));
        }

        static public void AddScrollRectValueChange(GameObject oWnd, LuaFunction callback)
        {
            if (oWnd == null)
            {
                return;
            }
            ScrollRect scrollrect = oWnd.GetComponent<ScrollRect>();
            if (scrollrect == null)
            {
                Debug.LogError(oWnd.name + "donot contains a ScrollRect Component");
                return;
            }
            scrollrect.onValueChanged.AddListener(Vec2Delegate(callback));
        }

        static public void AddSliderValueChange(GameObject oWnd, LuaFunction callback)
        {
            if (oWnd == null)
            {
                return;
            }

            Slider slider = oWnd.GetComponent<Slider>();
            if (slider == null)
            {
                Debug.LogError(oWnd.name + "donot contains a Slider Component");
                return;
            }
            slider.onValueChanged.AddListener(FloatDelegate(callback));
        }

        static public void AddToggleValueChange(GameObject oWnd, LuaFunction callback)
        {
            if (oWnd == null)
            {
                return;
            }

            Toggle toggle = oWnd.GetComponent<Toggle>();
            if (toggle == null)
            {
                Debug.LogError(oWnd.name + "donnot contains a Toggle Component");
                return;
            }
            toggle.onValueChanged.AddListener(BoolDelegate(callback));
        }

		static public void AddDropdownValueChange(GameObject oWnd, LuaFunction callback)
		{
			if (oWnd == null)
			{
				return;
			}

			Dropdown dropdown = oWnd.GetComponent<Dropdown>();
			if (dropdown == null)
			{
				Debug.LogError(oWnd.name + "donnot contains a dropdown Component");
				return;
			}
			dropdown.onValueChanged.AddListener(IntDelegate(callback));
		}
	}
}