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

internal static class MultiviolaInstructionProgramDefinitionsVerification
{
    internal static void VerifyMultiviolaInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyMultiviolaInstructionProgramDefinitions), () => VerifyMultiviolaInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    internal static void VerifyMultiviolaInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < MultiviolaInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                MultiviolaInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa20000 | definition.Address),
                $"Multiviola mechanics word $A2:{definition.Address:X4}");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = NewMultiviolaInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        Type type = typeof(RoomEnemySystem);
        enemies.BindFixtureBus(guard);
        var initialize = new Action<RoomEnemySlot>(enemies.MultiviolaBehavior.InitializeMultiviola);

        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.EnemyDefinitionPointer = EnemyDefinitionId.Multiviola;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        slot.Parameter1 = 0x0058;
        slot.Parameter2 = 1;
        initialize(slot);
        AssertEqual(MultiviolaInstructionProgramDefinitions.Flying,
            slot.CurrentInstruction,
            "real Multiviola initializer installs compiled flying program");

        object?[] processArguments =
            [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
        for (int call = 0; call < 15; call++)
        {
            slot.InstructionTimer = 1;
            EnemyInstructionRunner.Process(enemies, processArguments);
        }
        AssertEqual(unchecked((ushort)(MultiviolaInstructionProgramDefinitions.Flying + 4)),
            slot.CurrentInstruction,
            "Multiviola program completes its native goto and first repeated frame");
        // The artwork suite compares all fourteen compiled visual selectors to
        // the cartridge. Live instruction execution must no longer read them.
        AssertEqual(0, guard.ObservedWords.Count,
            "Multiviola execution uses compiled spritemap selectors");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Multiviola mechanics byte");
        AssertThrows<InvalidDataException>(
            () => MultiviolaInstructionProgramDefinitions.ReadMechanicsWord(0xb2de),
            "Multiviola spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => MultiviolaInstructionProgramDefinitions.ReadMechanicsWord(0xb318),
            "unused Multiviola program is rejected as production mechanics");

        _ = ProbeMultiviolaInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeMultiviolaInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Multiviola allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Multiviola mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Multiviola instruction mechanics: 16 compiled words, the complete production " +
            "loop and compiled spritemap selectors pass with mechanics bytes forbidden.");
    }

    private static int ProbeMultiviolaInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += MultiviolaInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? MultiviolaInstructionProgramDefinitions.Flying
                    : (ushort)0xb314);
        }
        return checksum;
    }

    private static CartridgeReadGuard NewMultiviolaInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => MultiviolaInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address),
                address => $"Production read compiled Multiviola mechanics byte ${address:X6}.")
            .ObserveWords(0xa20000, MultiviolaInstructionProgramDefinitions.PresentationWordCount, MultiviolaInstructionProgramDefinitions.PresentationWordAddress);

    [VerificationCommand("--multiviola-instruction-mechanics")]
    internal static int MultiviolaInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyMultiviolaInstructionProgramDefinitions), () => VerifyMultiviolaInstructionProgramDefinitions());
        return 0;
    }
}
