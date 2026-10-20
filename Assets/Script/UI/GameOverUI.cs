
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [Header("ゲームオーバーUI")]
    [SerializeField] private GameObject gameOverPanel;

    [Header("ゲームクリアUI")]
    [SerializeField] private GameObject gameClearPanel;

    private void Awake()
    {
        // 最初はゲームオーバーUIを非表示にする
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // 最初はゲームクリアUIを非表示にする
        if (gameClearPanel != null)
            gameClearPanel.SetActive(false);
    }

    // ゲームオーバーUIを表示する
    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    // ゲームクリアUIを表示する
    public void ShowGameClear()
    {
        if (gameClearPanel != null)
        {
            gameClearPanel.SetActive(true);
        }
    }
}
