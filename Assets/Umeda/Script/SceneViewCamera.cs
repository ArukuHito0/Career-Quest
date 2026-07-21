using UnityEngine;
using UnityEngine.InputSystem;

public class SceneViewCamera : MonoBehaviour
{
    [Header("感度・ブレ対策")]
    public float lookSpeed = 0.2f;
    public float deadZone = 0.5f; // これ以下の動きは無視

    [Header("移動設定")]
    public float initialVelocity = 10f;
    public float acceleration = 25f;
    public float dashMultiplier = 2f;
    public float stopTime = 0.1f;

    public float currentSpeed = 0f;
    private float rotationX = 0f;
    private float rotationY = 0f;
    private bool isDashing = false;

    void Start()
    {
        // 初期角度の設定
        rotationX = transform.eulerAngles.y;
        rotationY = transform.eulerAngles.x;
    }

    // LateUpdateで更新することで、入力に対する視覚的なラグを最小化
    void LateUpdate()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.rightButton.isPressed)
        {
            // 1. マウス入力の即時取得（スムージングなしでダイレクトに）
            Vector2 rawDelta = Mouse.current.delta.ReadValue();

            // ブレ防止（しきい値以下は無視）
            if (rawDelta.magnitude < deadZone) rawDelta = Vector2.zero;

            // 2. 回転処理（遅延なし）
            rotationX += rawDelta.x * lookSpeed;
            rotationY -= rawDelta.y * lookSpeed;
            rotationY = Mathf.Clamp(rotationY, -89f, 89f);
            transform.eulerAngles = new Vector3(rotationY, rotationX, 0);

            // 3. Shift即時切り替え
            bool shiftPressed = Keyboard.current.leftShiftKey.isPressed;
            if (shiftPressed != isDashing)
            {
                isDashing = shiftPressed;
                currentSpeed *= (isDashing ? dashMultiplier : (1f / dashMultiplier));
            }

            // 4. 移動処理（直接加算で遅延なし）
            Vector3 inputDir = GetInputDirection();
            if (inputDir.sqrMagnitude > 0)
            {
                float currentAccel = isDashing ? (acceleration * dashMultiplier) : acceleration;
                currentSpeed += currentAccel * Time.deltaTime;
                transform.position += inputDir * currentSpeed * Time.deltaTime;
            }
            else
            {
                ApplyBrake();
            }
        }
        else
        {
            ApplyBrake();
        }
    }

    private void ApplyBrake()
    {
        if (currentSpeed > 0)
        {
            currentSpeed -= (currentSpeed / Mathf.Max(stopTime, 0.01f)) * Time.deltaTime;
            if (currentSpeed < 0) currentSpeed = 0;
        }
    }

    private Vector3 GetInputDirection()
    {
        Vector3 dir = Vector3.zero;
        if (Keyboard.current.wKey.isPressed) dir += transform.forward;
        if (Keyboard.current.sKey.isPressed) dir -= transform.forward;
        if (Keyboard.current.dKey.isPressed) dir += transform.right;
        if (Keyboard.current.aKey.isPressed) dir -= transform.right;
        if (Keyboard.current.eKey.isPressed) dir += Vector3.up;
        if (Keyboard.current.qKey.isPressed) dir -= Vector3.up;
        return dir.normalized;
    }
}