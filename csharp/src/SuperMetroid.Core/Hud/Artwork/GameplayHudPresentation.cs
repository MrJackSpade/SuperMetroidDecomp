using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Hud.Artwork;

/// <summary>
/// The installed gameplay HUD's layout (#1466): where each of the HUD's images is placed for each of its values. The import
/// pipeline decodes and validates the editable HUD document into it and pictures every image, keeping the source artwork it pictures
/// them from to itself (#1616); <see cref="HudState.Shown"/> places the images the HUD's values select. Placements are screen
/// pixels across the HUD: the static top row at its top, then the band the values are drawn over.
/// </summary>
public sealed class GameplayHudPresentation
{
    /// <summary>The decoded layout.</summary>
    private readonly GameplayHudPresentationParts parts;

    /// <summary>Takes the import's decoded, validated HUD layout.</summary>
    internal GameplayHudPresentation(GameplayHudPresentationParts parts)
    {
        this.parts = Ensure.NotNull(parts);
        if (parts.EnergyTankCorners.Length != GameplayHudDefinitions.EnergyTankCount ||
            parts.AutoReserveCorners.Length != GameplayHudDefinitions.AutoReserveCellCount ||
            parts.IconAnchors.Length != GameplayHudDefinitions.ItemCount || parts.IconsShowOverTemplate.Length != GameplayHudDefinitions.ItemCount)
            throw new ArgumentException("The gameplay HUD's layout does not place each of its elements.", nameof(parts));
    }


    /// <summary>The minimap's size: five area-map squares across and three down.</summary>
    public static Extent16<MapTiles> MinimapSize => new(5, 3);

    /// <summary>The static top row, across the HUD's top.</summary>
    public static PlacedHudImage TopRow => new(HudTopRowImage.Instance, Position16<ScreenPixels>.Zero);

    /// <summary>The template, across the band the HUD's values are drawn over, below the static top row.</summary>
    public static PlacedHudImage Template => new(HudTemplateImage.Instance, new Position16<ScreenPixels>(0, GameplayHudDefinitions.TopRowHeightPixels));


    /// <summary>
    /// Item icon <paramref name="icon"/>'s place: the icon where it has been added over a template that leaves room for it,
    /// otherwise the template, in the shade the current selection gives them; null where it shows the template unshaded.
    /// </summary>
    /// <param name="icon">The item icon.</param>
    /// <param name="added">Whether the HUD has added the icon.</param>
    /// <param name="shade">The shade the current selection gives the icon's place, or null.</param>
    public PlacedHudImage? Icon(HudIcon icon, bool added, HudIconShade? shade)
    {
        int order = IconOrder(icon);
        HudIconCover cover = added && parts.IconsShowOverTemplate[order] ? HudIconCover.Icon : HudIconCover.Template;
        return cover == HudIconCover.Template && shade is null ? null
            : new PlacedHudImage(new HudIconImage(icon, cover, shade), parts.IconAnchors[order]);
    }

    /// <summary>One energy tank full or empty.</summary>
    /// <param name="tank">The tank, 0 through 13.</param>
    /// <param name="shown">Whether the tank is full or empty.</param>
    public PlacedHudImage EnergyTank(int tank, HudEnergyTank shown) =>
        new(new HudEnergyTankImage(shown), parts.EnergyTankCorners[Ensure.BetweenInclusive(0, GameplayHudDefinitions.EnergyTankCount - 1, tank)]);

    /// <summary>The two-digit energy count.</summary>
    /// <param name="remainder">The energy beyond full tanks, 0 through 99.</param>
    public IEnumerable<PlacedHudImage> EnergyCount(int remainder) =>
        Digits(remainder, parts.HealthAnchor, 2).Select(digit => new PlacedHudImage(new HudEnergyDigitImage(digit.Numeral), digit.Corner));

