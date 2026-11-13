using System.Threading;
using System.Threading.Tasks;

public interface IPageTransition
{
    public Task PageTransition(CancellationToken token = default);
}