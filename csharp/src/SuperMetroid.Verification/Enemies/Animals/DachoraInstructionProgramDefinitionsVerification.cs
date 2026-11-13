using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Animals.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Animals;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.World.Artwork;

/// <summary>Checks the Dachora's and its echoes' typed animations.</summary>
internal static class DachoraInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge for the Dachora animation checks.</summary>
    internal static void VerifyDachoraInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyDachoraInstructionProgramDefinitions), () => VerifyDachoraInstructionProgramDefinitions(
            CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>
    /// The real initializer picks idle or echo by facing; every looping program runs on time; each speed-up keeps the
    /// pose in the next-faster run; the launch steps the charge into the right echo; the programs show exactly the
    /// installed Dachora frames; the program oracle matches bank $A7.
    /// </summary>
    internal static void VerifyDachoraInstructionProgramDefinitions(CartridgeImportAddressSpace rom)
    {
        InitializerCase[] initializerCases =
        [
            new InitializerCase(0, DachoraSegment.IdleLeft),
            new InitializerCase(1, DachoraSegment.IdleRight),
            new InitializerCase(0xfffe, DachoraSegment.EchoLeft),
            new InitializerCase(0xffff, DachoraSegment.EchoRight),
        ];
        foreach ((ushort parameter, DachoraSegment expected) in initializerCases)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot) = NewDachora(parameter);
            slot.Bind(EnemyDefinitionId.Dachora, slot.Definition);
            enemies.DachoraBehavior.InitializeDachora(slot);
            AssertEqual(new AnimationPlayback<DachoraSegment>(DachoraAnimations.StartingWith(expected), AnimationPlace.StartOfAnimation), enemies.SlotTable.Slots[0].EntityState<DachoraEntity, DachoraEnemyState>()?.Playback,
                $"Dachora initializer parameter ${parameter:X4}");
        }

        foreach (DachoraSegment program in Enum.GetValues<DachoraSegment>())
        {
            // The self-looping animations: one phase of their own segment, played again and again.
            if (!DachoraAnimations.StartingAt.TryGetValue(program, out Animation<DachoraSegment>? animation) ||
                animation is not { Phases: [{ Plays: 1 } only], End: AnimationEnd.Loop { Phase: 0 } } || only.Segment != program)
                continue;
            (RoomEnemySystem enemies, RoomEnemySlot slot) = NewDachora(0);
            slot.Bind(EnemyDefinitionId.Dachora, slot.Definition);
            enemies.DachoraBehavior.InitializeDachora(slot);
            DachoraEnemyState state = enemies.SlotTable.Slots[0].EntityState<DachoraEntity, DachoraEnemyState>() ?? throw new InvalidOperationException("The Dachora has no state.");
            state.Playback = new(DachoraAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);
            slot.AnimationClock.StepOnNextUpdate();
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(DachoraAnimations.Segments, program, DachoraFrameToolingDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(DachoraAnimations.Segments, program)),
                $"Dachora {program} loop");
        }

        foreach ((DachoraSegment slow, DachoraSegment fast, DachoraSegment fastest) in new[]
                 {
                     new VerifyDachoraInstructionProgramDefinitionsCase(DachoraSegment.RunningLeft, DachoraSegment.RunningLeftFast, DachoraSegment.RunningLeftVeryFast),
                     new VerifyDachoraInstructionProgramDefinitionsCase(DachoraSegment.RunningRight, DachoraSegment.RunningRightFast, DachoraSegment.RunningRightVeryFast),
                 })
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot) = NewDachora(0);
            slot.Bind(EnemyDefinitionId.Dachora, slot.Definition);
            enemies.DachoraBehavior.InitializeDachora(slot);
            DachoraEnemyState state = enemies.SlotTable.Slots[0].EntityState<DachoraEntity, DachoraEnemyState>() ?? throw new InvalidOperationException("The Dachora has no state.");
            RoomLevelData empty = VerificationHelpersVerification.CreateEmptyRoom(new(16, 16));
            // One $1000 subspeed step from just below each threshold lands exactly on whole speeds four and eight.
            state.Playback = new(DachoraAnimations.StartingWith(slow), new AnimationPlace(0, 3));
            state.Speed = SignedSixteenSixteen.FromParts(3, 0xf000);
            enemies.DachoraBehavior.AccelerateDachora(slot, state, empty);
            AssertEqual(new AnimationPlayback<DachoraSegment>(DachoraAnimations.StartingWith(fast), new AnimationPlace(0, 3)), state.Playback, $"Dachora {slow} speeds up at the same pose");
            state.Speed = SignedSixteenSixteen.FromParts(7, 0xf000);
            enemies.DachoraBehavior.AccelerateDachora(slot, state, empty);
            AssertEqual(new AnimationPlayback<DachoraSegment>(DachoraAnimations.StartingWith(fastest), new AnimationPlace(0, 3)), state.Playback, $"Dachora {fast} speeds up at the same pose");
        }

        AssertEqual(DachoraSegment.EchoRight, DachoraAnimations.AfterSleep(DachoraSegment.ChargeRight),
            "the shinespark launch steps the charge into the right-facing echo");
        AssertTrue(DachoraAnimationsTooling.Spritemaps(Enum.GetValues<DachoraSegment>()).Where(pointer => pointer != 0x804d).ToHashSet().SetEquals(
                EnemySpritemapDefinitions.Frames.Where(frame => frame.Name.StartsWith("dachora_", StringComparison.Ordinal)).Select(frame => frame.Pointer)),
            "the Dachora's programs show exactly its installed frames");
        Suite(nameof(DachoraAnimationsVerification.VerifyDachoraAnimations), () => DachoraAnimationsVerification.VerifyDachoraAnimations(rom));

        Console.WriteLine(
            "Dachora: the initializer's idle and echo choices, every loop, both speed-ups at the same pose and the " +
            "launch into the echo pass.");
    }

    /// <summary>A Dachora with spawn parameter <paramref name="parameter"/> in the first enemy slot.</summary>
    private static DachoraInstructionProgramDefinitionsNewDachoraResult NewDachora(ushort parameter)
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Dachora, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        slot.SetSpawnParameter1(EnemyDefinitionId.Dachora, parameter);
        return new DachoraInstructionProgramDefinitionsNewDachoraResult(enemies, slot);
    }

    /// <summary>Runs the Dachora animation verification.</summary>
    [VerificationCommand("--dachora-instruction-program-definitions")]
    internal static int DachoraInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyDachoraInstructionProgramDefinitions), () => VerifyDachoraInstructionProgramDefinitions());
        return 0;
    }
}
