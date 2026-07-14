using UnityEngine;

public abstract class SpreadSheetSO : ScriptableObject
{
   [HideInInspector] public string sheetUrl;
   [HideInInspector] public string sheetName;
   [HideInInspector] public string dataName;
}