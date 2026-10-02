using UnityEngine;

public class BillBoard : MonoBehaviour
{
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (mainCamera == null)
            return;

        transform.forward = mainCamera.transform.forward;
    }
}
