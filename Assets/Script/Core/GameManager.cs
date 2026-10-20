using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // ほかのスクリプトからGameManagerを呼び出せるようにする
    public static GameManager Instance { get; private set; }

    [Header("リザルト設定")]
    [SerializeField] private string resultSceneName = "ResultScene";

    [Tooltip("ゲームオーバー表示からリザルトシーンへ移るまでの秒数")]
    [SerializeField] private float resultSceneDelay = 1.5f;

    [Header("UI設定")]
    [SerializeField] private GameOverUI gameOverUI;

    // ゲームクリア処理が実行済みかどうか
    public bool IsGameClear { get; private set; }

    // ゲームオーバー処理が実行済みかどうか
    public bool IsGameOver { get; private set; }

    // クリアとゲームオーバーのどちらかが確定したか
    public bool IsGameEnded => IsGameClear || IsGameOver;

    private void Awake()
    {
        // シーン内でGameManagerが重複していないかを確認する
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void GameOver()
    {
        // すでにクリアかゲームオーバーなら、処理を繰り返さない
        if (IsGameEnded)
            return;

        // シーン名が設定されているか確認する
        if (string.IsNullOrWhiteSpace(resultSceneName))
        {
            Debug.LogError("GameManager:リザルトシーン名が設定されていません");
            return;
        }

        IsGameOver = true;

        // ゲームオーバー画面を表示する
        if (gameOverUI != null)
        {
            gameOverUI.ShowGameClear();
        }

        EndGame();
    }

    // ゲームクリア処理を開始
    public void GameClear()
    {
        // すでにクリアかゲームオーバーなら、処理を繰り返さない
        if (IsGameEnded)
            return;

        // シーン名が設定されているか確認する
        if (string.IsNullOrWhiteSpace(resultSceneName))
        {
            Debug.LogError("GameManager:リザルトシーン名が設定されていません");
            return;
        }

        IsGameClear = true;
        Debug.Log("ゲームクリア");
        
        // ゲームクリア画面を表示する
        if (gameOverUI != null)
        {
            gameOverUI.ShowGameOver();
        }

        EndGame();
    }

    // ゲームを終了し、リザルトシーンへ遷移する
    private void EndGame()
    {
        // ゲーム内の時間を停止する
        Time.timeScale = 0f;

        // シーン遷移処理
        StartCoroutine(LoadResultSceneAfterDelay());
    }

    private IEnumerator LoadResultSceneAfterDelay()
    {
        // 指定した時間待機する
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, resultSceneDelay));

        // 停止した時間を元に戻す
        Time.timeScale = 1f;

        // シーンを読み込む
        SceneManager.LoadScene(resultSceneName);
    }

private void OnDestroy()
    {
        // このインスタンスが登録中のものなら解除する
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
