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

    /// <summary>Starts with an empty frame over <paramref name="artwork"/>.</summary>
    public ScreenSprites(SpriteArtwork artwork) => Artwork = Ensure.NotNull(artwork);

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
    public void AddPart(SnesSpritemapXWord x, SnesSpritemapYByte y, SnesObjAttributeWord attributes, Position16<OamPixels> origin)
    {
        if (next >= Capacity)
            return;
        ushort placedX = unchecked((ushort)(origin.X + x.Raw));
        int placedY = (byte)origin.Y + y.Raw;
        if (Hides(placedY, y.IsNegative))
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
        var result = new SpriteDraw[finishedCount];
        for (int index = 0; index < finishedCount; index++)
            result[index] = Draw(finished[index], index);
        return result;
    }

    /// <summary>
    /// A one-part draw of <paramref name="placedObject"/>, as signed screen pixels: X's upper half lies left of the screen
    /// and Y wraps above it below the visible field.
    /// </summary>
    private SpriteDraw Draw(ScreenObject placedObject, int index)
    {
        int nineBitX = placedObject.Position.X & (XRange - 1);
        int y = placedObject.Position.Y;
        int screenX = nineBitX >= XRange / 2 ? nineBitX - XRange : nineBitX;
        int screenY = y >= SnesPpuLayout.ScreenHeightPixels ? y - ByteRange : y;
        int size = placedObject.IsLarge ? 2 * SpriteArtwork.CharacterPixels : SpriteArtwork.CharacterPixels;
        SnesObjAttributeWord attributes = placedObject.Attributes;
        var part = new SpriteFramePart(default, new Extent16<ScreenPixels>((ushort)size, (ushort)size),
            attributes.TileNumber, attributes.FlipFlags, attributes.PaletteIndex, attributes.Priority);
        return new SpriteDraw(new SpriteSource(SpriteSourceKind.FrontendObjects, index), new SpriteFrame("frontend object", [part]), Artwork,
            new Position16<ScreenPixels>(unchecked((ushort)screenX), unchecked((ushort)screenY)), default, 0, 0);
    }

    /// <summary>One placed object: its attributes, size, and position (nine-bit X as stored in sixteen bits, eight-bit Y).</summary>
    private readonly record struct ScreenObject(SnesObjAttributeWord Attributes, bool IsLarge, Position16<OamPixels> Position);
}
