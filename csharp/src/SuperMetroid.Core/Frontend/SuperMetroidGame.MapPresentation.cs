using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    // Host-owned immutable content is rebound after graph restoration, not embedded
    // in debugger states where it would hide newer user overrides on disk.
    [NonSerialized] private AreaMapPresentationCatalog? mapPresentation;

    public string? MapPresentationIdentity => mapPresentation?.ContentIdentity;

    /// <summary>Attaches the host's current catalog after construction or debugger-state load.</summary>
    public void BindMapPresentation(AreaMapPresentationCatalog? catalog)
    {
        mapPresentation = catalog;
        if (runtime is not null) runtime.MapPresentation = catalog;
        pauseMenu?.BindMapPresentation(catalog);
    }
}
