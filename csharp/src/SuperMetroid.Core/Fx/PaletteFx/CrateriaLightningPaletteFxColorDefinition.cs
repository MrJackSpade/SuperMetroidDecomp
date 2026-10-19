namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Calculated setup, two repeated flash groups and neutral intervals for one lightning owner.</summary>
public sealed class CrateriaLightningPaletteFxColorDefinition
{
    /// <summary>Binds the selected lightning owner.</summary>
    internal CrateriaLightningPaletteFxColorDefinition(CrateriaLightningPaletteOwner owner)
    {
        Owner = owner;
    }

    /// <summary>Program identity selecting either the live $F765 surface lightning or retained unused $F769 dark-lightning mechanics and color resource.</summary>
    public CrateriaLightningPaletteOwner Owner { get; }
    /// <summary>Determines which cartridge layout, CGRAM destination, and record width apply.</summary>
    internal bool IsSurface => Owner == CrateriaLightningPaletteOwner.SurfaceLightning;
    /// <summary>Live BGR555 words per timed record: eight targeting CGRAM colors 84..91 for surface lightning, or seven targeting colors 65..71 for dark lightning.</summary>
    public int ColorsPerFrame => IsSurface ? 8 : 7;

    /// <summary>The dark program inserts a second 240-update neutral record before its last flash group.</summary>
    internal int NeutralCount => IsSurface ? 1 : 2;

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public PaletteFxColorRows Rows => new(IsSurface ? 355 : 368, 12 + NeutralCount, ColorsPerFrame);

    /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
    public PaletteFxColorKey Color(int frame, int color) => Rows.Color(frame, color);
}
