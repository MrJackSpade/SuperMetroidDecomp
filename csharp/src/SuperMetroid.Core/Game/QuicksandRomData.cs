namespace SuperMetroid.Core.Game;

/// <summary>Bank-$94 inside-block dispatch and bank-$84 sand setup identities.</summary>
public static class QuicksandRomData
{
    /// <summary>$84:B4C4's $0030 middle-word clamp: 0.1875 pixels per stationary surface probe.</summary>
    public const int SurfaceProbeLimit = 0x3000;
    /// <summary>Immediate 16.16 extra displacement in PlmSetup_B71F_SubmergingQuicksand.</summary>
    public const int SubmergingDisplacement = 0x12000;
    /// <summary>Immediate 16.16 extra displacement in PlmSetup_B723_SandfallsSlow.</summary>
    public const int SlowFallsDisplacement = 0x14000;
    /// <summary>Immediate 16.16 extra displacement in PlmSetup_B727_SandFallsFast.</summary>
    public const int FastFallsDisplacement = 0x1c000;

}
