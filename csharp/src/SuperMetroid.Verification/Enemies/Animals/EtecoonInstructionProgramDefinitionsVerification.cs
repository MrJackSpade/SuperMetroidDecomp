using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Animals.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.Animals;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the Etecoon's typed animations.</summary>
internal static class EtecoonInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge for the Etecoon animation checks.</summary>
    internal static void VerifyEtecoonInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyEtecoonInstructionProgramDefinitions), () => VerifyEtecoonInstructionProgramDefinitions(
            CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>
    /// The real initializer starts the initial loop; every looping program runs on time; the four flex cycles count
    /// down; the programs show exactly the installed Etecoon frames; the program oracle matches bank $A7.
    /// </summary>
    internal static void VerifyEtecoonInstructionProgramDefinitions(SuperMetroidAddressSpace rom)
    {
        foreach (EtecoonProgram program in Enum.GetValues<EtecoonProgram>())
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, EtecoonEnemyState state) = NewEtecoon();
            AssertEqual(AnimationProgramCursor<EtecoonProgram>.Start(EtecoonProgram.Initial), state.Program, "real Etecoon initializer");
            if (EtecoonAnimations.Programs.Steps(program)[^1] is not AnimationProgramStep<EtecoonProgram, EtecoonFrame, NoAnimationCue>.GoToStep last || last.Target != program)
                continue;
            state.Program = AnimationProgramCursor<EtecoonProgram>.Start(program);
            slot.InstructionTimer = NativeCountdown.Start(1);
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(EtecoonAnimations.Programs, program, EtecoonFrameDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(EtecoonAnimations.Programs, program)),
                $"Etecoon {program} loop");
        }

        {
            // Four six-pose flex cycles, then two held poses: 4 x 6 x 8 + 2 x 32 updates reach the final sleep.
            (RoomEnemySystem enemies, RoomEnemySlot slot, EtecoonEnemyState state) = NewEtecoon();
            state.Program = AnimationProgramCursor<EtecoonProgram>.Start(EtecoonProgram.Flexing);
            slot.InstructionTimer = NativeCountdown.Start(1);
            _ = EnemyInstructionRunner.ShownFrames(slot, 4 * 6 * 8 + 2 * 32);
            AnimationProgramCursor<EtecoonProgram> held = state.Program;
            _ = EnemyInstructionRunner.ShownFrames(slot, 8);
            AssertEqual(held, state.Program, "Etecoon flex ends asleep");
            AssertEqual(EtecoonAnimations.Spritemaps(EtecoonProgram.FlexingLoop).Last(), slot.SpritemapPointer, "Etecoon flex ends on its last pose");
            AssertEqual((ushort)0, slot.Timer.RawValue, "Etecoon flex loop count expires");
        }

        AssertTrue(EtecoonAnimations.Spritemaps(Enum.GetValues<EtecoonProgram>()).Where(pointer => pointer != 0x804d).ToHashSet().SetEquals(
                EnemySpritemapDefinitions.Frames.Where(frame => frame.Name.StartsWith("etecoon_", StringComparison.Ordinal)).Select(frame => frame.Pointer)),
            "the Etecoon's programs show exactly its installed frames");
        Suite(nameof(EtecoonAnimationsVerification.VerifyEtecoonAnimations), () => EtecoonAnimationsVerification.VerifyEtecoonAnimations(rom));

        Console.WriteLine("Etecoon: the real initializer, every loop and the four-cycle flex pass.");
    }

    /// <summary>An initialized Etecoon in the first enemy slot.</summary>
    private static (RoomEnemySystem Enemies, RoomEnemySlot Slot, EtecoonEnemyState State) NewEtecoon()
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Etecoon, slot.Definition);
        slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = 0xa7 });
        EtecoonBehavior.InitializeEtecoon(slot);
        return (enemies, slot, enemies.EtecoonStates[0] ?? throw new InvalidOperationException("The Etecoon has no state."));
    }

    /// <summary>Runs the Etecoon animation verification.</summary>
    [VerificationCommand("--etecoon-instruction-program-definitions")]
    internal static int EtecoonInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyEtecoonInstructionProgramDefinitions), () => VerifyEtecoonInstructionProgramDefinitions());
        return 0;
    }
}
