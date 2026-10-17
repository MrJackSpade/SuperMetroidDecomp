using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.PipeBugs;
using SuperMetroid.Core.Enemies.PipeBugs.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus;
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
    internal static void VerifyPipeBugBehavior()
    {
        AssertEqual(PipeBugProgram.ZebRisingLeft, Initialized(EnemyDefinitionId.Zeb, 0).Program.Program, "Zeb starts rising left");
        AssertEqual(PipeBugProgram.ZebboRisingLeft, Initialized(EnemyDefinitionId.Zebbo, 1).Program.Program, "Zebbo starts rising left");
        AssertEqual(PipeBugProgram.GametRisingLeft, Initialized(EnemyDefinitionId.Gamet, 0).Program.Program, "Gamet starts rising left");
        AssertEqual(PipeBugProgram.GeegaFlyingLeft, Initialized(EnemyDefinitionId.Geega, 1).Program.Program, "Geega with parameter one starts flying left");
        AssertEqual(PipeBugProgram.GeegaFlyingRight, Initialized(EnemyDefinitionId.Geega, 0).Program.Program, "Geega with parameter zero starts flying right");

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
            enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            PipeBugEnemyState state = Initialize(enemies, slot, definition, parameter);
            PipeBugsBehavior.InstallPipeBugProgram(slot, state, program);
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(PipeBugAnimations.Programs, program, PipeBugFrameDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(PipeBugAnimations.Programs, program)),
                $"Pipe Bug {program} loop");
        }
        Console.WriteLine("  Pipe Bug behavior: four initializers start their native loops and all sixteen loops run on time.");
    }

    /// <summary>
    /// Drives the production main AI through the Pipe Bugs' typed state. A Norfair member waiting for its stagger target
    /// of 72 counts one per update, wraps from 65535 to zero, and compares by the sign of the sixteen-bit difference, so
    /// a count of 32840 has not reached the target; reaching it departs toward Samus and clears the count. A Brinstar bug
    /// keeps its flight direction in variable A's sign bit, leaving the word's other bits alone, and emerges facing the
    /// side Samus is on. A yellow bug flies at its 1.5-pixel 16.16 speed, and its arc counts the speed record down to
    /// zero, ends the rise without going below it, then counts back up.
    /// </summary>
    internal static void VerifyPipeBugStateQuantities()
    {
        foreach ((int before, int after, bool departs) in new[] { (70, 71, false), (71, 0, true), (65535, 0, false), (32839, 32840, false) })
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128));
            PipeBugEnemyState state = Initialize(enemies, slot, EnemyDefinitionId.Gamet, 0);
            state.Function = PipeBugEnemyFunction.NorfairWaitForStagger;
            state.StaggerTarget = 72;
            state.FormationCounter = before;
            enemies.PipeBugsBehavior.RunPipeBugsMainAi(slot, Frame(new(200, 128)));
            AssertEqual(after, state.FormationCounter, $"Norfair Pipe Bug count after {before}");
            AssertEqual(departs ? PipeBugEnemyFunction.NorfairFlyRight : PipeBugEnemyFunction.NorfairWaitForStagger, state.Function,
                $"Norfair Pipe Bug departs only on reaching its target (count {before})");
        }

        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128));
            PipeBugEnemyState state = Initialize(enemies, slot, EnemyDefinitionId.Zeb, 0);
            state.Function = PipeBugEnemyFunction.BrinstarWaitForSamus;
            slot.VariableA = 5;
            enemies.PipeBugsBehavior.RunPipeBugsMainAi(slot, Frame(new(28, 78)));
            AssertTrue(state.BrinstarFliesLeft, "Brinstar Pipe Bug flies left of Samus on her left");
            AssertEqual(EnemyWordMasks.SignBit + 5, slot.VariableA, "Brinstar Pipe Bug direction is variable A's sign bit alone");
            AssertEqual(PipeBugEnemyFunction.BrinstarWaitForSamus, state.Function, "Brinstar Pipe Bug waits while Samus is 100 pixels away");
            enemies.PipeBugsBehavior.RunPipeBugsMainAi(slot, Frame(new(148, 78)));
            AssertTrue(!state.BrinstarFliesLeft, "Brinstar Pipe Bug flies right toward Samus on her right");
            AssertEqual(5, slot.VariableA, "Brinstar Pipe Bug clears only the sign bit");
            AssertEqual(PipeBugEnemyFunction.BrinstarEmerge, state.Function, "Brinstar Pipe Bug emerges for Samus 20 pixels away");
            AssertEqual(PipeBugAnimationSelector.FacingRight, state.AnimationState, "Brinstar Pipe Bug emerges facing right");
        }

        {
            const int onePixel = 65536;
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128));
            slot.Parameter2 = 24;
            PipeBugEnemyState state = Initialize(enemies, slot, EnemyDefinitionId.Geega, 0);
            AssertEqual(SnesSignedSixteenSixteen.FromRaw(onePixel + onePixel / 2), state.PositiveVelocity, "yellow Pipe Bug speed record 24 is 1.5 pixels");
            state.Function = PipeBugEnemyFunction.YellowArcRight;
            state.ArcRising = true;
            state.ArcCounter = 1;
            (int counter, bool rising)[] expected = [(0, true), (0, false), (1, false)];
            for (int update = 0; update < expected.Length; update++)
            {
                enemies.PipeBugsBehavior.RunPipeBugsMainAi(slot, Frame(new(200, 128)));
                AssertEqual(SnesFixedPosition.FromRaw(checked((uint)(128 * onePixel + (update + 1) * (onePixel + onePixel / 2)))), slot.Position.X,
                    $"yellow Pipe Bug arc update {update} carries its 16.16 speed");
                AssertEqual(expected[update].counter, state.ArcCounter, $"yellow Pipe Bug arc record after update {update}");
                if (update < 2)
                    AssertEqual(expected[update].rising, state.ArcRising, $"yellow Pipe Bug arc phase after update {update}");
            }
        }
        Console.WriteLine("  Pipe Bug state: the formation count wraps and compares by sign, the direction bit stands alone, and the arc record never goes below zero.");

        static ClockedEnemyAiFrame Frame(Position16<RoomPixels> samusPosition) => new(
            new RoomEnemyFrame
            {
                Camera = Position16<RoomPixels>.Zero,
                TimeIsFrozen = false,
                Samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(samusPosition) },
            },
            new FrameCounters(0));
    }

    /// <summary>A freshly initialized Pipe Bug's state.</summary>
    private static PipeBugEnemyState Initialized(EnemyDefinitionId definition, ushort parameter)
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        return Initialize(enemies, enemies.SlotTable.Slots[0], definition, parameter);
    }

    /// <summary>Runs <paramref name="definition"/>'s real initializer on <paramref name="slot"/>.</summary>
    private static PipeBugEnemyState Initialize(RoomEnemySystem enemies, RoomEnemySlot slot, EnemyDefinitionId definition, ushort parameter)
    {
        slot.Bind(definition, slot.Definition);
        slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = PipeBugFrameToolingDefinitions.Bank });
        slot.Parameter1 = parameter;
        switch (definition)
        {
            case EnemyDefinitionId.Zeb or EnemyDefinitionId.Zebbo:
                PipeBugsBehavior.InitializeBrinstarPipeBug(slot);
                break;
            case EnemyDefinitionId.Gamet:
                PipeBugsBehavior.InitializeNorfairPipeBug(slot);
                break;
            default:
                PipeBugsBehavior.InitializeYellowPipeBug(slot);
                break;
        }
        return enemies.PipeBugStates[slot.SlotIndex] ?? throw new InvalidOperationException("The Pipe Bug initializer left no state.");
    }

    /// <summary>Runs the Pipe Bug behavior check and the program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--pipe-bug-instruction-mechanics")]
    internal static int PipeBugBehaviorCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyPipeBugBehavior), () => VerifyPipeBugBehavior());
        Suite(nameof(VerifyPipeBugStateQuantities), () => VerifyPipeBugStateQuantities());
        Suite(nameof(PipeBugAnimationsVerification.VerifyPipeBugAnimations), () => PipeBugAnimationsVerification.VerifyPipeBugAnimations(rom));
        return 0;
    }
}
