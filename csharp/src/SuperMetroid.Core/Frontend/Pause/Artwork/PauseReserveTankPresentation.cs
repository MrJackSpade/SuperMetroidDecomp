using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>
/// Editable reserve-strip origins and palette, independent of reserve energy and fill selection. The strip is drawn from
/// the installed pause sheet, whose reserve compositions belong to the import pipeline that reads the installed
/// presentation (#1463).
/// </summary>
public abstract class PauseReserveTankPresentation
{
    /// <summary>Per-coordinate deviations from the six stock reserve-strip anchors.</summary>
    internal readonly Dictionary<int, AnchorOverride> anchorOverrides;
    /// <summary>Stores independent anchor edits and the selected reserve palette.</summary>
    /// <param name="anchorOverrides">Per-coordinate deviations from the stock anchors.</param>
    /// <param name="palette">The palette row every reserve-strip part draws in.</param>
    internal PauseReserveTankPresentation(Dictionary<int, AnchorOverride> anchorOverrides, int palette)
    { this.anchorOverrides = anchorOverrides; Palette = Ensure.BetweenInclusive(0, SpritePart.MaximumPalette, palette); }

    /// <summary>Gets one reserve-strip screen anchor, falling back independently to each stock coordinate.</summary>
    /// <param name="index">Zero-based tank or trailing-cap position from 0 through 5.</param>
    /// <returns>The configured screen-pixel anchor for that strip position.</returns>
    public Position16<SpritePixels> Anchor(int index)
    {
        var basis = PauseReserveTankParts.StockAnchor(index);
        return anchorOverrides.TryGetValue(index, out var value) ? new(checked((ushort)(value.X ?? basis.X)), checked((ushort)(value.Y ?? basis.Y))) : basis;
    }
    /// <summary>The palette row every reserve-strip part draws in.</summary>
    internal int Palette { get; }

    /// <summary>Draws one full, empty, partially filled, or end-cap reserve frame of the pause sheet at a strip position.</summary>
    /// <param name="entries">The screen's sprite entries.</param>
    /// <param name="sheet">The pause sheet.</param>
    /// <param name="visual">The one of the ten reserve visuals to draw.</param>
    /// <param name="index">Zero-based tank or trailing-cap anchor index.</param>
    /// <exception cref="InvalidOperationException">The native visual identity is unknown.</exception>
    public void Draw(ScreenSprites entries, SpriteSheet sheet, PauseReserveTankVisual visual, int index)
    {
        // The strip position is checked before the visual.
        Position16<SpritePixels> anchor = Anchor(index);
        string name = PauseReserveTankParts.Frames().Where(frame => frame.Id == visual).Select(frame => frame.Name)
            .SingleOrDefault() ?? throw new InvalidOperationException($"Undefined PauseReserveTankVisual {visual}.");
        Ensure.NotNull(entries).AddImage(Ensure.NotNull(sheet).Frame(PauseReserveTankParts.SheetFrameName(name)), anchor);
    }
}
