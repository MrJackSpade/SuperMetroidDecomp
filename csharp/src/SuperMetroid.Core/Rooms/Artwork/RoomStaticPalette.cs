using System.Buffers.Binary;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>The 128 base BG colors selected by one room graphics-set palette source.</summary>
public sealed class RoomStaticPalette
{
    /// <summary>Validated little-endian BGR555 colours, owned independently of the JSON authoring data.</summary>
    private readonly byte[] nativeBytes;

    /// <summary>Wraps the packed colours assembled by <see cref="FromColours"/>.</summary>
    private RoomStaticPalette(byte[] nativeBytes) => this.nativeBytes = nativeBytes;

    /// <summary>Packs the room's selected background colours in the order the room load transfers them.</summary>
    /// <param name="colours">The selected background colours.</param>
    internal static RoomStaticPalette FromColours(ReadOnlySpan<Bgr555> colours)
    {
        var native = new byte[RoomAssetRomData.GraphicsLayout.BackgroundPaletteByteCount];
        for (int index = 0; index < colours.Length; index++)
            BinaryPrimitives.WriteUInt16LittleEndian(native.AsSpan(index * Bgr555.ByteCount), colours[index].ToWord());
        return new RoomStaticPalette(native);
    }

    /// <summary>The 128 colours packed as little-endian BGR555 words, two bytes each, after discarding the unused high bit.</summary>
    public ReadOnlyMemory<byte> PackedColours => nativeBytes;

    /// <summary>Loads all 128 colors into CGRAM beginning at color zero.</summary>
    /// <param name="cgram">The color memory that receives the 256-byte native transfer.</param>
    public void LoadTo(IPaletteColors cgram)
    {
        Ensure.NotNull(cgram);
        new CgramPalettes(cgram).Background.LoadPortBytes(nativeBytes);
    }
}
