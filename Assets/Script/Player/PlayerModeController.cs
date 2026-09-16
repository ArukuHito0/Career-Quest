using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;

public class PlayerModeController : MonoBehaviour
{
    // 護衛モードに変更するキー
    [SerializeField] private Key escortKey = Key.Q;

    // 運搬モードに変更するキー
    [SerializeField] private Key carryKey = Key.W;

    // モード変更範囲
    [SerializeField] private float changeRange = 5f;

    // プレイヤー一覧
    [SerializeField] private PlayerMove[] players;

    // 範囲を表示するオブジェクト
    [SerializeField] private Transform rangeMarker;

    // マウス位置を取得するカメラ
    [SerializeField] private Camera mainCamera;

    // 地面として扱うLayer
    [SerializeField] private LayerMask groundLayer;

    // 現在範囲表示中か
    private bool isSelecting;

    // 現在選択しているモード
    private PlayerStateManager.PlayerMode selectedMode;

    // 現在の範囲の中心
    private Vector3 targetPosition;

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (rangeMarker != null)
            rangeMarker.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Mouse.current == null)
            return;

        // 護衛モードボタン
        if (Keyboard.current[escortKey].wasPressedThisFrame)
        {
            StartModeSelection(PlayerStateManager.PlayerMode.Escort);
        }

        // 運搬モードボタン
        if (Keyboard.current[carryKey].wasPressedThisFrame)
        {
            StartModeSelection(PlayerStateManager.PlayerMode.Carry);
        }

        // モード変更中
        if (isSelecting)
        {
            UpdateRangePosition();

            // 右クリックで確定
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                ChangePlayersMode();

                isSelecting = false;

                if (rangeMarker != null)
                    rangeMarker.gameObject.SetActive(false);
            }
        }
    }

    // モード選択を開始
    private void StartModeSelection(PlayerStateManager.PlayerMode mode)
    {
        selectedMode = mode;
        isSelecting = true;

        Debug.Log($"変更先モード：{selectedMode}");

        if (rangeMarker != null)
            rangeMarker.gameObject.SetActive(true);
    }

    // マウス位置に範囲を移動
    private void UpdateRangePosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer))
            return;

        // NavMesh上の位置を取得
        if (!NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
            return;

        targetPosition = navHit.position;

        // 範囲表示をマウス位置へ移動
        if (rangeMarker != null)
        {
            rangeMarker.position = targetPosition;
            rangeMarker.localScale = new Vector3(changeRange * 2f, 0.1f, changeRange * 2f);
        }
    }

    // 範囲内のプレイヤーを指定したモードへ変更
    private void ChangePlayersMode()
    {
        if (players == null || players.Length == 0)
        {
            Debug.LogWarning("PlayerModeControllerにプレイヤーが登録されていません");
            return;
        }

        foreach (PlayerMove player in players)
        {
            if (player == null)
                continue;

            PlayerStateManager stateManager = player.GetComponent<PlayerStateManager>();

            if (stateManager == null)
                continue;

            // お化けは対象外
            if (stateManager.IsGhost())
                continue;

            // プレイヤーとの距離を計算
            Vector3 offset = player.transform.position - targetPosition;
            offset.y = 0f;

            float distance = offset.magnitude;

            // 範囲外なら対象外
            if (distance > changeRange)
                continue;

            // 護衛モードへ変更
            if (selectedMode == PlayerStateManager.PlayerMode.Escort)
            {
                stateManager.SetEscortMode();
            }

            // 運搬モードへ変更
            if (selectedMode == PlayerStateManager.PlayerMode.Carry)
            {
                stateManager.SetCarryMode();
            }
        }
    }
}