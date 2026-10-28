using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Samus.Movement;

/// <summary>Checks native address ranges for Samus pose, speed, liquid, and slope movement tables.</summary>
internal static class SamusMovementDefinitionsVerification
{
    /// <summary>
    /// Walks the named movement tables through their last retail entry so an address typo,
    /// truncated range, wrong record width, or cross-bank pointer fails in verification.
    /// </summary>
    internal static void VerifySamusMovementRomData()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine("  Samus movement ROM data: retail range audit skipped (private ROM absent).");
            return;
        }

        SuperMetroidAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        int poseCount = SamusBodyArtworkCatalog.PoseCount;

        TouchRange(
            bus,
            SamusSourceDefinitions.PoseDefinitions.Value,
            poseCount * SamusSourceDefinitions.PoseDefinitionByteCount,
            "pose definitions");
        TouchRange(
            bus,
            SamusMovementDefinitions.Poses.AnimationDelayListPointers,
            poseCount * sizeof(ushort),
            "animation-delay pointers");
        TouchRange(
            bus,
            SamusMovementDefinitions.Poses.TransitionListPointers,
            poseCount * sizeof(ushort),
            "pose-transition pointers");

        // Both pointer tables contain same-bank addresses. Validate every named pose rather
        // than merely proving that the pointer-table bytes themselves happen to be mapped.
        for (int pose = 0; pose < poseCount; pose++)
        {
            ushort animationPointer = CartridgeImportSource.Require(bus).ReadCartridgeWord((SamusMovementDefinitions.Poses.AnimationDelayListPointers + pose * sizeof(ushort)));
            ushort transitionPointer = CartridgeImportSource.Require(bus).ReadCartridgeWord((SamusMovementDefinitions.Poses.TransitionListPointers + pose * sizeof(ushort)));
            AssertTrue(animationPointer >= 0x8000,
                $"pose ${pose:X2} animation list remains in bank $91 ROM");
            AssertTrue(transitionPointer >= 0x8000,
                $"pose ${pose:X2} transition list remains in bank $91 ROM");
            bus.ReadCartridgeByte(SamusSourceDefinitions.PoseBank.Value | animationPointer);
            bus.ReadCartridgeByte(SamusSourceDefinitions.PoseBank.Value | transitionPointer);
        }

        int movementTypeCount = (byte)SamusMovementType.Special + 1;
        foreach (ushort table in new[]
        {
            SamusMovementDefinitions.HorizontalMotion.NormalAirSpeedTable,
            SamusMovementDefinitions.HorizontalMotion.WaterSpeedTable,
            SamusMovementDefinitions.HorizontalMotion.LavaAcidSpeedTable,
        })
        {
            TouchRange(
                bus,
                SamusSourceDefinitions.MovementBank.Value | table,
                movementTypeCount * SpeedTableEntryConstants.ByteCount,
                $"horizontal speed table ${table:X4}");
        }

        int liquidTableByteCount = 3 * sizeof(ushort);
        foreach (int table in new[]
        {
            SamusMovementDefinitionsVerticalMotionConstants.NormalJumpSpeeds,
            SamusMovementDefinitionsVerticalMotionConstants.NormalJumpSubspeeds,
            SamusMovementDefinitionsVerticalMotionConstants.HiJumpSpeeds,
            SamusMovementDefinitionsVerticalMotionConstants.HiJumpSubspeeds,
            SamusMovementDefinitionsVerticalMotionConstants.WallJumpSpeeds,
            SamusMovementDefinitionsVerticalMotionConstants.WallJumpSubspeeds,
            SamusMovementDefinitionsVerticalMotionConstants.HiWallJumpSpeeds,
            SamusMovementDefinitionsVerticalMotionConstants.HiWallJumpSubspeeds,
            SamusMovementDefinitionsVerticalMotionConstants.KnockbackSpeeds,
            SamusMovementDefinitionsVerticalMotionConstants.KnockbackSubspeeds,
            SamusMovementDefinitionsVerticalMotionConstants.BombJumpSpeeds,
            SamusMovementDefinitionsVerticalMotionConstants.BombJumpSubspeeds,
            SamusMovementDefinitionsVerticalMotionConstants.GravitySubaccelerations,
            SamusMovementDefinitionsVerticalMotionConstants.GravityAccelerations,
        })
        {
            TouchRange(bus, table, liquidTableByteCount, $"liquid-indexed table ${table:X6}");
        }

        foreach (int speedRecord in Enum.GetValues<SamusStandaloneSpeedRecord>().Select(SamusHorizontalMotionDefinitions.StandaloneAddressOf))
        {
            TouchRange(bus, speedRecord, SpeedTableEntryConstants.ByteCount,
                $"standalone speed record ${speedRecord:X6}");
        }

        TouchRange(bus, SamusMovementDefinitionsSlopes.HorizontalMultipliers,
            32 * 2 * sizeof(ushort), "non-square slope multipliers");
        TouchRange(bus, SamusMovementDefinitionsSlopes.AlignmentHeights,
            32 * 16, "non-square slope height profiles");

        Console.WriteLine(
            $"  Samus movement ROM data: {poseCount} poses, {movementTypeCount} movement " +
            "records, liquid tables, effects, and 32 slope profiles are in range.");
    }

    /// <summary>Checks both ends of a positive-length table range stay mapped within one SNES bank.</summary>
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
