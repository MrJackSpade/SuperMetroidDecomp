using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The sprite character bytes one region of a graphics set's characters spans (#1392, #1406): where the cartridge's uploads
/// fall, which extraction alone knows. Core names the region by its ordinal.
/// </summary>
/// <param name="FirstByte">The region's first character table byte.</param>
/// <param name="ByteCount">Its bytes.</param>
internal sealed record CharacterRange(int FirstByte, int ByteCount)
{
    /// <summary>The uploads among <paramref name="uploads"/> that show over <paramref name="range"/> when made: those covering it whole.</summary>
    internal static IEnumerable<SpriteArtworkPlacement> Over(IEnumerable<SpriteArtworkPlacement> uploads, CharacterRange range)
    {
        Ensure.NotNull(range);
        return Ensure.NotNull(uploads).Where(upload =>
            upload.FirstByte <= range.FirstByte && range.FirstByte + range.ByteCount <= upload.FirstByte + upload.ByteCount);
    }

    /// <summary>Whether <paramref name="byteCount"/> bytes from <paramref name="firstByte"/> share a byte with this range.</summary>
    internal bool Overlaps(int firstByte, int byteCount) => firstByte < FirstByte + ByteCount && FirstByte < firstByte + byteCount;
}
