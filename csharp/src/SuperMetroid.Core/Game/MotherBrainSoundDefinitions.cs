using SuperMetroid.Core.Audio;

namespace SuperMetroid.Core.Game;

/// <summary>The sounds Mother Brain's programs and encounter queue.</summary>
internal static class MotherBrainSoundDefinitions
{
    /// <summary>Library-2 sound $7E: the roar of the neck stretch, the Hyper Beam recoil and the dying drool.</summary>
    internal static SoundEffectId Roar { get; } = SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x007e);

    /// <summary>Library-2 sound $6F: the cry as the mouth opens for an onion-ring or bomb attack.</summary>
    internal static SoundEffectId AttackCry { get; } = SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x006f);

    /// <summary>Library-3 sound $17: the first onion ring of a volley.</summary>
    internal static SoundEffectId OnionRings { get; } = SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 0x0017);

    /// <summary>Library-2 sound $63: the red hand beam fires.</summary>
    internal static SoundEffectId HandBeam { get; } = SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x0063);

    /// <summary>Library-3 sound $13: a bomb, onion ring or stray hand beam explodes.</summary>
    internal static SoundEffectId Explosion { get; } = SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 0x0013);

    /// <summary>Library-2 sound $29: floor dust bursts as Mother Brain rises.</summary>
    internal static SoundEffectId AscentDust { get; } = SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x0029);

    /// <summary>$A9:BE96 queues library-one $02 when the rainbow drain runs out.</summary>
    internal const ushort StopRainbowDrain = 0x0002;

    /// <summary>$A9:C889 queues library-one $40 when the Baby begins draining Mother Brain.</summary>
    internal const ushort StartRainbowDrain = 0x0040;

    /// <summary>Library-2 sound $7F: the rainbow beam starts charging.</summary>
    internal static SoundEffectId RainbowBeamCharge { get; } = SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x007f);
}
