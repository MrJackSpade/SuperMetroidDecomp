namespace SuperMetroid.Core.Game;

/// <summary>Pinned bank-$90 horizontal mechanics, separate from animation and palette definitions.</summary>
internal static class SamusHorizontalMotionDefinitions
{
    /// <summary>$90:9F25 XAccelSpeeds_DiagonalBombJump: acceleration, maximum and deceleration whole/fraction pairs.</summary>
    private static readonly SpeedTableEntry DiagonalBombJump = new(0, 0x3000, 3, 0, 0, 0x0800);

    /// <summary>$90:9F31/$9F3D/$9F49 XAccelSpeeds_DisconnectGrappleInAir/InWater/InLavaAcid: three identical authored records.</summary>
    private static readonly SpeedTableEntry GrappleRelease = new(0, 0x3000, 15, 0, 0, 0x1000);

    /// <summary>
    /// Recognizes exact standalone entry addresses. Unknown/unaligned addresses are not
    /// clamped to a known record: callers may still be reading live memory or other data.
    /// </summary>
    internal static bool TryResolveStandalone(int address, out SpeedTableEntry entry)
    {
        switch (address)
        {
            case SamusMovementRomData.VerticalMotion.DiagonalBombJumpHorizontalSpeed:
                entry = DiagonalBombJump;
                return true;
            case SamusMovementRomData.VerticalMotion.GrappleReleaseAirSpeed:
            case SamusMovementRomData.VerticalMotion.GrappleReleaseWaterSpeed:
            case SamusMovementRomData.VerticalMotion.GrappleReleaseLavaAcidSpeed:
                entry = GrappleRelease;
                return true;
            default:
                entry = default;
                return false;
        }
    }
}
