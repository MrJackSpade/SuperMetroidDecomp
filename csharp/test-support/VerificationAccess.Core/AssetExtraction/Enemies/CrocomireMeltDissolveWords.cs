using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The native VRAM word of each Crocomire melt band, which only verification reads (#1406).</summary>
internal static class CrocomireMeltDissolveWords
{
    /// <summary>The cartridge's upload destination of each band.</summary>
    extension(CrocomireMeltDissolve)
    {
        /// <summary>The first VRAM word the cartridge uploads <paramref name="band"/>'s characters to.</summary>
        internal static int BandFirstWord(int band) => BgCharacterWordsConstants.WordOf(BgCharacterSet.Tiles, CrocomireMeltDissolve.BandFirstCharacter(band));
    }
}
