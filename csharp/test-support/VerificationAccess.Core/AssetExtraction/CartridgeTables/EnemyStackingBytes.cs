using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// The cartridge's enemy draw layers naming the enemies' standings (#2618), which only verification's header decode reads:
/// the game draws its enemies layer by layer, the lower first, and Samus with her shots after the third.
/// </summary>
internal static class EnemyStackingBytes
{
    /// <summary>The draw layer each standing stands at.</summary>
    internal static readonly NativeWordIdentities<EnemyStacking> Layers = new(new Dictionary<EnemyStacking, ushort>
    {
        [EnemyStacking.Frontmost] = 0,
        [EnemyStacking.InFrontOfSamus] = 2,
        [EnemyStacking.JustBehindSamus] = 4,
        [EnemyStacking.BehindSamus] = 5,
        [EnemyStacking.FarBehindSamus] = 6,
        [EnemyStacking.Rearmost] = 7,
    });

    /// <summary>The standing an enemy header's layer byte (+$39) names.</summary>
    internal static EnemyStacking Decode(byte layer) => Layers.Decode(layer, "enemy draw layer");
}
