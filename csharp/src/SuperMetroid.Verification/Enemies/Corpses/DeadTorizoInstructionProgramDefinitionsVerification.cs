using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Programs;
namespace SuperMetroid.Verification.Enemies.Corpses;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Dead Torizo's compiled instruction words and actual corpse initialization against bank $A9.</summary>
internal static class DeadTorizoInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the pinned cartridge and runs the Dead Torizo mechanics comparison.</summary>
    internal static void VerifyDeadTorizoInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyDeadTorizoInstructionProgramDefinitions), () => VerifyDeadTorizoInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks native words, guarded production execution, mechanics boundaries, and allocation behavior.</summary>
    internal static void VerifyDeadTorizoInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < DeadTorizoInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                DeadTorizoInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa90000 | definition.Address),
                $"Dead Torizo mechanics word $A9:{definition.Address:X4}");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        bool sawPresentationWord = false;
        var guard = NewDeadTorizoInstructionReadGuard(rom, () => sawPresentationWord = true);
        var enemies = new RoomEnemySystem { TileArtwork = InstalledFixtures.EnemyTiles };
        Type type = typeof(RoomEnemySystem);
        enemies.BindFixtureBus(guard);
        var initialize = new Action<RoomEnemySlot>(enemies.DeadTorizoBehavior.InitializeDeadTorizo);

        RoomEnemySlot corpse = enemies.SlotTable.Slots[0];
        corpse.EnemyDefinitionPointer = EnemyDefinitionId.CorpseTorizo;
        corpse.Definition = default(RoomEnemyDefinition) with { Bank = 0xa9 };
        initialize(corpse);
        AssertEqual(DeadTorizoInstructionProgramDefinitions.Stationary,
            corpse.CurrentInstruction,
            "real Dead Torizo initializer installs compiled stationary program");

        object?[] processArguments =
            [corpse, null, null, Position16<RoomPixels>.Zero, (ushort)0];
        corpse.InstructionTimer = NativeCountdown.Start(1);
        EnemyInstructionRunner.Process(enemies, processArguments);
        corpse.InstructionTimer = NativeCountdown.Start(1);
        EnemyInstructionRunner.Process(enemies, processArguments);
        AssertEqual(DeadTorizoInstructionProgramDefinitions.SleepOpcode,
            corpse.CurrentInstruction,
            "Dead Torizo program reaches terminal sleep");
        AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa90000 | DeadTorizoInstructionProgramDefinitionsTooling.PresentationWord),
            corpse.SpritemapPointer,
            "compiled Dead Torizo spritemap selector matches the cartridge");
        AssertTrue(!sawPresentationWord,
            "Dead Torizo execution uses the compiled spritemap selector");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids both compiled Dead Torizo mechanics words");
        AssertThrows<InvalidDataException>(
            () => DeadTorizoInstructionProgramDefinitions.ReadMechanicsWord(
                DeadTorizoInstructionProgramDefinitionsTooling.PresentationWord),
            "Dead Torizo spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => DeadTorizoInstructionProgramDefinitions.ReadMechanicsWord(
                DeadTorizoInstructionProgramDefinitions.FirstAdjacentPresentationData),
            "adjacent Dead Torizo spritemap data is rejected as instruction mechanics");

        _ = ProbeDeadTorizoInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeDeadTorizoInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Dead Torizo allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Dead Torizo mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Dead Torizo instruction mechanics: two compiled words, the real initializer, " +
            "terminal sleep, and the compiled spritemap selector pass with mechanics bytes forbidden.");
    }

    /// <summary>Exercises warmed instruction lookups repeatedly so the caller can measure per-frame allocation.</summary>
    private static int ProbeDeadTorizoInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += DeadTorizoInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? DeadTorizoInstructionProgramDefinitions.Stationary
                    : DeadTorizoInstructionProgramDefinitions.SleepOpcode);
        }
        return checksum;
    }

    /// <summary>Rejects runtime rereads of compiled mechanics bytes and observes the presentation selector.</summary>
    private static CartridgeReadGuard NewDeadTorizoInstructionReadGuard(ISnesAddressSpace source,
        Action sawPresentationWord) =>
        new CartridgeReadGuard(source)
            .Forbid(DeadTorizoInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Dead Torizo mechanics byte ${address:X6}.")
            .Observe(address =>
                {
                    int presentation =
                        0xa90000 | DeadTorizoInstructionProgramDefinitionsTooling.PresentationWord;
                    return address == presentation || address == presentation + 1;
                },
                _ => sawPresentationWord());

    /// <summary>Dispatches the cartridge-backed Dead Torizo instruction verification command.</summary>
    [VerificationCommand("--dead-torizo-instruction-mechanics")]
    internal static int DeadTorizoInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyDeadTorizoInstructionProgramDefinitions), () => VerifyDeadTorizoInstructionProgramDefinitions());
        return 0;
    }
}
