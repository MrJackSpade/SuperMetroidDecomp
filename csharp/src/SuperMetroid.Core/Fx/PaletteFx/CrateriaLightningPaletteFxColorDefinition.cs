namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Calculated setup, two repeated flash groups and neutral intervals for one lightning owner.</summary>
/// <param name="owner">The lightning program.</param>
/// <param name="firstRecord">The first colour record its program shows.</param>
/// <param name="colorsPerFrame">Live BGR555 words per timed record.</param>
/// <param name="neutralCount">The 240-update neutral records before the last flash group.</param>
public sealed class CrateriaLightningPaletteFxColorDefinition(
    CrateriaLightningPaletteOwner owner, int firstRecord, int colorsPerFrame, int neutralCount)
{
    /// <summary>Program identity selecting either the live $F765 surface lightning or retained unused $F769 dark-lightning mechanics and color resource.</summary>
    public CrateriaLightningPaletteOwner Owner { get; } = owner;

    /// <summary>Live BGR555 words per timed record: eight targeting CGRAM colors 84..91 for surface lightning, or seven targeting colors 65..71 for dark lightning.</summary>
    public int ColorsPerFrame { get; } = colorsPerFrame;

    /// <summary>The dark program inserts a second 240-update neutral record before its last flash group.</summary>
    internal int NeutralCount { get; } = neutralCount;

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public PaletteFxColorRows Rows { get; } = new(firstRecord, 12 + neutralCount, colorsPerFrame);

    /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
    public PaletteFxColorKey Color(int frame, int color) => Rows.Color(frame, color);
}
