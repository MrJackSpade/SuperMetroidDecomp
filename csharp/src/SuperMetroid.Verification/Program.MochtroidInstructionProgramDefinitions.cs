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
using static CompiledEnemyVisualSelectorAssertionsVerification;
using static SuiteTimingVerification;

internal static class MochtroidInstructionProgramDefinitionsVerification
{
    internal static void VerifyMochtroidInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyMochtroidInstructionProgramDefinitions), () => VerifyMochtroidInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    internal static void VerifyMochtroidInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < MochtroidInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                MochtroidInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa30000 | definition.Address),
                $"Mochtroid mechanics word $A3:{definition.Address:X4}");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = NewMochtroidInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        Type type = typeof(RoomEnemySystem);
        enemies.BindFixtureBus(guard);
        var initialize = new Action<RoomEnemySlot>(enemies.MochtroidBehavior.InitializeMochtroid);
        RoomEnemySlot mochtroid = enemies.SlotTable.Slots[0];
        mochtroid.EnemyDefinitionPointer = EnemyDefinitionId.Mochtroid;
        mochtroid.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };
        initialize(mochtroid);
        MochtroidEnemyState state = enemies.MochtroidStates[0]!;
        AssertEqual(MochtroidInstructionProgramDefinitions.FreeFlight,
            mochtroid.CurrentInstruction,
            "real Mochtroid initializer installs compiled free-flight loop");

        ExecuteMochtroidProgram(enemies, mochtroid, callCount: 5);
        AssertEqual(unchecked((ushort)(MochtroidInstructionProgramDefinitions.FreeFlight + 4)),
            mochtroid.CurrentInstruction,
            "Mochtroid free-flight program loops to its second frame");

        MochtroidBehavior.SetMochtroidInstructionList(mochtroid, state, MochtroidInstructionProgramDefinitions.Attached);
        AssertEqual(MochtroidInstructionProgramDefinitions.Attached,
            state.InstalledInstructionList,
            "production Mochtroid list switch installs attached loop");
        ExecuteMochtroidProgram(enemies, mochtroid, callCount: 5);
        AssertEqual(unchecked((ushort)(MochtroidInstructionProgramDefinitions.Attached + 4)),
            mochtroid.CurrentInstruction,
            "Mochtroid attached program loops to its second frame");

        AssertEqual(0, guard.ObservedWords.Count,
            "Mochtroid execution uses compiled spritemap selectors");
        for (int index = 0;
             index < MochtroidInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort operand = MochtroidInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            AssertCompiledEnemyVisualSelector(rom, EnemyDefinitionId.Mochtroid,
                0xa3, operand, $"Mochtroid $A3:{operand:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Mochtroid mechanics byte");
        AssertThrows<InvalidDataException>(
            () => MochtroidInstructionProgramDefinitions.ReadMechanicsWord(
                MochtroidInstructionProgramDefinitionsTooling.PresentationWordAddress(0)),
            "Mochtroid spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => MochtroidInstructionProgramDefinitions.ReadMechanicsWord(
                MochtroidInstructionProgramDefinitions.FirstAdjacentMechanicsData),
            "adjacent Mochtroid shake data is rejected as instruction mechanics");

        _ = ProbeMochtroidInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeMochtroidInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Mochtroid allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Mochtroid mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Mochtroid instruction mechanics: twelve compiled words, the real initializer, " +
            "the production state switch, both complete loops, and eight compiled spritemap " +
            "selectors pass with mechanics bytes forbidden.");
    }

    private static void ExecuteMochtroidProgram(
        RoomEnemySystem enemies,
        RoomEnemySlot mochtroid,
        int callCount)
    {
        object?[] arguments =
            [mochtroid, null, null, Position16<RoomPixels>.Zero, (ushort)0];
        for (int call = 0; call < callCount; call++)
        {
            mochtroid.InstructionTimer = NativeCountdown.Start(1);
            EnemyInstructionRunner.Process(enemies, arguments);
        }
    }

    private static int ProbeMochtroidInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += MochtroidInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? MochtroidInstructionProgramDefinitions.FreeFlight
                    : MochtroidInstructionProgramDefinitions.Attached);
        }
        return checksum;
    }

    private static CartridgeReadGuard NewMochtroidInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => MochtroidInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address),
                address => $"Production read compiled Mochtroid mechanics byte ${address:X6}.")
            .ObserveWords(0xa30000, MochtroidInstructionProgramDefinitionsTooling.PresentationWordCount, MochtroidInstructionProgramDefinitionsTooling.PresentationWordAddress);

    [VerificationCommand("--mochtroid-instruction-program-definitions")]
    internal static int MochtroidInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyMochtroidInstructionProgramDefinitions), () => VerifyMochtroidInstructionProgramDefinitions());
        return 0;
    }
}
