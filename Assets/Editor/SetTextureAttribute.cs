using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace WCTool
{

    public class SetTextureAttribute : EditorWindow
    {
        [MenuItem("WCTool/Set Texture Attribute")]
        static void Init()
        {
            EditorWindow.GetWindow<SetTextureAttribute>(false, "修改Texture属性", true);
        }

        private bool m_bSetMipMap = false;
        private bool m_bMipMapValue = false;

        private bool m_bSetFormat = false;
        private int m_iIndex = 0;
        private int m_iAIndex = 0;

        private bool m_bSetTextureType = false;
        private int m_iTTIndex = 0;

        void OnGUI()
        {
            //GUIStyle gs = new GUIStyle();
            //gs.fontSize = 13;
            //gs.wordWrap = true;

            EditorGUILayout.Separator();
            m_bSetTextureType = EditorGUILayout.BeginToggleGroup("开启TextureType设置功能:", m_bSetTextureType);//开始
            string[] texType = { "Texture", "Normal Map", "Advanced" };
            m_iTTIndex = EditorGUILayout.Popup("TextureType:", m_iTTIndex, texType);
            EditorGUILayout.EndToggleGroup();//结束

            EditorGUILayout.Separator();
            m_bSetMipMap = EditorGUILayout.BeginToggleGroup("开启Mip Map设置功能:", m_bSetMipMap);//开始
            m_bMipMapValue = EditorGUILayout.Toggle("Generate Mip Maps", m_bMipMapValue);
            EditorGUILayout.EndToggleGroup();//结束

            EditorGUILayout.Separator();
            m_bSetFormat = EditorGUILayout.BeginToggleGroup("开启Format设置功能:", m_bSetFormat);//开始
            string[] str = { "Compressed", "16 bits", "Truecolor" };
            m_iIndex = EditorGUILayout.Popup("非Advanced的Format:", m_iIndex, str);

            string[] astr = { "Automatic Compressed", "Automatic 16 bits", "Automatic Truecolor", "ETC_RGB4" };
            m_iAIndex = EditorGUILayout.Popup("Advanced的Format:", m_iAIndex, astr);
            EditorGUILayout.EndToggleGroup();//结束

            //选择的文件夹，直接选择assets获取不到
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);

            List<string> listFileNames = new List<string>();

            if (File.Exists(path))
            {
                path = path.Replace("Assets/", "/");
                listFileNames.Add(Application.dataPath + path);
            }

            //开关，控制是否设置，需要按了按钮才会为true
            bool doSet = false;

            EditorGUILayout.Separator();
            GUILayout.Space(5);
            GUILayout.Label("注意: 这里如果选择的单个文件，则对单个文件进行设置");
            if (GUILayout.Button("只对文件夹下的图片进行操作,子文件夹下不会被设置", GUILayout.Width(300)))
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
            if (GUILayout.Button("对文件夹下的所有图片进行操作,包括所有子文件夹的图片", GUILayout.Width(300)))
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
            if (GUILayout.Button("对Assets下所有图片进行操作，请慎重考虑！", GUILayout.Width(300)))
            {
                listFileNames.Clear();
                doSet = true;
                string[] fileEntries = Directory.GetFiles(Application.dataPath, "*", SearchOption.AllDirectories);

                listFileNames.AddRange(fileEntries);
            }//end if

            if (doSet && listFileNames.Count > 0)
            {
                for (int i = 0; i < listFileNames.Count; ++i)
                {
                    if (Path.GetExtension(listFileNames[i]) != ".meta")
                    {
                        listFileNames[i] = listFileNames[i].Remove(0, Application.dataPath.Length);
                        SetTheAttribute("Assets" + listFileNames[i]);
                    }
                }
            }
        }

        //设置图片的Generate Mip Maps属性
        private void SetTheAttribute(string filePath)
        {
            //直接处理
            TextureImporter textureImporter = AssetImporter.GetAtPath(filePath) as TextureImporter;

            if (!textureImporter)
                return;

            if (m_bSetTextureType)
            {
                switch (m_iTTIndex)
                {
                    case 0:
                        textureImporter.textureType = TextureImporterType.Default;
                        break;
                    case 1:
                        textureImporter.textureType = TextureImporterType.NormalMap;
                        break;
                    case 2:
                        textureImporter.textureType = TextureImporterType.Default;
                        break;
                }
            }

            //设置Mip Map
            if (m_bSetMipMap && textureImporter.mipmapEnabled != m_bMipMapValue)
            {
                textureImporter.mipmapEnabled = m_bMipMapValue;
            }

            //设置Format
            if (m_bSetFormat)
            {
                //各种格式
                if (textureImporter.textureType != TextureImporterType.Default)
                {
                    switch (m_iIndex)
                    {
                        case 0:
                            textureImporter.textureFormat = TextureImporterFormat.AutomaticCompressed;
                            break;
                        case 1:
                            textureImporter.textureFormat = TextureImporterFormat.Automatic16bit;
                            break;
                        case 2:
                            textureImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
                            break;
                    }
                }
                else
                {
                    switch (m_iAIndex)
                    {
                        case 0:
                            textureImporter.textureFormat = TextureImporterFormat.AutomaticCompressed;
                            break;
                        case 1:
                            textureImporter.textureFormat = TextureImporterFormat.Automatic16bit;
                            break;
                        case 2:
                            textureImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
                            break;
                        case 3:
                            textureImporter.textureFormat = TextureImporterFormat.ETC_RGB4;
                            break;
                    }
                }
            }

            TextureImporterSettings tis = new TextureImporterSettings();
            textureImporter.ReadTextureSettings(tis);
            textureImporter.SetTextureSettings(tis);
            AssetDatabase.ImportAsset(filePath);
        }

        private Object[] GetSelectedTextures()
        {
            return Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets);
        }
    }
}
