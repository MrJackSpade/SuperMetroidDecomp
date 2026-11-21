using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>
/// The installed reserve-strip origins, independent of reserve energy and fill selection; the installation is their only
/// source (#1596). The strip is drawn from the installed pause sheet, whose reserve compositions belong to the import
/// pipeline that reads the installed presentation and never reach the game (#1463, #1616).
/// </summary>
public sealed class PauseReserveTankPresentation
{
    /// <summary>
    /// The installed anchor of each place of the strip, in strip order. A place has no role of its own: Samus's reserve tanks
    /// take the places from the first, and the trailing cap the place after her last tank, wherever that falls.
    /// </summary>
    internal readonly Offset16<ScreenPixels>[] anchors;

    /// <summary>Keeps the complete installed anchors.</summary>
    /// <param name="anchors">The six strip anchors, in strip order.</param>
    /// <exception cref="InvalidDataException">The strip does not hold exactly six anchors.</exception>
    internal PauseReserveTankPresentation(Offset16<ScreenPixels>[] anchors)
    {
        Ensure.NotNull(anchors);
        if (anchors.Length != PauseReserveTankDefinitions.AnchorCount)
            throw new InvalidDataException($"Reserve tanks require {PauseReserveTankDefinitions.AnchorCount} anchors.");
        this.anchors = [.. anchors];
    }

    /// <summary>Gets one reserve-strip screen anchor as installed.</summary>
    /// <param name="index">The place's place in the strip, from 0 through 5.</param>
    /// <returns>The installed screen-pixel anchor for that strip position.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The position is not one of the strip's six.</exception>
    internal Offset16<ScreenPixels> Anchor(int index) =>
        (uint)index < (uint)anchors.Length ? anchors[index] : throw new ArgumentOutOfRangeException(nameof(index));

    /// <summary>
    /// Draws one full, empty, partially filled, or end-cap reserve frame of the pause sheet at a strip position, in the
    /// strip's own colours (#1544).
    /// </summary>
    /// <param name="entries">The screen's sprites.</param>
    /// <param name="sheet">The pause sheet.</param>
    /// <param name="visual">The one of the ten reserve visuals to draw.</param>
    /// <param name="index">The place's place in the strip: a tank's, counted from the first, or the one after her last tank for the trailing cap.</param>
    public void Draw(ScreenSprites entries, SpriteSheet sheet, PauseReserveTankVisual visual, int index)
    {
        // The strip position is checked before the visual.
        Offset16<ScreenPixels> anchor = Anchor(index);
        string name = PauseReserveTankDefinitions.InstalledNames.Of(visual);
        Ensure.NotNull(entries).AddImage(Ensure.NotNull(sheet).Frame(PauseReserveTankParts.SheetFrameName(name)), anchor, PictureShading.Standard);
    }
}
