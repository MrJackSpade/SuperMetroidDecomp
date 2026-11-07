namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Crocomire's library-2 sound effects, queued with a six-entry admission limit.</summary>
internal static class CrocomireSoundDefinitions
{
    /// <summary>The sound queued as each fight projectile leaves the mouth.</summary>
    internal static readonly SoundEffectId ProjectileSpit = Library2Sound.ChozoGrab;

    /// <summary>The acid burn, repeated every 32 frames while the body sinks.</summary>
    internal static readonly SoundEffectId AcidBurn = Library2Sound.ShotPipeBug;

    /// <summary>The big explosion of the body and skeleton programs and the spike-wall pieces.</summary>
    internal static readonly SoundEffectId BigExplosion = Library2Sound.BigExplosion;

    /// <summary>The crumble of the bridge and the hidden wall.</summary>
    internal static readonly SoundEffectId Crumble = Library2Sound.HeavyRumble;

    /// <summary>The hidden wall's rumble.</summary>
    internal static readonly SoundEffectId WallRumble = Library2Sound.FireballImpact;

    /// <summary>The sound that starts the second melt dissolve.</summary>
    internal static readonly SoundEffectId SecondMeltDissolve = Library2Sound.KraidRoar;

    /// <summary>The spike wall breaking apart.</summary>
    internal static readonly SoundEffectId SpikeWallBreak = Library2Sound.ShotSuperDesgeega;

    /// <summary>The sound queued as the collapsing bridge drops the body into the acid.</summary>
    internal static readonly SoundEffectId BridgeCollapse = Library2Sound.DachoraShinespark;

    /// <summary>The sound the unused claw branch queues for each claw strike on Samus.</summary>
    internal static readonly SoundEffectId ClawStrike = Library2Sound.DachoraShinespark;

    /// <summary>The sound the unused near-spike-wall charge queues when a hit interrupts it.</summary>
    internal static readonly SoundEffectId UnusedChargeInterrupted = Library2Sound.ShotCrocomire;

    /// <summary>Crocomire's cry.</summary>
    internal static readonly SoundEffectId Cry = Library2Sound.CrocomireCry;

    /// <summary>The skeleton collapsing.</summary>
    internal static readonly SoundEffectId SkeletonCollapse = Library2Sound.CrocomireSkeletonCollapse;

    /// <summary>The quake queued with each screen shake of a heavy step.</summary>
    internal static readonly SoundEffectId ScreenShake = Library2Sound.Quake;

    /// <summary>The melting cry, which starts the first melt dissolve.</summary>
    internal static readonly SoundEffectId FirstMeltDissolve = Library2Sound.CrocomireMeltingCry;
}
