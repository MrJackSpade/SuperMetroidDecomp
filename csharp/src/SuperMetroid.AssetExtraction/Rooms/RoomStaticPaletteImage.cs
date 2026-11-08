using SuperMetroid.AssetExtraction.Video;
using System.Buffers.Binary;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// A room palette's colours in the cartridge's order (#1464): what the import composers colour room sheets from. Core
/// never holds the palette, and no host loads it (#1544).
/// </summary>
internal static class RoomStaticPaletteImage
{
    /// <summary>The 128 background colours <paramref name="palette"/> loads, in the cartridge's order.</summary>
    internal static RgbColor[] Colours(RoomStaticPalette palette) => Ensure.NotNull(palette).Colours.ToArray();
}
