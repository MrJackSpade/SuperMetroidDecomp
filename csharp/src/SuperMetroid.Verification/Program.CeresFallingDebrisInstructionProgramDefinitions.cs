using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Assets;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static DoorDefinitionsVerification;
using static EnemyProjectileProgramOracleVerification;
using static ImportedSpritemapsVerification;
using static SuiteTimingVerification;

internal static class CeresFallingDebrisInstructionProgramDefinitionsVerification
{
    internal static void VerifyCeresFallingDebrisInstructionProgramDefinitions() =>
        Suite(nameof(VerifyCeresFallingDebrisInstructionProgramDefinitions), () => VerifyCeresFallingDebrisInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));

    internal static void VerifyCeresFallingDebrisInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < CeresFallingDebrisInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                CeresFallingDebrisInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = ReadNativeEnemyProjectileProgramWord(rom, definition.Address);
            AssertEqual(definition.Value, native,
                $"Ceres falling-debris mechanics word $86:{definition.Address:X4}");
        }

        var spriteArtwork = InstalledFixtures.EnemyTiles.ProjectileSpritemaps
            ?? throw new InvalidDataException("Projectile fixture requires installed sprites.");
        var executedOperands = new HashSet<ushort>();
        var guard = NewCeresFallingDebrisInstructionReadGuard(rom);

        foreach ((bool dark, RoomEnemyProjectileKind kind, ushort program, ushort sleep) in
                 new[]
                 {
                     (false, RoomEnemyProjectileKind.CeresFallingDebrisLight,
                         CeresFallingDebrisInstructionProgramDefinitions.Light, (ushort)0x9754),
                     (true, RoomEnemyProjectileKind.CeresFallingDebrisDark,
                         CeresFallingDebrisInstructionProgramDefinitions.Dark, (ushort)0x975a),
                 })
        {
            RoomEnemySystem enemies = NewSystem();
            enemies.Projectiles.SpawnCeresFallingDebris(96, dark);
            RoomEnemyProjectileSlot debris = enemies.Projectiles.EnemyProjectiles.Single(
                projectile => projectile.Kind == kind);
            AssertEqual(program, debris.InstructionPointer,
                "real Ceres debris producer selects the named pose");
            RunForcedTick(enemies, debris);
            AssertEqual(sleep, debris.InstructionPointer,
                "Ceres debris displays its pose before terminal sleep");
            RunForcedTick(enemies, debris);
            AssertEqual(sleep, debris.InstructionPointer,
                "Ceres debris terminal sleep retains its own opcode");
            AssertEqual((ushort)0, debris.InstructionTimer,
                "Ceres debris terminal sleep stores the native zero timer");

            debris.InstructionPointer = CommonEnemyProjectileInstructionProgramDefinitions.Delete;
            debris.InstructionTimer = 1;
            RunForcedTick(enemies, debris);
            AssertTrue(!debris.IsActive,
                "Ceres debris shot reaction reaches the compiled shared delete program");
        }

        AssertEqual(0, guard.ObservedWords.Count,
            "CeresFallingDebris execution performs no live spritemap operand reads");
        AssertEqual(CeresFallingDebrisInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count, "CeresFallingDebris executes every native visual operand");
        for (int index = 0; index < CeresFallingDebrisInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort address = CeresFallingDebrisInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"CeresFallingDebris executes native presentation operand {address:X4}");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0x86, address, out ushort selector),
                "CeresFallingDebris has a compiled visual selector");
            AssertEqual(ReadVerificationWord(rom, 0x860000 | address), selector,
                "CeresFallingDebris compiled selector matches the cartridge");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids private and shared-delete debris mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => CeresFallingDebrisInstructionProgramDefinitions.ReadMechanicsWord(0x9752),
            "Ceres debris spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => CeresFallingDebrisInstructionProgramDefinitions.ReadMechanicsWord(0x974e),
            "word before Ceres debris programs is rejected as mechanics");

        _ = ProbeCeresFallingDebrisInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeCeresFallingDebrisInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Ceres debris allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Ceres debris mechanics lookups allocate no storage");

        Console.WriteLine(
            "Ceres falling-debris instruction mechanics: four compiled words, both real " +
            "producers, terminal sleeps, shared deletion, and two installed sprite frames match native OAM.");

        RoomEnemySystem NewSystem()
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guard);
            return enemies;
        }

        void RunForcedTick(RoomEnemySystem enemies, RoomEnemyProjectileSlot projectile)
        {
            projectile.InstructionTimer = 1;
            enemies.Projectiles.ProcessEnemyProjectileInstructions(projectile, new SamusState(), Position16<RoomPixels>.Zero);
            VerifyExecutedProjectileFrame(rom, projectile, spriteArtwork, executedOperands);
        }
    }

    private static int ProbeCeresFallingDebrisInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += CeresFallingDebrisInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? CeresFallingDebrisInstructionProgramDefinitions.Light
                    : CeresFallingDebrisInstructionProgramDefinitions.Dark);
        }
        return checksum;
    }

    private static CartridgeReadGuard NewCeresFallingDebrisInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => CeresFallingDebrisInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address),
                address => $"Production read compiled Ceres debris mechanics byte ${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase, CeresFallingDebrisInstructionProgramDefinitions.PresentationWordCount, CeresFallingDebrisInstructionProgramDefinitions.PresentationWordAddress);

    [VerificationCommand("--ceres-debris-instruction-mechanics")]
    internal static int CeresDebrisInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyCeresFallingDebrisInstructionProgramDefinitions), () => VerifyCeresFallingDebrisInstructionProgramDefinitions());
        return 0;
    }

    [VerificationCommand("--ceres-debris-fixture")]
    internal static int CeresDebrisFixtureCommand()
    {
        Suite(nameof(VerifyCeresFallingDebrisInstructionProgramDefinitions), () => VerifyCeresFallingDebrisInstructionProgramDefinitions());
        return 0;
    }
}
