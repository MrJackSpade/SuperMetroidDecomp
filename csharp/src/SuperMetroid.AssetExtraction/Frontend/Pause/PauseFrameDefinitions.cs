namespace SuperMetroid.AssetExtraction.Frontend.Pause;

/// <summary>
/// The pause frame as the cartridge lays it out (#1467): where its button rows replace the backdrop's and the palettes the
/// labels are recoloured with. Only the import reads them; it pictures the frame once.
/// </summary>
internal static class PauseFrameDefinitions
{
    /// <summary>The backdrop row the two button rows start on.</summary>
    internal const int ButtonRowsFirstRow = 25;
    /// <summary>The button page's row the two shown button rows start on.</summary>
    internal const int ButtonSourceFirstRow = 9;
    /// <summary>The button rows the frame shows.</summary>
    internal const int ButtonRowCount = 2;
    /// <summary>The colour variant of a lit label.</summary>
    internal const int LitPalette = 2;
    /// <summary>The colour variant of a dim label.</summary>
    internal const int DimPalette = 5;
}
