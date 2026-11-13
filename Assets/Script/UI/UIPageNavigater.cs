using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public enum PageFlipType
{
    Forward,
    Back,
}

public class UIPageNavigater : MonoBehaviour
{
    [SerializeField] private UIPage[] pages;

    [SerializeField] private CustomButton previousButton;
    [SerializeField] private CustomButton nextButton;

    [SerializeField] private int currentPage = 1;

    private int PageIdx => currentPage - 1;

    private bool isTransitioning = false;
    private CancellationTokenSource destroyCts;

    private void Awake()
    {
        destroyCts = new CancellationTokenSource();

        if (pages != null && pages.Length > 0)
        {
            currentPage = Mathf.Clamp(
                currentPage, 1, pages.Length);
        }
    }

    private void Start()
    {
        previousButton?.AddListener(ButtonEventType.Click, PageBack);
        nextButton?.AddListener(ButtonEventType.Click, PageForward);

        UpdateNavigationButtons();
    }

    private void OnDestroy()
    {
        previousButton?.RemoveListener(ButtonEventType.Click, PageBack);
        nextButton?.RemoveListener(ButtonEventType.Click, PageForward);

        destroyCts?.Cancel();
        destroyCts?.Dispose();
    }

    private async void PageForward()
    {
        await PageFlip(PageFlipType.Forward);
    }

    private async void PageBack()
    {
        await PageFlip(PageFlipType.Back);
    }

    private async Task PageFlip(PageFlipType flipType)
    {
        if (isTransitioning)
            return;

        if (pages == null || pages.Length == 0)
            return;

        int nextPage = currentPage +
            (flipType == PageFlipType.Forward ? 1 : -1);

        if (nextPage < 1 || nextPage > pages.Length)
            return;

        isTransitioning = true;

        ChangeButtonsEnabled();

        try
        {
            var previousPage = pages[PageIdx];
            var targetPage = pages[nextPage - 1];

            await previousPage.OnPageExitAsync(destroyCts.Token);

            destroyCts.Token.ThrowIfCancellationRequested();

            previousPage.gameObject.SetActive(false);
            currentPage = nextPage;
            targetPage.gameObject.SetActive(true);

            await targetPage.OnPageEnterAsync(destroyCts.Token);
        }
        catch (OperationCanceledException)
        {
            // ‘JˆÚ‚ªƒLƒƒƒ“ƒZƒ‹‚³‚ê‚½
        }
        catch (Exception ex)
        {
            Debug.LogException(ex, this);
        }
        finally
        {
            isTransitioning = false;
            ChangeButtonsEnabled();
            UpdateNavigationButtons();
        }
    }

    private void UpdateNavigationButtons()
    {
        previousButton?.gameObject.SetActive(currentPage > 1);
        nextButton?.gameObject.SetActive(currentPage < pages.Length);
    }

    private void ChangeButtonsEnabled()
    {
        if(previousButton != null)
            previousButton.Interactable = !isTransitioning;

        if(nextButton != null)
            nextButton.Interactable = !isTransitioning;
    }
}