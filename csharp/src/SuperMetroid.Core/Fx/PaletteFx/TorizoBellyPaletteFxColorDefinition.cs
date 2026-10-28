namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One Torizo-specific entry and six-frame belly palette loop.</summary>
public sealed class TorizoBellyPaletteFxColorDefinition(
        TorizoBellyPaletteOwner owner,
        PaletteFxObject paletteFxObject,
        PaletteFxProgram program)
{
    /// <summary>The Torizo variant represented by this program.</summary>
    public TorizoBellyPaletteOwner Owner { get; } = owner;

    /// <summary>The palette-FX object that installs this program.</summary>
    public PaletteFxObject Object { get; } = paletteFxObject;

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public PaletteFxColorRows Rows { get; } = PaletteFxColorRows.Of(program, TorizoBellyPaletteFxColorDefinitions.ColorsPerFrame);
}
