using System.Buffers.Binary;

using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>The 128 base BG colors selected by one room graphics-set palette source.</summary>
public sealed class RoomStaticPalette
{
    /// <summary>Validated little-endian BGR555 colours, owned independently of the JSON authoring data.</summary>
    private readonly byte[] packedColours;

    /// <summary>Wraps the packed colours assembled by <see cref="FromColours"/>.</summary>
    private RoomStaticPalette(byte[] packedColours) => this.packedColours = packedColours;

    /// <summary>Packs the room's selected background colours in the order the room load transfers them.</summary>
    /// <param name="colours">The selected background colours.</param>
    internal static RoomStaticPalette FromColours(ReadOnlySpan<RgbColor> colours)
    {
        var packed = new byte[RoomStaticPaletteFormat.ByteCount];
        for (int index = 0; index < colours.Length; index++)
            BinaryPrimitives.WriteUInt16LittleEndian(packed.AsSpan(index * RgbColor.ByteCount), colours[index].ToWord());
        return new RoomStaticPalette(packed);
    }

    /// <summary>The 128 colours packed as little-endian BGR555 words, two bytes each, after discarding the unused high bit.</summary>
    public ReadOnlyMemory<byte> PackedColours => packedColours;
}
