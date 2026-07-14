using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class MapEditor : MonoBehaviour
{
    [System.Serializable]
    public class BlockData { public string name; public GameObject surfacePrefab; public GameObject internalPrefab; public BlockType type; }
    public enum BlockType { Soil, Grass, Sand, Rock, Snow, Water, Magma }

    public List<BlockData> blockLibrary;
    public LayerMask buildableLayer;
    public int currentBlockIndex = 0;

    private Dictionary<int, Dictionary<Vector3Int, GameObject>> layers = new Dictionary<int, Dictionary<Vector3Int, GameObject>>();

    void Start()
    {
        // 高さを1に設定して201x201の範囲を生成
        int targetY = 1;
        for (int x = -100; x <= 100; x++)
        {
            for (int z = -100; z <= 100; z++)
            {
                PlaceBlock(new Vector3Int(x, targetY, z));
            }
        }
    }

    public void PlaceBlock(Vector3Int pos)
    {
        int y = pos.y;
        if (!layers.ContainsKey(y)) layers[y] = new Dictionary<Vector3Int, GameObject>();
        if (layers[y].ContainsKey(pos)) return;

        // 指定されたレイヤー（高さ）のブロックは表面用プレハブを使用
        GameObject prefab = blockLibrary[currentBlockIndex].surfacePrefab;

        GameObject go = Instantiate(prefab, pos, Quaternion.identity);

        // 安全なレイヤー設定
        go.layer = GetFirstLayer(buildableLayer);

        layers[y].Add(pos, go);
    }

    private int GetFirstLayer(LayerMask mask)
    {
        for (int i = 0; i < 32; i++) if (((mask.value >> i) & 1) == 1) return i;
        return 0; // Defaultレイヤー
    }

    void Update()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        if (Mouse.current != null && Mouse.current.leftButton.isPressed) PerformAction();
    }

    void PerformAction()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, buildableLayer))
        {
            // ヒットした座標のX, Zと、指定した高さを組み合わせて配置
            Vector3Int pos = new Vector3Int(Mathf.RoundToInt(hit.point.x), 1, Mathf.RoundToInt(hit.point.z));
            PlaceBlock(pos);
        }
    }
}