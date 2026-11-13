namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The sounds Mother Brain's animations and encounter queue.</summary>
internal static class MotherBrainSoundDefinitions
{
    /// <summary>The roar of the neck stretch, the Hyper Beam recoil and the dying drool.</summary>
    internal static SoundEffectId Roar { get; } = EnemySound.MotherBrainHighCry;

    /// <summary>The cry as the mouth opens for an onion-ring or bomb attack.</summary>
    internal static SoundEffectId AttackCry { get; } = EnemySound.MotherBrainLowCry;

    /// <summary>The first onion ring of a volley.</summary>
    internal static SoundEffectId OnionRings { get; } = RoomSound.MotherBrainBlueRings;

    /// <summary>The red hand beam fires.</summary>
    internal static SoundEffectId HandBeam { get; } = EnemySound.MotherBrainHandBeam;

    /// <summary>A bomb, onion ring or stray hand beam explodes.</summary>
    internal static SoundEffectId Explosion { get; } = RoomSound.ProjectileImpact;

    /// <summary>Floor dust bursts as Mother Brain rises.</summary>
    internal static SoundEffectId AscentDust { get; } = EnemySound.HeavyRumble;

    /// <summary>The rainbow drain's looping voice stops.</summary>
    internal static SoundEffectId StopRainbowDrain { get; } = SamusSound.Silence;

    /// <summary>The rainbow drain's looping voice starts, also when the Baby begins draining Mother Brain.</summary>
    internal static SoundEffectId StartRainbowDrain { get; } = SamusSound.MotherBrainRainbowBeam;

    /// <summary>The rainbow beam starts charging.</summary>
    internal static SoundEffectId RainbowBeamCharge { get; } = EnemySound.MotherBrainRainbowCharge;
}
