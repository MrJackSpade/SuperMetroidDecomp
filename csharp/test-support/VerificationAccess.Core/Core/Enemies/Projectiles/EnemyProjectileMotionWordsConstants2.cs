using SuperMetroid.Core.Enemies.KiHunters;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Cartridge values and helpers of <see cref="EnemyProjectileMotionWords"/> that only verification reads.</summary>
internal static class EnemyProjectileMotionWordsConstants2
{
    /// <summary>
    /// The routine that launches Ki-Hunter acid spat each way (#2338): both stand for
    /// <see cref="KiHunterAcidMotion.Launching"/>, and the acid's state keeps the way it was spat.
    /// </summary>
    internal static readonly IReadOnlyDictionary<HorizontalDirection, ushort> KiHunterAcidLaunches = new Dictionary<HorizontalDirection, ushort>
    {
        [HorizontalDirection.Left] = 0xcfd5,
        [HorizontalDirection.Right] = 0xcfe6,
    };
}
