#if UNITY_EDITOR
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace SpreadSheetLoader
{
    public class SheetLoadData
    {
        public string sheetName;
        public List<SheetFieldInfo> fields;
        public List<SheetEnumInfo> enums;
        public JArray data;
    }

    public class SheetFieldInfo
    {
        public string name;
        public string type;
    }

    public class SheetEnumInfo
    {
        public string name;
        public List<string> items;
    }

    public class SpreadSheetLoader : EditorWindow
    {
        private const string GAS_URL = "https://script.google.com/macros/s/AKfycbxu4bK-FSAdrgkCmCdFJA2DSYx3Eiu-RlTM85z7cTtCjU1cy1fnxekPoRDf9Y_sijAL/exec";
        
        private string outputFilePath = "Assets/Script/ScriptableObjectCS/fromSpreadSheet";
        private string sheetUrl = string.Empty;
        private string sheetName = string.Empty;
        private string dataName = string.Empty;

        private SheetLoadData sheetData;

        [MenuItem("Tool/SpreadSheetLoader")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(SpreadSheetLoader));
        }

        private Vector2 scrollPos;
        private bool isOpen = true;

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Create Settings", EditorStyles.boldLabel);

            outputFilePath = EditorGUILayout.TextField("Path", outputFilePath);
            sheetUrl = EditorGUILayout.TextField("SheetURL", sheetUrl);
            sheetName = EditorGUILayout.TextField("SheetName", sheetName);
            dataName = EditorGUILayout.TextField("DataName", dataName);

            if (GUILayout.Button("Create New ScriptableObject"))
            {
                if (string.IsNullOrEmpty(sheetUrl))
                {
                    Debug.LogError("スプレッドシートのURLが指定されていません");
                    return;
                }
                if(string.IsNullOrEmpty(sheetName))
                {
                    Debug.LogError("スプレッドシートの名前が指定されていません");
                    return;
                }
                if (string.IsNullOrEmpty(dataName))
                {
                    Debug.LogError("データを格納するクラスの名前が指定されていません");
                    return;
                }

                EditorCoroutineUtility.StartCoroutine(GenerateScriptableObjectCSandDataClassCS(), this);
            }

            EditorGUILayout.Space(30);

            var guids = AssetDatabase.FindAssets("t:SpreadSheetSO");
            var assetPaths = guids.Select(AssetDatabase.GUIDToAssetPath).ToArray();
            var scriptableObjects = assetPaths
                .Select(AssetDatabase.LoadAssetAtPath<SpreadSheetSO>)
                .Where(x => x != null)
                .ToArray();

            isOpen = EditorGUILayout.BeginFoldoutHeaderGroup(isOpen, "SpreadSheetSO List");
            if (isOpen)
            {
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

                foreach (var so in scriptableObjects)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.ObjectField(so, typeof(SpreadSheetSO), false, GUILayout.Width(350));
                        
                        GUILayout.Space(10);

                        if (GUILayout.Button("Update", GUILayout.Width(100)))
                        {
                            EditorCoroutineUtility.StartCoroutine(UpdateSpreadSheetSO(so), this);
                        }
                    }
                }

                if (GUILayout.Button("Update All"))
                {
                    foreach(var so in scriptableObjects)
                        EditorCoroutineUtility.StartCoroutine(UpdateSpreadSheetSO(so), this);
                }

                EditorGUILayout.EndScrollView();
            }
        }

        // ScriptableObjectとデータ格納用のクラスのCSファイルを生成
        private IEnumerator GenerateScriptableObjectCSandDataClassCS()
        {
            WWWForm form = new WWWForm();
            form.AddField("url", sheetUrl);
            form.AddField("name", sheetName);

            using (UnityWebRequest www = UnityWebRequest.Post(GAS_URL, form))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    sheetData = JsonConvert.DeserializeObject<SheetLoadData>(www.downloadHandler.text);

                    if (!Directory.Exists(outputFilePath))
                    {
                        Directory.CreateDirectory(outputFilePath);
                    }

                    GenerateEnumCS(sheetData);
                    GenerateDataCS(sheetData);
                    GenerateScriptableObjectCS(sheetData);

                    SetSessionStates();

                    AssetDatabase.Refresh();

                    yield break;
                }
                else
                {
                    Debug.LogError("対象のデプロイが存在しないか、送信する値が間違えています");

                    yield break;
                }
            }
        }

        private IEnumerator UpdateSpreadSheetSO(SpreadSheetSO so)
        {
            WWWForm form = new WWWForm();
            form.AddField("url", so.sheetUrl);
            form.AddField("name", so.sheetName);

            using (UnityWebRequest www = UnityWebRequest.Post(GAS_URL, form))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    sheetData = JsonConvert.DeserializeObject<SheetLoadData>(www.downloadHandler.text);

                    AutoAssetCreator.UpdateAndPopulateSO(so, sheetData.data.ToString());

                    yield break;
                }
                else
                {
                    Debug.LogError("対象のデプロイが存在しないか、送信する値が間違えています");

                    yield break;
                }
            }
        }

        private void SetSessionStates()
        {
            SessionState.SetBool("SpreadSheetLoader_Pending", true);
            SessionState.SetString("SheetURL", sheetUrl);
            SessionState.SetString("SheetName", sheetData.sheetName);
            SessionState.SetString("DataName", dataName);
            SessionState.SetString("Json", sheetData.data.ToString(Formatting.None));
        }

        // データ格納用クラスのCSファイル作成
        private void GenerateDataCS(SheetLoadData data)
        {
            string path = Path.Combine(outputFilePath, $"{dataName}.cs");

            using (StreamWriter sw = new StreamWriter(path))
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("using System.Collections;");
                sb.AppendLine("using System.Collections.Generic;\n");
                sb.AppendLine("[System.Serializable]");
                sb.AppendLine($"public class {dataName}");
                sb.AppendLine("{");
                foreach (var field in data.fields)
                {
                    sb.AppendLine($"    public {field.type} {field.name};");
                }
                sb.AppendLine("}");

                sw.WriteLine(sb.ToString());
            }
        }

        // SO用のCSファイル作成
        private void GenerateScriptableObjectCS(SheetLoadData data)
        {
            string path = Path.Combine(outputFilePath, $"{data.sheetName}.cs");

            using (StreamWriter sw = new StreamWriter(path))
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("using UnityEngine;");
                sb.AppendLine("using System.Collections;");
                sb.AppendLine("using System.Collections.Generic;\n");
                sb.AppendLine($"public class {data.sheetName} : SpreadSheetSO");
                sb.AppendLine("{");
                sb.AppendLine($"    [SerializeField] public List<{dataName}> {dataName.ToLowerFirst()}List;");
                sb.AppendLine("}");

                sw.WriteLine(sb.ToString());
            }
        }

        private void GenerateEnumCS(SheetLoadData data)
        {
            if (data.enums == null || data.enums.Count <= 0) return;

            string path = Path.Combine(outputFilePath, $"SpreadSheetEnums.cs");

            using (StreamWriter sw = new StreamWriter(path))
            {
                StringBuilder sb = new StringBuilder();

                foreach (var enumInfo in data.enums)
                {
                    sb.AppendLine($"public enum {enumInfo.name}");
                    sb.AppendLine("{");

                    foreach (var item in enumInfo.items)
                    {
                        sb.AppendLine($"    {item},");
                    }

                    sb.AppendLine("}");
                }

                sw.WriteLine(sb.ToString());
            }
        }
    }
}
#endif