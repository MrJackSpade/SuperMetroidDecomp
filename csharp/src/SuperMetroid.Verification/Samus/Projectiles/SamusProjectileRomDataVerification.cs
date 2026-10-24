using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Samus.Projectiles.ProjectileSoundRoutingVerification;
using static SuperMetroid.Verification.Samus.Movement.SamusMovementDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>Audits retail addresses and pointer-table strides for Samus projectile origins, beams, missiles, trails, and slopes.</summary>
internal static class SamusProjectileRomDataVerification
{
    /// <summary>
    /// Validates projectile table shapes plus every beam data-to-instruction indirection
    /// against the retail cartridge, catching both wrong bases and wrong record strides.
    /// </summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Samus projectile ROM data")]
    internal static void VerifySamusProjectileRomData()
    {
        AssertEqual(12, SamusProjectileRomData.Beams.CombinationCount,
            "retail beam-combination table count");
        AssertEqual(39, SamusProjectileRomDataTrailsConstants.InstructionPointerCount,
            "retail trail pointer count");

        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine("  Samus projectile ROM data: retail range audit skipped (private ROM absent).");
            return;
        }

        SuperMetroidAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        Suite(nameof(VerifyProjectileSoundRoutingDefinitions), () => VerifyProjectileSoundRoutingDefinitions(bus));
        foreach (int originTable in new[]
        {
            SamusProjectileRomData.Origins.DefaultX,
            SamusProjectileRomData.Origins.DefaultY,
            SamusProjectileRomData.Origins.RunningX,
            SamusProjectileRomData.Origins.RunningY,
            SamusProjectileRomData.Origins.FlareDefaultX,
            SamusProjectileRomData.Origins.FlareDefaultY,
            SamusProjectileRomData.Origins.FlareRunningX,
            SamusProjectileRomData.Origins.FlareRunningY,
        })
        {
            TouchRange(bus, originTable,
                SamusProjectileRomData.Origins.DirectionCount * sizeof(ushort),
                $"projectile origin table ${originTable:X6}");
        }

        TouchRange(bus, SamusProjectileRomData.Beams.UnchargedCooldowns,
            SamusProjectileRomData.Beams.ChargedRowOffset +
                SamusProjectileRomData.Beams.CombinationCount,
            "uncharged beam cooldown rows");
        TouchRange(bus, SamusProjectileRomData.Beams.AutoFireCooldowns,
            SamusProjectileRomData.Beams.CombinationCount, "beam auto-fire cooldowns");
        TouchRange(bus, SamusProjectileRomData.Beams.UnchargedSounds,
            SamusProjectileRomData.Beams.CombinationCount * sizeof(ushort),
            "uncharged beam sounds");
        TouchRange(bus, SamusProjectileRomData.Beams.ChargedSounds,
            SamusProjectileRomData.Beams.CombinationCount * sizeof(ushort),
            "charged beam sounds");
        TouchRange(bus, SamusProjectileRomData.Beams.XAccelerations,
            SamusProjectileRomData.Origins.DirectionCount * sizeof(ushort),
            "beam X accelerations");
        TouchRange(bus, SamusProjectileRomData.Beams.YAccelerations,
            SamusProjectileRomData.Origins.DirectionCount * sizeof(ushort),
            "beam Y accelerations");
        TouchRange(bus, SamusCartridgeTables.TilePointers,
            SamusProjectileRomData.Beams.CombinationCount * sizeof(ushort),
            "beam tile pointers");
        TouchRange(bus, SamusCartridgeTables.PalettePointers,
            SamusProjectileRomData.Beams.CombinationCount * sizeof(ushort),
            "beam palette pointers");

