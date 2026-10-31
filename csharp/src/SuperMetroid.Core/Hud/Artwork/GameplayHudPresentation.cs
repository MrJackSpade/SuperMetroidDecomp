using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Hud.Artwork;

/// <summary>
/// The installed gameplay HUD's layout (#1466): where each of the HUD's images is placed for each of its values. The import
/// pipeline decodes and validates the editable HUD document into it and pictures every image; <see cref="HudState.Shown"/>
/// places the images the HUD's values select. Placements are on the HUD's four rows: the static top row, then the three
/// rows the values are drawn over.
/// </summary>
public class GameplayHudPresentation
{
    /// <summary>The decoded layout.</summary>
    private readonly GameplayHudPresentationParts parts;

    /// <summary>Takes the import's decoded, validated HUD layout.</summary>
    internal GameplayHudPresentation(GameplayHudPresentationParts parts)
    {
        this.parts = Ensure.NotNull(parts);
        if (parts.EnergyTankCells.Length != GameplayHudDefinitions.EnergyTankCount ||
            parts.AutoReserveCells.Length != GameplayHudDefinitions.AutoReserveCellCount ||
            parts.IconAnchors.Length != GameplayHudDefinitions.ItemCount || parts.IconsShowOverTemplate.Length != GameplayHudDefinitions.ItemCount)
            throw new ArgumentException("The gameplay HUD's layout does not place each of its elements.", nameof(parts));
    }

    /// <summary>Gets the SHA-256 identity of the source presentation document.</summary>
    public string ContentIdentity => parts.ContentIdentity;

    /// <summary>The five-by-three minimap's size in HUD cells.</summary>
    public static Extent16<TilemapCells> MinimapSize => new(5, 3);

    /// <summary>The static top row, across the HUD's first row.</summary>
    public static PlacedHudImage TopRow => new(HudTopRowImage.Instance, Position16<TilemapCells>.Zero);

    /// <summary>The template, across the three rows the HUD's values are drawn over.</summary>
    public static PlacedHudImage Template => new(HudTemplateImage.Instance, Drawn(Position16<TilemapCells>.Zero));

    /// <summary>
    /// Whether item <paramref name="itemIndex"/>'s icon draws when the HUD adds it: the cartridge draws an icon only over
    /// a blank anchor, and nothing but the template ever sits under an icon.
    /// </summary>
    /// <param name="itemIndex">The zero-based item icon index.</param>
    public bool IconShowsOverTemplate(int itemIndex) => parts.IconsShowOverTemplate[ItemIndex(itemIndex)];

    /// <summary>
    /// Item <paramref name="itemIndex"/>'s cells: the icon where it has been added over a template that leaves room for it,
    /// otherwise the template, in the shade the selection last gave them; null where they show the template unshaded.
    /// </summary>
    /// <param name="itemIndex">The zero-based item icon index.</param>
    /// <param name="added">Whether the HUD has added the icon.</param>
    /// <param name="shade">The shade the selection last gave the cells, or null.</param>
    public PlacedHudImage? Icon(int itemIndex, bool added, HudIconShade? shade)
    {
        int item = ItemIndex(itemIndex);
        HudIconCover cover = added && parts.IconsShowOverTemplate[item] ? HudIconCover.Icon : HudIconCover.Template;
        return cover == HudIconCover.Template && shade is null ? null
            : new PlacedHudImage(new HudIconImage(item, cover, shade), Drawn(parts.IconAnchors[item]));
    }

    /// <summary>One energy tank full or empty.</summary>
    /// <param name="tank">The tank, 0 through 13.</param>
    /// <param name="shown">Whether the tank is full or empty.</param>
    public PlacedHudImage EnergyTank(int tank, HudEnergyTank shown) =>
        new(new HudEnergyTankImage(shown), Drawn(parts.EnergyTankCells[Ensure.BetweenInclusive(0, GameplayHudDefinitions.EnergyTankCount - 1, tank)]));

