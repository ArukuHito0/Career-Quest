
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitSelectionUI : MonoBehaviour
{
    public enum SelectionType
    {
        All,
        Fighter,
        Tank,
        Shooter,
        Carrier,
        Escort
    }

    // 現在ONになっている選択項目
    private HashSet<SelectionType> selectedTypes = new HashSet<SelectionType>();

    [Header("選択ボタン")]
    [SerializeField] private Button allButton;
    [SerializeField] private Button fighterButton;
    [SerializeField] private Button tankButton;
    [SerializeField] private Button shooterButton;
    [SerializeField] private Button carrierButton;
    [SerializeField] private Button escortButton;

    [Header("ボタンの色")]
    [SerializeField] private Color selectedColor = Color.green;
    [SerializeField] private Color unselectedColor = Color.white;

    private void Awake()
    {
        // ゲーム開始時はAllだけを選択
        selectedTypes.Clear();
        selectedTypes.Add(SelectionType.All);

        UpdateButtonColors();
    }

    public void SelectAll() => ToggleSelection(SelectionType.All);
    public void SelectFighter() => ToggleSelection(SelectionType.Fighter);
    public void SelectTank() => ToggleSelection(SelectionType.Tank);
    public void SelectShooter() => ToggleSelection(SelectionType.Shooter);
    public void SelectCarrier() => ToggleSelection(SelectionType.Carrier);
    public void SelectEscort() => ToggleSelection(SelectionType.Escort);

    private void ToggleSelection(SelectionType type)
    {
        if (selectedTypes.Contains(type))
        {
            selectedTypes.Remove(type);
        }
        else
        {
            selectedTypes.Add(type);
        }

        UpdateButtonColors();

        Debug.Log("現在の選択：" + string.Join(", ", selectedTypes));
    }

    public bool IsSelected(SelectionType type)
    {
        return selectedTypes.Contains(type);
    }

    // ユニットが移動指示の対象かどうか
    public bool IsTarget(PlayerMove player)
    {
        if (player == null)
            return false;

        // AllがONなら全ユニットが対象
        if (IsSelected(SelectionType.All))
            return true;

        UnitClass unitClass = player.GetComponent<UnitClass>();
        PlayerStateManager state = player.GetComponent<PlayerStateManager>();

        if (state == null)
            return false;

        // Carry状態のユニット
        if (IsSelected(SelectionType.Carrier) && state.IsCarryMode())
            return true;

        // Escort状態のユニット
        if (!state.IsEscortMode())
            return false;

        // EscortボタンがONなら全クラスのEscort状態ユニット
        if (IsSelected(SelectionType.Escort))
            return true;

        // クラス別の選択
        if (unitClass == null)
            return false;

        if (IsSelected(SelectionType.Fighter) &&
            unitClass.CurrentClass == UnitClass.ClassType.Fighter)
            return true;

        if (IsSelected(SelectionType.Tank) &&
            unitClass.CurrentClass == UnitClass.ClassType.Tank)
            return true;

        if (IsSelected(SelectionType.Shooter) &&
            unitClass.CurrentClass == UnitClass.ClassType.Shooter)
            return true;

        return false;
    }

    // お宝への移動指示を出せる状態かどうか
    public bool IsTreasureTargetSelected()
    {
        return IsSelected(SelectionType.All) ||
               IsSelected(SelectionType.Carrier);
    }

    // 選択状態に応じてボタンの色を更新
    private void UpdateButtonColors()
    {
        UpdateButtonColor(allButton, SelectionType.All);
        UpdateButtonColor(fighterButton, SelectionType.Fighter);
        UpdateButtonColor(tankButton, SelectionType.Tank);
        UpdateButtonColor(shooterButton, SelectionType.Shooter);
        UpdateButtonColor(carrierButton, SelectionType.Carrier);
        UpdateButtonColor(escortButton, SelectionType.Escort);
    }

    private void UpdateButtonColor(Button button, SelectionType type)
    {
        if (button == null || button.targetGraphic == null)
            return;

        button.targetGraphic.color =
            IsSelected(type) ? selectedColor : unselectedColor;
    }
}
