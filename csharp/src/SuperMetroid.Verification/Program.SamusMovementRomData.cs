using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Walks the named movement tables through their last retail entry so an address typo,
    /// truncated range, wrong record width, or cross-bank pointer fails in verification.
    /// </summary>
    static void VerifySamusMovementRomData()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine("  Samus movement ROM data: retail range audit skipped (private ROM absent).");
            return;
        }

        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        int poseCount = typeof(SamusPoseIds)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.IsLiteral && field.FieldType == typeof(byte))
            .Select(field => (byte)field.GetRawConstantValue()!)
            .Max() + 1;

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
            ushort animationPointer = ReadMovementWord(
                bus,
                SamusMovementRomData.Poses.AnimationDelayListPointers + pose * sizeof(ushort));
            ushort transitionPointer = ReadMovementWord(
                bus,
                SamusMovementRomData.Poses.TransitionListPointers + pose * sizeof(ushort));
            AssertTrue(animationPointer >= 0x8000,
                $"pose ${pose:X2} animation list remains in bank $91 ROM");
            AssertTrue(transitionPointer >= 0x8000,
                $"pose ${pose:X2} transition list remains in bank $91 ROM");
            bus.ReadByte(SamusMovementRomData.Banks.Pose | animationPointer);
            bus.ReadByte(SamusMovementRomData.Banks.Pose | transitionPointer);
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
            SamusMovementRomData.VerticalMotion.NormalJumpSpeeds,
            SamusMovementRomData.VerticalMotion.NormalJumpSubspeeds,
            SamusMovementRomData.VerticalMotion.HiJumpSpeeds,
            SamusMovementRomData.VerticalMotion.HiJumpSubspeeds,
            SamusMovementRomData.VerticalMotion.WallJumpSpeeds,
            SamusMovementRomData.VerticalMotion.WallJumpSubspeeds,
            SamusMovementRomData.VerticalMotion.HiWallJumpSpeeds,
            SamusMovementRomData.VerticalMotion.HiWallJumpSubspeeds,
            SamusMovementRomData.VerticalMotion.KnockbackSpeeds,
            SamusMovementRomData.VerticalMotion.KnockbackSubspeeds,
            SamusMovementRomData.VerticalMotion.BombJumpSpeeds,
            SamusMovementRomData.VerticalMotion.BombJumpSubspeeds,
            SamusMovementRomData.VerticalMotion.GravitySubaccelerations,
            SamusMovementRomData.VerticalMotion.GravityAccelerations,
        })
        {
            TouchRange(bus, table, liquidTableByteCount, $"liquid-indexed table ${table:X6}");
        }

        foreach (int speedRecord in new[]
        {
            SamusMovementRomData.VerticalMotion.DiagonalBombJumpHorizontalSpeed,
            SamusMovementRomData.VerticalMotion.GrappleReleaseAirSpeed,
            SamusMovementRomData.VerticalMotion.GrappleReleaseWaterSpeed,
            SamusMovementRomData.VerticalMotion.GrappleReleaseLavaAcidSpeed,
        })
        {
            TouchRange(bus, speedRecord, SpeedTableEntry.ByteCount,
                $"standalone speed record ${speedRecord:X6}");
        }

        TouchRange(bus, SamusMovementRomData.Environment.WaterSplashTypes,
            movementTypeCount, "water-splash movement-type table");
        TouchRange(bus, SamusMovementRomData.Environment.RunningFootstepFrames,
            10, "running footstep frames");
        TouchRange(bus, SamusMovementRomData.Environment.CrateriaFootstepTypes,
            64, "Crateria room footstep table");
        TouchRange(bus, SamusMovementRomData.Environment.AtmosphericAnimationFrameCounts,
            16, "atmospheric frame-count table");
        TouchRange(bus, SamusMovementRomData.Slopes.HorizontalMultipliers,
            32 * 2 * sizeof(ushort), "non-square slope multipliers");
        TouchRange(bus, SamusMovementRomData.Slopes.AlignmentHeights,
            32 * 16, "non-square slope height profiles");

        Console.WriteLine(
            $"  Samus movement ROM data: {poseCount} poses, {movementTypeCount} movement " +
            "records, liquid tables, effects, and 32 slope profiles are in range.");
    }

    private static void TouchRange(
        SuperMetroidAddressSpace bus,
        int address,
        int byteCount,
        string description)
    {
        AssertTrue(byteCount > 0, $"{description} has a positive byte count");
        int finalAddress = checked(address + byteCount - 1);
        AssertEqual(address >> 16, finalAddress >> 16,
            $"{description} remains inside its native bank");
        bus.ReadByte(address);
        bus.ReadByte(finalAddress);
    }

    private static ushort ReadMovementWord(SuperMetroidAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}
