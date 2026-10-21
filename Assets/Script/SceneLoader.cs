using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using CareerQuest.Core;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private SceneDatabase sceneDatabase;

    public float LoadProgress {  get; private set; }
    public bool IsLoading { get; private set; }

    private void Awake()
    {
        ServiceLocator.Register(this);

        DontDestroyOnLoad(this);
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister(this);
    }

    public void Load(SceneType type)
    {
        if (!sceneDatabase.TryGetSceneName(type, out string sceneName))
        {
            Debug.LogError($"SceneType '{type}' Ç™SceneDatabaseÇ…ìoò^Ç≥ÇÍÇƒÇ¢Ç‹ÇπÇÒÅB");
            return;
        }

        StartCoroutine(SceneLoading(sceneName));
    }

    private IEnumerator SceneLoading(string sceneName)
    {
        IsLoading = true;
        LoadProgress = 0f;

        AsyncOperation ope = SceneManager.LoadSceneAsync(sceneName);

        while (!ope.isDone)
        {
            LoadProgress = Mathf.Clamp01(ope.progress / 0.9f);
            yield return null;
        }

        LoadProgress = 1f;
        IsLoading = false;
    }
}
