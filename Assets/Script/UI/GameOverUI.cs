using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [Header("ゲームオーバー表示")]
    [SerializeField] private GameObject gameOverPanel;

    private void Awake()
    {
        // ゲームオーバー画面を非表示にしておく
        if ((gameOverPanel != null))
        {
            gameOverPanel.SetActive(false);
        }
    }

    // ゲームオーバー画面を表示する
    public void Show()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }
}
