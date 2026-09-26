namespace SuperMetroid.Desktop;

public sealed partial class PlayableGameControl
{
    // Kept by the host across state loads. Restart reloads disk overrides explicitly.
    private SuperMetroid.Core.Assets.AreaMapPresentationCatalog? mapPresentation;
    private SuperMetroid.Core.Assets.GameplayBasePaletteCatalog? gameplayBasePalettes;
}
