
using System.Collections;
using UnityEngine;
using CareerQuest.Core;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Tooltip("ゲーム終了表示からリザルトシーンへ移るまでの秒数")]
    [SerializeField] private float resultSceneDelay = 1.5f;

    [Header("UI設定")]
    [SerializeField] private GameOverUI gameOverUI;

    public bool IsGameClear { get; private set; }
    public bool IsGameOver { get; private set; }

    public bool IsGameEnded => IsGameClear || IsGameOver;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void GameOver()
    {
        if (IsGameEnded) return;

        IsGameOver = true;

        if (gameOverUI != null)
        {
            gameOverUI.ShowGameOver();
        }

        EndGame();
    }

    public void GameClear()
    {
        if (IsGameEnded) return;

        IsGameClear = true;

        Debug.Log("ゲームクリア");

        if (gameOverUI != null)
        {
            gameOverUI.ShowGameClear();
        }

        EndGame();
    }

    private void EndGame()
    {
        // ゲーム内の時間を停止
        Time.timeScale = 0f;

        // リザルトシーンへの遷移を開始
        StartCoroutine(LoadResultSceneAfterDelay());
    }

    private IEnumerator LoadResultSceneAfterDelay()
    {
        // timeScale = 0 でも待機する
        yield return new WaitForSecondsRealtime(
            Mathf.Max(0f, resultSceneDelay)
        );

        // シーン遷移前に時間を戻す
        Time.timeScale = 1f;

        // SceneLoader経由でリザルトシーンへ
        SceneLoader loader;

        try
        {
            loader = ServiceLocator.Resolve<SceneLoader>();
        }
        catch (System.InvalidOperationException e)
        {
            Debug.LogError($"リザルトシーンへ遷移できませんでした: {e.Message}");
            yield break;
        }

        loader.Load(SceneType.Result);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
