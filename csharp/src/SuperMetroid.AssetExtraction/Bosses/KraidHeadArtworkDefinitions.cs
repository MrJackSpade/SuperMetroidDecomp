using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.Kraid;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// Where the cartridge stores Kraid's four head tilemaps, by mouth stage: extraction's own knowledge, which the game
/// never reads.
/// </summary>
[EncodingOwner("cartridge addresses of Kraid's head tilemaps")]
internal static partial class KraidHeadArtworkDefinitions
{
    /// <summary>Where each head tilemap is stored, by the head pose it shows.</summary>
    private static readonly Dictionary<KraidHeadFrame, SnesAddress> ByHead = new()
    {
        [KraidHeadFrame.Closed] = new(0xa7, 0x97c8),
        [KraidHeadFrame.Opening] = new(0xa7, 0x9ac8),
        [KraidHeadFrame.Wider] = new(0xa7, 0x9dc8),
        [KraidHeadFrame.Open] = new(0xa7, 0xa0c8),
    };

    /// <summary>The bank-$A7 address of the head tilemap <paramref name="head"/> shows.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="head"/> is no head pose the import extracts.</exception>
    internal static ushort Address(KraidHeadFrame head) =>
        ByHead.TryGetValue(head, out SnesAddress address) ? address.Offset : throw new ArgumentOutOfRangeException(nameof(head), head, "Kraid's head has no such tilemap.");
}
