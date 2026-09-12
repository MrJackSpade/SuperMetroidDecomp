namespace SuperMetroid.Core.Game;

/// <summary>Pinned bank-$90 horizontal mechanics, separate from animation and palette definitions.</summary>
internal static class SamusHorizontalMotionDefinitions
{
    /// <summary>$90:9F55 SamusXSpeedTable_Normal: 26 authored maximum-speed records, in movement-byte order.</summary>
    private static ReadOnlySpan<uint> AirMaximums =>
    [
        0, 0x2c000, 0x14000, 0x16000, 0x34000, 0, 0x10000, 0x10000,
        0x10000, 0x20000, 0x50000, 0, 0, 0x20000, 0, 0x14000,
        0x08000, 0x34000, 0x14000, 0x10000, 0x16000, 0, 0x14000, 0, 0, 0x50000,
    ];

    /// <summary>$90:A08D SamusXSpeedTable_InWater: 28 authored maximum-speed records.</summary>
    private static ReadOnlySpan<uint> WaterMaximums =>
    [
        0, 0x2c000, 0x14000, 0x16000, 0x2c000, 0, 0x10000, 0x10000,
        0x18000, 0x20000, 0x50000, 0, 0, 0x20000, 0, 0x14000,
        0x08000, 0x2c000, 0x14000, 0x18000, 0x16000, 0, 0x14000, 0,
        0, 0x08000, 0x50000, 0x50000,
    ];

    /// <summary>$90:A1DD SamusXSpeedTable_InAcidLava: 28 authored maximum-speed records.</summary>
    private static ReadOnlySpan<uint> LavaMaximums =>
    [
        0, 0x1c000, 0x14000, 0x16000, 0x2c000, 0, 0x10000, 0x10000,
        0x16000, 0x20000, 0x50000, 0, 0, 0x20000, 0, 0x14000,
        0x08000, 0x2c000, 0x14000, 0x16000, 0x16000, 0, 0x14000, 0,
        0, 0x08000, 0x50000, 0x50000,
    ];

    /// <summary>
    /// Resolves exact records in the three adjacent native tables. Address-based selection
    /// deliberately lets air movement bytes 26/27 read water rows 0/1. Higher reads outside
    /// the authored data are left to the caller's address space, not clamped or rejected.
    /// </summary>
    internal static bool TryResolveIndexed(int address, out SpeedTableEntry entry)
    {
        int offset = address - (SamusMovementRomData.Banks.Movement | SamusMovementRomData.HorizontalMotion.NormalAirSpeedTable);
        int count = AirMaximums.Length + WaterMaximums.Length + LavaMaximums.Length;
        if (offset < 0 || offset >= count * SpeedTableEntry.ByteCount || offset % SpeedTableEntry.ByteCount != 0)
        {
            entry = default;
            return false;
        }
        int row = offset / SpeedTableEntry.ByteCount;
        bool air = row < AirMaximums.Length;
        uint maximum;
        ushort deceleration;
        if (air)
        {
            maximum = AirMaximums[row];
            deceleration = 0x8000;
        }
        else
        {
            row -= AirMaximums.Length;
            bool water = row < WaterMaximums.Length;
            if (!water) row -= WaterMaximums.Length;
            maximum = water ? WaterMaximums[row] : LavaMaximums[row];
            deceleration = water ? (ushort)0x0800 : (ushort)0x4000;
        }
        // All rows use the same authored acceleration except these named movement cases.
        uint acceleration = (SamusMovementType)row switch
        {
            SamusMovementType.Running => air ? 0x3000u : 0x0400u,
            SamusMovementType.UnusedGlitchBall or SamusMovementType.UnusedGlitchBallAlternate => 0x20000u,
            SamusMovementType.Knockback => 0x18000u,
            SamusMovementType.MorphBallGround or SamusMovementType.MorphBallFalling or
                SamusMovementType.SpringBallGround or SamusMovementType.SpringBallInAir or
                SamusMovementType.SpringBallFalling when !air => 0x0400u,
            _ => 0xc000u,
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
