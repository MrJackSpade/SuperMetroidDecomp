using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable suit, suitless, and whiteout colors for Samus's death sequence.</summary>
/// <remarks>Palette choices are visual; the fatal-damage phases and frame durations remain compiled.</remarks>
public sealed class SamusDeathPaletteArtworkCatalog
{
    public const int SuitCount = 3;
    public const int ColorCount = SamusPaletteRomData.Common.ColorsPerObjPalette;

    private readonly ushort[][][] suited;
    private readonly ushort[][] suitless;
    private readonly ushort[] whiteout;
    private readonly ushort[] explosionPaletteIndices;

    public SamusDeathPaletteArtworkCatalog(ushort[][][] suited, ushort[][] suitless,
        ushort[] whiteout, ushort[] explosionPaletteIndices)
    {
        ArgumentNullException.ThrowIfNull(suited);
        ArgumentNullException.ThrowIfNull(suitless);
        ArgumentNullException.ThrowIfNull(whiteout);
        ArgumentNullException.ThrowIfNull(explosionPaletteIndices);
        if (suited.Length != SuitCount ||
            suited.Any(family => family is null ||
                family.Length != SamusPaletteRomData.Death.PaletteCount ||
                family.Any(row => row is null || row.Length != ColorCount ||
                    row.Any(color => color > 0x7fff))) ||
            suitless.Length != SamusPaletteRomData.Death.PaletteCount ||
            suitless.Any(row => row is null || row.Length != ColorCount ||
                row.Any(color => color > 0x7fff)) ||
            whiteout.Length != SamusPaletteRomData.Death.WhiteoutShadeCount ||
            whiteout.Any(color => color > 0x7fff) ||
            explosionPaletteIndices.Length != SamusDeathExplosionTimingDefinitions.RecordCount ||
            explosionPaletteIndices.Any(index => index >= SamusPaletteRomData.Death.PaletteCount))
            throw new InvalidDataException("Samus death-palette artwork has an invalid shape or RGB5 color.");
        this.suited = suited.Select(family => family.Select(row => (ushort[])row.Clone()).ToArray()).ToArray();
        this.suitless = suitless.Select(row => (ushort[])row.Clone()).ToArray();
        this.whiteout = (ushort[])whiteout.Clone();
        this.explosionPaletteIndices = (ushort[])explosionPaletteIndices.Clone();
    }

    public ushort SuitedColor(int suit, int palette, int color) => suited[suit][palette][color];
    public ushort SuitlessColor(int palette, int color) => suitless[palette][color];
    public ushort WhiteoutColor(int index) => whiteout[index];
    public ushort ExplosionPaletteIndex(int frame) => explosionPaletteIndices[frame];
}
