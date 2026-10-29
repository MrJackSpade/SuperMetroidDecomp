using System.Buffers.Binary;

using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>The 128 base BG colors selected by one room graphics-set palette source.</summary>
public sealed class RoomStaticPalette
{
    /// <summary>Validated little-endian BGR555 colours, owned independently of the JSON authoring data.</summary>
    private readonly byte[] packedColours;

    /// <summary>Wraps the packed colours assembled by <see cref="FromColours"/> and the sets the import pipeline decoded them into.</summary>
    private RoomStaticPalette(byte[] packedColours, ShownBackgroundColourSets sets)
    {
        this.packedColours = packedColours;
        Sets = sets;
    }

    /// <summary>Packs the room's selected background colours in the order the room load transfers them.</summary>
    /// <param name="colours">The selected background colours, in the installed palette's order.</param>
    /// <param name="sets">The same colours, by the background set each fills, which the import pipeline decoded.</param>
    internal static RoomStaticPalette FromColours(ReadOnlySpan<RgbColor> colours, ShownBackgroundColourSets sets)
    {
        Ensure.NotNull(sets);
        var packed = new byte[RoomStaticPaletteFormat.ByteCount];
        for (int index = 0; index < colours.Length; index++)
            BinaryPrimitives.WriteUInt16LittleEndian(packed.AsSpan(index * RgbColor.ByteCount), colours[index].ToWord());
        return new RoomStaticPalette(packed, sets);
    }

    /// <summary>The 128 colours packed as little-endian BGR555 words, two bytes each, after discarding the unused high bit.</summary>
    public ReadOnlyMemory<byte> PackedColours => packedColours;

    /// <summary>The colours a room load gives each background set (#1464).</summary>
    public ShownBackgroundColourSets Sets { get; }
}
