namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Crocomire's library-2 sound effects, queued with a six-entry admission limit.</summary>
internal static class CrocomireSoundDefinitions
{
    /// <summary>Library-2 sound $1C, queued as each fight projectile leaves the mouth.</summary>
    internal static readonly SoundEffectId ProjectileSpit = Library2Sound.ChozoGrab;

    /// <summary>Library-2 sound $22, the acid burn repeated every 32 frames while the body sinks.</summary>
    internal static readonly SoundEffectId AcidBurn = Library2Sound.ShotPipeBug;

    /// <summary>Library-2 sound $25, the big explosion of the body and skeleton programs and the spike-wall pieces.</summary>
    internal static readonly SoundEffectId BigExplosion = Library2Sound.BigExplosion;

    /// <summary>Library-2 sound $29, the crumble of the bridge and the hidden wall.</summary>
    internal static readonly SoundEffectId Crumble = Library2Sound.HeavyRumble;

    /// <summary>Library-2 sound $2B, the hidden wall's rumble.</summary>
    internal static readonly SoundEffectId WallRumble = Library2Sound.FireballImpact;

    /// <summary>Library-2 sound $2D, which starts the second melt dissolve.</summary>
    internal static readonly SoundEffectId SecondMeltDissolve = Library2Sound.KraidRoar;

    /// <summary>Library-2 sound $30, the spike wall breaking apart.</summary>
    internal static readonly SoundEffectId SpikeWallBreak = Library2Sound.ShotSuperDesgeega;

    /// <summary>Library-2 sound $3B, queued as the collapsing bridge drops the body into the acid.</summary>
    internal static readonly SoundEffectId BridgeCollapse = Library2Sound.DachoraShinespark;

    /// <summary>Library-2 sound $3B, queued by the unused claw branch for each claw strike on Samus.</summary>
    internal static readonly SoundEffectId ClawStrike = Library2Sound.DachoraShinespark;

    /// <summary>Library-2 sound $54, queued by the unused near-spike-wall charge when a hit interrupts it.</summary>
    internal static readonly SoundEffectId UnusedChargeInterrupted = Library2Sound.ShotCrocomire;

    /// <summary>Library-2 sound $74, Crocomire's cry.</summary>
    internal static readonly SoundEffectId Cry = Library2Sound.CrocomireCry;

    /// <summary>Library-2 sound $75, the skeleton collapsing.</summary>
    internal static readonly SoundEffectId SkeletonCollapse = Library2Sound.CrocomireSkeletonCollapse;

    /// <summary>Library-2 sound $76, queued with each screen shake of a heavy step.</summary>
    internal static readonly SoundEffectId ScreenShake = Library2Sound.Quake;

    /// <summary>Library-2 sound $77, which starts the first melt dissolve.</summary>
    internal static readonly SoundEffectId FirstMeltDissolve = Library2Sound.CrocomireMeltingCry;
}
