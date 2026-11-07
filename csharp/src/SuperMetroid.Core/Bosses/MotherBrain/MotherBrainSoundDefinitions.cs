namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The sounds Mother Brain's programs and encounter queue.</summary>
internal static class MotherBrainSoundDefinitions
{
    /// <summary>The roar of the neck stretch, the Hyper Beam recoil and the dying drool.</summary>
    internal static SoundEffectId Roar { get; } = Library2Sound.MotherBrainHighCry;

    /// <summary>The cry as the mouth opens for an onion-ring or bomb attack.</summary>
    internal static SoundEffectId AttackCry { get; } = Library2Sound.MotherBrainLowCry;

    /// <summary>The first onion ring of a volley.</summary>
    internal static SoundEffectId OnionRings { get; } = Library3Sound.MotherBrainBlueRings;

    /// <summary>The red hand beam fires.</summary>
    internal static SoundEffectId HandBeam { get; } = Library2Sound.MotherBrainHandBeam;

    /// <summary>A bomb, onion ring or stray hand beam explodes.</summary>
    internal static SoundEffectId Explosion { get; } = Library3Sound.ProjectileImpact;

    /// <summary>Floor dust bursts as Mother Brain rises.</summary>
    internal static SoundEffectId AscentDust { get; } = Library2Sound.HeavyRumble;

    /// <summary>The rainbow drain's looping voice stops.</summary>
    internal static SoundEffectId StopRainbowDrain { get; } = Library1Sound.Silence;

    /// <summary>The rainbow drain's looping voice starts, also when the Baby begins draining Mother Brain.</summary>
    internal static SoundEffectId StartRainbowDrain { get; } = Library1Sound.MotherBrainRainbowBeam;

    /// <summary>The rainbow beam starts charging.</summary>
    internal static SoundEffectId RainbowBeamCharge { get; } = Library2Sound.MotherBrainRainbowCharge;
}
