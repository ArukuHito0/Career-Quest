#if UNITY_EDITOR
using System.IO;
using System.Text;

namespace SpreadSheetLoader
{
    public static class CSFileGenerator
    {
        // データ格納用クラスのCSファイル作成
        public static void GenerateDataCS(SheetLoadData data, string dataName, string outputFilePath)
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
                sb.AppendLine("}\n");

                sb.Append(CreateEnums(data, outputFilePath));

                sw.WriteLine(sb.ToString());
            }
        }

        // SO用のCSファイル作成
        public static void GenerateScriptableObjectCS(SheetLoadData data, string dataName, string outputFilePath)
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

        // Enum作成用の関数
        private static string CreateEnums(SheetLoadData data, string outputFilePath)
        {
            if (data.enums == null || data.enums.Count <= 0) return string.Empty;

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

            return sb.ToString();
        }
    }
}
#endif