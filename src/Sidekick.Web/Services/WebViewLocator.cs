using Sidekick.Common.Ui.Views;

namespace Sidekick.Web.Services;

public class WebViewLocator : IViewLocator
{
    /// <inheritdoc />
    public bool SupportsMinimize => false;

    /// <inheritdoc />
    public bool SupportsMaximize => false;

    public List<SidekickWebWrapper> Views { get; } = [];

    public void Open(SidekickViewType type, string url)
    {
        Views.ForEach(x => x.NavigationManager.NavigateTo(url));
    }

    public void Close(SidekickViewType type)
    {
        Views.ForEach(x => x.NavigationManager.NavigateTo("/home"));
    }

    public bool IsOverlayOpened() => false;
}
