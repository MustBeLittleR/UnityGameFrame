using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace WCG
{
    public class AddAniEvent : MonoBehaviour
    {
        [Serializable]
        public class AniEvent
        {
            public string m_AniName;
            public float[] m_Times;
        }
        public AniEvent[] m_AniEvents;

        private Animation m_Animation = null;
        private RenderObj m_renderObj = null;

        void Awake()
        {
            m_Animation = gameObject.GetComponent<Animation>();
            if (null != m_Animation)
            {
                for (int i = 0; i < m_AniEvents.Length; i++)
                {
                    AniEvent aniEvent = m_AniEvents[i];
                    if (null != aniEvent)
                    {
                        if (!string.IsNullOrEmpty(aniEvent.m_AniName) && aniEvent.m_Times.Length > 0)
                        {
                            AnimationClip aniClip = m_Animation.GetClip(aniEvent.m_AniName);
                            if (null != aniClip)
                            {
                                AnimationEvent evt = new AnimationEvent();
                                for (int j = 0; j < aniEvent.m_Times.Length; j++)
                                {
                                    evt.time = aniEvent.m_Times[j];
                                    evt.functionName = "OnAniEvent";
                                    evt.intParameter = j + 1;
                                    aniClip.AddEvent(evt);
                                }
                            }
                            else
                            {
                                Debug.AssertFormat(false, "{0} No_Animation: {1}", gameObject.name, aniEvent.m_AniName);
                            }
                        }
                    }
                }
            }
        }

        public void OnAniEvent(int para)
        {
            if (null == m_renderObj)
                m_renderObj = gameObject.GetComponent<RenderObj>();

            if (null != m_renderObj)
                m_renderObj.OnAniEvent();
        }
    }
}
