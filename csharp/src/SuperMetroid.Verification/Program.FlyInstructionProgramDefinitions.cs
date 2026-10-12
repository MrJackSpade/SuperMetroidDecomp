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

/// <summary>Represents fly instruction program definitions verification.</summary>
internal static class FlyInstructionProgramDefinitionsVerification
{
    /// <summary>Verifies fly instruction program definitions.</summary>
    internal static void VerifyFlyInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyFlyInstructionProgramDefinitions), () => VerifyFlyInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Verifies fly instruction program definitions.</summary>
    internal static void VerifyFlyInstructionProgramDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < FlyInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                FlyInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadFlyInstructionWord(rom, 0xa20000 | definition.Address),
                $"fly-family instruction mechanics word $A2:{definition.Address:X4}");
        }

        var guard = new CartridgeReadGuard(rom)
            .Forbid(FlyInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled fly-family mechanics byte ${address:X6}.")
            .ObserveWords(0xa20000,
                FlyInstructionProgramDefinitionsTooling.PresentationWordCount,
                FlyInstructionProgramDefinitionsTooling.PresentationWordAddress);
        foreach (EnemyDefinitionId definition in new EnemyDefinitionId[]
                 {
                     EnemyDefinitionId.Mellow,
                     EnemyDefinitionId.Mella,
                     EnemyDefinitionId.Menu,
                 })
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guard);
            Action<RoomEnemySlot> initialize = enemies.FliesBehavior.InitializeFly;
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.EnemyDefinitionPointer = definition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
            initialize(slot);
            AssertEqual(FlyInstructionProgramDefinitions.Flight, slot.CurrentInstruction,
                $"fly ${(int)definition:X4} initializer program");

            object?[] arguments =
                [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
            for (int frame = 0; frame < 9; frame++)
                EnemyInstructionRunner.Process(enemies, arguments);
            AssertEqual(unchecked((ushort)(FlyInstructionProgramDefinitions.Flight + 4)),
                slot.CurrentInstruction,
                $"fly ${(int)definition:X4} completes native animation loop");
        }

        AssertEqual(0, guard.ObservedWords.Count,
            "fly-family animation uses compiled spritemap selectors");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled fly-family mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => FlyInstructionProgramDefinitions.ReadMechanicsWord(0xb015),
            "fly-family spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => FlyInstructionProgramDefinitions.ReadMechanicsWord(0xb027),
            "adjacent unused movement data is rejected as mechanics");

        _ = ProbeFlyInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeFlyInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "fly-family allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed fly-family mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Fly-family instruction mechanics: six compiled words, all three real " +
            "initializers, the complete loop, and four compiled spritemap selectors pass " +
            "without instruction or presentation ROM reads.");
    }

    /// <summary>Probe fly instruction mechanics allocation.</summary>
    private static int ProbeFlyInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
            checksum += FlyInstructionProgramDefinitions.ReadMechanicsWord(
                FlyInstructionProgramDefinitions.Flight);
        return checksum;
    }

    /// <summary>Reads fly instruction word.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for fly instruction words")]
    internal static ushort ReadFlyInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) => CartridgeImportSource.Require(bus).ReadCartridgeWord(address);

    /// <summary>Fly instruction mechanics command.</summary>
    [VerificationCommand("--fly-instruction-mechanics")]
    internal static int FlyInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyFlyInstructionProgramDefinitions), () => VerifyFlyInstructionProgramDefinitions());
        return 0;
    }
}
