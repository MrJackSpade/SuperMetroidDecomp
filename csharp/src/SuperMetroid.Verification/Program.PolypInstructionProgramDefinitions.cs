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

/// <summary>Represents polyp instruction program definitions verification.</summary>
internal static class PolypInstructionProgramDefinitionsVerification
{
    /// <summary>Verifies polyp instruction program definitions.</summary>
    internal static void VerifyPolypInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyPolypInstructionProgramDefinitions), () => VerifyPolypInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Verifies polyp instruction program definitions.</summary>
    internal static void VerifyPolypInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < PolypInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                PolypInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa20000 | definition.Address),
                $"Polyp mechanics word $A2:{definition.Address:X4}");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = NewPolypInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        Type type = typeof(RoomEnemySystem);
        enemies.BindFixtureBus(guard);
        enemies.VerificationServices().SeedRandom = (Action<ushort>)(_ => { });
        var initialize = new Action<RoomEnemySlot>(enemies.PolypBehavior.InitializePolyp);

        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.EnemyDefinitionPointer = EnemyDefinitionId.LavaRocks;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        initialize(slot);
        AssertEqual(PolypInstructionProgramDefinitions.Stationary,
            slot.CurrentInstruction,
            "real Polyp initializer installs compiled stationary program");

        object?[] processArguments =
            [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
        slot.InstructionTimer = 1;
        EnemyInstructionRunner.Process(enemies, processArguments);
        AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa20000 | PolypInstructionProgramDefinitions.PresentationWord), slot.SpritemapPointer,
            "Polyp selects its exact native frame without cartridge reads");
        slot.InstructionTimer = 1;
        EnemyInstructionRunner.Process(enemies, processArguments);
        AssertEqual(unchecked((ushort)(PolypInstructionProgramDefinitions.Stationary + 4)),
            slot.CurrentInstruction,
            "Polyp program reaches terminal sleep");
        AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa20000 | PolypInstructionProgramDefinitions.PresentationWord),
            slot.SpritemapPointer, "Actual Polyp stationary sprite matches the native operand");
        AssertEqual(slot.SpritemapPointer,
            PolypInstructionProgramDefinitions.FrameAt(PolypInstructionProgramDefinitions.PresentationWord),
            "Named Polyp selector installed by the actual program");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids both compiled Polyp mechanics words");
        AssertThrows<InvalidDataException>(
            () => PolypInstructionProgramDefinitions.ReadMechanicsWord(
                PolypInstructionProgramDefinitions.PresentationWord),
            "Polyp spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => PolypInstructionProgramDefinitions.ReadMechanicsWord(0xb520),
            "adjacent cooldown table is rejected as instruction mechanics");

        _ = ProbePolypInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbePolypInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Polyp allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Polyp mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Polyp instruction mechanics: two compiled words, the real initializer, " +
            "terminal sleep, and exact frame selection pass with instruction bytes forbidden.");
    }

    /// <summary>Probe polyp instruction mechanics allocation.</summary>
    private static int ProbePolypInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += PolypInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? PolypInstructionProgramDefinitions.Stationary
                    : (ushort)0xb51e);
        }
        return checksum;
    }

    /// <summary>New polyp instruction read guard.</summary>
    private static CartridgeReadGuard NewPolypInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(PolypInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Polyp mechanics byte ${address:X6}.")
            .Forbid(address => address - (0xa20000 | PolypInstructionProgramDefinitions.PresentationWord) is 0 or 1,
                _ => "Production read the compiled Polyp visual operand.");

    /// <summary>Polyp instruction mechanics command.</summary>
    [VerificationCommand("--polyp-instruction-mechanics")]
    internal static int PolypInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyPolypInstructionProgramDefinitions), () => VerifyPolypInstructionProgramDefinitions());
        return 0;
    }
}
