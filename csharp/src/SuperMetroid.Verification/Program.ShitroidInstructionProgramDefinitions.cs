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

/// <summary>Represents shitroid instruction program definitions verification.</summary>
internal static class ShitroidInstructionProgramDefinitionsVerification
{
    /// <summary>Verifies shitroid instruction program definitions.</summary>
    internal static void VerifyShitroidInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyShitroidInstructionProgramDefinitions), () => VerifyShitroidInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Verifies shitroid instruction program definitions.</summary>
    internal static void VerifyShitroidInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < ShitroidInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                ShitroidInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa90000 | definition.Address),
                $"Shitroid mechanics word $A9:{definition.Address:X4}");
        }

        for (int index = 0; index < ShitroidInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort address = ShitroidInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(ShitroidVisualDefinitions.Bank, address, out ushort selector),
                "Shitroid presentation operand has an installed visual identity");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa90000 | address), selector,
                $"Shitroid native visual operand $A9:{address:X4}");
        }
        ushort randomNumber = 0;
        var guard = NewShitroidInstructionReadGuard(rom);
        var executedOperands = new HashSet<ushort>();
        var enemies = new RoomEnemySystem { TileArtwork = InstalledFixtures.EnemyTiles };
        enemies.BindFixtureBus(guard);
        enemies.Presentation._cgram = new SnesCgram();
        enemies.VerificationServices().ReadRandom = (Func<ushort>)(() => randomNumber);
        var initialize = new Action<RoomEnemySlot, ushort>(enemies.ShitroidBehavior.InitializeShitroid);

        RoomEnemySlot shitroid = enemies.SlotTable.Slots[0];
        shitroid.EnemyDefinitionPointer = EnemyDefinitionId.BabyMetroid;
        shitroid.Definition = default(RoomEnemyDefinition) with { Bank = 0xa9 };
        initialize(shitroid, 0);
        AssertEqual(ShitroidInstructionProgramDefinitions.Normal,
            shitroid.CurrentInstruction,
            "real Shitroid initializer installs compiled normal program");

        ExecuteShitroidProgram(rom, executedOperands, new ShitroidInterpreter { Instructions = enemies.Instructions }, shitroid, ShitroidInstructionProgramDefinitions.FinishDraining, callCount: 2);
        AssertEqual(ShitroidInstructionProgramDefinitions.Normal,
            shitroid.CurrentInstruction,
            "finish-draining program falls through to normal program");

        ExecuteShitroidProgram(rom, executedOperands, new ShitroidInterpreter { Instructions = enemies.Instructions }, shitroid, ShitroidInstructionProgramDefinitions.Normal, callCount: 5);
        AssertEqual(unchecked((ushort)(ShitroidInstructionProgramDefinitions.Normal + 4)),
            shitroid.CurrentInstruction,
            "normal callback loops and installs its first repeated frame");

        ExecuteShitroidProgram(rom, executedOperands, new ShitroidInterpreter { Instructions = enemies.Instructions }, shitroid, ShitroidInstructionProgramDefinitions.LatchedOn, callCount: 5);
        AssertEqual(unchecked((ushort)(ShitroidInstructionProgramDefinitions.LatchedOn + 4)),
            shitroid.CurrentInstruction,
            "latched callback loops and installs its first repeated frame");

        randomNumber = 0;
        ExecuteShitroidProgram(rom, executedOperands, new ShitroidInterpreter { Instructions = enemies.Instructions }, shitroid, ShitroidInstructionProgramDefinitions.Remorse, callCount: 9);
        AssertEqual(unchecked((ushort)(ShitroidInstructionProgramDefinitions.Remorse + 4)),
            shitroid.CurrentInstruction,
            "clear-high-bit remorse branch follows its compiled loop target");

        randomNumber = 0x8000;
        ExecuteShitroidProgram(rom, executedOperands, new ShitroidInterpreter { Instructions = enemies.Instructions }, shitroid, ShitroidInstructionProgramDefinitions.Remorse, callCount: 21);
        AssertEqual(unchecked((ushort)(ShitroidInstructionProgramDefinitions.Remorse + 4)),
            shitroid.CurrentInstruction,
            "set-high-bit remorse path plays every trailing frame and loops");
        AssertEqual((ushort?)0x0052, enemies.ShitroidBehavior.LastShitroidSoundEffectLibrary2,
            "set-high-bit remorse branch publishes native Shitroid cry");

        AssertEqual(0, guard.ObservedWords.Count,
            "Shitroid sprite selection performs zero live cartridge reads");
        AssertEqual(ShitroidInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count, "all Shitroid visual operands execute");
        for (int index = 0; index < ShitroidInstructionProgramDefinitions.PresentationWordCount; index++)
            AssertTrue(executedOperands.Contains(ShitroidInstructionProgramDefinitions.PresentationWordAddress(index)),
                "every Shitroid program frame executes with its native selector");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Shitroid mechanics byte");
        AssertThrows<InvalidDataException>(
            () => ShitroidInstructionProgramDefinitions.ReadMechanicsWord(
                ShitroidInstructionProgramDefinitions.PresentationWordAddress(0)),
            "Shitroid spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => ShitroidInstructionProgramDefinitions.ReadMechanicsWord(
                ShitroidInstructionProgramDefinitions.FirstAdjacentCallbackCode),
            "adjacent Shitroid callback code is rejected as instruction mechanics");

        _ = ProbeShitroidInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeShitroidInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Shitroid allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Shitroid mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Shitroid instruction mechanics: thirty-five compiled words, the real " +
            "initializer, finish-draining fallthrough, normal/latched loops, both remorse " +
            "branches, native cry, and thirty native sprite selections pass with mechanics " +
            "bytes forbidden and zero live visual operand reads.");
    }

    /// <summary>Executes shitroid program.</summary>
    private static void ExecuteShitroidProgram(SuperMetroidAddressSpace rom, HashSet<ushort> executedOperands, ShitroidInterpreter interpreter, RoomEnemySlot shitroid, ushort program, int callCount)
    {
        shitroid.CurrentInstruction = program;
        for (int call = 0; call < callCount; call++)
        {
            shitroid.InstructionTimer = 1;
            interpreter.Instructions.ProcessInstructions(shitroid, samus: null, level: null, Position16<RoomPixels>.Zero, controllerInput: 0);
            if (shitroid.InstructionTimer != 0)
            {
                ushort operand = unchecked((ushort)(shitroid.CurrentInstruction - 2));
                executedOperands.Add(operand);
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa90000 | operand), shitroid.SpritemapPointer,
                    $"Shitroid executed visual operand $A9:{operand:X4} matches the cartridge");
            }
        }
    }

    /// <summary>Probes shitroid instruction mechanics allocation.</summary>
    private static int ProbeShitroidInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += ShitroidInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? ShitroidInstructionProgramDefinitions.Normal
                    : ShitroidInstructionProgramDefinitions.RemorseLoopOpcode);
        }
        return checksum;
    }

    /// <summary>New shitroid instruction read guard.</summary>
    private static CartridgeReadGuard NewShitroidInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(ShitroidInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Shitroid mechanics byte ${address:X6}.")
            .ObserveWords(0xa90000, ShitroidInstructionProgramDefinitions.PresentationWordCount, ShitroidInstructionProgramDefinitions.PresentationWordAddress);

    /// <summary>Shitroid instruction mechanics command.</summary>
    [VerificationCommand("--shitroid-instruction-mechanics")]
    internal static int ShitroidInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyShitroidInstructionProgramDefinitions), () => VerifyShitroidInstructionProgramDefinitions());
        return 0;
    }
}
