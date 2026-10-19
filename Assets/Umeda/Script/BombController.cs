using UnityEngine;

public class BombController : MonoBehaviour
{
    [Header("爆発設定")]
    public float explosionRadius = 5.0f; // 爆弾の爆発影響範囲

    public bool isExplosion = false;

    void Update()
    {
        if (isExplosion)
        {
            IsExplosion();
            isExplosion = false; // 1回だけ実行されるようにフラグを戻す
        }
    }

    void IsExplosion()
    {
        // 1. 爆発範囲内のColliderをすべて取得
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider hit in colliders)
        {
            // 2. 取得したオブジェクトに VoxelExplodeable コンポーネントがついているか確認
            VoxelExplodeable voxelTarget = hit.GetComponent<VoxelExplodeable>();
            if (voxelTarget != null)
            {
                // ボクセル化＆爆散メソッドを呼び出す
                voxelTarget.TriggerExplosion(transform.position);
            }
        }

        // 3. 爆弾自身を消滅させる
        Destroy(gameObject);
    }
}