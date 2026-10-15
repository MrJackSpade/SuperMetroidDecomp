using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.PipeBugs;
using SuperMetroid.Core.Enemies.PipeBugs.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.PipeBugs;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the Pipe Bugs' initial animations and that each of their sixteen loops runs through the real behavior.</summary>
internal static class PipeBugBehaviorVerification
{
    /// <summary>
    /// The Zeb, Zebbo, Gamet and Geega initializers start their native first loops, and every loop installed through
    /// <see cref="PipeBugsBehavior.InstallPipeBugProgram"/> runs one pass and returns to its first pose on time.
    /// </summary>
    internal static void VerifyPipeBugBehavior(SuperMetroidAddressSpace rom)
    {
        AssertEqual(PipeBugProgram.ZebRisingLeft, Initialized(rom, EnemyDefinitionId.Zeb, 0).Program.Program, "Zeb starts rising left");
        AssertEqual(PipeBugProgram.ZebboRisingLeft, Initialized(rom, EnemyDefinitionId.Zebbo, 1).Program.Program, "Zebbo starts rising left");
        AssertEqual(PipeBugProgram.GametRisingLeft, Initialized(rom, EnemyDefinitionId.Gamet, 0).Program.Program, "Gamet starts rising left");
        AssertEqual(PipeBugProgram.GeegaFlyingLeft, Initialized(rom, EnemyDefinitionId.Geega, 1).Program.Program, "Geega with parameter one starts flying left");
        AssertEqual(PipeBugProgram.GeegaFlyingRight, Initialized(rom, EnemyDefinitionId.Geega, 0).Program.Program, "Geega with parameter zero starts flying right");

        foreach (PipeBugProgram program in Enum.GetValues<PipeBugProgram>())
        {
            (EnemyDefinitionId definition, ushort parameter) = program.ToString() switch
            {
                var name when name.StartsWith("Zebbo", StringComparison.Ordinal) => (EnemyDefinitionId.Zebbo, (ushort)1),
                var name when name.StartsWith("Zeb", StringComparison.Ordinal) => (EnemyDefinitionId.Zeb, (ushort)0),
                var name when name.StartsWith("Gamet", StringComparison.Ordinal) => (EnemyDefinitionId.Gamet, (ushort)0),
                _ => (EnemyDefinitionId.Geega, (ushort)0),
            };
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(rom);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            PipeBugEnemyState state = Initialize(enemies, slot, definition, parameter);
            PipeBugsBehavior.InstallPipeBugProgram(slot, state, program);
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(PipeBugAnimations.Programs, program, PipeBugFrameDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(enemies, slot, EnemyInstructionRunner.LoopUpdates(PipeBugAnimations.Programs, program)),
                $"Pipe Bug {program} loop");
        }
        Console.WriteLine("  Pipe Bug behavior: four initializers start their native loops and all sixteen loops run on time.");
    }

    /// <summary>A freshly initialized Pipe Bug's state.</summary>
    private static PipeBugEnemyState Initialized(SuperMetroidAddressSpace rom, EnemyDefinitionId definition, ushort parameter)
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(rom);
        return Initialize(enemies, enemies.SlotTable.Slots[0], definition, parameter);
    }

    /// <summary>Runs <paramref name="definition"/>'s real initializer on <paramref name="slot"/>.</summary>
    private static PipeBugEnemyState Initialize(RoomEnemySystem enemies, RoomEnemySlot slot, EnemyDefinitionId definition, ushort parameter)
    {
        slot.EnemyDefinitionPointer = definition;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = PipeBugFrameDefinitions.Bank };
        slot.Parameter1 = parameter;
        switch (definition)
        {
            case EnemyDefinitionId.Zeb or EnemyDefinitionId.Zebbo:
                enemies.PipeBugsBehavior.InitializeBrinstarPipeBug(slot);
                break;
            case EnemyDefinitionId.Gamet:
                enemies.PipeBugsBehavior.InitializeNorfairPipeBug(slot);
                break;
            default:
                enemies.PipeBugsBehavior.InitializeYellowPipeBug(slot);
                break;
        }
        return enemies.PipeBugStates[slot.SlotIndex] ?? throw new InvalidOperationException("The Pipe Bug initializer left no state.");
    }

    /// <summary>Runs the Pipe Bug behavior check and the program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--pipe-bug-instruction-mechanics")]
    internal static int PipeBugBehaviorCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyPipeBugBehavior), () => VerifyPipeBugBehavior(rom));
        Suite(nameof(PipeBugAnimationsVerification.VerifyPipeBugAnimations), () => PipeBugAnimationsVerification.VerifyPipeBugAnimations(rom));
        return 0;
    }
}
