using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Bosses.SporeSpawn.Artwork;

/// <summary>Cartridge values and helpers of <see cref="SporeSpawnColorCatalog"/> that only verification reads.</summary>
internal static class SporeSpawnColorsConstants
{
    /// <summary>Resolves one death-script layer's packed RGB5 ink without writing a palette.</summary>
    internal static RgbColor Death(SporeSpawnColorCatalog colors, SporeSpawnDeathPaletteLayer layer, int frame, int color) =>
        layer switch
        {
            SporeSpawnDeathPaletteLayer.Sprite => colors.ResolveDeathSprite(frame, color),
            SporeSpawnDeathPaletteLayer.Level => colors.ResolveDeathLevel(frame, color),
            SporeSpawnDeathPaletteLayer.Background => colors.ResolveDeathBackground(frame, color),
            _ => throw new ArgumentOutOfRangeException(nameof(layer)),
        };
}
