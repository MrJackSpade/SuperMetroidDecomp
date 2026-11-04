using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// A frontend screen's sprite entries (#1358, #1392): each placed pre-coloured frame becomes a whole-frame draw, in
/// emission order, frontmost first. Screens publish these draws instead of writing sprite records. Frames land where the
/// game places them: nine-bit X and eight-bit Y read by the spritemap loader the origin selects. A frame is always drawn
/// whole, as its one pre-assembled image; a crowded screen never cuts a frame short (#1513). The placed frames are the screen's state, and the draws are made from them.
/// </summary>
public sealed class ScreenSprites
{
    /// <summary>The values of an eight-bit coordinate.</summary>
    private const int ByteRange = 256;

    /// <summary>The values of the nine-bit X.</summary>
    private const int XRange = 512;

    /// <summary>The frames placed since <see cref="BeginFrame"/>, in emission order.</summary>
    private readonly List<ScreenObject> placed = [];

    /// <summary>The frames of the most recently finished frame, in emission order.</summary>
    private ScreenObject[] finished = [];

    /// <summary>The draws of the finished frame, until the frame changes.</summary>
    private SpriteDraw[]? draws;

    /// <summary>The draws of the most recently finished frame, frontmost first.</summary>
    public ImmutableArray<SpriteDraw> Draws => ImmutableCollectionsMarshal.AsImmutableArray(draws ??= FinishedDraws());

    /// <summary>Starts a frame: clears the placed frames.</summary>
    public void BeginFrame() => placed.Clear();

    /// <summary>Finishes the frame: its placed frames become the published draws.</summary>
    public void FinishFrame()
    {
        finished = [.. placed];
        draws = null;
    }

    /// <summary>
    /// Places a whole pre-coloured frame at <paramref name="origin"/> (#1392); an empty frame places nothing. The loader's
    /// vertical-wrap hiding hides only parts an image clips, so the frame needs none.
    /// </summary>
    /// <param name="frame">The frame placed.</param>
    /// <param name="origin">Where the frame's origin lands.</param>
    /// <param name="shading">
    /// The shades and colour effects the screen shows the frame in (#1512); <see cref="PictureShading.Standard"/> where
    /// the screen shows no effect on it.
    /// </param>
    /// <param name="side">
    /// Which spritemap loader draws the frame: on screen, the origin's low Y byte lies on or below the
    /// screen; off screen, it lies above it.
    /// </param>
    public void AddImage(SpriteSheetFrame frame, Position16<SpritePixels> origin, PictureShading shading,
        SpriteOrigin side = SpriteOrigin.OnScreen)
    {
        Ensure.NotNull(frame);
        Ensure.NotNull(shading);
        if (frame.IsEmpty)
            return;
        placed.Add(new ScreenObject(origin, frame, side, shading));
    }

    /// <summary>
    /// Places a pre-coloured frame written straight to the sprite list (#1392), with no spritemap loader: the frame lands
    /// at <paramref name="position"/>, and a Y below the screen wraps above it as the screen wraps it.
    /// </summary>
    /// <param name="frame">The frame.</param>
    /// <param name="position">Where the frame lands.</param>
    /// <param name="shading">
    /// The shades and colour effects the screen shows the frame in (#1512); <see cref="PictureShading.Standard"/> where
    /// the screen shows no effect on it.
    /// </param>
    public void AddObjectImage(SpriteSheetFrame frame, Position16<SpritePixels> position, PictureShading shading)
    {
        Ensure.NotNull(frame);
        Ensure.NotNull(shading);
        if (frame.IsEmpty)
            return;
        // An object's Y wraps above the screen exactly where the off-screen loader reads an origin.
        placed.Add(new ScreenObject(position, frame,
            WordWrap.LowByte(position.Y) >= ScreenGeometry.ScreenHeightPixels ? SpriteOrigin.OffScreen : SpriteOrigin.OnScreen, shading));
    }

    /// <summary>The finished frame's placed frames as draws.</summary>
    private SpriteDraw[] FinishedDraws()
    {
        var result = new SpriteDraw[finished.Length];
        for (int index = 0; index < finished.Length; index++)
            result[index] = Draw(finished[index], index);
        return result;
    }

    /// <summary>The draw of <paramref name="placedObject"/>'s frame, its X in signed screen pixels: the upper half lies left of the screen.</summary>
    private static ImageSpriteDraw Draw(ScreenObject placedObject, int index)
    {
        int nineBitX = placedObject.Position.X & (XRange - 1);
        int screenX = nineBitX >= XRange / 2 ? nineBitX - XRange : nineBitX;
        // The loaders read only the origin's low Y byte: the on-screen loader places it on or below the screen, so a
        // frame just below the bottom still shows the parts that reach up into it, and the off-screen loader above it.
        int imageY = WordWrap.LowByte(placedObject.Position.Y) - (placedObject.Side == SpriteOrigin.OffScreen ? ByteRange : 0);
        return new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.FrontendObjects, index), placedObject.Image,
            new Position16<ScreenPixels>(CoordinateAxis.Wrap(screenX), CoordinateAxis.Wrap(imageY)), SpriteMirror.None)
            { Shading = placedObject.Shading };
    }

    /// <summary>
    /// One placed frame at its position (nine-bit X as stored in sixteen bits, eight-bit Y), with the loader that reads its
    /// origin and the shading the screen shows it in (#1512).
    /// </summary>
    private readonly record struct ScreenObject(Position16<SpritePixels> Position, SpriteSheetFrame Image, SpriteOrigin Side, PictureShading Shading);
}
