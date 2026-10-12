using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static KraidRockGeneratedProgramsVerification;
using static SuiteTimingVerification;

internal static class KraidRockProjectileInstructionProgramDefinitionsVerification
{
    internal static void VerifyKraidRockProjectileInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyKraidRockProjectileInstructionProgramDefinitions), () => VerifyKraidRockProjectileInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    internal static void VerifyKraidRockProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        Suite(nameof(VerifyKraidRockMechanicsMapping), () => VerifyKraidRockMechanicsMapping(rom));
        Suite(nameof(VerifyKraidRockPresentationMapping), () => VerifyKraidRockPresentationMapping());

        var guard = NewKraidRockProjectileInstructionGuard(rom);
        var selectedPresentationWords = new HashSet<ushort>();
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(guard);
        ushort selectedRandom = 0;
        enemies.VerificationServices().ReadRandom = (Func<ushort>)(() => selectedRandom);


        var source = new RoomEnemySlot(0)
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(128, 224)),
            VramTilesIndex = 0x0200,
            PaletteIndex = 0x0c00,
        };

        selectedRandom = 0x0020;
        AssertTrue((bool)enemies.KraidBehavior.SpawnKraidSpitRock(source),
            "real Kraid spit producer allocates its rock");
        selectedRandom = 0x0020;
        AssertTrue((bool)enemies.KraidBehavior.SpawnKraidCeilingRock((ushort)96),
            "real Kraid ceiling producer allocates its rock");

        var kraidState = new KraidEnemyState();
        selectedRandom = 0x0000;
        enemies.KraidBehavior.RequestKraidRisingRock(source, kraidState);
        selectedRandom = 0x0010;
        enemies.KraidBehavior.RequestKraidRisingRock(source, kraidState);

        selectedRandom = 0x0002;
        AssertTrue((bool)enemies.KagoBehavior.SpawnKagoBug(source),
            "real Kago producer allocates its bug");

        RoomEnemyProjectileSlot spit = Find(RoomEnemyProjectileKind.KraidSpitRock);
        RoomEnemyProjectileSlot ceiling = Find(RoomEnemyProjectileKind.KraidCeilingRock);
        RoomEnemyProjectileSlot risingLeft = Find(
            RoomEnemyProjectileKind.KraidRisingRockLeft);
        RoomEnemyProjectileSlot risingRight = Find(
            RoomEnemyProjectileKind.KraidRisingRockRight);
        RoomEnemyProjectileSlot kago = Find(RoomEnemyProjectileKind.KagoBug);

        AssertEqual(KraidRockProjectileInstructionProgramDefinitions.SharedRockAndKagoBug,
            spit.InstructionPointer,
            "Kraid spit rock starts in the named shared pose");
        AssertEqual(KraidRockProjectileInstructionProgramDefinitions.SharedRockAndKagoBug,
            ceiling.InstructionPointer,
            "Kraid ceiling rock starts in the named shared pose");
        AssertEqual(KraidRockProjectileInstructionProgramDefinitions.SharedRockAndKagoBug,
            risingLeft.InstructionPointer,
            "left rising rock starts in the named shared pose");
        AssertEqual(KraidRockProjectileInstructionProgramDefinitions.RisingRockRight,
            risingRight.InstructionPointer,
            "right rising rock starts in its named mirrored pose");
        AssertEqual(KraidRockProjectileInstructionProgramDefinitions.SharedRockAndKagoBug,
            kago.InstructionPointer,
            "Kago bug starts in the cartridge's shared Kraid-rock pose");

        foreach (RoomEnemyProjectileSlot projectile in
                 new[] { spit, ceiling, risingLeft, risingRight, kago })
        {
            ushort expectedSleep = projectile.Kind ==
                    RoomEnemyProjectileKind.KraidRisingRockRight
                ? KraidRockProjectileInstructionProgramDefinitions.RisingRockRightSleep
                : KraidRockProjectileInstructionProgramDefinitions.SharedRockAndKagoBugSleep;
            RunForcedTick(projectile);
            AssertEqual(expectedSleep, projectile.InstructionPointer,
                $"{projectile.Kind} schedules its terminal sleep");
            RunForcedTick(projectile);
            AssertEqual(expectedSleep, projectile.InstructionPointer,
                $"{projectile.Kind} sleeps at the authored opcode");
            AssertEqual((ushort)0, projectile.InstructionTimer,
                $"{projectile.Kind} sleep leaves the timer stopped");
        }

        spit.InstructionPointer =
            KraidRockProjectileInstructionProgramDefinitions.SpitRockShot;
        RunForcedTick(spit);
        AssertEqual(
            (ushort)EnemyProjectilePreInstruction.KraidRockSpit_UsePalette0,
            spit.PreInstruction,
            "Kraid spit shot program installs the native palette-zero pre-instruction");
        AssertEqual((ushort)0x9c91, spit.InstructionPointer,
            "Kraid spit shot program schedules its first explosion frame");
        RunForcedTicks(spit, 4);
        AssertEqual(KraidRockProjectileInstructionProgramDefinitions.SpitRockShotDelete,
            spit.InstructionPointer,
            "Kraid spit shot program reaches deletion after all five explosion frames");
        RunForcedTick(spit);
        AssertTrue(!spit.IsActive,
            "Kraid spit shot program deletes after its complete explosion");

        ceiling.InstructionPointer =
            CommonEnemyProjectileInstructionProgramDefinitions.Delete;
        RunForcedTick(ceiling);
        AssertTrue(!ceiling.IsActive,
            "non-spit Kraid rock shot reaction reaches the compiled shared delete program");

        AssertEqual(
            KraidRockProjectileInstructionProgramDefinitions.PresentationWordCount,
            selectedPresentationWords.Count,
            "all live Kraid-rock projectile frames select installed operands");
        for (int index = 0;
             index < KraidRockProjectileInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = KraidRockProjectileInstructionProgramDefinitions
                .PresentationWordAddress(index);
            AssertTrue(selectedPresentationWords.Contains(address),
                $"production execution selects Kraid-rock presentation $86:{address:X4}");
        }

        AssertEqual(0, guard.ObservedWords.Count,
            "Kraid-rock frames never read presentation operands from ROM");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Kraid-rock and shared-delete mechanics byte");
        AssertThrows<InvalidDataException>(
            () => KraidRockProjectileInstructionProgramDefinitions.ReadMechanicsWord(0x9c7f),
            "Kraid-rock spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => KraidRockProjectileInstructionProgramDefinitions.ReadMechanicsWord(0x9ca3),
            "adjacent Kraid-rock initializer is rejected as mechanics");
        AssertTrue(KraidRockProjectileInstructionProgramDefinitions.Owns(
                RoomEnemyProjectileKind.KagoBug,
                KraidRockProjectileInstructionProgramDefinitions.SharedRockAndKagoBug),
            "Kago bug owns the one shared initial pose");
        AssertTrue(!KraidRockProjectileInstructionProgramDefinitions.Owns(
                RoomEnemyProjectileKind.KagoBug,
                KraidRockProjectileInstructionProgramDefinitions.SpitRockShot),
            "Kago bug does not claim Kraid's shot program");
        AssertTrue(!KraidRockProjectileInstructionProgramDefinitions.Owns(
                RoomEnemyProjectileKind.KraidRisingRockRight,
                KraidRockProjectileInstructionProgramDefinitions.SharedRockAndKagoBug),
            "right rising rock does not claim the mirrored left/shared pose");

        _ = ProbeKraidRockProjectileInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeKraidRockProjectileInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Kraid-rock allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Kraid-rock projectile mechanics lookups allocate no storage");

        Console.WriteLine(
            "Kraid-rock projectile instruction mechanics: twelve compiled words, all " +
            "four real Kraid rock producers, the shared Kago pose, both sleeps, the " +
            "complete five-frame shot explosion, and seven installed frame selectors pass " +
            "without instruction ROM reads.");

        RoomEnemyProjectileSlot Find(RoomEnemyProjectileKind kind) =>
            enemies.Projectiles.EnemyProjectiles.Single(projectile => projectile.Kind == kind);

        void RunForcedTicks(RoomEnemyProjectileSlot projectile, int count)
        {
            for (int tick = 0; tick < count; tick++)
                RunForcedTick(projectile);
        }

        void RunForcedTick(RoomEnemyProjectileSlot projectile)
        {
            projectile.InstructionTimer = 1;
            enemies.Projectiles.ProcessEnemyProjectileInstructions(projectile, new SamusState(), Position16<RoomPixels>.Zero);
            if (projectile.IsActive && projectile.InstructionTimer != 0)
            {
                ushort operand = (ushort)(projectile.InstructionPointer - 2);
                AssertEqual(operand, projectile.PresentationOperandAddress,
                    "Kraid rock selects the just-executed native frame operand");
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | ((ushort)(operand - 2))),
                    projectile.InstructionTimer, "Kraid rock frame retains the native duration");
                selectedPresentationWords.Add(projectile.PresentationOperandAddress);
            }
        }
    }

    private static int ProbeKraidRockProjectileInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += KraidRockProjectileInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? KraidRockProjectileInstructionProgramDefinitions.SharedRockAndKagoBug
                    : KraidRockProjectileInstructionProgramDefinitions.SpitRockShot);
        }
        return checksum;
    }

    private static CartridgeReadGuard NewKraidRockProjectileInstructionGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address =>
                KraidRockProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address),
                address => $"Production read compiled Kraid-rock mechanics byte ${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase, KraidRockProjectileInstructionProgramDefinitions.PresentationWordCount, KraidRockProjectileInstructionProgramDefinitions.PresentationWordAddress);

    [VerificationCommand("--kraid-rock-projectile-instruction-mechanics")]
    internal static int KraidRockProjectileInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyKraidRockProjectileInstructionProgramDefinitions), () => VerifyKraidRockProjectileInstructionProgramDefinitions());
        return 0;
    }
}
