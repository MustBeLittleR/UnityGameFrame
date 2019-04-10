using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace WCTool
{

    public class SetSoundAttribute : EditorWindow
    {
        [MenuItem("WCTool/Set Sound Attribute")]
        static void Init()
        {
            EditorWindow.GetWindow<SetSoundAttribute>(false, "修改音效文件属性", true);
        }

        private bool m_bSetThreeD = false;
        private bool m_bThreedValue = false;

        private bool m_bSetCompress = false;
        private float m_iCompressValue = 80;

        void OnGUI()
        {
            GUILayout.Space(10);
            GUILayout.Label("功能说明：\n设置音效源文件(如:.ogg文件)的格式为2D/3D");
            GUILayout.Space(10);

            EditorGUILayout.Separator();
            m_bSetThreeD = EditorGUILayout.BeginToggleGroup("声音2D/3D设置:", m_bSetThreeD);//开始
            m_bThreedValue = EditorGUILayout.Toggle("3D Sound", m_bThreedValue);
            EditorGUILayout.EndToggleGroup();//结束

            EditorGUILayout.Separator();
            m_bSetCompress = EditorGUILayout.BeginToggleGroup("Compress:", m_bSetCompress);//开始
            m_iCompressValue = EditorGUILayout.Slider("Compression(kbps)", m_iCompressValue, 1.0f, 512.0f, null);
            EditorGUILayout.EndToggleGroup();//结束

            //选择的文件夹，直接选择assets获取不到
            //UnityEngine.Object[] objects = Selection.objects;

            //for (int i = 0; i < objects.Length; ++i)
            //{
                string path = AssetDatabase.GetAssetPath(Selection.activeObject);

                List<string> listFileNames = new List<string>();

                if (File.Exists(path))
                {
                    path = path.Replace("Assets/", "");
                    listFileNames.Add(Application.dataPath + "/" + path);
                }

                //开关，控制是否设置，需要按了按钮才会为true
                bool doSet = false;

                EditorGUILayout.Separator();
                GUILayout.Space(5);
                GUILayout.Label("注意: 这里如果选择的单个文件，则对单个文件进行设置");
                if (GUILayout.Button("只对文件夹下的进行操作,子文件夹下不会被设置", GUILayout.Width(300)))
                {
                    doSet = true;
                    if (Directory.Exists(path))
                    {
                        //获取相对Assets的路径
                        path = path.Replace("Assets/", "");

                        listFileNames.AddRange(Directory.GetFiles(Application.dataPath + "/" + path));
                    }
                }//end if

                GUILayout.Space(10);
                if (GUILayout.Button("对文件夹下的所有文件进行设置,包括所有子文件夹下的", GUILayout.Width(300)))
                {
                    doSet = true;
                    if (Directory.Exists(path))
                    {
                        //获取相对Assets的路径
                        path = path.Replace("Assets/", "");

                        //获取全路径Application.dataPath + "/" + path，并获取路径下的所有文件
                        string[] fileEntries = Directory.GetFiles(Application.dataPath + "/" + path, "*", SearchOption.AllDirectories);

                        listFileNames.AddRange(fileEntries);
                    }
                }//end if

                GUILayout.Space(10);
                if (GUILayout.Button("对Assets下所有音效文件进行设置，请慎重考虑！", GUILayout.Width(300)))
                {
                    listFileNames.Clear();
                    doSet = true;
                    string[] fileEntries = Directory.GetFiles(Application.dataPath, "*", SearchOption.AllDirectories);

                    listFileNames.AddRange(fileEntries);
                }//end if

                if (doSet && listFileNames.Count > 0)
                {
                    for (int index = 0; index < listFileNames.Count; ++index)
                    {
                        if (Path.GetExtension(listFileNames[index]) != ".meta")
                        {
                            listFileNames[index] = listFileNames[index].Remove(0, Application.dataPath.Length);
                            SetTheAttribute("Assets" + listFileNames[index]);
                        }
                    }
                }
            //}
        }

        private void SetTheAttribute(string filePath)
        {
            //先判断是不是音频文件
            AudioImporter audioImporter = AssetImporter.GetAtPath(filePath) as AudioImporter;

            if (!audioImporter)
                return;

            if (m_bSetThreeD && audioImporter.threeD != m_bThreedValue)
            {
                audioImporter.threeD = m_bThreedValue;
            }

            if (Path.GetExtension(filePath) != ".wav")
            {
                int value = (int)m_iCompressValue * 1000;
//                if (m_bSetCompress && audioImporter.compressionBitrate != value)
//                {
//                    audioImporter.compressionBitrate = value;
//                }
            }

            AssetDatabase.ImportAsset(filePath);
        }

    }
}
