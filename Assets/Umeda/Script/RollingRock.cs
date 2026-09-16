using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class RollingRock : MonoBehaviour
{
    [System.Serializable]
    public class Waypoint { public Transform targetTransform; public bool autoProceed = false; }

    [Header("設定")]
    public List<Waypoint> waypoints = new List<Waypoint>();
    public float moveSpeed = 3f;
    public float rotationSpeedMultiplier = 200f; // 回転スピード調整用
    public float stopDistance = 0.05f;

    [Tooltip("オブジェクトの大きさの倍率を入力")]
    public float magnification = 1f;

    [Header("起動設定")]
    public bool rollByClick = false;       // クリックで転がるか
    public bool rollByPlayer = true;     // プレイヤー接近で転がるか（初期値：true）

    [Header("プレイヤー検知設定")]
    public Transform player;             // 手動設定用（空でもタグで自動取得します）
    public float detectionDistance = 3f; // プレイヤーが近づいたと判定する距離

    private Rigidbody rb;
    private int currentIndex = 0;
    private bool isRolling = false;
    private bool isGrounded = false;

    // 回転用：モデルを子要素にするか、Transformを直接回転させる
    public Transform visualMesh;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.isKinematic = false;
    }

    void Update()
    {
        // 1. 接地判定と物理の切り替え（落下以外は不動）
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.6f * magnification);
        rb.isKinematic = isGrounded && !isRolling;

        // 2. 移動開始判定
        if (isGrounded && !isRolling && currentIndex < waypoints.Count)
        {
            bool triggerClick = rollByClick && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && IsClicked();

            bool triggerPlayer = false;
            if (rollByPlayer)
            {
                // player変数にアサインされていない、またはシーン内で見つからない場合に備えてタグから取得
                Transform targetPlayer = player;
                if (targetPlayer == null)
                {
                    GameObject playerObj = GameObject.FindWithTag("Player");
                    if (playerObj != null)
                    {
                        targetPlayer = playerObj.transform;
                    }
                }

                // プレイヤーが見つかっていれば距離を測定
                if (targetPlayer != null)
                {
                    float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.position);
                    if (distanceToPlayer <= detectionDistance)
                    {
                        triggerPlayer = true;
                    }
                }
            }

            if (waypoints[currentIndex].autoProceed || triggerClick || triggerPlayer)
            {
                isRolling = true;
            }
        }
    }

    void FixedUpdate()
    {
        if (isRolling && currentIndex < waypoints.Count)
        {
            Transform target = waypoints[currentIndex].targetTransform;
            Vector3 targetXZ = new Vector3(target.position.x, transform.position.y, target.position.z);
            Vector3 direction = (targetXZ - transform.position).normalized;

            // スライド移動
            Vector3 move = direction * moveSpeed * Time.fixedDeltaTime;
            transform.position += move;

            // 回転演出：進行方向に垂直な軸で回す
            if (visualMesh != null)
            {
                Vector3 rotationAxis = Vector3.Cross(Vector3.up, direction);
                visualMesh.Rotate(rotationAxis, moveSpeed * Time.fixedDeltaTime * rotationSpeedMultiplier / 2f, Space.World);
            }

            // 停止距離判定
            if (Vector2.Distance(new Vector2(transform.position.x, transform.position.z),
                                new Vector2(target.position.x, target.position.z)) < stopDistance)
            {
                transform.position = new Vector3(target.position.x, target.position.y, target.position.z);
                isRolling = false;
                currentIndex++;
            }
        }
    }

    private bool IsClicked()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        return Physics.Raycast(ray, out hit) && hit.transform == this.transform;
    }
}