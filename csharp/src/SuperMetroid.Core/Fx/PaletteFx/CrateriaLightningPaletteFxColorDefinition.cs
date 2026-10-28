namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Calculated setup, two repeated flash groups and neutral intervals for one lightning owner.</summary>
public sealed class CrateriaLightningPaletteFxColorDefinition
{
    /// <summary>Builds the definition from its owner's program values.</summary>
    /// <param name="owner">The lightning program.</param>
    /// <param name="firstRecord">The first colour record its program shows.</param>
    /// <param name="colorsPerFrame">Live BGR555 words per timed record.</param>
    /// <param name="neutralCount">The 240-update neutral records before the last flash group.</param>
    internal CrateriaLightningPaletteFxColorDefinition(CrateriaLightningPaletteOwner owner, int firstRecord, int colorsPerFrame, int neutralCount)
    {
        Owner = owner;
        ColorsPerFrame = colorsPerFrame;
        NeutralCount = neutralCount;
        Rows = new(firstRecord, 12 + neutralCount, colorsPerFrame);
    }

    /// <summary>Program identity selecting either the live $F765 surface lightning or retained unused $F769 dark-lightning mechanics and color resource.</summary>
    public CrateriaLightningPaletteOwner Owner { get; }

    /// <summary>Live BGR555 words per timed record: eight targeting CGRAM colors 84..91 for surface lightning, or seven targeting colors 65..71 for dark lightning.</summary>
    public int ColorsPerFrame { get; }

    /// <summary>The dark program inserts a second 240-update neutral record before its last flash group.</summary>
    internal int NeutralCount { get; }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public PaletteFxColorRows Rows { get; }

    /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
    public PaletteFxColorKey Color(int frame, int color) => Rows.Color(frame, color);
}
