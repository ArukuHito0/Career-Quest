using UnityEngine;
using UnityEngine.UI;

public abstract class HPBarBase : MonoBehaviour
{
    // HPを表示するSlider
    [SerializeField] protected Slider hpSlider;

    protected virtual void Awake()
    {
        // HP管理クラスを取得
        FindHealth();

        if (hpSlider == null)
        {
            Debug.Log($"{gameObject.name}: HP Sliderが設定されていません");
            return;
        }

        if (GetMaxHealth() <= 0)
        {
            Debug.Log($"{gameObject.name}: HP管理クラスが見つかりません");
            return;
        }

        // HPバーの最大値を設定
        hpSlider.maxValue = GetMaxHealth();

        // 現在HPを設定
        hpSlider.value = GetCurrentHealth();
    }

    protected virtual void Update()
    {
        if (hpSlider == null)
            return;

        // 現在HPをHPバーに反映
        hpSlider.value = GetCurrentHealth();
    }

    // HP管理クラスを取得する
    protected abstract void FindHealth();

    // 最大HPを取得する
    protected abstract int GetMaxHealth();

    // 現在HPを取得する
    protected abstract int GetCurrentHealth();
}