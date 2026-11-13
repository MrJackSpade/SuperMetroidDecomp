using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>
/// The installed reserve-strip origins, independent of reserve energy and fill selection; the installation is their only
/// source (#1596). The strip is drawn from the installed pause sheet, whose reserve compositions belong to the import
/// pipeline that reads the installed presentation and never reach the game (#1463, #1616).
/// </summary>
public sealed class PauseReserveTankPresentation
{
    /// <summary>The installed anchor of each strip position, tanks then the trailing cap.</summary>
    internal readonly Position16<SpritePixels>[] anchors;

    /// <summary>Keeps the complete installed anchors.</summary>
    /// <param name="anchors">The six strip anchors, in strip order.</param>
    /// <exception cref="InvalidDataException">The strip does not hold exactly six anchors.</exception>
    internal PauseReserveTankPresentation(Position16<SpritePixels>[] anchors)
    {
        Ensure.NotNull(anchors);
        if (anchors.Length != PauseReserveTankDefinitions.AnchorCount)
            throw new InvalidDataException($"Reserve tanks require {PauseReserveTankDefinitions.AnchorCount} anchors.");
        this.anchors = [.. anchors];
    }

    /// <summary>Gets one reserve-strip screen anchor as installed.</summary>
    /// <param name="index">Zero-based tank or trailing-cap position from 0 through 5.</param>
    /// <returns>The installed screen-pixel anchor for that strip position.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The position is outside the six-entry strip.</exception>
    public Position16<SpritePixels> Anchor(int index) =>
        (uint)index < (uint)anchors.Length ? anchors[index] : throw new ArgumentOutOfRangeException(nameof(index));

    /// <summary>
    /// Draws one full, empty, partially filled, or end-cap reserve frame of the pause sheet at a strip position, in the
    /// strip's own colours (#1544).
    /// </summary>
    /// <param name="entries">The screen's sprite entries.</param>
    /// <param name="sheet">The pause sheet.</param>
    /// <param name="visual">The one of the ten reserve visuals to draw.</param>
    /// <param name="index">Zero-based tank or trailing-cap anchor index.</param>
    /// <exception cref="InvalidOperationException">The visual identity is unknown.</exception>
    public void Draw(ScreenSprites entries, SpriteSheet sheet, PauseReserveTankVisual visual, int index)
    {
        // The strip position is checked before the visual.
        Position16<SpritePixels> anchor = Anchor(index);
        string name = PauseReserveTankParts.Frames().Where(frame => frame.Id == visual).Select(frame => frame.Name)
            .SingleOrDefault() ?? throw new InvalidOperationException($"Undefined PauseReserveTankVisual {visual}.");
        Ensure.NotNull(entries).AddImage(Ensure.NotNull(sheet).Frame(PauseReserveTankParts.SheetFrameName(name)), anchor, PictureShading.Standard);
    }
}