    /// <summary>The ammunition count <paramref name="counter"/> shows.</summary>
    /// <param name="counter">The ammunition counter.</param>
    /// <param name="value">The ammunition count to display.</param>
    public IEnumerable<PlacedHudImage> Ammo(HudAmmoCounter counter, int value)
    {
        HudAmmoField field = counter switch
        {
            HudAmmoCounter.Missiles => new HudAmmoField(parts.MissileAmmoAnchor, 3),
            HudAmmoCounter.SuperMissiles => new HudAmmoField(parts.SuperMissileAmmoAnchor, 2),
            HudAmmoCounter.PowerBombs => new HudAmmoField(parts.PowerBombAmmoAnchor, 2),
            _ => throw new ArgumentOutOfRangeException(nameof(counter), counter, "Undefined ammunition counter."),
        };
        // A count the game has drained below zero shows as CountsRunBelowZero.HudShown describes.
        return Digits(CountsRunBelowZero.HudShown(value), field.Anchor, field.Digits)
            .Select(digit => new PlacedHudImage(new HudAmmoDigitImage(digit.Numeral), digit.Corner));
    }

    /// <summary>The AUTO reserve indicator's parts as <paramref name="indicator"/> shows them; a cleared indicator shows them blank.</summary>
    /// <param name="indicator">What the indicator shows.</param>
    public IEnumerable<PlacedHudImage> AutoReserve(HudReserveIndicator indicator)
    {
        if (!Enum.IsDefined(indicator))
            throw new ArgumentOutOfRangeException(nameof(indicator), indicator, "Undefined reserve indicator.");
        for (int cell = 0; cell < GameplayHudDefinitions.AutoReserveCellCount; cell++)
            yield return new PlacedHudImage(new HudReserveCellImage(indicator, cell), parts.AutoReserveCorners[cell]);
    }

    /// <summary>One minimap square: a revealed map square in its shade, or the empty square.</summary>
    /// <param name="output">The minimap square, counted in area-map squares from the minimap's upper-left square.</param>
    /// <param name="cell">What the minimap square shows.</param>
    public PlacedHudImage MinimapCell(Position16<MapTiles> output, MinimapCell cell)
    {
        if (!MinimapSize.Contains(output)) throw new ArgumentOutOfRangeException(nameof(output));
        HudImage image = Ensure.NotNull(cell) switch
        {
            MinimapHiddenCell => HudMinimapHiddenImage.Instance,
            MinimapMapCell shown => HudMinimapCellImage.Of(shown),
            _ => throw new ArgumentOutOfRangeException(nameof(cell), cell, "Undefined minimap cell."),
        };
        const int square = GameplayHudDefinitions.MinimapSquarePixels;
        return new PlacedHudImage(image, parts.MinimapAnchor + new Offset16<ScreenPixels>(output.X * square, output.Y * square));
    }

    /// <summary>Where <paramref name="icon"/> stands in <see cref="GameplayHudDefinitions.IconNames"/> order, the order the layout lists the icons in.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="icon"/> names no item icon.</exception>
    internal static int IconOrder(HudIcon icon)
    {
        int order = Array.IndexOf(GameplayHudDefinitions.Icons, icon);
        return order >= 0 ? order : throw new ArgumentOutOfRangeException(nameof(icon), icon, "Undefined item icon.");
    }

    /// <summary>Each digit of <paramref name="value"/>'s last <paramref name="count"/> decimal digits and where it is shown.</summary>
    private static IEnumerable<HudDigitPlacement> Digits(int value, Position16<ScreenPixels> anchor, int count)
    {
        int divisor = count == 3 ? 100 : 10;
        for (int digit = 0; digit < count; digit++)
        {
            yield return new HudDigitPlacement((value / divisor) % GameplayHudDefinitions.Numerals,
                anchor + new Offset16<ScreenPixels>(digit * GameplayHudDefinitions.DigitWidthPixels, 0));
            divisor /= GameplayHudDefinitions.Numerals;
        }
    }
}
