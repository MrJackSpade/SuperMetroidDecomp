using System.Buffers.Binary;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// A room palette's colours in the cartridge's order (#1464): what the import composers colour room sheets from and
/// what a palette's installed identity hashes. Core never holds the palette.
/// </summary>
internal static class RoomStaticPaletteImage
{
    /// <summary>The 128 background colours <paramref name="palette"/> loads, in the cartridge's order.</summary>
    internal static RgbColor[] Colours(RoomStaticPalette palette) => Ensure.NotNull(palette).Colours.ToArray();

    /// <summary>
    /// The 128 background colours <paramref name="palette"/> loads, in the cartridge's order, packed as little-endian
    /// BGR555 words, two bytes each, as the room load transfers them.
    /// </summary>
    internal static byte[] PackedColours(RoomStaticPalette palette)
    {
        RgbColor[] colours = Colours(palette);
        var packed = new byte[colours.Length * RgbColor.ByteCount];
        for (int index = 0; index < colours.Length; index++)
            BinaryPrimitives.WriteUInt16LittleEndian(packed.AsSpan(index * RgbColor.ByteCount), colours[index].ToWord());
        return packed;
    }
}
