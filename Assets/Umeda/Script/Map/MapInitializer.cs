using UnityEngine;

public class MapInitializer : MonoBehaviour
{
    [Header("配置設定")]
    public GameObject blockPrefab; // 配置するブロックのプレハブ
    public int width = 201;        // X方向のサイズ
    public int depth = 201;        // Z方向のサイズ

    void Start()
    {
        InitializeMap();
    }

    void InitializeMap()
    {
        if (blockPrefab == null)
        {
            Debug.LogError("配置するブロックのプレハブが設定されていません。");
            return;
        }

        // 中心を(0,0,0)にするためのオフセット
        int xOffset = width / 2;
        int zOffset = depth / 2;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                // グリッド座標の計算 (例: -100 ～ 100)
                Vector3 pos = new Vector3(x - xOffset, 0, z - zOffset);

                // 配置
                Instantiate(blockPrefab, pos, Quaternion.identity, transform);
            }
        }

        Debug.Log($"{width} * 1 * {depth} のマップを初期化しました。");
    }
}