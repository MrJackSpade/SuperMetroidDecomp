namespace SuperMetroid.Core.Game;

/// <summary>Bank-$94 inside-block dispatch and bank-$84 sand setup identities.</summary>
public static class QuicksandRomData
{
    /// <summary>$94:9B06, area-indexed pointers used by BlockInsideReact_SpecialAir at $94:9B16.</summary>
    public const int InsideAreaTables = 0x949b06;
    /// <summary>Bank $94, area-specific block-reaction table pointers.</summary>
    public const int CollisionBank = 0x940000;
    /// <summary>Bank $84, PLM headers and setup entry points.</summary>
    public const int PlmBank = 0x840000;
    /// <summary>$94:92D9, area-specific special-air collision PLM lists.</summary>
    public const int CollisionAreaTables = 0x9492d9;
    /// <summary>$84:B4C4, PlmSetup_QuicksandSurfaceB; carry and quicksand contact publication.</summary>
    public const ushort SurfaceCollision = 0xb4c4;
    /// <summary>$84:B541, submerging sand collision clears vertical speed and gravity.</summary>
    public const ushort SubmergingCollision = 0xb541;
    /// <summary>$84:B4C4's $0030 middle-word clamp: 0.1875 pixels per stationary surface probe.</summary>
    public const int SurfaceProbeLimit = 0x3000;
    /// <summary>$84:B408, PlmSetup_QuicksandSurface; bottom, center, and top body sampling.</summary>
    public const ushort SurfaceSetup = 0xb408;
    /// <summary>$84:B497, PlmSetup_B71F_SubmergingQuicksand.</summary>
    public const ushort SubmergingSetup = 0xb497;
    /// <summary>$84:B4A8, PlmSetup_B723_SandfallsSlow.</summary>
    public const ushort SlowFallsSetup = 0xb4a8;
    /// <summary>$84:B4B6, PlmSetup_B727_SandFallsFast.</summary>
    public const ushort FastFallsSetup = 0xb4b6;
    /// <summary>$84:B48B, upward/falling surface extra-Y values, indexed by Gravity Suit.</summary>
    public const int MovingSurfaceDisplacement = 0x84b48b;
    /// <summary>$84:B48F, stationary surface extra-Y values, indexed by Gravity Suit.</summary>
    public const int StationarySurfaceDisplacement = 0x84b48f;
    /// <summary>$84:B493, upward speed limits, indexed by Gravity Suit.</summary>
    public const int SurfaceJumpLimit = 0x84b493;
    /// <summary>Immediate 16.16 extra displacement in PlmSetup_B71F_SubmergingQuicksand.</summary>
    public const int SubmergingDisplacement = 0x12000;
    /// <summary>Immediate 16.16 extra displacement in PlmSetup_B723_SandfallsSlow.</summary>
    public const int SlowFallsDisplacement = 0x14000;
    /// <summary>Immediate 16.16 extra displacement in PlmSetup_B727_SandFallsFast.</summary>
    public const int FastFallsDisplacement = 0x1c000;
}
