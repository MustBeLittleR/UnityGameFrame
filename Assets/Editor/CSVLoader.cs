using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;

namespace SceneEditor
{
    public class CSVLoader
    {
        private static CSVLoader m_oInstance;
        public static CSVLoader _Instance   
        {
            get
            {
                if (m_oInstance == null)
                {
                    m_oInstance = new CSVLoader();
                }
                return m_oInstance;
            }
        }

        public void LoadCSV( string filepath ,string[] IDList, out Dictionary<int, List<string>> outValue, out Dictionary<string,int> OutIdDC)
        {
            outValue = new Dictionary<int, List<string>>();
            OutIdDC = new Dictionary<string, int>();
            try
            {
                string strline;
                string[] aryline;
                int ColCount = 0;
                bool blnFlag = true;
                StreamReader mysr = new StreamReader(filepath, System.Text.Encoding.UTF8);
                Dictionary<string, bool> NeedList = new Dictionary<string,bool>();
               
                for (int i = 0; i < IDList.Length; i++)
                {
                    NeedList.Add(IDList[i], true);
                }
                bool ss = false;

                while ((strline = mysr.ReadLine()) != null)
                {

                    ColCount = ColCount + 1;
                    if (ColCount == 2)
                    {
                        continue;
                    }
                    aryline = strline.Split(new char[] { ',' });
                    if (blnFlag)
                    {
                        for (int i = 0; i < aryline.Length; i++)
                        {
                            List<string> temp = new List<string>();
                            if (NeedList.TryGetValue(aryline[i], out ss))
                            {
                                OutIdDC.Add(aryline[i], i);
                                outValue.Add(i, temp);
                            }

                        }
                        blnFlag = false;
                    }
                    else
                    {
                        
                        for (int i = 0; i < aryline.Length; i++)
                        {
                            List<string> temp = new List<string>();
                            if (outValue.TryGetValue(i, out temp))
                            {
                                temp.Add(aryline[i]);
                            }
                        }
                    }
                    
                }
                mysr.Close();
            }
            catch (Exception e)
            {
            }
            return;
        }

        CSVLoader()
        {

        }
        ~CSVLoader()
        {

        }



    }
}
