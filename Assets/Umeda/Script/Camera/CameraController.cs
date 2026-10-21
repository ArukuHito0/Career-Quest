using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;

public class CameraController : MonoBehaviour
{
    [Header("移動設定")]
    [Tooltip("移動速度")]
    public float moveSpeed = 10f;

    [Header("ドラッグ移動設定")]
    [Tooltip("ホイールドラッグによる移動の感度")]
    public float dragSpeed = 0.05f;

    [Header("ズーム設定")]
    [Tooltip("ズーム速度")]
    public float zoomSpeed = 10f;

    [Header("自動追従・全体表示設定")]
    [Tooltip("自動追従モードかどうか")]
    public bool isAutoFollow = false;
    [Tooltip("シーン内のプレイヤー全員のTransform")]
    public List<Transform> players = new List<Transform>();
    [Tooltip("プレイヤーたちを囲む際のカメラの高さの基本値")]
    public float baseCameraHeight = 20f;
    [Tooltip("プレイヤーが広がった時に追加で離れるマージン")]
    public float framingMargin = 5f;
    [Tooltip("追従時のスムーズさ")]
    public float followSmoothSpeed = 5f;

    [Header("UI表示用")]
    public TextMeshProUGUI statusText; // 例：「自動追従：ON/OFF」を表示するテキスト

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();

        // プレイヤーリストが空ならタグで自動収集を試みる
        if (players.Count == 0)
        {
            GameObject[] foundPlayers = GameObject.FindGameObjectsWithTag("Player");
            foreach (var p in foundPlayers)
            {
                players.Add(p.transform);
            }
        }
    }

    void Update()
    {
        // Cキーでモードを切り替え
        if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame)
        {
            ToggleAutoFollow();
        }

        if (isAutoFollow)
        {
            HandleAutoFollow();
        }
        else
        {
            HandleMove();
            HandleDragMove();
            HandleZoom();
        }
    }

    // 自動追従モードの切り替え（UIのボタンからも呼び出し可能）
    public void ToggleAutoFollow()
    {
        isAutoFollow = !isAutoFollow;

        // 画面上のテキストを更新
        UpdateStatusUI();

        Debug.Log("カメラ自動追従モード: " + (isAutoFollow ? "ON" : "OFF"));
    }

    // 20体全員の中心を計算し、画面の真ん中に収まるように追従・調整する
    void HandleAutoFollow()
    {
        if (players == null || players.Count == 0) return;

        // 1. 全プレイヤーの平均中心（重心）を計算
        Vector3 centerPoint = Vector3.zero;
        int activeCount = 0;

        foreach (var player in players)
        {
            if (player != null && player.gameObject.activeInHierarchy)
            {
                centerPoint += player.position;
                activeCount++;
            }
        }

        if (activeCount == 0) return;
        centerPoint /= activeCount;

        // 2. 中心から一番離れているプレイヤーとの距離を計算
        float maxDistance = 0f;
        foreach (var player in players)
        {
            if (player != null && player.gameObject.activeInHierarchy)
            {
                float dist = Vector3.Distance(new Vector3(centerPoint.x, 0, centerPoint.z), new Vector3(player.position.x, 0, player.position.z));
                if (dist > maxDistance)
                {
                    maxDistance = dist;
                }
            }
        }

        // 3. 目標位置の決定
        // 重心（centerPoint）が画面の中央（真下）に来るよう、
        // カメラの現在の向き（forward / backward の比率）を基準にオフセットを計算します。
        float targetHeight = baseCameraHeight + (maxDistance + framingMargin);

        // カメラの斜め上の角度を維持するため、前方の逆向きベクトルを利用して位置を決定
        Vector3 backwardDir = new Vector3(cam.transform.forward.x, 0, cam.transform.forward.z).normalized;
        if (backwardDir == Vector3.zero) backwardDir = Vector3.back; // 安全用

        // 重心位置を基準に、後ろ＆上へ離れた位置をターゲットにする（これで重心が常に画面中央になる）
        Vector3 targetPosition = centerPoint - (backwardDir * (targetHeight * 0.7f)) + (Vector3.up * targetHeight);

        // スムーズに追従
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSmoothSpeed);
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

        Vector3 forward = cam.transform.forward;
        Vector3 right = cam.transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 moveDir = (forward * v + right * h).normalized;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

    // マウスホイール押し込みドラッグによる移動
    void HandleDragMove()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.middleButton.isPressed)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();

            if (delta.sqrMagnitude > 0.001f)
            {
                Vector3 right = cam.transform.right;
                Vector3 forward = cam.transform.forward;

                right.y = 0f;
                forward.y = 0f;

                right.Normalize();
                forward.Normalize();

                Vector3 moveDelta = (right * -delta.x + forward * -delta.y) * dragSpeed;
                transform.position += moveDelta;
            }
        }
    }

    // マウススクロールによるズーム
    void HandleZoom()
    {
        if (Mouse.current == null) return;

        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) < 0.01f) return;

        Vector3 zoomDir = cam.transform.forward;
        transform.position += zoomDir * (scroll * 0.001f) * zoomSpeed;
    }

    void UpdateStatusUI()
    {
        if (statusText != null)
        {
            statusText.text = isAutoFollow ? "カメラモード: 自動追従" : "カメラモード: 手動操作";
        }
    }
}