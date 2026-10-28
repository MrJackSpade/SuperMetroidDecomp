using SuperMetroid.AssetExtraction.CartridgeTables;
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
    internal static void VerifySamusProjectileRomData()
    {
        AssertEqual(12, SamusFiringTableDefinitions.CombinationCount,
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
            SamusSourceDefinitionsConstants.FiringTables.Value + SamusFiringTableDefinitions.DefaultXOrigins,
            SamusSourceDefinitionsConstants.FiringTables.Value + SamusFiringTableDefinitions.DefaultYOrigins,
            SamusSourceDefinitionsConstants.FiringTables.Value + SamusFiringTableDefinitions.RunningXOrigins,
            SamusSourceDefinitionsConstants.FiringTables.Value + SamusFiringTableDefinitions.RunningYOrigins,
            SamusSourceDefinitions.ChargeFlareDefaultXOffsets.Value,
            SamusSourceDefinitions.ChargeFlareDefaultYOffsets.Value,
            SamusSourceDefinitions.ChargeFlareRunningXOffsets.Value,
            SamusSourceDefinitions.ChargeFlareRunningYOffsets.Value,
        })
        {
            TouchRange(bus, originTable,
                SamusFiringTableDefinitions.DirectionCount * sizeof(ushort),
                $"projectile origin table ${originTable:X6}");
        }

        TouchRange(bus, SamusSourceDefinitionsConstants.FiringTables.Value + SamusFiringTableDefinitions.UnchargedCooldowns,
            SamusFiringTableDefinitions.ChargedRowOffset +
                SamusFiringTableDefinitions.CombinationCount,
            "uncharged beam cooldown rows");
        TouchRange(bus, SamusSourceDefinitionsConstants.FiringTables.Value + SamusFiringTableDefinitions.AutoFireCooldowns,
            SamusFiringTableDefinitions.CombinationCount, "beam auto-fire cooldowns");
        TouchRange(bus, SamusProjectileBeamTables.UnchargedSounds,
            SamusFiringTableDefinitions.CombinationCount * sizeof(ushort),
            "uncharged beam sounds");
        TouchRange(bus, SamusProjectileBeamTables.ChargedSounds,
            SamusFiringTableDefinitions.CombinationCount * sizeof(ushort),
            "charged beam sounds");
        TouchRange(bus, SamusSourceDefinitionsConstants.FiringTables.Value + SamusFiringTableDefinitions.BeamXAccelerations,
            SamusFiringTableDefinitions.DirectionCount * sizeof(ushort),
            "beam X accelerations");
        TouchRange(bus, SamusSourceDefinitionsConstants.FiringTables.Value + SamusFiringTableDefinitions.BeamYAccelerations,
            SamusFiringTableDefinitions.DirectionCount * sizeof(ushort),
            "beam Y accelerations");
        TouchRange(bus, SamusSourceDefinitions.TilePointers.Value,
            SamusFiringTableDefinitions.CombinationCount * sizeof(ushort),
            "beam tile pointers");
        TouchRange(bus, SamusSourceDefinitions.PalettePointers.Value,
            SamusFiringTableDefinitions.CombinationCount * sizeof(ushort),
            "beam palette pointers");

        foreach (int dataPointerTable in new[]
        {
            SamusProjectileBeamTables.UnchargedDataPointers,
            SamusProjectileBeamTables.ChargedDataPointers,
        })
        {
            TouchRange(bus, dataPointerTable,
                SamusFiringTableDefinitions.CombinationCount * sizeof(ushort),
                $"beam data-pointer table ${dataPointerTable:X6}");
            for (int beam = 0; beam < SamusFiringTableDefinitions.CombinationCount; beam++)
            {
                ushort dataPointer = ReadProjectileWord(
                    bus,
                    dataPointerTable + beam * sizeof(ushort));
                AssertTrue(dataPointer >= 0x8000,
                    $"beam {beam} data record remains in bank $93 ROM");
                int dataAddress = SamusSourceDefinitions.ProjectileDataBank.Value | dataPointer;
                TouchRange(bus, dataAddress, SamusProjectileBeamTables.DataRecordByteCount,
                    $"beam {beam} data record");

                for (int direction = 0;
                    direction < SamusFiringTableDefinitions.DirectionCount;
                    direction++)
                {
                    ushort instructionPointer = ReadProjectileWord(
                        bus,
                        dataAddress + sizeof(ushort) + direction * sizeof(ushort));
                    AssertTrue(instructionPointer >= 0x8000,
                        $"beam {beam} direction {direction} instruction remains in bank $93 ROM");
                    TouchRange(
                        bus,
                        SamusSourceDefinitions.ProjectileDataBank.Value | instructionPointer,
                        SamusProjectileBeamTables.InstructionRecordByteCount,
                        $"beam {beam} direction {direction} instruction record");
                }
            }
        }

        TouchRange(bus, SamusProjectileNonBeamTables.DataPointers,
            (SamusProjectileNonBeamTables.SuperMissileLinkDataPointers -
                SamusProjectileNonBeamTables.DataPointers),
            "non-beam data-pointer table");
        TouchRange(bus, SamusSourceDefinitionsConstants.FiringTables.Value + SamusFiringTableDefinitions.MissileAccelerations,
            SamusFiringTableDefinitions.DirectionCount *
                SamusProjectileNonBeamTables.AccelerationRecordByteCount,
            "missile acceleration records");
        TouchRange(bus, SamusSourceDefinitionsConstants.FiringTables.Value + SamusFiringTableDefinitions.SuperMissileAccelerations,
            SamusFiringTableDefinitions.DirectionCount *
                SamusProjectileNonBeamTables.AccelerationRecordByteCount,
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
                SamusFiringTableDefinitions.CombinationCount * sizeof(ushort),
                $"trail offset-family table ${offsets:X6}");
        }

        TouchRange(bus, SamusProjectileRomDataCollision.NonSquareSlopeDefinitions,
            32 * 16, "projectile non-square slope profiles");
        TouchRange(bus, SamusProjectileRomDataCollision.SquareSlopeDefinitions,
            4, "projectile square-slope quadrants");
        bus.ReadCartridgeByte(SamusSourceDefinitions.MovementBank.Value |
            ProjectileTrailNative.InstructionAddress(ProjectileTrailInstruction.MoveLeftDown));
        bus.ReadCartridgeByte(SamusSourceDefinitions.MovementBank.Value |
            ProjectileTrailNative.InstructionAddress(ProjectileTrailInstruction.MoveRightDown));
        bus.ReadCartridgeByte(SamusSourceDefinitions.MovementBank.Value |
            ProjectileTrailNative.InstructionAddress(ProjectileTrailInstruction.MoveLeftUp));
        bus.ReadCartridgeByte(SamusSourceDefinitions.ProjectileDataBank.Value |
            (ushort)SamusProjectileInstruction.Delete);
        bus.ReadCartridgeByte(SamusSourceDefinitions.ProjectileDataBank.Value |
            (ushort)SamusProjectileInstruction.GoTo);

        Console.WriteLine(
            "  Samus projectile ROM data: origins, 12 beam combinations, 200 directional " +
            "instructions, missiles, explosions, trails, and slopes are in range.");
    }

    /// <summary>Reads a projectile table word from the retail cartridge as the audit's address oracle.</summary>
    internal static ushort ReadProjectileWord(SuperMetroidAddressSpace bus, int address) =>
        CartridgeImportSource.Require(bus).ReadCartridgeWord(address);
}
