using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Animals.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.Animals;
using SuperMetroid.Verification.Harness;

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
    internal static void VerifyDachoraInstructionProgramDefinitions(SuperMetroidAddressSpace rom)
    {
        (ushort Parameter, DachoraProgram Expected)[] initializerCases =
        [
            (0, DachoraProgram.IdleLeft),
            (1, DachoraProgram.IdleRight),
            (0xfffe, DachoraProgram.EchoLeft),
            (0xffff, DachoraProgram.EchoRight),
        ];
        foreach ((ushort parameter, DachoraProgram expected) in initializerCases)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot) = NewDachora(parameter);
            slot.Bind(EnemyDefinitionId.Dachora, slot.Definition);
            DachoraBehavior.InitializeDachora(slot);
            AssertEqual(AnimationProgramCursor<DachoraProgram>.Start(expected), enemies.SlotTable.Slots[0].EntityState<DachoraEntity, DachoraEnemyState>()?.Program,
                $"Dachora initializer parameter ${parameter:X4}");
        }

        foreach (DachoraProgram program in Enum.GetValues<DachoraProgram>())
        {
            if (DachoraAnimations.Programs.Steps(program)[^1] is not { Kind: AnimationProgramStepKind.GoTo } last || last.Target != program)
                continue;
            (RoomEnemySystem enemies, RoomEnemySlot slot) = NewDachora(0);
            slot.Bind(EnemyDefinitionId.Dachora, slot.Definition);
            DachoraBehavior.InitializeDachora(slot);
            DachoraEnemyState state = enemies.SlotTable.Slots[0].EntityState<DachoraEntity, DachoraEnemyState>() ?? throw new InvalidOperationException("The Dachora has no state.");
            state.Program = AnimationProgramCursor<DachoraProgram>.Start(program);
            slot.InstructionTimer = NativeCountdown.Start(1);
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(DachoraAnimations.Programs, program, DachoraFrameDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(DachoraAnimations.Programs, program)),
                $"Dachora {program} loop");
        }

        foreach ((DachoraProgram slow, DachoraProgram fast, DachoraProgram fastest) in new[]
                 {
                     (DachoraProgram.RunningLeft, DachoraProgram.RunningLeftFast, DachoraProgram.RunningLeftVeryFast),
                     (DachoraProgram.RunningRight, DachoraProgram.RunningRightFast, DachoraProgram.RunningRightVeryFast),
                 })
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot) = NewDachora(0);
            slot.Bind(EnemyDefinitionId.Dachora, slot.Definition);
            DachoraBehavior.InitializeDachora(slot);
            DachoraEnemyState state = enemies.SlotTable.Slots[0].EntityState<DachoraEntity, DachoraEnemyState>() ?? throw new InvalidOperationException("The Dachora has no state.");
            var empty = new RoomLevelData(new Extent16<RoomBlocks>(16, 16), new RoomLevelPlanes(new ushort[256], new byte[256], new ushort[256]), new byte[8]);
            // One $1000 subspeed step from just below each threshold lands exactly on whole speeds four and eight.
            state.Program = new AnimationProgramCursor<DachoraProgram>(slow, 3);
            state.Speed = SnesSignedSixteenSixteen.FromParts(3, 0xf000);
            enemies.DachoraBehavior.AccelerateDachora(slot, state, empty);
            AssertEqual(new AnimationProgramCursor<DachoraProgram>(fast, 3), state.Program, $"Dachora {slow} speeds up at the same pose");
            state.Speed = SnesSignedSixteenSixteen.FromParts(7, 0xf000);
            enemies.DachoraBehavior.AccelerateDachora(slot, state, empty);
            AssertEqual(new AnimationProgramCursor<DachoraProgram>(fastest, 3), state.Program, $"Dachora {fast} speeds up at the same pose");
        }

        AssertEqual(DachoraProgram.EchoRight, DachoraAnimations.AfterSleep(DachoraProgram.ChargeRight),
            "the shinespark launch steps the charge into the right-facing echo");
        AssertTrue(DachoraAnimations.Spritemaps(Enum.GetValues<DachoraProgram>()).Where(pointer => pointer != 0x804d).ToHashSet().SetEquals(
                EnemySpritemapDefinitions.Frames.Where(frame => frame.Name.StartsWith("dachora_", StringComparison.Ordinal)).Select(frame => frame.Pointer)),
            "the Dachora's programs show exactly its installed frames");
        Suite(nameof(DachoraAnimationsVerification.VerifyDachoraAnimations), () => DachoraAnimationsVerification.VerifyDachoraAnimations(rom));

        Console.WriteLine(
            "Dachora: the initializer's idle and echo choices, every loop, both speed-ups at the same pose and the " +
            "launch into the echo pass.");
    }

    /// <summary>A Dachora with spawn parameter <paramref name="parameter"/> in the first enemy slot.</summary>
    private static (RoomEnemySystem Enemies, RoomEnemySlot Slot) NewDachora(ushort parameter)
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Dachora, slot.Definition);
        slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = 0xa7 });
        slot.Parameter1 = parameter;
        return (enemies, slot);
    }

    /// <summary>Runs the Dachora animation verification.</summary>
    [VerificationCommand("--dachora-instruction-program-definitions")]
    internal static int DachoraInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyDachoraInstructionProgramDefinitions), () => VerifyDachoraInstructionProgramDefinitions());
        return 0;
    }
}
