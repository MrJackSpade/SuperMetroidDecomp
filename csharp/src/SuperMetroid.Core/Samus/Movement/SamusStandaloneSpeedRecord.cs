namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Standalone bank-$90 horizontal speed records selected by literal address rather than movement type.</summary>
public enum SamusStandaloneSpeedRecord
{
    /// <summary>$90:9F25 XAccelSpeeds_DiagonalBombJump, used by diagonal bomb jumps.</summary>
    DiagonalBombJump = 0x909f25,
    /// <summary>$90:9F31 XAccelSpeeds_DisconnectGrappleInAir.</summary>
    GrappleReleaseAir = 0x909f31,
    /// <summary>$90:9F3D XAccelSpeeds_DisconnectGrappleInWater.</summary>
    GrappleReleaseWater = 0x909f3d,
    /// <summary>$90:9F49 XAccelSpeeds_DisconnectGrappleInLavaAcid.</summary>
    GrappleReleaseLavaAcid = 0x909f49,
}
