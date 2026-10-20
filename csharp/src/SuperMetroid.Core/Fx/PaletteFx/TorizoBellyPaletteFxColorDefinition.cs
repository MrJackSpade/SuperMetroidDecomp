namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One Torizo-specific entry and six-frame belly palette loop.</summary>
public sealed class TorizoBellyPaletteFxColorDefinition
{
    /// <summary>Binds one Torizo palette loop to its object and first record.</summary>
    internal TorizoBellyPaletteFxColorDefinition(
        TorizoBellyPaletteOwner owner,
        PaletteFxObject paletteFxObject,
        PaletteFxProgram program)
    {
        Owner = owner;
        Object = paletteFxObject;
        Rows = PaletteFxColorRows.Of(program, TorizoBellyPaletteFxColorDefinitions.ColorsPerFrame);
    }

    /// <summary>The Torizo variant represented by this program.</summary>
    public TorizoBellyPaletteOwner Owner { get; }

    /// <summary>The palette-FX object that installs this program.</summary>
    public PaletteFxObject Object { get; }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public PaletteFxColorRows Rows { get; }
}
