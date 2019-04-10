using UnityEngine;
using System.Collections.Generic;

namespace WCG
{
    public static class Profile
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        static Dictionary<string, MarkCost> ms_dicMarkCost = new Dictionary<string, MarkCost>();

        public class MarkCost
        {
            public string m_sMark;
            public int m_nCount;

            public float m_fTime;
            public float m_fTimeA;
            public float m_fMaxCostTime;
            public float m_fLimitTime;

            public int m_nFpsNum;
            public int m_nCallTimes;
            public int m_nMaxCallTimes;

            public void Beg(string sMark, float fLimitTime)
            {
                m_sMark = sMark;
                m_nCount = 0;
                m_fTime = .0f;
                m_fLimitTime = fLimitTime;
                m_nCallTimes = 0;
                m_nMaxCallTimes = 0;
                UpBeg();
            }
            public void UpBeg()
            {
                m_nFpsNum = Time.frameCount;
                m_fTimeA = Time.realtimeSinceStartup;
            }
            public void End(string sMark)
            {
                int nFpsNum = Time.frameCount;
                float fTime = Time.realtimeSinceStartup;
                float fDis = fTime - m_fTimeA;
                m_nCount++;

                m_fTime += fDis;

                if (m_fMaxCostTime < fDis)
                    m_fMaxCostTime = fDis;

                if (nFpsNum == m_nFpsNum)
                {
                    m_nCallTimes++;
                }
                else
                {
                    if (m_nCallTimes > m_nMaxCallTimes)
                        m_nMaxCallTimes = m_nCallTimes;

                    m_nCallTimes = 0;
                }
            }
            public void Print(float fLimitTime = -1.0f)
            {
                if (fLimitTime < 0)
                    fLimitTime = m_fLimitTime;

                if (m_fMaxCostTime >= fLimitTime)
                {
                    Debug.LogWarningFormat("-MarkTag-:{0} AllCallTimes:{1} AverageCostTime:{2} MaxCostTime:{3} SameframeMaxCallTimes:{4}", m_sMark, m_nCount, m_fTime / m_nCount, m_fMaxCostTime, m_nMaxCallTimes);
                }
            }
        }
#endif

        public static float ProfileBegin()
        {
            return Time.realtimeSinceStartup;
        }

        public static void ProfileEnd(string strName, float fbegin, float fTimeFilter = 0.0f)
        {
            float fDst = Time.realtimeSinceStartup - fbegin;
            if (fTimeFilter > 0.0f)
            {
                if (fDst >= fTimeFilter)
                {
                    Debug.LogWarning(strName + " Cost: " + fDst);
                }
                return;
            }
            Debug.LogWarning(strName + " Cost: " + fDst);
        }

        public static void BenchmarkBegin(string sStrMark, float fLimitTime = .1f)
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            MarkCost mc = null;
            if (!ms_dicMarkCost.TryGetValue(sStrMark, out mc))
            {
                mc = new MarkCost();
                mc.Beg(sStrMark, fLimitTime);
                ms_dicMarkCost[sStrMark] = mc;
            }
            else
            {
                mc.UpBeg();
            }
#endif
        }

        public static void BenchmarkEnd(string sStrMark)
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            MarkCost mc = null;
            if (ms_dicMarkCost.TryGetValue(sStrMark, out mc))
            {
                mc.End(sStrMark);
            }
#endif
        }

        public static void BenchmarkGet(float fLimitTime = -1.0f)
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            foreach (KeyValuePair< string, MarkCost > iter in ms_dicMarkCost)
            {
                MarkCost mark = iter.Value;
                mark.Print(fLimitTime);
            }
            ms_dicMarkCost.Clear();
#endif
        }
    }
}