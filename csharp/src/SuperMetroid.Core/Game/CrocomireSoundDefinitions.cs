namespace SuperMetroid.Core.Game;

/// <summary>Crocomire's library-2 sound effects, queued with a six-entry admission limit.</summary>
internal static class CrocomireSoundDefinitions
{
    /// <summary>Library-2 sound $1C, queued as each fight projectile leaves the mouth.</summary>
    internal const ushort ProjectileSpit = 0x001c;

    /// <summary>Library-2 sound $22, the acid burn repeated every 32 frames while the body sinks.</summary>
    internal const ushort AcidBurn = 0x0022;

    /// <summary>Library-2 sound $25, the big explosion of the body and skeleton programs and the spike-wall pieces.</summary>
    internal const ushort BigExplosion = 0x0025;

    /// <summary>Library-2 sound $29, the crumble of the bridge and the hidden wall.</summary>
    internal const ushort Crumble = 0x0029;

    /// <summary>Library-2 sound $2B, the hidden wall's rumble.</summary>
    internal const ushort WallRumble = 0x002b;

    /// <summary>Library-2 sound $2D, which starts the second melt dissolve.</summary>
    internal const ushort SecondMeltDissolve = 0x002d;

    /// <summary>Library-2 sound $30, the spike wall breaking apart.</summary>
    internal const ushort SpikeWallBreak = 0x0030;

    /// <summary>Library-2 sound $3B, queued as the collapsing bridge drops the body into the acid.</summary>
    internal const ushort BridgeCollapse = 0x003b;

    /// <summary>Library-2 sound $3B, queued by the unused claw branch for each claw strike on Samus.</summary>
    internal const ushort ClawStrike = 0x003b;

    /// <summary>Library-2 sound $54, queued by the unused near-spike-wall charge when a hit interrupts it.</summary>
    internal const ushort UnusedChargeInterrupted = 0x0054;

    /// <summary>Library-2 sound $74, Crocomire's cry.</summary>
    internal const ushort Cry = 0x0074;

    /// <summary>Library-2 sound $75, the skeleton collapsing.</summary>
    internal const ushort SkeletonCollapse = 0x0075;

    /// <summary>Library-2 sound $76, queued with each screen shake of a heavy step.</summary>
    internal const ushort ScreenShake = 0x0076;

    /// <summary>Library-2 sound $77, which starts the first melt dissolve.</summary>
    internal const ushort FirstMeltDissolve = 0x0077;
}
