namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The sounds Mother Brain's programs and encounter queue.</summary>
internal static class MotherBrainSoundDefinitions
{
    /// <summary>Library-2 sound $7E: the roar of the neck stretch, the Hyper Beam recoil and the dying drool.</summary>
    internal static SoundEffectId Roar { get; } = Library2Sound.MotherBrainHighCry;

    /// <summary>Library-2 sound $6F: the cry as the mouth opens for an onion-ring or bomb attack.</summary>
    internal static SoundEffectId AttackCry { get; } = Library2Sound.MotherBrainLowCry;

    /// <summary>Library-3 sound $17: the first onion ring of a volley.</summary>
    internal static SoundEffectId OnionRings { get; } = Library3Sound.MotherBrainBlueRings;

    /// <summary>Library-2 sound $63: the red hand beam fires.</summary>
    internal static SoundEffectId HandBeam { get; } = Library2Sound.MotherBrainHandBeam;

    /// <summary>Library-3 sound $13: a bomb, onion ring or stray hand beam explodes.</summary>
    internal static SoundEffectId Explosion { get; } = Library3Sound.ProjectileImpact;

    /// <summary>Library-2 sound $29: floor dust bursts as Mother Brain rises.</summary>
    internal static SoundEffectId AscentDust { get; } = Library2Sound.HeavyRumble;

    /// <summary>Library-1 sound $02: the rainbow drain's looping voice stops ($A9:BE96).</summary>
    internal static SoundEffectId StopRainbowDrain { get; } = Library1Sound.Silence;

    /// <summary>Library-1 sound $40: the rainbow drain's looping voice starts, also when the Baby begins draining Mother Brain ($A9:C889).</summary>
    internal static SoundEffectId StartRainbowDrain { get; } = Library1Sound.MotherBrainRainbowBeam;

    /// <summary>Library-2 sound $7F: the rainbow beam starts charging.</summary>
    internal static SoundEffectId RainbowBeamCharge { get; } = Library2Sound.MotherBrainRainbowCharge;
}
