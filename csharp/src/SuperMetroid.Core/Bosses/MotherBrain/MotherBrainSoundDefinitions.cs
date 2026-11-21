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

    /// <summary>
    /// A bomb, onion ring, stray hand beam, her death or the Baby's or the escape door's dust explodes; the explosion
    /// competes at low priority. The request carries its priority (#2580): the sound is one other requests ask for at
    /// normal priority.
    /// </summary>
    internal static SoundRequest Explosion { get; } = new(EnemySound.SmallExplosion, SoundRequestPriority.Low);

    /// <summary>Floor dust bursts as Mother Brain rises.</summary>
    internal static SoundEffectId AscentDust { get; } = EnemySound.HeavyRumble;

    /// <summary>The stop of the rainbow drain's looping sound.</summary>
    internal static SoundEffectId StopRainbowDrain { get; } = SamusSoundStop.Any;

    /// <summary>The rainbow drain's looping sound starts, also when the Baby begins draining Mother Brain.</summary>
    internal static SoundEffectId StartRainbowDrain { get; } = SamusSound.MotherBrainRainbowBeam;

    /// <summary>The rainbow beam starts charging.</summary>
    internal static SoundEffectId RainbowBeamCharge { get; } = EnemySound.MotherBrainRainbowCharge;
}
