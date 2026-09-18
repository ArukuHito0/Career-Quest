using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class PlayerStateManager : MonoBehaviour
{
    // 生存状態
    public enum LifeState
    {
        Alive,
        Ghost
    }

    // 行動モード
    public enum PlayerMode
    {
        Carry,
        Escort
    }

    // 現在の生存状態
    [SerializeField] private LifeState currentLifeState = LifeState.Alive;

    // 現在の行動モード
    [SerializeField] private PlayerMode currentMode = PlayerMode.Carry;

    // 着替えにかかる時間
    [SerializeField] private float changeModeTime = 2f;

    // 現在着替え中か
    private bool isChangingMode;

    // 現在の生存状態を取得
    public LifeState CurrentLifeState => currentLifeState;

    // 現在のモードを取得
    public PlayerMode CurrentMode => currentMode;

    // 着替え中か取得
    public bool IsChangingMode => isChangingMode;

    // 生きているか
    public bool IsAlive()
    {
        return currentLifeState == LifeState.Alive;
    }

    // お化け状態か
    public bool IsGhost()
    {
        return currentLifeState == LifeState.Ghost;
    }

    // 運搬モードか
    public bool IsCarryMode()
    {
        return currentMode == PlayerMode.Carry;
    }

    // 護衛モードか
    public bool IsEscortMode()
    {
        return currentMode == PlayerMode.Escort;
    }

    // 着替え中か
    public bool IsChanging()
    {
        return isChangingMode;
    }

    // 移動できるか
    public bool CanMove()
    {
        if (isChangingMode)
            return false;

        if (IsGhost())
            return false;

        return true;
    }

    // 攻撃できるか
    public bool CanAttack()
    {
        if (isChangingMode)
            return false;

        if (IsGhost())
            return false;

        return IsEscortMode();
    }

    // 運搬できるか
    public bool CanCarry()
    {
        if (isChangingMode)
            return false;

        if (IsGhost())
            return false;

        return IsCarryMode();
    }

    // 生存状態を変更
    public void SetLifeState(LifeState newState)
    {
        currentLifeState = newState;

        Debug.Log($"{gameObject.name}生存状態変更：{newState}");
    }

    // 指定したモードへ変更
    public void SetMode(PlayerMode newMode)
    {
        // お化け状態なら変更しない
        if (IsGhost())
            return;

        // すでに着替え中なら変更しない
        if (isChangingMode)
            return;

        // すでに指定されたモードなら変更しない
        if (currentMode == newMode)
            return;

        // 運搬中のプレイヤーを取得
        PlayerCarry playerCarry = GetComponent<PlayerCarry>();

        // 護衛モードへ変更する場合
        if (newMode == PlayerMode.Escort)
        {
            // 現在お宝を運搬中なら解除
            if (playerCarry != null && playerCarry.IsCarrying())
                playerCarry.Release();
        }

        // 移動を停止
        StopMovement();

        // 着替え開始
        StartCoroutine(ChangeModeCoroutine(newMode));
    }

    // 運搬モードへ変更
    public void SetCarryMode()
    {
        SetMode(PlayerMode.Carry);
    }

    // 護衛モードへ変更
    public void SetEscortMode()
    {
        SetMode(PlayerMode.Escort);
    }

    // プレイヤーの移動を停止
    private void StopMovement()
{
    PlayerMove playerMove = GetComponent<PlayerMove>();

    if (playerMove != null)
    {
        playerMove.StopMovement();
        return;
    }

    NavMeshAgent agent = GetComponent<NavMeshAgent>();

    if (agent == null)
        return;

    if (!agent.enabled)
        return;

    agent.ResetPath();
    agent.velocity = Vector3.zero;
}

    // 着替え処理
    private IEnumerator ChangeModeCoroutine(PlayerMode newMode)
    {
        isChangingMode = true;

        // 着替え開始時にもう一度移動を停止
        StopMovement();

        Debug.Log($"{gameObject.name}着替え開始");

        // 着替え中
        yield return new WaitForSeconds(changeModeTime);

        // 着替え完了後にモード変更
        currentMode = newMode;

        isChangingMode = false;

        Debug.Log($"{gameObject.name}着替え完了→{currentMode}");

        // もともと向かっていた場所へ再び移動
        PlayerMove playerMove = GetComponent<PlayerMove>();

        if (playerMove != null)
        {
            playerMove.ResumeMove();
        }
    }

    // お化け状態へ変更
    public void SetGhost()
    {
        currentLifeState = LifeState.Ghost;

        // 着替え中だった場合は解除
        isChangingMode = false;

        // 移動停止
        StopMovement();

        Debug.Log($"{gameObject.name}がお化けになりました");
    }

    // 生存状態へ変更
    public void SetAlive()
    {
        currentLifeState = LifeState.Alive;

        Debug.Log($"{gameObject.name}が復活しました");
    }
}