#if UNITY_EDITOR
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SpreadSheetLoader
{
    public static class AutoAssetCreator
    {
        private const string ASSET_CREATE_PATH = "Assets/ScriptableObject/SpreadSheetData";

        /// <summary>
        /// ScriptableObjectを作成
        /// </summary>
        /// <param name="sheetName"></param>
        /// <param name="dataClassName"></param>
        /// <param name="jsonData"></param>
        [InitializeOnLoadMethod]
        private static void Create()
        {
            if (!SessionState.GetBool("SpreadSheetLoader_Pending", false))
                return;

            try
            {
                CreateAndPopulateSO();
            }
            catch (Exception e)
            {
                Debug.LogError($"SOアセットの自動生成中にエラーが発生しました: {e}");
            }
        }

        // 指定したパスにSOを作成する関数
        private static void CreateAndPopulateSO()
        {
            SessionState.SetBool("SpreadSheetLoader_Pending", false);

            string sheetUrl = SessionState.GetString("SheetURL", "");
            string sheetName = SessionState.GetString("SheetName", "");
            string dataName = SessionState.GetString("DataName", "");
            string jsonData = SessionState.GetString("Json", "");

            //Debug.Log(jsonData);

            Type soType = FindType(sheetName);   // 作成したいSOのクラスを取得
            Type dataType = FindType(dataName);  // 取得したJSONのデータを格納するクラスを取得

            if (soType == null || dataType == null)
            {
                if(soType == null)
                    Debug.LogError($"{sheetName}クラスが見つかりませんでした");
                if(dataType == null)
                    Debug.LogError($"{dataName}クラスが見つかりませんでした");
                return;
            }

            ScriptableObject so = ScriptableObject.CreateInstance(soType);  // SOのインスタンスを作成

            PopulateSO(so, dataName, jsonData);
            SetMetaData(so, sheetUrl, sheetName, dataName);

            // フォルダが存在しない場合、新しく作成する
            if (!Directory.Exists(ASSET_CREATE_PATH))
            {
                Directory.CreateDirectory(ASSET_CREATE_PATH);
            }

            // 指定したパスにSOを作成
            string path = Path.Combine(ASSET_CREATE_PATH, $"{sheetName}.asset");
            CreateSO(so, path);

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 渡されたSpreadSheetSOのデータを更新
        /// </summary>
        /// <param name="so"></param>
        public static void UpdateAndPopulateSO(SpreadSheetSO so, string jsonData)
        {
            PopulateSO(so, so.dataName, jsonData);

            // フォルダが存在しない場合、新しく作成する
            if (!Directory.Exists(ASSET_CREATE_PATH))
            {
                Directory.CreateDirectory(ASSET_CREATE_PATH);
            }

            string path = Path.Combine(ASSET_CREATE_PATH, $"{so.sheetName}.asset");
            UpdateSO(so, path);

            AssetDatabase.Refresh();
        }

        private static void PopulateSO(ScriptableObject so, string dataName, string jsonData)
        {
            Type dataType = FindType(dataName);
            var parsedData = ParsedJsonData(dataType, jsonData);
            SetJsonData(so, dataName, parsedData);
        }

        // JsonのデータをdataTypeに変換してリスト化したものを返す
        private static object ParsedJsonData(Type dataType, string jsonData)
        {
            var listType = typeof(List<>).MakeGenericType(dataType);  // JSONデータを格納するクラスのリストを作成
            return JsonConvert.DeserializeObject(jsonData, listType); // JSONデータをデシリアライズしてリストに格納
        }

        // 渡されたScriptableObject内のデータリストにparsedDataを代入
        private static void SetJsonData(ScriptableObject so, string dataName, object parsedData)
        {
            FieldInfo dataListField = so.GetType().GetField($"{dataName.ToLowerFirst()}List");
            dataListField.SetValue(so, parsedData);
        }

        // URL・シート名・データ名を保存
        private static void SetMetaData(ScriptableObject so, string url, string sheetName, string dataName)
        {
            var soType = so.GetType();

            FieldInfo urlField = soType.GetField("sheetUrl");
            urlField.SetValue(so, url);

            FieldInfo sheetNameField = soType.GetField("sheetName");
            sheetNameField.SetValue(so, sheetName);

            FieldInfo dataNameField = soType.GetField("dataName");
            dataNameField.SetValue(so, dataName);
        }

        // 渡されたパスにScriptableObjectを作成
        private static void CreateSO(ScriptableObject so, string path)
        {
            AssetDatabase.CreateAsset(so, path);
            AssetDatabase.SaveAssets();
        }

        private static void UpdateSO(ScriptableObject so, string path)
        {
            EditorUtility.SetDirty(so);
            AssetDatabase.SaveAssetIfDirty(so);
        }
        
        private static Type FindType(string typeName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(typeName);

                if (type != null)
                {
                    return type;
                }
            }

            Debug.LogError($"Not Found: {typeName}");
            return null;
        }
    }
}
#endif