namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete exploding-Zebes layer-fade control program.</summary>
public sealed class ZebesExplosionLayerFadePaletteFxColorDefinition(
        ZebesExplosionLayerFadePaletteFxProgramOwner owner,
        PaletteFxObject paletteFxObject,
        PaletteFxProgram program,
        int colorByteIndex,
        int frameDuration)
{
    /// <summary>The mutually exclusive faded layer.</summary>
    public ZebesExplosionLayerFadePaletteFxProgramOwner Owner { get; } = owner;

    /// <summary>The palette-FX object that installs this program.</summary>
    /// <remarks><c>$8D:E1DC</c> is crust and <c>$8D:E1E0</c> is grey clouds.</remarks>
    public PaletteFxObject Object { get; } = paletteFxObject;

    /// <summary>The first destination byte in CGRAM.</summary>
    public int ColorByteIndex { get; } = colorByteIndex;

    /// <summary>The cartridge-authored duration of each timed record.</summary>
    public int FrameDuration { get; } = frameDuration;

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public PaletteFxColorRows Rows { get; } = PaletteFxColorRows.Of(program, ZebesExplosionLayerFadePaletteFxColorDefinitions.ColorsPerFrame);
}
