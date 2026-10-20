using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.SpacePirates;
using SuperMetroid.Verification.Harness;

/// <summary>Checks compiled Pirate laser and claw projectile streams against retail words and live producers.</summary>
internal static class SpacePirateProjectileInstructionProgramDefinitionsVerification
{
    /// <summary>Loads retail data for the Pirate projectile instruction check.</summary>
    internal static void VerifySpacePirateProjectileInstructionProgramDefinitions() =>
        Suite(nameof(VerifySpacePirateProjectileInstructionProgramDefinitions), () => VerifySpacePirateProjectileInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));

    /// <summary>Executes both laser and claw facings through callbacks, loops, and deletion while checking migrated word ownership.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Space Pirate projectile instruction program definitions")]
    internal static void VerifySpacePirateProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < SpacePirateProjectileInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                SpacePirateProjectileInstructionProgramDefinitionsTooling.MechanicsWord(index);
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | definition.Address);
            AssertEqual(definition.Value, native,
                $"Space Pirate projectile mechanics word $86:{definition.Address:X4}");
        }

        var spriteArtwork = InstalledFixtures.EnemyTiles.ProjectileSpritemaps
            ?? throw new InvalidDataException("Projectile fixture requires installed sprites.");
        var executedOperands = new HashSet<ushort>();
        var guard = NewSpacePirateProjectileInstructionReadGuard(rom);
        Func<EnemyProjectileAnimation, ushort?> nativePointers =
            SpacePirateProjectileAnimationsVerification.Family.NativePointers(CartridgeImportSource.Require(rom));

        foreach ((bool movingRight, ushort program, ushort loop,
                  EnemyProjectilePreInstruction movement, ushort expectedX) in new[]
                 {
                     (false,
                         SpacePirateProjectileInstructionProgramDefinitions.LaserLeft,
                         SpacePirateProjectileInstructionProgramToolingDefinitions.LaserLeftLoop,
                         EnemyProjectilePreInstruction.Pirate_MotherBrain_Laser_Left,
                         (ushort)124),
                     (true,
                         SpacePirateProjectileInstructionProgramToolingDefinitions.LaserRight,
                         SpacePirateProjectileInstructionProgramToolingDefinitions.LaserRightLoop,
                         EnemyProjectilePreInstruction.Pirate_MotherBrain_Laser_Right,
                         (ushort)132),
                 })
        {
            RoomEnemySystem enemies = NewSystem();
            RoomEnemySlot source = enemies.SlotTable.Slots[0];
            source.Position = source.Position.WithWhole(new(128, 96));
            source.SetSpawnParameter1(0);
            RoomEnemyProjectileSlot laser = enemies.WalkingSpacePirateBehavior.SpawnPirateMotherBrainLaser(source, new Position16<RoomPixels>(128, 96), movingRight) ?? throw new InvalidOperationException("Verification requires enemies.WalkingSpacePirateBehavior.SpawnPirateMotherBrainLaser(source, new Position16<R....");
            AssertEqual(program, NativePointer(laser),
                "real Pirate/Mother Brain laser producer selects its facing program");
            AssertEqual(EnemyProjectilePreInstruction.RTS_86A05B, laser.PreInstruction,
                "laser initializer keeps muzzle frames stationary");
            RunForcedTicks(enemies, laser, 3);
            AssertEqual((ushort)128, laser.Position.X.Whole,
                "three laser muzzle frames remain stationary");
            RunForcedTick(enemies, laser);
            AssertEqual(movement, laser.PreInstruction,
                "laser program installs its facing movement callback");
            AssertEqual(expectedX, laser.Position.X.Whole,
                "laser callback executes immediately on installation");
            RunForcedTicks(enemies, laser, 10);
            AssertEqual(unchecked((ushort)(loop + 4)), NativePointer(laser),
                "laser completes its authored frames and loops");
        }

        foreach ((ushort direction, ushort program, ushort loop, EnemyProjectilePreInstruction movement) in new[]
                 {
                     ((ushort)0,
                         SpacePirateProjectileInstructionProgramToolingDefinitions.ClawLeft,
                         SpacePirateProjectileInstructionProgramToolingDefinitions.ClawLeftLoop,
                         EnemyProjectilePreInstruction.PirateClaw_Left),
                     ((ushort)1,
                         SpacePirateProjectileInstructionProgramToolingDefinitions.ClawRight,
                         SpacePirateProjectileInstructionProgramToolingDefinitions.ClawRightLoop,
                         EnemyProjectilePreInstruction.PirateClaw_Right),
                 })
        {
            RoomEnemySystem enemies = NewSystem();
            RoomEnemySlot source = enemies.SlotTable.Slots[0];
            source.Position = source.Position.WithWhole(new(128, 96));
            var state = new NinjaSpacePirateEnemyState();
            enemies.NinjaSpacePirateBehavior.SpawnNinjaPirateClaw(source, state, throwsRight: direction != 0, Offset16<RoomPixels>.Zero);
            RoomEnemyProjectileSlot claw = enemies.Projectiles.EnemyProjectiles.Single(
                projectile => projectile.Kind == RoomEnemyProjectileKind.PirateClaw);
            AssertEqual(program, NativePointer(claw),
                "real Ninja Pirate claw producer selects its facing program");
            RunForcedTick(enemies, claw);
            AssertEqual(movement, claw.PreInstruction,
                "claw program installs its facing movement callback");
            RunForcedTicks(enemies, claw, 8);
            AssertEqual(unchecked((ushort)(loop + 4)), NativePointer(claw),
                "claw completes all eight authored frames and loops");
        }

        RoomEnemySystem deleteSystem = NewSystem();
        RoomEnemySlot deleteSource = deleteSystem.SlotTable.Slots[0];
        var deleteState = new NinjaSpacePirateEnemyState();
        deleteSystem.NinjaSpacePirateBehavior.SpawnNinjaPirateClaw(deleteSource, deleteState, throwsRight: false, Offset16<RoomPixels>.Zero);
        RoomEnemyProjectileSlot deleting = deleteSystem.Projectiles.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.PirateClaw);
        deleting.StartAnimation(EnemyProjectileDeleteAnimation.Instance);
        RunForcedTick(deleteSystem, deleting);
        AssertTrue(!deleting.IsActive,
            "Space Pirate projectile shot reaction reaches shared compiled deletion");

        AssertEqual(0, guard.ObservedWords.Count,
            "SpacePirateProjectile execution performs no live spritemap operand reads");
        AssertEqual(SpacePirateProjectileInstructionProgramDefinitionsTooling.PresentationWordCount,
            executedOperands.Count, "SpacePirateProjectile executes every native visual operand");
        for (int index = 0; index < SpacePirateProjectileInstructionProgramDefinitionsTooling.PresentationWordCount; index++)
        {
            ushort address = SpacePirateProjectileInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"SpacePirateProjectile executes native presentation operand {address:X4}");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0x86, address, out ushort selector),
                "SpacePirateProjectile has a compiled visual selector");
            AssertEqual(ReadVerificationWord(rom, 0x860000 | address), selector,
                "SpacePirateProjectile compiled selector matches the cartridge");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids private and shared Space Pirate mechanics bytes");

        Console.WriteLine(
            "Space Pirate projectile instruction mechanics: fifty-eight compiled words, " +
            "both real producers/facings, immediate laser motion, claw loops, shared " +
            "deletion, and forty-two installed sprite frames match native OAM without operand reads.");

        ushort NativePointer(RoomEnemyProjectileSlot projectile) =>
            nativePointers(projectile.Animation ?? throw new InvalidOperationException("A laser or claw runs a typed animation."))
                ?? throw new InvalidOperationException("The laser or claw runs another family's animation.");

        RoomEnemySystem NewSystem()
        {
            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
            return enemies;
        }

        void RunForcedTick(
            RoomEnemySystem enemies,
            RoomEnemyProjectileSlot projectile)
        {
            projectile.InstructionTimer = NativeCountdown.Start(1);
            enemies.Projectiles.ProcessEnemyProjectileInstructions(projectile, new SamusState(), Position16<RoomPixels>.Zero);
            // The shown frame's operand sits just before the instruction the native pointer stands on.
            VerifyExecutedProjectileFrame(rom, projectile, spriteArtwork, executedOperands,
                projectile.IsActive ? unchecked((ushort)(NativePointer(projectile) - 2)) : null);
        }

        void RunForcedTicks(
            RoomEnemySystem enemies,
            RoomEnemyProjectileSlot projectile,
            int count)
        {
            for (int tick = 0; tick < count; tick++)
                RunForcedTick(enemies, projectile);
        }
    }

    /// <summary>Rejects compiled projectile mechanics reads and records presentation operands used by production.</summary>
    private static CartridgeReadGuard NewSpacePirateProjectileInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(SpacePirateProjectileInstructionProgramDefinitionsToolingConstants.IsCompiledMechanicsByte,
                address => $"Production read compiled Space Pirate mechanics byte ${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase, SpacePirateProjectileInstructionProgramDefinitionsTooling.PresentationWordCount, SpacePirateProjectileInstructionProgramDefinitionsTooling.PresentationWordAddress);

    /// <summary>Runs the retail-backed Pirate projectile instruction migration command.</summary>
    [VerificationCommand("--space-pirate-projectile-instruction-mechanics")]
    internal static int SpacePirateProjectileInstructionMechanicsCommand()
    {
        Suite(nameof(VerifySpacePirateProjectileInstructionProgramDefinitions), () => VerifySpacePirateProjectileInstructionProgramDefinitions());
        return 0;
    }
}
