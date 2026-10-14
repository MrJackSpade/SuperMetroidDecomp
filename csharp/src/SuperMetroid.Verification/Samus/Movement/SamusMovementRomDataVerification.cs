using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Verification.Samus.Movement;

/// <summary>Checks native address ranges for Samus pose, speed, liquid, and slope movement tables.</summary>
internal static class SamusMovementRomDataVerification
{
    /// <summary>
    /// Walks the named movement tables through their last retail entry so an address typo,
    /// truncated range, wrong record width, or cross-bank pointer fails in verification.
    /// </summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Samus movement ROM data")]
    internal static void VerifySamusMovementRomData()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine("  Samus movement ROM data: retail range audit skipped (private ROM absent).");
            return;
        }

        SuperMetroidAddressSpace bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        int poseCount = Enum.GetValues<SamusPoseId>()
            .Max(pose => (byte)pose) + 1;

        TouchRange(
            bus,
            SamusMovementRomData.Poses.Definitions,
            poseCount * SamusMovementRomData.Poses.DefinitionByteCount,
            "pose definitions");
        TouchRange(
            bus,
            SamusMovementRomData.Poses.AnimationDelayListPointers,
            poseCount * sizeof(ushort),
            "animation-delay pointers");
        TouchRange(
            bus,
            SamusMovementRomData.Poses.TransitionListPointers,
            poseCount * sizeof(ushort),
            "pose-transition pointers");

        // Both pointer tables contain same-bank addresses. Validate every named pose rather
        // than merely proving that the pointer-table bytes themselves happen to be mapped.
        for (int pose = 0; pose < poseCount; pose++)
        {
            ushort animationPointer = CartridgeImportSource.Require(bus).ReadCartridgeWord((SamusMovementRomData.Poses.AnimationDelayListPointers + pose * sizeof(ushort)));
            ushort transitionPointer = CartridgeImportSource.Require(bus).ReadCartridgeWord((SamusMovementRomData.Poses.TransitionListPointers + pose * sizeof(ushort)));
            AssertTrue(animationPointer >= 0x8000,
                $"pose ${pose:X2} animation list remains in bank $91 ROM");
            AssertTrue(transitionPointer >= 0x8000,
                $"pose ${pose:X2} transition list remains in bank $91 ROM");
            bus.ReadCartridgeByte(SamusMovementRomData.Banks.Pose | animationPointer);
            bus.ReadCartridgeByte(SamusMovementRomData.Banks.Pose | transitionPointer);
        }

        int movementTypeCount = (byte)SamusMovementType.Special + 1;
        foreach (ushort table in new[]
        {
            SamusMovementRomData.HorizontalMotion.NormalAirSpeedTable,
            SamusMovementRomData.HorizontalMotion.WaterSpeedTable,
            SamusMovementRomData.HorizontalMotion.LavaAcidSpeedTable,
        })
        {
            TouchRange(
                bus,
                SamusMovementRomData.Banks.Movement | table,
                movementTypeCount * SpeedTableEntry.ByteCount,
                $"horizontal speed table ${table:X4}");
        }

        int liquidTableByteCount = 3 * sizeof(ushort);
        foreach (int table in new[]
        {
            SamusMovementRomDataVerticalMotionConstants.NormalJumpSpeeds,
            SamusMovementRomDataVerticalMotionConstants.NormalJumpSubspeeds,
            SamusMovementRomDataVerticalMotionConstants.HiJumpSpeeds,
            SamusMovementRomDataVerticalMotionConstants.HiJumpSubspeeds,
            SamusMovementRomDataVerticalMotionConstants.WallJumpSpeeds,
            SamusMovementRomDataVerticalMotionConstants.WallJumpSubspeeds,
            SamusMovementRomDataVerticalMotionConstants.HiWallJumpSpeeds,
            SamusMovementRomDataVerticalMotionConstants.HiWallJumpSubspeeds,
            SamusMovementRomDataVerticalMotionConstants.KnockbackSpeeds,
            SamusMovementRomDataVerticalMotionConstants.KnockbackSubspeeds,
            SamusMovementRomDataVerticalMotionConstants.BombJumpSpeeds,
            SamusMovementRomDataVerticalMotionConstants.BombJumpSubspeeds,
            SamusMovementRomDataVerticalMotionConstants.GravitySubaccelerations,
            SamusMovementRomDataVerticalMotionConstants.GravityAccelerations,
        })
        {
            TouchRange(bus, table, liquidTableByteCount, $"liquid-indexed table ${table:X6}");
        }

        foreach (int speedRecord in new[]
        {
            (int)SamusStandaloneSpeedRecord.DiagonalBombJump,
            (int)SamusStandaloneSpeedRecord.GrappleReleaseAir,
            (int)SamusStandaloneSpeedRecord.GrappleReleaseWater,
            (int)SamusStandaloneSpeedRecord.GrappleReleaseLavaAcid,
        })
        {
            TouchRange(bus, speedRecord, SpeedTableEntry.ByteCount,
                $"standalone speed record ${speedRecord:X6}");
        }

        TouchRange(bus, SamusMovementRomDataSlopes.HorizontalMultipliers,
            32 * 2 * sizeof(ushort), "non-square slope multipliers");
        TouchRange(bus, SamusMovementRomDataSlopes.AlignmentHeights,
            32 * 16, "non-square slope height profiles");

        Console.WriteLine(
            $"  Samus movement ROM data: {poseCount} poses, {movementTypeCount} movement " +
            "records, liquid tables, effects, and 32 slope profiles are in range.");
    }

    /// <summary>Checks both ends of a positive-length table range stay mapped within one SNES bank.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle reads at both ends of a Samus movement data range")]
    internal static void TouchRange(
        SuperMetroidAddressSpace bus,
        int address,
        int byteCount,
        string description)
    {
        AssertTrue(byteCount > 0, $"{description} has a positive byte count");
        int finalAddress = checked(address + byteCount - 1);
        AssertEqual(SnesAddressMath.BankNumber(address), SnesAddressMath.BankNumber(finalAddress),
            $"{description} remains inside its native bank");
        bus.ReadCartridgeByte(address);
        bus.ReadCartridgeByte(finalAddress);
    }
}
