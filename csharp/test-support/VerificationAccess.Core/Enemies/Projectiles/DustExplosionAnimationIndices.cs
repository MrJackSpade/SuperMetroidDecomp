namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// Verification's native view of <see cref="DustExplosionAnimation"/>: the index the dust-cloud-or-explosion projectile's spawn
/// parameter gives each animation in the cartridge's thirty-entry selector table ($86:E42C). Core's values are arbitrary.
/// The other sixteen indices (0, 5, 7, 8, 13 to 16, 19, 20, 22, 23 and 25 to 28) are passed by no retail spawner, so they
/// name no animation of the game (#1686).
/// </summary>
internal static class DustExplosionAnimationIndices
{
    /// <summary>The selector table's entry count.</summary>
    internal const int SelectorCount = 30;

    /// <summary>Each animation of the game by its selector index.</summary>
    private static readonly Dictionary<int, DustExplosionAnimation> Animations = new()
    {
        [1] = DustExplosionAnimation.MotherBrainElbowChargeParticle,
        [2] = DustExplosionAnimation.MotherBrainElbowChargeEnergy,
        [3] = DustExplosionAnimation.SmallExplosion,
        [4] = DustExplosionAnimation.MotherBrainRainbowBeamExplosion,
        [6] = DustExplosionAnimation.DudShotTinyExplosion,
        [9] = DustExplosionAnimation.SmallDustCloud,
        [10] = DustExplosionAnimation.CorpseDustCloud,
        [11] = DustExplosionAnimation.EyeDoorSmokePuff,
        [12] = DustExplosionAnimation.Smoke,
        [17] = DustExplosionAnimation.RockParticles,
        [18] = DustExplosionAnimation.ShortBigDustCloud,
        [21] = DustExplosionAnimation.BigDustCloud,
        [24] = DustExplosionAnimation.LongDraygonBreathBubbles,
        [29] = DustExplosionAnimation.BigExplosion,
    };

    /// <summary>The animation selector <paramref name="index"/> names, or null for one no retail spawner passes.</summary>
    internal static DustExplosionAnimation? TryDecode(int index) =>
        Animations.TryGetValue(index, out DustExplosionAnimation animation) ? animation : null;

    /// <summary>The animation selector <paramref name="index"/> names.</summary>
    /// <exception cref="InvalidDataException">No retail spawner passes the index.</exception>
    internal static DustExplosionAnimation Decode(int index) => TryDecode(index)
        ?? throw new InvalidDataException($"Misc-dust selector {index} names no animation of the game.");
}
