namespace SuperMetroid.Desktop;

public sealed partial class PlayableGameControl
{
    /// <summary>Installed ending-palette data retained for game binding and presentation identity; null before installation loads it.</summary>
    private SuperMetroid.Core.Assets.EndingPaletteCatalog? endingPaletteArt;
}
