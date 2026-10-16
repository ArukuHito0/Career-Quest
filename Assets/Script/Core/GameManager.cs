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

    // ゲームオーバー処理が実行済みかどうか
    public bool IsGameOver { get; private set; }

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
        // すでにゲームオーバーなら、処理を繰り返さない
        if (IsGameOver)
            return;

        // シーン名が未設定ならエラーを表示して終了
        if (string.IsNullOrWhiteSpace(resultSceneName))
        {
            Debug.LogError("GameManager:リザルトシーン名が設定されていません");
            return;
        }

        IsGameOver = true;

        // ゲームオーバー画面を表示する
        if (gameOverUI != null)
        {
            gameOverUI.Show();
        }

        // ゲーム全体の時間を止める
        Time.timeScale = 0;

        // シーン遷移を行う
        StartCoroutine(LoadResultSceneAfterDelay());
    }

    // 指定秒数待ってからリザルトシーンを読み込む
    private IEnumerator LoadResultSceneAfterDelay()
    {
        // 指定した時間待機する
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, resultSceneDelay));

        // 次のシーンが停止状態にならないように時間を元に戻す
        Time.timeScale = 1f;

        // リザルトシーンを読み込む
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
