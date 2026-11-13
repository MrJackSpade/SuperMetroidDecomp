namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Crocomire's enemy-library sound effects, queued with a six-entry admission limit.</summary>
internal static class CrocomireSoundDefinitions
{
    /// <summary>The sound queued as each fight projectile leaves the mouth.</summary>
    internal static readonly SoundEffectId ProjectileSpit = EnemySound.ChozoGrab;

    /// <summary>The acid burn, repeated every 32 frames while the body sinks.</summary>
    internal static readonly SoundEffectId AcidBurn = EnemySound.ShotPipeBug;

    /// <summary>The big explosion of the body and skeleton animations and the spike-wall pieces.</summary>
    internal static readonly SoundEffectId BigExplosion = EnemySound.BigExplosion;

    /// <summary>The crumble of the bridge and the hidden wall.</summary>
    internal static readonly SoundEffectId Crumble = EnemySound.HeavyRumble;

    /// <summary>The hidden wall's rumble.</summary>
    internal static readonly SoundEffectId WallRumble = EnemySound.FireballImpact;

    /// <summary>The sound that starts the second melt dissolve.</summary>
    internal static readonly SoundEffectId SecondMeltDissolve = EnemySound.KraidRoar;

    /// <summary>The spike wall breaking apart.</summary>
    internal static readonly SoundEffectId SpikeWallBreak = EnemySound.ShotSuperDesgeega;

    /// <summary>The sound queued as the collapsing bridge drops the body into the acid.</summary>
    internal static readonly SoundEffectId BridgeCollapse = EnemySound.DachoraShinespark;

    /// <summary>Crocomire's cry.</summary>
    internal static readonly SoundEffectId Cry = EnemySound.CrocomireCry;

    /// <summary>The skeleton collapsing.</summary>
    internal static readonly SoundEffectId SkeletonCollapse = EnemySound.CrocomireSkeletonCollapse;

    /// <summary>The quake queued with each screen shake of a heavy step.</summary>
    internal static readonly SoundEffectId ScreenShake = EnemySound.Quake;

    /// <summary>The melting cry, which starts the first melt dissolve.</summary>
    internal static readonly SoundEffectId FirstMeltDissolve = EnemySound.CrocomireMeltingCry;
}
