using System.Reflection;
using SuperMetroid.Core.Hardware;
using static DoorDefinitionsVerification;
using static EnemyProjectileProgramOracleVerification;
using static ImportedSpritemapsVerification;
using static SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;

/// <summary>Checks compiled downward-gate projectile programs through live close and open lifecycles.</summary>
internal static class DownwardGateProjectileInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge for the gate projectile mechanics comparison.</summary>
    internal static void VerifyDownwardGateProjectileInstructionProgramDefinitions() =>
        Suite(nameof(VerifyDownwardGateProjectileInstructionProgramDefinitions), () => VerifyDownwardGateProjectileInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));

    /// <summary>Checks native words, guarded four-stage motion, executed sprite operands, shared deletion, and allocation-free reads.</summary>
    internal static void VerifyDownwardGateProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Static | BindingFlags.Instance |
            BindingFlags.NonPublic;
        for (int index = 0;
             index < DownwardGateProjectileInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                DownwardGateProjectileInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = ReadNativeEnemyProjectileProgramWord(rom, definition.Address);
            AssertEqual(definition.Value, native,
                $"downward-gate projectile mechanics word $86:{definition.Address:X4}");
        }

        var spriteArtwork = InstalledFixtures.EnemyTiles.ProjectileSpritemaps
            ?? throw new InvalidDataException("Projectile fixture requires installed sprites.");
        var executedOperands = new HashSet<ushort>();
        var guard = NewDownwardGateProjectileInstructionReadGuard(rom);

        RoomEnemySystem closingSystem = NewSystem();
        RoomEnemyProjectileSlot closing = Spawn(
            closingSystem,
            RoomEnemyProjectileKind.DownwardGateMoving);
        AssertEqual(DownwardGateProjectileInstructionProgramDefinitions.Moving,
            closing.InstructionPointer,
            "real downward-moving gate producer selects the named program");
        AssertEqual((ushort)32, closing.Position.Y.Whole,
            "downward-moving gate begins at its PLM row");
        Process(closingSystem, closing);
        AssertEqual((ushort)0x0100, closing.Velocity.Y.RawValue,
            "downward-moving gate installs positive one-pixel velocity");
        AssertEqual((ushort)EnemyProjectilePreInstruction.DownwardGateMovementPreInstruction,
            closing.PreInstruction,
            "downward-moving gate installs its translated movement callback");
        RunMovementFrames(closingSystem, closing, 64);
        AssertTrue(closing.IsActive,
            "downward-moving gate remains resident after reaching the closed position");
        AssertEqual((ushort)96, closing.Position.Y.Whole,
            "downward-moving gate traverses exactly four sixteen-pixel stages");
        AssertEqual(DownwardGateProjectileInstructionProgramDefinitions.ClosedSleep,
            closing.InstructionPointer,
            "downward-moving gate hands off to the closed sleep");
        AssertEqual((ushort)EnemyProjectilePreInstruction.RTS_868170, closing.PreInstruction,
            "downward-moving gate clears movement when it parks");

        RoomEnemySystem openingSystem = NewSystem();
        RoomEnemyProjectileSlot opening = Spawn(
            openingSystem,
            RoomEnemyProjectileKind.DownwardGateClosed);
        AssertEqual(DownwardGateProjectileInstructionProgramDefinitions.Closed,
            opening.InstructionPointer,
            "real closed-gate producer selects the named program");
        AssertEqual((ushort)96, opening.Position.Y.Whole,
            "closed gate begins four blocks below its PLM row");
        Process(openingSystem, opening);
        AssertEqual(DownwardGateProjectileInstructionProgramDefinitions.ClosedSleep,
            opening.InstructionPointer,
            "closed gate displays its parked frame and sleeps");
        openingSystem.DownwardGateBehavior.ApplyDownwardGateProjectileRequest(
            new DownwardGateProjectileRequest(
                DownwardGateProjectileOperation.Wake,
                0,
                42),
            roomWidthInBlocks: 16);
        Process(openingSystem, opening);
        AssertEqual(unchecked((ushort)-0x0100), opening.Velocity.Y.RawValue,
            "woken gate retains its authored upward velocity");
        AssertEqual((ushort)EnemyProjectilePreInstruction.DownwardGateMovementPreInstruction,
            opening.PreInstruction,
            "woken gate installs its translated movement callback");
        RunMovementFrames(openingSystem, opening, 63);
        AssertTrue(opening.IsActive,
            "opening gate remains active one pixel before its fourth stage completes");
        AssertEqual((ushort)33, opening.Position.Y.Whole,
            "opening gate reaches the final pixel before deletion");
        RunMovementFrames(openingSystem, opening, 1);
        AssertTrue(!opening.IsActive,
            "opening gate deletes after exactly four sixteen-pixel stages");

        RoomEnemySystem shotSystem = NewSystem();
        RoomEnemyProjectileSlot shot = Spawn(
            shotSystem,
            RoomEnemyProjectileKind.DownwardGateClosed);
        shot.InstructionPointer = CommonEnemyProjectileInstructionProgramDefinitions.Delete;
        shot.InstructionTimer = NativeCountdown.Start(1);
        Process(shotSystem, shot);
        AssertTrue(!shot.IsActive,
            "downward-gate shot reaction reaches shared compiled deletion");

        AssertEqual(0, guard.ObservedWords.Count,
            "Projectile presentation performs zero live cartridge reads");
        AssertEqual(DownwardGateProjectileInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count, "Every native visual operand executes");
        for (int index = 0;
             index < DownwardGateProjectileInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = DownwardGateProjectileInstructionProgramDefinitions
                .PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"production executes downward-gate presentation $86:{address:X4}");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0x86, address, out ushort selector),
                $"compiled visual selector exists at $86:{address:X4}");
            AssertEqual(ReadVerificationWord(rom, 0x860000 | address), selector,
                $"compiled visual selector matches native operand $86:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids private and shared downward-gate mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => DownwardGateProjectileInstructionProgramDefinitions.ReadMechanicsWord(0xe546),
            "downward-gate spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => DownwardGateProjectileInstructionProgramDefinitions.ReadMechanicsWord(0xe533),
            "downward-gate callback body is rejected as mechanics");

        _ = ProbeDownwardGateProjectileInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeDownwardGateProjectileInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "downward-gate allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed downward-gate mechanics lookups allocate no storage");

        Console.WriteLine(
            "Downward-gate projectile instruction mechanics: twenty-eight compiled words, " +
            "both real producers, four-stage close/open lifecycles, shared deletion, and " +
            "nine executed native sprite compositions pass with zero live operand reads.");

        // Create an enemy system whose compiled gate and shared instruction bytes cannot be read from the cartridge.
        RoomEnemySystem NewSystem()
        {
            var system = new RoomEnemySystem();
            system.BindFixtureBus(guard);
            return system;
        }

        // Use the real gate behavior producer to allocate a moving or already closed projectile.
        RoomEnemyProjectileSlot Spawn(
            RoomEnemySystem system,
            RoomEnemyProjectileKind kind)
        {
            system.DownwardGateBehavior.ApplyDownwardGateProjectileRequest(
                new DownwardGateProjectileRequest(
                    DownwardGateProjectileOperation.Spawn,
                    (ushort)kind,
                    42),
                roomWidthInBlocks: 16);
            return system.Projectiles.EnemyProjectiles.Single(projectile => projectile.Kind == kind);
        }

        // Advance projectile instructions and check the executed visual operand against installed artwork.
        void Process(RoomEnemySystem system, RoomEnemyProjectileSlot projectile)
        {
            system.Projectiles.ProcessEnemyProjectileInstructions(projectile, new SamusState(), Position16<RoomPixels>.Zero);
            VerifyExecutedProjectileFrame(rom, projectile, spriteArtwork, executedOperands);
        }

        // Move the gate one pixel per update and process its instruction frame while it remains active.
        void RunMovementFrames(
            RoomEnemySystem system,
            RoomEnemyProjectileSlot projectile,
            int count)
        {
            for (int frame = 0; frame < count; frame++)
            {
                DownwardGateBehavior.RunDownwardGateProjectileMovement(projectile);
                if (projectile.IsActive)
                    Process(system, projectile);
            }
        }
    }

    /// <summary>Repeatedly consumes compiled moving and closed control words for steady-state allocation measurement.</summary>
    private static int ProbeDownwardGateProjectileInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += DownwardGateProjectileInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? DownwardGateProjectileInstructionProgramDefinitions.Moving
                    : DownwardGateProjectileInstructionProgramDefinitions.Closed);
        }
        return checksum;
    }

    /// <summary>Forbids compiled gate/shared control reads and observes any remaining presentation operand reads.</summary>
    private static CartridgeReadGuard NewDownwardGateProjectileInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address =>
                    DownwardGateProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                    CommonEnemyProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address),
                address => $"Production read compiled downward-gate mechanics byte ${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase,
                DownwardGateProjectileInstructionProgramDefinitions.PresentationWordCount,
                DownwardGateProjectileInstructionProgramDefinitions.PresentationWordAddress);

    /// <summary>Runs the retail-cartridge gate projectile instruction verification.</summary>
    [VerificationCommand("--downward-gate-projectile-instruction-mechanics")]
    internal static int DownwardGateProjectileInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyDownwardGateProjectileInstructionProgramDefinitions), () => VerifyDownwardGateProjectileInstructionProgramDefinitions());
        return 0;
    }
}
