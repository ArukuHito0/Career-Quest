using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using CareerQuest.Core;

/// <summary>
/// 自作ボタンのイベント類
/// </summary>
public enum ButtonEventType
{
    Enter,  // マウスポインタが重なった時
    Exit,   // マウスポインタが外れた時
    Click,  // 左クリックが完了した時
    Up,     // 左クリックを離した時
    Down,   // 左クリックを押した時
}

/// <summary>
/// uGUIで使用する自作ボタンクラス
/// </summary>
[DefaultExecutionOrder(0)]
public class CustomButton : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler,
    IPointerUpHandler,
    IPointerDownHandler
{
    // ------------フィールド変数とプロパティ------------
    #region Fields&Propaties
    [SerializeField] private UnityEvent onClick;
    [SerializeField] private UnityEvent onEnter;
    [SerializeField] private UnityEvent onExit;
    [SerializeField] private UnityEvent onUp;
    [SerializeField] private UnityEvent onDown;

    [SerializeField] private bool interactable = true; // ボタンが有効か
    public bool Interactable
    {
        get => interactable;
        set
        {
            interactable = value;
            Refresh();
        }
    }

    private Func<bool> _condition;          // 使用条件
    private bool lastCanInteract = true;    // 前回の有効・無効状態

    /// <summary>
    /// ボタンの有効・無効状態が切り替わった時のイベント
    /// </summary>
    public event Action<bool> InteractableChanged;

    /// <summary>
    /// ボタンが有効で、使用条件を満たしているか
    /// </summary>
    public bool CanInteract => interactable && (_condition?.Invoke() ?? true);
    #endregion

    private UnityEvent OnButtonEvent(ButtonEventType type)
    {
        return type switch
        {
            ButtonEventType.Click => onClick,
            ButtonEventType.Enter => onEnter,
            ButtonEventType.Exit => onExit,
            ButtonEventType.Up => onUp,
            ButtonEventType.Down => onDown,
            _ => onClick
        };
    }

    // ----------リスナー(イベントの登録・解除)----------
    #region Listeners
    /// <summary>
    /// 引数1の ButtonEventType に対応するイベントに引数2の関数を購読する
    /// </summary>
    public void AddListener(ButtonEventType type, UnityAction action)
        => OnButtonEvent(type).AddListener(action);

    /// <summary>
    /// 引数1の ButtonEventType に対応するイベントに登録されている引数2の関数を解除する
    /// </summary>
    public void RemoveListener(ButtonEventType type, UnityAction action) 
        => OnButtonEvent(type).RemoveListener(action);

    public void RemoveAllListeners()
    {
        onClick.RemoveAllListeners();
        onEnter.RemoveAllListeners();
        onExit.RemoveAllListeners();
        onUp.RemoveAllListeners();
        onDown.RemoveAllListeners();
    }
    #endregion

    // ----------ボタン有効・無効----------
    #region Conditions
    /// <summary>
    /// ボタンの使用条件を設定する
    /// </summary>
    /// <param name="eva"></param>
    public void SetCondition(Func<bool> eva) => _condition = eva;

    /// <summary>
    /// ボタンの使用条件を無くす
    /// </summary>
    public void ClearCondition() => SetCondition(null);

    // ボタンの有効・無効状態が切り替わった際に実行される
    private void Refresh()
    {
        bool now = CanInteract;
        if (now == lastCanInteract) return;
        lastCanInteract = now;
        InteractableChanged?.Invoke(now);
    }
    #endregion

    //----------イベント----------
    #region Events

    // 渡された ButtonEventType に応じて実際に処理を実行する関数
    private void Evaluate(PointerEventData e, ButtonEventType type)
    {
        if (!CanInteract)
            return;

        if (type != ButtonEventType.Enter && type != ButtonEventType.Exit)
            if (e.button != PointerEventData.InputButton.Left)
                return;

        OnButtonEvent(type)?.Invoke();
    }

    public void OnPointerEnter(PointerEventData e) => Evaluate(e, ButtonEventType.Enter);
    public void OnPointerExit(PointerEventData e) => Evaluate(e, ButtonEventType.Exit);
    public void OnPointerClick(PointerEventData e) => Evaluate(e, ButtonEventType.Click);
    public void OnPointerUp(PointerEventData e) => Evaluate(e, ButtonEventType.Up);
    public void OnPointerDown(PointerEventData e) => Evaluate(e, ButtonEventType.Down);
    #endregion

    private void OnEnable()
    {
        ServiceLocator.Resolve<CustomButtonManager>().AddButton(this);
    }

    private void OnDisable()
    {
        ServiceLocator.Resolve<CustomButtonManager>().RemoveButton(this);
    }
}
