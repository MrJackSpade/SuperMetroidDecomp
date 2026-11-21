using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// The room effect's overlay blended over gameplay (#1468, #1706): a liquid below its surface or an atmosphere's repeating
/// picture (<see cref="OverlayPicture"/>), its horizontal scroll described line by line, as water's wave moves it sideways
/// below the surface. The backend draws it.
/// </summary>
public sealed record OverlayBlendLayer : DisplayLayer
{
    /// <summary>The layer's horizontal scroll line by line, added to the screen position before each picture repeats across.</summary>
    public LineScroll Horizontal { get; }
    /// <summary>What the overlay shows (#1514, #1706): the room effect's liquid or atmosphere, or null where it shows nothing.</summary>
    public OverlayPicture? Picture { get; }
    /// <summary>
    /// First participating screen line, inclusive, a real screen line: the overlay covers the lines from it to the screen's
    /// bottom, so a first line at the screen's height, 224, covers none of them.
    /// </summary>
    public int FirstLine { get; }
    /// <summary>
    /// Whether the overlay brightens the screen under it, adding its colours as lava, acid, fog and rain do, or darkens it,
    /// subtracting them as water does. Where a gameplay blend combines with the overlay lying behind what it blends
    /// (<see cref="GameplayBlendLayer.OverlayBehind"/>), that blend's operation applies instead.
    /// </summary>
    public BlendOperation Operation { get; }

    /// <summary>Captures an overlay to blend with its horizontal line scroll, checking the first line and operation.</summary>
    /// <param name="picture">What the overlay shows, or null where it shows nothing.</param>
    /// <param name="firstLine">Inclusive first screen line, within the screen or at its height; at the screen's height no visible lines participate.</param>
    /// <param name="operation">Whether the overlay brightens or darkens the screen under it.</param>
    /// <param name="horizontal">The layer's horizontal scroll line by line.</param>
    public OverlayBlendLayer(OverlayPicture? picture, int firstLine, BlendOperation operation, LineScroll horizontal)
    {
        if (firstLine is < 0 or > ScreenGeometry.ScreenHeightPixels)
            throw new ArgumentOutOfRangeException(nameof(firstLine));
        Picture = picture;
        FirstLine = firstLine;
        Operation = operation;
        Horizontal = Ensure.NotNull(horizontal);
    }
}