    /// <summary>The two-digit energy count.</summary>
    /// <param name="remainder">The energy beyond full tanks, 0 through 99.</param>
    public IEnumerable<PlacedHudImage> EnergyCount(int remainder) =>
        Digits(remainder, parts.HealthAnchor, 2).Select(digit => new PlacedHudImage(new HudEnergyDigitImage(digit.Numeral), digit.Cell));

    /// <summary>The ammunition count for a projectile item.</summary>
    /// <param name="itemIndex">Zero for missiles, one for Super Missiles, or two for Power Bombs.</param>
    /// <param name="value">The ammunition count to display.</param>
    public IEnumerable<PlacedHudImage> Ammo(int itemIndex, int value)
    {
        (Position16<TilemapCells> anchor, int digits) = itemIndex switch
        {
            0 => new HudAmmoField(parts.MissileAmmoAnchor, 3),
            1 => new HudAmmoField(parts.SuperMissileAmmoAnchor, 2),
            2 => new HudAmmoField(parts.PowerBombAmmoAnchor, 2),
            _ => throw new ArgumentOutOfRangeException(nameof(itemIndex)),
        };
        // The digits divide the stored word, so a count the cartridge has drained below zero draws as its word.
        return Digits(SamusQuantity.Encode(value), anchor, digits).Select(digit => new PlacedHudImage(new HudAmmoDigitImage(digit.Numeral), digit.Cell));
    }

    /// <summary>The AUTO reserve indicator's cells as <paramref name="indicator"/> shows them; a cleared indicator shows blank cells.</summary>
    /// <param name="indicator">What the indicator shows.</param>
    public IEnumerable<PlacedHudImage> AutoReserve(HudReserveIndicator indicator)
    {
        if (!Enum.IsDefined(indicator))
            throw new ArgumentOutOfRangeException(nameof(indicator), indicator, "Undefined reserve indicator.");
        for (int cell = 0; cell < GameplayHudDefinitions.AutoReserveCellCount; cell++)
            yield return new PlacedHudImage(new HudReserveCellImage(indicator, cell), Drawn(parts.AutoReserveCells[cell]));
    }

    /// <summary>One minimap cell: a revealed map cell's shape and mirroring in its shade, or the empty cell.</summary>
    /// <param name="output">The zero-based minimap column and row.</param>
    /// <param name="cell">What the minimap cell shows.</param>
    public PlacedHudImage MinimapCell(Position16<TilemapCells> output, MinimapCell cell)
    {
        if (!MinimapSize.Contains(output)) throw new ArgumentOutOfRangeException(nameof(output));
        HudImage image = Ensure.NotNull(cell) switch
        {
            MinimapHiddenCell => HudMinimapHiddenImage.Instance,
            MinimapMapCell shown => HudMinimapCellImage.Of(shown),
            _ => throw new ArgumentOutOfRangeException(nameof(cell), cell, "Undefined minimap cell."),
        };
        return new PlacedHudImage(image, Drawn(parts.MinimapAnchor + (output - Position16<TilemapCells>.Zero)));
    }

    /// <summary>Each digit of <paramref name="value"/>'s last <paramref name="count"/> decimal digits and the cell it sits in.</summary>
    private static IEnumerable<HudDigitPlacement> Digits(int value, Position16<TilemapCells> anchor, int count)
    {
        int divisor = count == 3 ? 100 : 10;
        for (int digit = 0; digit < count; digit++)
        {
            yield return new HudDigitPlacement((value / divisor) % GameplayHudDefinitions.Numerals,
                Drawn(anchor + new Offset16<TilemapCells>(digit, 0)));
            divisor /= GameplayHudDefinitions.Numerals;
        }
    }

    /// <summary>A cell of the three drawn rows, placed below the static top row.</summary>
    private static Position16<TilemapCells> Drawn(Position16<TilemapCells> cell) => cell + new Offset16<TilemapCells>(0, 1);

    /// <summary>Requires a zero-based item icon index.</summary>
    private static int ItemIndex(int itemIndex) => (uint)itemIndex < GameplayHudDefinitions.ItemCount
        ? itemIndex
        : throw new ArgumentOutOfRangeException(nameof(itemIndex));
}
