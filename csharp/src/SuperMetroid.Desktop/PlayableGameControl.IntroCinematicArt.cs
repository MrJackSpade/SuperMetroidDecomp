namespace SuperMetroid.Desktop;

public sealed partial class PlayableGameControl
{
    /// <summary>Installed opening-cinematic artwork catalog retained for game binding and installation diagnostics.</summary>
    private SuperMetroid.Core.Assets.IntroCinematicArtworkCatalog? introCinematicArt;

    /// <summary>Installed Samus body-art catalog retained for game binding and installation diagnostics.</summary>
    private SuperMetroid.Core.Assets.SamusBodyArtworkCatalog? samusBodyArt;
}
