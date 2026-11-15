using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// A frontend screen's sprite entries (#1358, #1392): each placed pre-coloured frame becomes a whole-frame draw, in
/// emission order, frontmost first. Screens publish these draws instead of writing sprite records. Every frame is published
/// where the screen places it, at a signed place on the screen (#2000), and the display clips whatever lies off the screen
/// (#1587). A frame is always drawn whole, as its one pre-assembled image; a crowded screen never cuts a frame short (#1513).
/// The placed frames are the screen's state, and the draws are made from them.
/// </summary>
public sealed class ScreenSprites
{
    /// <summary>The frames placed since <see cref="BeginFrame"/>, in emission order.</summary>
    private readonly List<PlacedPicture> placed = [];

    /// <summary>The frames of the most recently finished frame, in emission order.</summary>
    private PlacedPicture[] finished = [];

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
    /// Places a whole pre-coloured frame at <paramref name="place"/> (#1392); an empty frame places nothing. The frame is
    /// published wherever its origin lies, on the screen or off it, and the display clips it (#1587).
    /// </summary>
    /// <param name="frame">The frame placed.</param>
    /// <param name="place">
    /// Where the frame's origin lands, in pixels from the screen's top-left corner (#2000): left of or above the screen
    /// is negative.
    /// </param>
    /// <param name="shading">
    /// The shades and colour effects the screen shows the frame in (#1512); <see cref="PictureShading.Standard"/> where
    /// the screen shows no effect on it.
    /// </param>
    public void AddImage(SpriteSheetFrame frame, Offset16<ScreenPixels> place, PictureShading shading)
    {
        Ensure.NotNull(frame);
        Ensure.NotNull(shading);
        if (frame.IsEmpty)
            return;
        placed.Add(new PlacedPicture(place, frame, shading));
    }

    /// <summary>The finished frame's placed frames as draws.</summary>
    private SpriteDraw[] FinishedDraws()
    {
        var result = new SpriteDraw[finished.Length];
        for (int index = 0; index < finished.Length; index++)
            result[index] = Draw(finished[index]);
        return result;
    }

    /// <summary>The draw of <paramref name="placed"/>'s frame at its place, the draw's screen origin.</summary>
    private static ImageSpriteDraw Draw(PlacedPicture placed) =>
        new(new SpriteSource(SpriteSourceKind.FrontendScreen), placed.Image, placed.Place, SpriteMirror.None)
            { Shading = placed.Shading };

    /// <summary>One placed frame at its place, with the shading the screen shows it in (#1512).</summary>
    private readonly record struct PlacedPicture(Offset16<ScreenPixels> Place, SpriteSheetFrame Image, PictureShading Shading);
}
