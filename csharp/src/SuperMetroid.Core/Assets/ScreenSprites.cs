using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// A frontend screen's sprite entries (#1358): each placed part becomes a one-part whole-frame draw over the screen's
/// owned OBJ artwork, in emission order, frontmost first. Screens publish these draws instead of writing OAM. Parts are
/// placed as the cartridge's spritemap loaders place them (<c>$81:879F</c>): nine-bit X, eight-bit Y, the vertical-wrap
/// hiding that parks a part off screen, and the 128-object limit. The placed objects are the screen's state; the OBJ
/// page is host-bound artwork, rebound after debugger restoration, and the draws are made from both.
/// </summary>
public sealed class ScreenSprites
{
    /// <summary>The most objects a frame holds.</summary>
    public const int Capacity = 128;

    /// <summary>The X a hidden part is parked at, beyond the visible screen.</summary>
    private const ushort ParkedX = 384;

    /// <summary>The values of an eight-bit coordinate.</summary>
    private const int ByteRange = 256;

    /// <summary>The values of the nine-bit X.</summary>
    private const int XRange = 512;

    /// <summary>The objects placed since <see cref="BeginFrame"/>, by object index.</summary>
    private readonly ScreenObject[] placed = new ScreenObject[Capacity];

    /// <summary>The objects of the most recently finished frame, by object index.</summary>
    private readonly ScreenObject[] finished = new ScreenObject[Capacity];

    /// <summary>The index the next object takes.</summary>
    private int next;

    /// <summary>The number of objects the most recently finished frame placed.</summary>
    private int finishedCount;

    /// <summary>The draws of the finished frame over the bound page, until the frame or the page changes.</summary>
    [NonSerialized]
    private SpriteDraw[]? draws;

    /// <summary>Starts with an empty frame over <paramref name="artwork"/>, whose objects draw at <paramref name="sizes"/>.</summary>
    public ScreenSprites(SpriteArtwork artwork, ScreenObjectSizes sizes)
    {
        Artwork = Ensure.NotNull(artwork);
        Sizes = sizes;
    }

    /// <summary>The sizes a small and a large object draw at, from the screen's OBJ size selection.</summary>
    public ScreenObjectSizes Sizes { get; }

    /// <summary>The OBJ character page the screen's parts name.</summary>
    [field: NonSerialized]
    [AllowNull]
    public SpriteArtwork Artwork
    {
        get => field ?? throw new InvalidOperationException("The screen's OBJ page is rebound after debugger restoration.");
        private set;
    }

    /// <summary>The draws of the most recently finished frame, frontmost first.</summary>
    public ImmutableArray<SpriteDraw> Draws => ImmutableCollectionsMarshal.AsImmutableArray(draws ??= FinishedDraws());

    /// <summary>Rebinds the screen's OBJ page after its installed artwork changes.</summary>
    public void BindArtwork(SpriteArtwork page)
    {
        Artwork = Ensure.NotNull(page);
        draws = null;
    }

    /// <summary>Starts a frame: clears the placed objects.</summary>
    public void BeginFrame() => next = 0;

    /// <summary>Finishes the frame: its placed objects become the published draws.</summary>
    public void FinishFrame()
    {
        Array.Copy(placed, finished, next);
        finishedCount = next;
        draws = null;
    }

    /// <summary>Places one compiled spritemap part relative to an on-screen <paramref name="origin"/>.</summary>
    public void AddPart(SnesSpritemapXWord x, SnesSpritemapYByte y, SnesObjAttributeWord attributes, Position16<OamPixels> origin) =>
        AddPart(x, y, attributes, origin, originSide: SpriteOrigin.OnScreen);

    /// <summary>Places one compiled spritemap part relative to a negative <paramref name="origin"/> ($81:8853's reversed wrap).</summary>
    public void AddOffScreenPart(SnesSpritemapXWord x, SnesSpritemapYByte y, SnesObjAttributeWord attributes, Position16<OamPixels> origin) =>
        AddPart(x, y, attributes, origin, originSide: SpriteOrigin.OffScreen);

    /// <summary>
    /// Places one small object whose attributes name its character, flips, palette and priority. Like a direct OAM
    /// store, the object index wraps after the last object instead of stopping.
    /// </summary>
    public void AddSmallObject(Position16<OamPixels> position, SnesObjAttributeWord attributes)
    {
        placed[next] = new ScreenObject(attributes, IsLarge: false, position);
        next = (next + 1) % Capacity;
    }

