namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The sounds Mother Brain's animations and encounter queue.</summary>
internal static class MotherBrainSoundDefinitions
{
    /// <summary>The roar of the neck stretch, the Hyper Beam recoil and the dying drool.</summary>
    internal static SoundEffectId Roar { get; } = EnemySound.LongWail;

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

    /// <summary>The rainbow drain's looping sound stops.</summary>
    internal static SoundEffectId StopRainbowDrain { get; } = SamusSound.Silence;

    /// <summary>The rainbow drain's looping sound starts, also when the Baby begins draining Mother Brain.</summary>
    internal static SoundEffectId StartRainbowDrain { get; } = SamusSound.MotherBrainRainbowBeam;

    /// <summary>The rainbow beam starts charging.</summary>
    internal static SoundEffectId RainbowBeamCharge { get; } = EnemySound.MotherBrainRainbowCharge;

    /// <summary>
    /// The cue sounds that request a priority other than the normal one, with that priority: the explosion competes at low
    /// priority.
    /// </summary>
    internal static IReadOnlyDictionary<SoundEffectId, SoundRequestPriority> CueSoundPriorities { get; } =
        new Dictionary<SoundEffectId, SoundRequestPriority>
        {
            [Explosion] = SoundRequestPriority.Low,
        };
}
