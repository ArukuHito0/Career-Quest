using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (mainCamera != null)
        {
            // カメラの方向を向きつつ、ワールドのY軸（上下）を固定して安定させる
            transform.LookAt(transform.position + mainCamera.transform.forward, Vector3.up);
        }
    }
}