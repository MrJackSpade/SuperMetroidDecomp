using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// Where the cartridge stores Kraid's four head tilemaps, by mouth stage: extraction's own knowledge, which the game
/// never reads.
/// </summary>
internal static class KraidHeadArtworkDefinitions
{
    /// <summary>Where each head tilemap is stored, from the closed mouth to the fully open one.</summary>
    private static readonly SnesAddress[] ByMouthStage = [new(0xa7, 0x97c8), new(0xa7, 0x9ac8), new(0xa7, 0x9dc8), new(0xa7, 0xa0c8)];

    /// <summary>The bank-$A7 address of the head tilemap shown at <paramref name="mouthStage"/>.</summary>
    internal static ushort Address(int mouthStage) => ByMouthStage[mouthStage].Offset;
}
