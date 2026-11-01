namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Standalone horizontal speed records selected directly rather than by movement type.</summary>
public enum SamusStandaloneSpeedRecord
{
    /// <summary>XAccelSpeeds_DiagonalBombJump, used by diagonal bomb jumps.</summary>
    DiagonalBombJump,
    /// <summary>XAccelSpeeds_DisconnectGrappleInAir.</summary>
    GrappleReleaseAir,
    /// <summary>XAccelSpeeds_DisconnectGrappleInWater.</summary>
    GrappleReleaseWater,
    /// <summary>XAccelSpeeds_DisconnectGrappleInLavaAcid.</summary>
    GrappleReleaseLavaAcid,
}
