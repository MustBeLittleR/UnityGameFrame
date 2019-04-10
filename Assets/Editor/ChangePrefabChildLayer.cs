using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace WCTool
{
    public class ChangePrefabChildLayer : EditorWindow
    {
        [MenuItem("WCTool/ChangePrefabChildLayer")]
        static void Init()
        {
            EditorWindow.GetWindow<ChangePrefabChildLayer>(false, "修改带有Skinned Mesh Renderer的Layer", true);
        }

        //private string m_fullFolder = "";

        private int m_layerIndex = 0;
        private string[] m_LayerName = { "Character" };

        void OnGUI()
        {
            GUILayout.Space(10);
            GUILayout.Label("功能说明：\n设置带有Skinned Mesh Renderer\n组件的预制节点的Layer\n(注:请选择单个文件，可多选)", EditorStyles.boldLabel);

            EditorGUILayout.Separator();
            m_layerIndex = EditorGUILayout.Popup("Layer:", m_layerIndex, m_LayerName);

            /*EditorGUILayout.Separator();
            GUILayout.Label("文件夹选择:");

            EditorGUILayout.BeginHorizontal();
            m_fullFolder = EditorGUILayout.TextField(m_fullFolder);
            if (GUILayout.Button("选择操作的文件夹"))
            {
                m_fullFolder = EditorUtility.OpenFolderPanel("选择操作的文件夹", Application.dataPath, m_fullFolder);
                EditorGUIUtility.editingTextField = false;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();
            if (GUILayout.Button("执行操作"))
            {
                List<string> listFileNames = new List<string>();

                if (Directory.Exists(m_fullFolder))
                {
                    listFileNames.AddRange(Directory.GetFiles(m_fullFolder));
                }

                for (int i = 0; i < listFileNames.Count; ++i)
                {
                    if (Path.GetExtension(listFileNames[i]) != ".meta")
                    {
                        listFileNames[i] = listFileNames[i].Remove(0, Application.dataPath.Length);
                        SetLayer("Assets" + listFileNames[i]);
                    }
                }
            }*/

            EditorGUILayout.Separator();
            if (GUILayout.Button("执行操作"))
            {
                UnityEngine.Object[] objects = Selection.objects;

                for (int i = 0; i < objects.Length; ++i)
                {
                    string path = AssetDatabase.GetAssetPath(objects[i]);
                    if (Path.GetExtension(path) != ".meta")
                    {
                        SetLayer(path);
                        //Debug.Log(path);
                    }
                }
            }
        }

        void SetLayer(string filePath)
        {
            GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath(filePath, typeof(GameObject));

            if(prefab != null)
            {
				int index = LayerMask.NameToLayer(m_LayerName[m_layerIndex]);
				//获取材质，修改材质的shader
				Shader sd = Shader.Find("Custom/CharacterLightingSurface");

				Transform transform = prefab.transform;

				if (index == -1)
				{
					Debug.Log("名字为" + m_LayerName[m_layerIndex] + "的Layer没有找到");
				}
				else
				{
					transform.gameObject.layer = index;
					foreach (Transform child in transform)
					{
						//if (index == -1)
						//{
						//	Debug.Log("名字为" + m_LayerName[m_layerIndex] + "的Layer没有找到");
						//}
						//else
						//{
							//修改Layer
							child.gameObject.layer = index;
							SkinnedMeshRenderer smr = child.GetComponent<SkinnedMeshRenderer>();
							if (smr != null)
							{
								//Debug.Log(LayerMask.NameToLayer("aaaa"));
								smr.GetComponent<Renderer>().sharedMaterial.shader = sd;
								AssetDatabase.SaveAssets();
							}
						//}
					}
				}
            }
        }
    }
}
