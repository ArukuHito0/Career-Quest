using UnityEngine;

public class PlayerVisualController : MonoBehaviour
{
    // プレイヤーの状態管理
    private PlayerStateManager stateManager;

    // 運搬モードの見た目
    [SerializeField] private GameObject carryVisual;

    // 護衛モードの見た目
    [SerializeField] private GameObject escortVisual;

    // 着替え中の見た目
    [SerializeField] private GameObject changingVisual;

    // お化け状態に見た目
    [SerializeField] private GameObject ghostVisual;

    private void Awake()
    {
        stateManager = GetComponent<PlayerStateManager>();
    }

    void Start()
    {
        UpdateVisual();
    }

    void Update()
    {
        UpdateVisual();
    }

    // 現在の状態に合わせて見た目を変更
    private void UpdateVisual()
    {
        if (stateManager == null)
            return;

        // お化け状態を優先
        if (stateManager.IsGhost())
        {
            SetVisual(false, false, false, true);
            return;
        }

        // 着替え中
        if (stateManager.IsChanging())
        {
            SetVisual(false, false, true, false);
            return;
        }

        // 運搬モード
        if (stateManager.IsCarryMode())
        {
            SetVisual(true, false, false, false);
            return;
        }

        // 護衛モード
        if (stateManager.IsEscortMode())
        {
            SetVisual(false, true, false, false);
        }
    }

    // 表示する見た目を設定
    private void SetVisual(bool carry, bool escort, bool changing, bool ghost)
    {
        if (carryVisual != null)
            carryVisual.SetActive(carry);

        if (escortVisual != null)
            escortVisual.SetActive(escort);

        if (changingVisual != null)
            changingVisual.SetActive(changing);

        if (ghostVisual != null)
            ghostVisual.SetActive(ghost);
    }
}
