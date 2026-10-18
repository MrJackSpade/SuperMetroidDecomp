namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One Torizo-specific entry and six-frame belly palette loop.</summary>
public sealed class TorizoBellyPaletteFxColorDefinition
{
    /// <summary>Binds one Torizo palette loop to its object and first record.</summary>
    internal TorizoBellyPaletteFxColorDefinition(
        TorizoBellyPaletteOwner owner,
        PaletteFxObject paletteFxObject,
        int firstRecord)
    {
        Owner = owner;
        Object = paletteFxObject;
        Rows = new(firstRecord, TorizoBellyPaletteFxColorDefinitions.FrameCount, TorizoBellyPaletteFxColorDefinitions.ColorsPerFrame);
    }

    /// <summary>The Torizo variant represented by this program.</summary>
    public TorizoBellyPaletteOwner Owner { get; }

    /// <summary>The palette-FX object that installs this program.</summary>
    public PaletteFxObject Object { get; }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public PaletteFxColorRows Rows { get; }

    /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
    public PaletteFxColorKey Color(int frame, int color) => Rows.Color(frame, color);
}
