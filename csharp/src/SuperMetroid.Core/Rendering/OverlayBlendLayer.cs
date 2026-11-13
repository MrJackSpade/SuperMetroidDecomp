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
    /// <summary>First participating screen line, inclusive; lines before it are untouched, and 224 disables the overlay for the entire frame.</summary>
    public int FirstLine { get; }
    /// <summary>
    /// Whether the overlay brightens the screen under it, adding its colours as lava, acid, fog and rain do, or darkens it,
    /// subtracting them as water does. Where a gameplay blend combines with the overlay lying behind what it blends
    /// (<see cref="GameplayBlendLayer.OverlayBehind"/>), that blend's operation applies instead.
    /// </summary>
    public BlendOperation Operation { get; }

    /// <summary>Captures an overlay to blend with its horizontal line scroll, checking the first line and operation.</summary>
    /// <param name="picture">What the overlay shows, or null where it shows nothing.</param>
    /// <param name="firstLine">Inclusive first screen line from 0 through 224; 224 means no visible lines participate.</param>
    /// <param name="operation">Whether the overlay brightens or darkens the screen under it.</param>
    /// <param name="horizontal">The layer's horizontal scroll line by line.</param>
    public OverlayBlendLayer(OverlayPicture? picture, int firstLine, BlendOperation operation, LineScroll horizontal)
    {
        if (firstLine is < 0 or > ScreenGeometry.ScreenHeightPixels)
            throw new ArgumentOutOfRangeException(nameof(firstLine));
        if (operation is not (BlendOperation.Add or BlendOperation.Subtract))
            throw new ArgumentOutOfRangeException(nameof(operation));
        Picture = picture;
        FirstLine = firstLine;
        Operation = operation;
        Horizontal = Ensure.NotNull(horizontal);
    }
}
