namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Calculated setup, two repeated flash groups and neutral intervals for one lightning owner.</summary>
public sealed class CrateriaLightningPaletteFxColorDefinition
{
    /// <summary>Only the catalog creates definitions.</summary>
    internal CrateriaLightningPaletteFxColorDefinition()
    {
    }

    /// <summary>Program identity selecting either the live $F765 surface lightning or retained unused $F769 dark-lightning mechanics and color resource.</summary>
    public required CrateriaLightningPaletteOwner Owner { get; init; }

    /// <summary>Live BGR555 words per timed record: eight for surface lightning, or seven for dark lightning.</summary>
    public required int ColorsPerFrame { get; init; }

    /// <summary>The dark program inserts a second 240-update neutral record before its last flash group.</summary>
    internal int NeutralCount { get; init; }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public required PaletteFxColorRows Rows { get; init; }

    /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
    public PaletteFxColorKey Color(int frame, int color) => Rows.Color(frame, color);
}