    /// <summary>
    /// Places a whole pre-coloured frame at <paramref name="origin"/> (#1392), standing for the objects of its
    /// composition; a frame of no parts places nothing. The loader's vertical-wrap hiding hides only parts an image
    /// clips, so the frame needs none; a frame the 128-object limit would cut short cannot show part of itself and
    /// fails instead.
    /// </summary>
    /// <param name="frame">The frame placed.</param>
    /// <param name="origin">Where the frame's origin lands.</param>
    /// <param name="paletteOverride">The row the draw places the frame's palette-inheriting parts in, or null for the row they were coloured in.</param>
    /// <exception cref="InvalidOperationException">The frame's parts do not fit in the objects left.</exception>
    public void AddImage(SpriteSheetFrame frame, Position16<OamPixels> origin, int? paletteOverride = null)
    {
        Ensure.NotNull(frame);
        if (frame.Objects == 0)
            return;
        if (next + frame.Objects > Capacity)
            throw new InvalidOperationException($"A {frame.Objects}-object frame at object {next} exceeds the screen's {Capacity} objects.");
        placed[next] = new ScreenObject(default, IsLarge: false, origin, frame, frame.Objects, paletteOverride);
        next += frame.Objects;
    }

    /// <summary>Applies the spritemap loader's vertical-wrap hiding and places the part, until the frame is full.</summary>
    private void AddPart(SnesSpritemapXWord x, SnesSpritemapYByte y, SnesObjAttributeWord attributes, Position16<OamPixels> origin,
        SpriteOrigin originSide)
    {
        if (next >= Capacity)
            return;
        ushort placedX = unchecked((ushort)(origin.X + x.Raw));
        int placedY = (byte)origin.Y + y.Raw;
        // The off-screen branch inverts the on-screen test: a part shows exactly when the on-screen loader would hide it.
        if (Hides(placedY, y.IsNegative) == (originSide == SpriteOrigin.OnScreen))
        {
            placedX = ParkedX;
            placedY = SnesPpuLayout.ScreenHeightPixels;
        }
        placed[next++] = new ScreenObject(attributes, x.IsLarge, new Position16<OamPixels>(placedX, (byte)placedY));
    }

    /// <summary>The native carry and sign test hiding a vertically wrapped part.</summary>
    private static bool Hides(int unsignedYSum, bool offsetIsNegative)
    {
        byte wrappedY = (byte)unsignedYSum;
        if (!offsetIsNegative)
            return unsignedYSum >= ByteRange || wrappedY >= SnesPpuLayout.ScreenHeightPixels;
        // $81:8800-$81:881C: negative offsets may wrap into $E0-$FF to appear partly above the screen.
        return (unsignedYSum & ByteRange) != 0 ? wrappedY >= SnesPpuLayout.ScreenHeightPixels : wrappedY < SnesPpuLayout.ScreenHeightPixels;
    }

    /// <summary>The finished frame's objects as draws over the bound page.</summary>
    private SpriteDraw[] FinishedDraws()
    {
        var result = new List<SpriteDraw>(finishedCount);
        // A frame stands for several objects; the next entry follows them.
        for (int index = 0; index < finishedCount; index += finished[index].Objects)
            result.Add(Draw(finished[index], index));
        return [.. result];
    }

    /// <summary>
    /// A one-part draw of <paramref name="placedObject"/>, or its whole frame, as signed screen pixels: X's upper half lies left of the screen
    /// and Y wraps above it below the visible field.
    /// </summary>
    private SpriteDraw Draw(ScreenObject placedObject, int index)
    {
        int nineBitX = placedObject.Position.X & (XRange - 1);
        int y = placedObject.Position.Y;
        int screenX = nineBitX >= XRange / 2 ? nineBitX - XRange : nineBitX;
        int screenY = y >= SnesPpuLayout.ScreenHeightPixels ? y - ByteRange : y;
        if (placedObject.Image is { } image)
        {
            return new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.FrontendObjects, index), image,
                new Position16<ScreenPixels>(unchecked((ushort)screenX), unchecked((ushort)screenY)), SpriteMirror.None,
                placedObject.PaletteOverride);
        }
        Extent16<ScreenPixels> size = placedObject.IsLarge ? Sizes.Large : Sizes.Small;
        SnesObjAttributeWord attributes = placedObject.Attributes;
        var part = new SpriteFramePart(default, size,
            attributes.TileNumber, attributes.FlipFlags, attributes.PaletteIndex, attributes.Priority);
        return new CharacterSpriteDraw(new SpriteSource(SpriteSourceKind.FrontendObjects, index), new SpriteFrame("frontend object", [part]), Artwork,
            new Position16<ScreenPixels>(unchecked((ushort)screenX), unchecked((ushort)screenY)), default, 0, 0);
    }

    /// <summary>
    /// One placed object: its attributes, size, and position (nine-bit X as stored in sixteen bits, eight-bit Y); or a
    /// whole frame placed at that position, standing for the objects of its parts, with the row its palette-inheriting
    /// parts take.
    /// </summary>
    private readonly record struct ScreenObject(SnesObjAttributeWord Attributes, bool IsLarge, Position16<OamPixels> Position,
        SpriteSheetFrame? Image = null, int Objects = 1, int? PaletteOverride = null);
}