        foreach (int dataPointerTable in new[]
        {
            SamusProjectileRomData.Beams.UnchargedDataPointers,
            SamusProjectileRomData.Beams.ChargedDataPointers,
        })
        {
            TouchRange(bus, dataPointerTable,
                SamusProjectileRomData.Beams.CombinationCount * sizeof(ushort),
                $"beam data-pointer table ${dataPointerTable:X6}");
            for (int beam = 0; beam < SamusProjectileRomData.Beams.CombinationCount; beam++)
            {
                ushort dataPointer = ReadProjectileWord(
                    bus,
                    dataPointerTable + beam * sizeof(ushort));
                AssertTrue(dataPointer >= 0x8000,
                    $"beam {beam} data record remains in bank $93 ROM");
                int dataAddress = SamusProjectileRomData.Banks.Projectile | dataPointer;
                TouchRange(bus, dataAddress, SamusProjectileRomData.Beams.DataRecordByteCount,
                    $"beam {beam} data record");

                for (int direction = 0;
                    direction < SamusProjectileRomData.Origins.DirectionCount;
                    direction++)
                {
                    ushort instructionPointer = ReadProjectileWord(
                        bus,
                        dataAddress + sizeof(ushort) + direction * sizeof(ushort));
                    AssertTrue(instructionPointer >= 0x8000,
                        $"beam {beam} direction {direction} instruction remains in bank $93 ROM");
                    TouchRange(
                        bus,
                        SamusProjectileRomData.Banks.Projectile | instructionPointer,
                        SamusProjectileRomData.Beams.InstructionRecordByteCount,
                        $"beam {beam} direction {direction} instruction record");
                }
            }
        }

        TouchRange(bus, SamusProjectileRomData.NonBeam.DataPointers,
            (SamusProjectileRomData.NonBeam.SuperMissileLinkDataPointers -
                SamusProjectileRomData.NonBeam.DataPointers),
            "non-beam data-pointer table");
        TouchRange(bus, SamusProjectileRomData.NonBeam.MissileAccelerations,
            SamusProjectileRomData.Origins.DirectionCount *
                SamusProjectileRomData.NonBeam.AccelerationRecordByteCount,
            "missile acceleration records");
        TouchRange(bus, SamusProjectileRomData.NonBeam.SuperMissileAccelerations,
            SamusProjectileRomData.Origins.DirectionCount *
                SamusProjectileRomData.NonBeam.AccelerationRecordByteCount,
            "Super Missile acceleration records");

        TouchRange(bus, SamusProjectileRomDataTrailsConstants.LeftInstructionPointers,
            SamusProjectileRomDataTrailsConstants.InstructionPointerCount * sizeof(ushort),
            "left trail instruction pointers");
        TouchRange(bus, SamusProjectileRomDataTrailsConstants.RightInstructionPointers,
            SamusProjectileRomDataTrailsConstants.InstructionPointerCount * sizeof(ushort),
            "right trail instruction pointers");
        foreach (int offsets in new[]
        {
            SamusProjectileRomDataTrailsConstants.UnchargedOffsetFamilies,
            SamusProjectileRomDataTrailsConstants.ChargedOffsetFamilies,
            SamusProjectileRomDataTrailsConstants.SpazerSbaOffsetFamilies,
        })
        {
            TouchRange(bus, offsets,
                SamusProjectileRomData.Beams.CombinationCount * sizeof(ushort),
                $"trail offset-family table ${offsets:X6}");
        }

        TouchRange(bus, SamusProjectileRomDataCollision.NonSquareSlopeDefinitions,
            32 * 16, "projectile non-square slope profiles");
        TouchRange(bus, SamusProjectileRomDataCollision.SquareSlopeDefinitions,
            4, "projectile square-slope quadrants");
        bus.ReadCartridgeByte(SamusProjectileRomData.Banks.Movement |
            ProjectileTrailNative.InstructionAddress(ProjectileTrailInstruction.MoveLeftDown));
        bus.ReadCartridgeByte(SamusProjectileRomData.Banks.Movement |
            ProjectileTrailNative.InstructionAddress(ProjectileTrailInstruction.MoveRightDown));
        bus.ReadCartridgeByte(SamusProjectileRomData.Banks.Movement |
            ProjectileTrailNative.InstructionAddress(ProjectileTrailInstruction.MoveLeftUp));
        bus.ReadCartridgeByte(SamusProjectileRomData.Banks.Projectile |
            (ushort)SamusProjectileInstruction.Delete);
        bus.ReadCartridgeByte(SamusProjectileRomData.Banks.Projectile |
            (ushort)SamusProjectileInstruction.GoTo);

        Console.WriteLine(
            "  Samus projectile ROM data: origins, 12 beam combinations, 200 directional " +
            "instructions, missiles, explosions, trails, and slopes are in range.");
    }

    /// <summary>Reads a projectile table word from the retail cartridge as the audit's address oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Samus projectile data words")]
    internal static ushort ReadProjectileWord(SuperMetroidAddressSpace bus, int address) =>
        CartridgeImportSource.Require(bus).ReadCartridgeWord(address);
}
