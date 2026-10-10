namespace SuperMetroid.Core.Game;

/// <summary>Pinned bank-$90 horizontal mechanics, separate from animation and palette definitions.</summary>
internal static class SamusHorizontalMotionDefinitions
{
    /// <summary>$90:9F55..A08C normal-air speed records end after damage boost.</summary>
    private const int AirRecordCount = 26;
    /// <summary>$90:A08D and A1DD water/lava records additionally include held and special movement.</summary>
    private const int LiquidRecordCount = 28;
    /// <summary>
    /// Resolves exact records in the three adjacent native tables. Address-based selection
    /// deliberately lets air movement bytes 26/27 read water rows 0/1. Addresses outside
    /// the authored records are not treated as additional mechanics definitions.
    /// </summary>
    internal static bool TryResolveIndexed(int address, out SpeedTableEntry entry)
    {
        int offset = address - (SamusMovementRomData.Banks.Movement | SamusMovementRomData.HorizontalMotion.NormalAirSpeedTable);
        int count = AirRecordCount + 2 * LiquidRecordCount;
        if (offset < 0 || offset >= count * SpeedTableEntry.ByteCount || offset % SpeedTableEntry.ByteCount != 0)
        {
            entry = default;
            return false;
        }
        int row = offset / SpeedTableEntry.ByteCount;
        bool air = row < AirRecordCount;
        bool water = false;
        ushort deceleration;
        if (air)
        {
            deceleration = 0x8000;
        }
        else
        {
            row -= AirRecordCount;
            water = row < LiquidRecordCount;
            if (!water) row -= LiquidRecordCount;
            deceleration = water ? (ushort)0x0800 : (ushort)0x4000;
        }
        uint maximum = (SamusMovementType)row switch
        {
            SamusMovementType.Running => air || water ? 0x2c000u : 0x1c000u,
            SamusMovementType.NormalJumping or SamusMovementType.PostureTransition or
                SamusMovementType.SpringBallInAir or SamusMovementType.Grappling => 0x14000u,
            SamusMovementType.SpinJumping or SamusMovementType.WallJumping => 0x16000u,
            SamusMovementType.MorphBallGround or SamusMovementType.SpringBallGround => air ? 0x34000u : 0x2c000u,
            SamusMovementType.Falling or SamusMovementType.UnusedGlitchBall => 0x10000u,
            SamusMovementType.MorphBallFalling or SamusMovementType.SpringBallFalling =>
                air ? 0x10000u : water ? 0x18000u : 0x16000u,
            SamusMovementType.UnusedGlitchBallAlternate or SamusMovementType.Unused0D => 0x20000u,
            SamusMovementType.Knockback or SamusMovementType.DraygonHeld or SamusMovementType.Special => 0x50000u,
            SamusMovementType.Moonwalking => 0x08000u,
            SamusMovementType.DamageBoost => air ? 0x50000u : 0x08000u,
            SamusMovementType.Standing or SamusMovementType.Crouching or SamusMovementType.Unused0B or
                SamusMovementType.Unused0C or SamusMovementType.TurningOnGround or SamusMovementType.RanIntoWall or
                SamusMovementType.TurningWhileJumping or SamusMovementType.TurningWhileFalling => 0,
            _ => throw new InvalidOperationException($"Undefined SamusMovementType {(SamusMovementType)row}."),
        };
        // All rows use the same authored acceleration except these named movement cases.
        uint acceleration = (SamusMovementType)row switch
        {
            SamusMovementType.Running => air ? 0x3000u : 0x0400u,
            SamusMovementType.UnusedGlitchBall or SamusMovementType.UnusedGlitchBallAlternate => 0x20000u,
            SamusMovementType.Knockback => 0x18000u,
            SamusMovementType.MorphBallGround or SamusMovementType.MorphBallFalling or
                SamusMovementType.SpringBallGround or SamusMovementType.SpringBallInAir or
                SamusMovementType.SpringBallFalling when !air => 0x0400u,
            SamusMovementType.Standing or SamusMovementType.NormalJumping or SamusMovementType.SpinJumping or
                SamusMovementType.MorphBallGround or SamusMovementType.Crouching or SamusMovementType.Falling or
                SamusMovementType.MorphBallFalling or SamusMovementType.Unused0B or SamusMovementType.Unused0C or
                SamusMovementType.Unused0D or SamusMovementType.TurningOnGround or
                SamusMovementType.PostureTransition or SamusMovementType.Moonwalking or
                SamusMovementType.SpringBallGround or SamusMovementType.SpringBallInAir or
                SamusMovementType.SpringBallFalling or SamusMovementType.WallJumping or
                SamusMovementType.RanIntoWall or SamusMovementType.Grappling or
                SamusMovementType.TurningWhileJumping or SamusMovementType.TurningWhileFalling or
                SamusMovementType.DamageBoost or SamusMovementType.DraygonHeld or SamusMovementType.Special => 0xc000u,
            _ => throw new InvalidOperationException($"Undefined SamusMovementType {(SamusMovementType)row}."),
        };
        entry = new((ushort)(acceleration >> 16), (ushort)acceleration,
            (ushort)(maximum >> 16), (ushort)maximum, 0, deceleration);
        return true;
    }

    /// <summary>$90:9F25 XAccelSpeeds_DiagonalBombJump: acceleration, maximum and deceleration whole/fraction pairs.</summary>
    private static readonly SpeedTableEntry DiagonalBombJump = new(0, 0x3000, 3, 0, 0, 0x0800);

    /// <summary>$90:9F31/$9F3D/$9F49 XAccelSpeeds_DisconnectGrappleInAir/InWater/InLavaAcid: three identical authored records.</summary>
    private static readonly SpeedTableEntry GrappleRelease = new(0, 0x3000, 15, 0, 0, 0x1000);

    /// <summary>
    /// Recognizes exact standalone entry addresses. Unknown/unaligned addresses are not
    /// clamped to a known record.
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
