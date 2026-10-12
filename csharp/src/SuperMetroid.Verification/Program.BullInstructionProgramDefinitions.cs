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
using static SuiteTimingVerification;

/// <summary>Represents bull instruction program definitions verification.</summary>
internal static class BullInstructionProgramDefinitionsVerification
{
    /// <summary>Verifies bull instruction program definitions.</summary>
    internal static void VerifyBullInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyBullInstructionProgramDefinitions), () => VerifyBullInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Verifies bull instruction program definitions.</summary>
    internal static void VerifyBullInstructionProgramDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic |
            BindingFlags.Static;
        for (int index = 0;
             index < BullInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                BullInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadBullInstructionWord(rom, 0xa80000 | definition.Address),
                $"Bull instruction mechanics word $A8:{definition.Address:X4}");
        }

        var guard = NewBullInstructionProgramReadGuard(rom, forbidPresentation: true);
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(guard);
        Action<RoomEnemySlot> initialize = enemies.BullBehavior.InitializeBull;
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.EnemyDefinitionPointer = EnemyDefinitionId.Bull;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
        initialize(slot);
        AssertEqual(BullInstructionProgramDefinitions.Normal, slot.CurrentInstruction,
            "Bull initializer program");

        object?[] processArguments =
            [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
        for (int frame = 0; frame < 41; frame++)
            EnemyInstructionRunner.Process(enemies, processArguments);
        AssertEqual(unchecked((ushort)(BullInstructionProgramDefinitions.Normal + 4)),
            slot.CurrentInstruction,
            "Bull normal program completes its native animation loop");
        AssertEqual((ushort)10, slot.InstructionTimer,
            "Bull normal loop restores its ten-frame duration");

        BullEnemyState state = enemies.BullStates[0] ?? throw new InvalidDataException(
            "Bull initializer did not publish typed state.");
        BullBehavior.ResolveBullImmuneShot(slot, state, (ushort)2);
        AssertEqual(BullInstructionProgramDefinitions.Shot, slot.CurrentInstruction,
            "Bull immune-shot program");
        for (int frame = 0; frame < 61; frame++)
            EnemyInstructionRunner.Process(enemies, processArguments);
        AssertEqual(unchecked((ushort)(BullInstructionProgramDefinitions.Normal + 4)),
            slot.CurrentInstruction,
            "Bull shot program repeats five times and returns to normal");
        AssertEqual((ushort)0, slot.Timer, "Bull shot loop exhausts its native repeat timer");

        AssertEqual(0, guard.ObservedWords.Count,
            "all Bull spritemap words select compiled presentation frames");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled Bull mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => BullInstructionProgramDefinitions.ReadMechanicsWord(0xd843),
            "Bull spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => BullInstructionProgramDefinitions.ReadMechanicsWord(0xd871),
            "adjacent Bull shot-angle data is rejected as instruction mechanics");

        _ = ProbeBullInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeBullInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Bull allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Bull mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Bull instruction mechanics: sixteen compiled words, the complete normal " +
            "loop, the five-cycle immune-shot response, and eight compiled visual selectors " +
            "pass with mechanics bytes forbidden.");
    }

    /// <summary>Probe bull instruction mechanics allocation.</summary>
    private static int ProbeBullInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
            checksum += BullInstructionProgramDefinitions.ReadMechanicsWord(
                BullInstructionProgramDefinitions.Normal);
        return checksum;
    }

    /// <summary>Reads bull instruction word.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Bull instruction words")]
    internal static ushort ReadBullInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) => CartridgeImportSource.Require(bus).ReadCartridgeWord(address);

    /// <summary>New bull instruction program read guard.</summary>
    internal static CartridgeReadGuard NewBullInstructionProgramReadGuard(
        ISnesAddressSpace source, bool forbidPresentation = false)
    {
        static ushort SelectorWord(int address) => Enumerable.Range(0, BullInstructionProgramDefinitionsTooling.PresentationWordCount)
            .Select(BullInstructionProgramDefinitionsTooling.PresentationWordAddress)
            .First(word => unchecked((ushort)address) == word || unchecked((ushort)address) == unchecked((ushort)(word + 1)));
        CartridgeReadGuard guard = new CartridgeReadGuard(source)
            .Forbid(BullInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Bull mechanics byte ${address:X6}.");
        return forbidPresentation
            ? guard.ForbidWords(0xa80000, BullInstructionProgramDefinitionsTooling.PresentationWordCount, BullInstructionProgramDefinitionsTooling.PresentationWordAddress,
                address => $"Installed Bull read cartridge visual selector $A8:{SelectorWord(address):X4}.")
            : guard.ObserveWords(0xa80000, BullInstructionProgramDefinitionsTooling.PresentationWordCount, BullInstructionProgramDefinitionsTooling.PresentationWordAddress);
    }

    /// <summary>Bull instruction mechanics command.</summary>
    [VerificationCommand("--bull-instruction-mechanics")]
    internal static int BullInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyBullInstructionProgramDefinitions), () => VerifyBullInstructionProgramDefinitions());
        return 0;
    }
}
