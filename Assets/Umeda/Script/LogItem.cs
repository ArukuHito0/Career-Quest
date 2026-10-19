using UnityEngine;

public class LogItem : MonoBehaviour
{
    [HideInInspector]
    public bool isCollected = false; // 二重取得を防ぐフラグ
}