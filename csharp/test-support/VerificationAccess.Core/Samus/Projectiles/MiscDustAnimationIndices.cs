namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Verification's native view of <see cref="MiscDustAnimation"/>: the index the dust-cloud-or-explosion projectile's spawn
/// parameter gives each animation in the cartridge's thirty-entry selector table ($86:E42C). Core's values are arbitrary.
/// The other sixteen indices (0, 5, 7, 8, 13 to 16, 19, 20, 22, 23 and 25 to 28) are passed by no retail spawner, so they
/// name no animation of the game (#1686).
/// </summary>
internal static class MiscDustAnimationIndices
{
    /// <summary>The selector table's entry count.</summary>
    internal const int SelectorCount = 30;

    /// <summary>Each animation of the game by its selector index.</summary>
    private static readonly Dictionary<int, MiscDustAnimation> Animations = new()
    {
        [1] = MiscDustAnimation.MotherBrainElbowChargeParticle,
        [2] = MiscDustAnimation.MotherBrainElbowChargeEnergy,
        [3] = MiscDustAnimation.SmallExplosion,
        [4] = MiscDustAnimation.MotherBrainRainbowBeamExplosion,
        [6] = MiscDustAnimation.DudShotTinyExplosion,
        [9] = MiscDustAnimation.SmallDustCloud,
        [10] = MiscDustAnimation.CorpseDustCloud,
        [11] = MiscDustAnimation.EyeDoorSweatDrop,
        [12] = MiscDustAnimation.Smoke,
        [17] = MiscDustAnimation.RockParticles,
        [18] = MiscDustAnimation.ShortBigDustCloud,
        [21] = MiscDustAnimation.BigDustCloud,
        [24] = MiscDustAnimation.LongDraygonBreathBubbles,
        [29] = MiscDustAnimation.BigExplosion,
    };

    /// <summary>The animation selector <paramref name="index"/> names, or null for one no retail spawner passes.</summary>
    internal static MiscDustAnimation? TryDecode(int index) =>
        Animations.TryGetValue(index, out MiscDustAnimation animation) ? animation : null;

    /// <summary>The animation selector <paramref name="index"/> names.</summary>
    /// <exception cref="InvalidDataException">No retail spawner passes the index.</exception>
    internal static MiscDustAnimation Decode(int index) => TryDecode(index)
        ?? throw new InvalidDataException($"Misc-dust selector {index} names no animation of the game.");
}
