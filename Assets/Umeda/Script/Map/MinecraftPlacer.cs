using UnityEngine;
using UnityEngine.InputSystem;

public class MinecraftPlacer : MonoBehaviour
{
    public GameObject blockPrefab;
    public float reachDistance = 10f;

    void Update()
    {
        // 左クリックが押された瞬間
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryPlaceBlock();
        }
    }

    void TryPlaceBlock()
    {
        // 1. マウスの位置からレイを生成
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        // 2. レイヤーマスクの設定（もし床や他のオブジェクトと干渉するなら指定が必要）
        if (Physics.Raycast(ray, out RaycastHit hit, reachDistance))
        {
            // 3. 配置座標の計算
            // ヒットしたブロックの中心座標 ＋ 法線ベクトル（これだけで確実に隣のグリッドになる）
            Vector3 spawnPos = hit.collider.transform.position + hit.normal;

            // 4. 重複確認：配置先に何もなければ配置
            if (!Physics.CheckSphere(spawnPos, 0.45f))
            {
                Instantiate(blockPrefab, spawnPos, Quaternion.identity);
            }
        }
    }
}