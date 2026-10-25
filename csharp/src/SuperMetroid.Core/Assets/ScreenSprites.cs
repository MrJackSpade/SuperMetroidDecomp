using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// A frontend screen's sprite entries (#1358, #1392): each placed pre-coloured frame becomes a whole-frame draw, in
/// emission order, frontmost first. Screens publish these draws instead of writing OAM. Frames land as the cartridge
/// places their objects: nine-bit X, eight-bit Y read by the spritemap loader the origin selects, and the 128-object
/// limit. The placed frames are the screen's state, and the draws are made from them.
/// </summary>
public sealed class ScreenSprites
{
    /// <summary>The most objects a frame holds.</summary>
    public const int Capacity = 128;

    /// <summary>The values of an eight-bit coordinate.</summary>
    private const int ByteRange = 256;

    /// <summary>The values of the nine-bit X.</summary>
    private const int XRange = 512;

    /// <summary>The frames placed since <see cref="BeginFrame"/>, by the index of their first object.</summary>
    private readonly ScreenObject[] placed = new ScreenObject[Capacity];

    /// <summary>The frames of the most recently finished frame, by the index of their first object.</summary>
    private readonly ScreenObject[] finished = new ScreenObject[Capacity];

    /// <summary>The index the next object takes.</summary>
    private int next;

    /// <summary>The number of objects the most recently finished frame placed.</summary>
    private int finishedCount;

    /// <summary>The draws of the finished frame, until the frame changes.</summary>
    [NonSerialized]
    private SpriteDraw[]? draws;

    /// <summary>The draws of the most recently finished frame, frontmost first.</summary>
    public ImmutableArray<SpriteDraw> Draws => ImmutableCollectionsMarshal.AsImmutableArray(draws ??= FinishedDraws());

    /// <summary>Starts a frame: clears the placed objects.</summary>
    public void BeginFrame() => next = 0;

    /// <summary>Finishes the frame: its placed objects become the published draws.</summary>
    public void FinishFrame()
    {
        Array.Copy(placed, finished, next);
        finishedCount = next;
        draws = null;
    }

    /// <summary>
    /// Places a whole pre-coloured frame at <paramref name="origin"/> (#1392), standing for the objects of its
    /// composition; a frame of no parts places nothing. The loader's vertical-wrap hiding hides only parts an image
    /// clips, so the frame needs none. A frame the 128-object limit cuts short places its first parts as their own
    /// pre-coloured images, as the cartridge's loader keeps the parts that fit and drops the rest.
    /// </summary>
    /// <param name="frame">The frame placed.</param>
    /// <param name="origin">Where the frame's origin lands.</param>
    /// <param name="paletteOverride">The row the draw places the frame's palette-inheriting parts in, or null for the row they were coloured in.</param>
    /// <param name="side">
    /// Which spritemap loader the cartridge draws the frame with: on screen, the origin's low Y byte lies on or below the
    /// screen; off screen, it lies above it.
    /// </param>
    public void AddImage(SpriteSheetFrame frame, Position16<SpritePixels> origin, int? paletteOverride = null,
        SpriteOrigin side = SpriteOrigin.OnScreen)
    {
        Ensure.NotNull(frame);
        if (frame.Objects == 0 || next >= Capacity)
            return;
        if (next + frame.Objects <= Capacity)
        {
            placed[next] = new ScreenObject(origin, frame, frame.Objects, paletteOverride, side);
            next += frame.Objects;
            return;
        }
        // Each part keeps the frame's origin and loader, so it lands where the whole frame would show it.
        foreach (SpriteSheetFramePart part in frame.Parts.Take(Capacity - next))
            placed[next++] = new ScreenObject(origin, part.Image, 1, paletteOverride, side);
    }

    /// <summary>
    /// Places a pre-coloured frame of one object written straight to OAM (#1392), with no spritemap loader: the object
    /// lands at <paramref name="position"/>, and a Y below the screen wraps above it as the PPU wraps it. Like a direct
    /// OAM store, the object index wraps after the last object instead of stopping, and the frame then ends at the
    /// wrapped index.
    /// </summary>
    /// <exception cref="ArgumentException">The frame is not one object at its origin.</exception>
    public void AddObjectImage(SpriteSheetFrame frame, Position16<SpritePixels> position)
    {
        Ensure.NotNull(frame);
        if (frame.Parts is not [{ Offset: var offset }] || offset != default)
            throw new ArgumentException("The frame is not one object at its origin.", nameof(frame));
        // Every object before a wrapped index is one a store has rewritten, so no earlier frame straddles it.
        int index = next % Capacity;
        // An object's Y wraps above the screen exactly where the off-screen loader reads an origin.
        placed[index] = new ScreenObject(position, frame, 1, null,
            (byte)position.Y >= SnesPpuLayout.ScreenHeightPixels ? SpriteOrigin.OffScreen : SpriteOrigin.OnScreen);
        next = index + 1;
    }

    /// <summary>The finished frame's objects as draws.</summary>
    private SpriteDraw[] FinishedDraws()
    {
        var result = new List<SpriteDraw>(finishedCount);
        // A frame stands for several objects; the next entry follows them.
        for (int index = 0; index < finishedCount; index += finished[index].Objects)
            result.Add(Draw(finished[index], index));
        return [.. result];
    }

    /// <summary>The draw of <paramref name="placedObject"/>'s frame, its X in signed screen pixels: the upper half lies left of the screen.</summary>
    private static ImageSpriteDraw Draw(ScreenObject placedObject, int index)
    {
        int nineBitX = placedObject.Position.X & (XRange - 1);
        int screenX = nineBitX >= XRange / 2 ? nineBitX - XRange : nineBitX;
        // The loaders read only the origin's low Y byte: the on-screen loader places it on or below the screen, so a
        // frame just below the bottom still shows the parts that reach up into it, and the off-screen loader above it.
        int imageY = (byte)placedObject.Position.Y - (placedObject.Side == SpriteOrigin.OffScreen ? ByteRange : 0);
        return new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.FrontendObjects, index), placedObject.Image,
            new Position16<ScreenPixels>(unchecked((ushort)screenX), unchecked((ushort)imageY)), SpriteMirror.None,
            placedObject.PaletteOverride);
    }

    /// <summary>
    /// One placed frame at its position (nine-bit X as stored in sixteen bits, eight-bit Y), standing for the objects of
    /// its parts, with the row its palette-inheriting parts take and the loader that reads its origin.
    /// </summary>
    private readonly record struct ScreenObject(Position16<SpritePixels> Position, SpriteSheetFrame Image, int Objects, int? PaletteOverride,
        SpriteOrigin Side);
}
