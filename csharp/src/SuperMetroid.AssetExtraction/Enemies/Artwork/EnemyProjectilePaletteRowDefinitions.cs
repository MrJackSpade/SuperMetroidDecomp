using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>
/// The palette row the game ORs into every part of a projectile's frames where the projectile's initialization stores a
/// palette index (#2640); every other projectile frame's parts show their own rows.
/// </summary>
internal static partial class EnemyProjectilePaletteRowDefinitions
{
    /// <summary>
    /// Kraid's rock row, which his initialization loads his rock colours into: the row his rocks' initialization stores
    /// ($86:9D0C stores palette index $0600 for the rocks of instruction lists $86:9C7D and $86:9C83).
    /// </summary>
    private const int KraidRocksRow = 3;

    /// <summary>
    /// The row Draygon's goop is drawn in (#2655), which his opening's colours run on into: the row its initialization stores
    /// ($86:8D37 stores palette index $0400 for every frame of instruction lists $86:8C3A and $86:8C58). Its bursting frames'
    /// parts name the row after their own, so they show the row after this one.
    /// </summary>
    private const int DraygonGoopRow = 2;

    /// <summary>The row each listed projectile frame's initialization ORs into its parts.</summary>
    internal static IReadOnlyDictionary<EnemyProjectileFrameId, int> Rows { get; } = new Dictionary<EnemyProjectileFrameId, int>
    {
        [EnemyProjectileFrameId.KraidRockLarge] = KraidRocksRow,
        [EnemyProjectileFrameId.KraidRockSmall] = KraidRocksRow,
        [EnemyProjectileFrameId.DraygonGoopSpiky] = DraygonGoopRow,
        [EnemyProjectileFrameId.DraygonGoopPuffed] = DraygonGoopRow,
        [EnemyProjectileFrameId.DraygonGoopRounded] = DraygonGoopRow,
        [EnemyProjectileFrameId.DraygonGoopLopsided] = DraygonGoopRow,
        [EnemyProjectileFrameId.DraygonGoopCurled] = DraygonGoopRow,
        [EnemyProjectileFrameId.DraygonGoopScattered] = DraygonGoopRow,
    };
}
