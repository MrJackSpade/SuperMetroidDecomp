namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Standalone bank-$90 horizontal speed records selected directly rather than by movement type.</summary>
public enum SamusStandaloneSpeedRecord
{
    /// <summary>$90:9F25 XAccelSpeeds_DiagonalBombJump, used by diagonal bomb jumps.</summary>
    DiagonalBombJump,
    /// <summary>$90:9F31 XAccelSpeeds_DisconnectGrappleInAir.</summary>
    GrappleReleaseAir,
    /// <summary>$90:9F3D XAccelSpeeds_DisconnectGrappleInWater.</summary>
    GrappleReleaseWater,
    /// <summary>$90:9F49 XAccelSpeeds_DisconnectGrappleInLavaAcid.</summary>
    GrappleReleaseLavaAcid,
}
