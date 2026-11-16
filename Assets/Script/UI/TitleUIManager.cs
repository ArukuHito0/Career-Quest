using UnityEngine;
using CareerQuest.Core;
using UnityEngine.Events;

public class TitleUIManager : MonoBehaviour
{
    [SerializeField] private Canvas titleUICanvas;
    private Canvas audioSettingsCanvas;

    private void Start()
    {
        audioSettingsCanvas = AudioUIManager.Instance.GetComponent<Canvas>();
    }

    private void OnEnable()
    {
        CustomButtonSubscriber.Subscribe("ContinueButton", ButtonEventType.Click, LoadLobby);
        CustomButtonSubscriber.Subscribe("StartButton", ButtonEventType.Click, LoadLobby);
        CustomButtonSubscriber.Subscribe("SettingsButton", ButtonEventType.Click, OpenAudioSettings);
        CustomButtonSubscriber.Subscribe("QuitButton", ButtonEventType.Click, QuitGame);
        CustomButtonSubscriber.Subscribe("CloseAudioSettingsButton", ButtonEventType.Click, CloseAudioSettings);
    }

    private void OnDisable()
    {
        CustomButtonSubscriber.Unsubscribe("ContinueButton", ButtonEventType.Click, LoadLobby);
        CustomButtonSubscriber.Unsubscribe("StartButton", ButtonEventType.Click, LoadLobby);
        CustomButtonSubscriber.Unsubscribe("SettingsButton", ButtonEventType.Click, OpenAudioSettings);
        CustomButtonSubscriber.Unsubscribe("QuitButton", ButtonEventType.Click, QuitGame);
        CustomButtonSubscriber.Unsubscribe("CloseAudioSettingsButton", ButtonEventType.Click, CloseAudioSettings);
    }

    private void LoadLobby()
    {
        ServiceLocator.Resolve<SceneLoader>()?.Load(SceneType.Stage);
    }

    private void OpenAudioSettings()
    {
        audioSettingsCanvas.enabled = true;
        titleUICanvas.enabled = false;
    }

    private void CloseAudioSettings()
    {
        audioSettingsCanvas.enabled = false;
        titleUICanvas.enabled = true;
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
