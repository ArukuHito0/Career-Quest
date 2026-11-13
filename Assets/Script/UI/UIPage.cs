using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class UIPage : MonoBehaviour
{
    [SerializeField] private UnityEvent onEnter;
    [SerializeField] private UnityEvent onExit;

    protected IPageTransition enterTransition;
    protected IPageTransition exitTransition;

    public async Task OnPageEnterAsync(CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        if (enterTransition != null)
            await enterTransition.PageTransition(token);

        onEnter?.Invoke();
    }

    public async Task OnPageExitAsync(CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        if (exitTransition != null)
            await exitTransition.PageTransition(token);

        onExit?.Invoke();
    }
}
