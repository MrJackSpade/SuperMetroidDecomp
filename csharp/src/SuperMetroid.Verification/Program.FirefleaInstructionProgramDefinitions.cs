using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;

internal static class FirefleaInstructionProgramDefinitionsVerification
{
    internal static void VerifyFirefleaInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyFirefleaInstructionProgramDefinitions), () => VerifyFirefleaInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    internal static void VerifyFirefleaInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

        AssertEqual(54, FirefleaInstructionProgramDefinitionsTooling.MechanicsWordCount,
            "Fireflea compiled mechanics word count");
        AssertEqual(52, FirefleaInstructionProgramDefinitionsTooling.PresentationWordCount,
            "Fireflea live presentation word count");
        for (int index = 0;
             index < FirefleaInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                FirefleaInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa30000 | definition.Address),
                $"Fireflea mechanics word $A3:{definition.Address:X4}");
        }

        CartridgeReadGuard guard = new CartridgeReadGuard(rom).Forbid(
            address => FirefleaInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                (SnesAddressMath.BankBase(address)) == 0xa30000 &&
                (FirefleaInstructionProgramDefinitions.IsPresentationWord(unchecked((ushort)address)) ||
                 FirefleaInstructionProgramDefinitions.IsPresentationWord(unchecked((ushort)(address - 1)))),
            address => $"Production read compiled Fireflea instruction byte ${address:X6}.");
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(guard);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.EnemyDefinitionPointer = EnemyDefinitionId.Fireflea;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };
        slot.Parameter1 = 0;
        slot.Parameter2 = 0;
        enemies.FirefleaBehavior.InitializeFireflea(slot);
        AssertEqual(FirefleaInstructionProgramDefinitions.Loop,
            slot.CurrentInstruction,
            "Fireflea real initializer selects compiled loop");

        object?[] arguments =
            [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
        for (int call = 0;
             call < FirefleaInstructionProgramDefinitions.FrameCount + 1;
             call++)
        {
            slot.InstructionTimer = 1;
            EnemyInstructionRunner.Process(enemies, arguments);
            ushort operand = FirefleaInstructionProgramDefinitionsTooling.PresentationWordAddress(
                call % FirefleaInstructionProgramDefinitions.FrameCount);
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa30000 | operand);
            AssertEqual(native, EnemySpritemapDefinitions.FirefleaFrameAt(operand),
                $"compiled Fireflea visual selector $A3:{operand:X4}");
            AssertEqual(native, slot.SpritemapPointer,
                $"Fireflea production frame {call} matches cartridge selection");
        }
        AssertEqual(unchecked((ushort)(FirefleaInstructionProgramDefinitions.Loop + 4)),
            slot.CurrentInstruction,
            "Fireflea completes all 52 frames and loops to the first frame");

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled Fireflea mechanics and visual bytes");

        AssertThrows<InvalidDataException>(
            () => FirefleaInstructionProgramDefinitions.ReadMechanicsWord(
                FirefleaInstructionProgramDefinitionsTooling.PresentationWordAddress(0)),
            "Fireflea spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => FirefleaInstructionProgramDefinitions.ReadMechanicsWord(
                FirefleaInstructionProgramDefinitions.AdjacentUnusedData),
            "adjacent unused Fireflea data is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => EnemySpritemapDefinitions.FirefleaFrameAt(
                FirefleaInstructionProgramDefinitions.AdjacentUnusedData),
            "adjacent unused Fireflea data is rejected as presentation");

        _ = ProbeFirefleaInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeFirefleaInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Fireflea allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Fireflea mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Fireflea instruction mechanics: 54 compiled words, the complete 52-frame " +
            "loop, and 52 exact frame selections pass with mechanics and visual source bytes forbidden.");
    }

    private static int ProbeFirefleaInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += FirefleaInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? FirefleaInstructionProgramDefinitions.Loop
                    : unchecked((ushort)(FirefleaInstructionProgramDefinitions.Loop + 4)));
        }
        return checksum;
    }

    [VerificationCommand("--fireflea-instruction-program-definitions")]
    internal static int FirefleaInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyFirefleaInstructionProgramDefinitions), () => VerifyFirefleaInstructionProgramDefinitions());
        return 0;
    }
}
