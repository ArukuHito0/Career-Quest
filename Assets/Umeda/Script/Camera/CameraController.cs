using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("移動設定")]
    [Tooltip("移動速度")]
    public float moveSpeed = 10f;

    [Header("ズーム設定")]
    [Tooltip("ズーム速度")]
    public float zoomSpeed = 10f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        HandleMove();
        HandleZoom();
    }

    // WASDによる水平移動（高さ固定）
    void HandleMove()
    {
        if (Keyboard.current == null) return;

        float h = 0f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) h += 1f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) h -= 1f;

        float v = 0f;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) v += 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) v -= 1f;

        if (h == 0 && v == 0) return;

        // カメラの水平方向の向きを取得し、Y軸の成分をゼロにして高さを固定する
        Vector3 forward = cam.transform.forward;
        Vector3 right = cam.transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        // 入力に応じた移動ベクトルを計算
        Vector3 moveDir = (forward * v + right * h).normalized;

        // 位置を更新（World空間を基準に移動）
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

    // マウススクロールによるズーム
    void HandleZoom()
    {
        if (Mouse.current == null) return;

        // New Input Systemのスクロール値を取得（環境によって値の大きさが異なるため係数を調整）
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) < 0.01f) return;

        // カメラの前方ベクトルに沿って移動させることでズームを表現
        Vector3 zoomDir = cam.transform.forward;
        transform.position += zoomDir * (scroll * 0.001f) * zoomSpeed;
    }
}