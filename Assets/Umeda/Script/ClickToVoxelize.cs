using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

// 1. メインのクリック処理用クラス
public class ClickToVoxelize : MonoBehaviour
{
    [Header("分割設定")]
    [Tooltip("各ボクセル（破片）の一辺の長さ")]
    public float voxelSize = 0.1f;
    [Tooltip("爆発力")]
    public float explosionForce = 0.1f;
    [Tooltip("爆発範囲")]
    public float explosionRadius = 1.0f;
    [Tooltip("破壊時間")]
    public float destroyTime = 1.0f;
    [Tooltip("縮小時間")]
    public float shrinkDuration = 0.5f;

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform == this.transform)
                {
                    DivideIntoVoxels();
                }
            }
        }
    }

    private void DivideIntoVoxels()
    {
        Bounds bounds = GetComponent<Renderer>().bounds;
        Vector3 size = bounds.size;

        int xCount = Mathf.CeilToInt(size.x / voxelSize);
        int yCount = Mathf.CeilToInt(size.y / voxelSize);
        int zCount = Mathf.CeilToInt(size.z / voxelSize);

        GameObject container = new GameObject("VoxelContainer");
        VoxelContainerManager manager = container.AddComponent<VoxelContainerManager>();

        container.transform.position = transform.position;
        container.transform.rotation = transform.rotation;

        Material originalMaterial = GetComponent<Renderer>().material;
        float radius = Mathf.Max(size.x, size.y, size.z) / 2.0f;

        for (int x = 0; x < xCount; x++)
        {
            for (int y = 0; y < yCount; y++)
            {
                for (int z = 0; z < zCount; z++)
                {
                    Vector3 localPos = new Vector3(
                        (x - xCount / 2.0f + 0.5f) * voxelSize,
                        (y - yCount / 2.0f + 0.5f) * voxelSize,
                        (z - zCount / 2.0f + 0.5f) * voxelSize
                    );

                    if (localPos.magnitude > radius) continue;

                    GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    part.transform.SetParent(container.transform);
                    part.transform.position = transform.position + transform.rotation * localPos;
                    part.transform.localScale = new Vector3(voxelSize, voxelSize, voxelSize);
                    part.GetComponent<Renderer>().material = originalMaterial;

                    Rigidbody rb = part.AddComponent<Rigidbody>();
                    rb.mass = voxelSize * voxelSize * voxelSize;
                    rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, 0.5f);

                    manager.RegisterVoxel();
                    part.AddComponent<VoxelSelfDestruct>().Initialize(destroyTime, shrinkDuration, manager);
                }
            }
        }
        Destroy(gameObject);
    }
}

// 2. 親オブジェクトの管理クラス (ClickToVoxelizeの外側)
public class VoxelContainerManager : MonoBehaviour
{
    private int activeVoxels = 0;
    public void RegisterVoxel() => activeVoxels++;
    public void UnregisterVoxel()
    {
        activeVoxels--;
        if (activeVoxels <= 0) Destroy(gameObject);
    }
}

// 3. 破片の自律消滅クラス (ClickToVoxelizeの外側)
public class VoxelSelfDestruct : MonoBehaviour
{
    private VoxelContainerManager manager;
    private float delay, duration;

    public void Initialize(float d, float dur, VoxelContainerManager mgr)
    {
        delay = d; duration = dur; manager = mgr;
        StartCoroutine(DestroyRoutine());
    }

    private IEnumerator DestroyRoutine()
    {
        yield return new WaitForSeconds(delay);
        float elapsed = 0f;
        Vector3 initialScale = transform.localScale;
        while (elapsed < duration)
        {
            if (this == null) yield break;
            transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        manager.UnregisterVoxel();
        Destroy(gameObject);
    }
}