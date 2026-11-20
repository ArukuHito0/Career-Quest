using UnityEngine;
using CareerQuest.Core;

public class ResultManager : MonoBehaviour
{
    [SerializeField] private CustomButton toLobbyButton;

    private void Awake()
    {
        toLobbyButton.AddListener(ButtonEventType.Click, LoadTitle);
    }

    private void LoadTitle()
    {
        ServiceLocator.Resolve<SceneLoader>()?.Load(SceneType.Title);
    }

    private void LoadShop()
    {
        ServiceLocator.Resolve<SceneLoader>()?.Load(SceneType.Shop);
    }

    private void LoadSceneSubscribes()
    {
        CustomButtonSubscriber.Subscribe("ToLobbyButton", ButtonEventType.Click, LoadTitle);
        CustomButtonSubscriber.Subscribe("ToShopButton", ButtonEventType.Click, LoadShop);
    }

    private void LoadSceneUnsubscribes()
    {
        CustomButtonSubscriber.Unsubscribe("ToLobbyButton", ButtonEventType.Click, LoadTitle);
        CustomButtonSubscriber.Unsubscribe("ToShopButton", ButtonEventType.Click, LoadShop);
    }
}
