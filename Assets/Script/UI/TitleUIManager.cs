using UnityEngine;
using CareerQuest.Core;
using UnityEngine.Events;

public class TitleUIManager : MonoBehaviour
{
    [SerializeField] private Canvas titleUICanvas;
    [SerializeField] private Canvas audioSettingsCanvas;

    private void Start()
    {
        CustomButtonSubscriber.Subscribe("ContinueButton", ButtonEventType.Click, LobbyLoad);
        CustomButtonSubscriber.Subscribe("StartButton", ButtonEventType.Click, LobbyLoad);
        CustomButtonSubscriber.Subscribe("SettingsButton", ButtonEventType.Click, OpenAudioSettings);
        CustomButtonSubscriber.Subscribe("QuitButton", ButtonEventType.Click, QuitGame);
        CustomButtonSubscriber.Subscribe("CloseAudioSettingsButton", ButtonEventType.Click, CloseAudioSettings);
    }

    private void LobbyLoad()
    {
        ServiceLocator.Resolve<SceneLoader>().Load(SceneType.Lobby);
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
