using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Metroids;
using SuperMetroid.Core.Enemies.Metroids.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.Metroids;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Mochtroid's flight and attached loops and its change-detecting install.</summary>
internal static class MochtroidBehaviorVerification
{
    /// <summary>
    /// The real initializer installs the free-flight loop, which pulses on time; switching to the attached loop starts
    /// it, while re-requesting the installed loop leaves the running animation alone, as the native change detector does.
    /// </summary>
    internal static void VerifyMochtroidBehavior(SuperMetroidAddressSpace rom)
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(rom);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.EnemyDefinitionPointer = EnemyDefinitionId.Mochtroid;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = MochtroidFrameDefinitions.Bank };
        enemies.MochtroidBehavior.InitializeMochtroid(slot);
        MochtroidEnemyState state = enemies.MochtroidStates[0] ?? throw new InvalidOperationException("Mochtroid has no state.");
        AssertEqual((MochtroidProgram?)MochtroidProgram.FreeFlight, state.InstalledProgram, "initializer installs free flight");
        AssertEqual(EnemyInstructionRunner.ExpectedLoop(MochtroidAnimations.Programs, MochtroidProgram.FreeFlight, MochtroidFrameDefinitions.Spritemap),
            EnemyInstructionRunner.ShownFrames(enemies, slot, EnemyInstructionRunner.LoopUpdates(MochtroidAnimations.Programs, MochtroidProgram.FreeFlight)),
            "Mochtroid free-flight loop");

        AnimationProgramCursor<MochtroidProgram> running = state.Program;
        MochtroidBehavior.SetMochtroidProgram(slot, state, MochtroidProgram.FreeFlight);
        AssertEqual(running, state.Program, "re-requesting the installed loop keeps it running");

        MochtroidBehavior.SetMochtroidProgram(slot, state, MochtroidProgram.Attached);
        AssertEqual(AnimationProgramCursor<MochtroidProgram>.Start(MochtroidProgram.Attached), state.Program, "switching starts the attached loop");
        AssertEqual(EnemyInstructionRunner.ExpectedLoop(MochtroidAnimations.Programs, MochtroidProgram.Attached, MochtroidFrameDefinitions.Spritemap),
            EnemyInstructionRunner.ShownFrames(enemies, slot, EnemyInstructionRunner.LoopUpdates(MochtroidAnimations.Programs, MochtroidProgram.Attached)),
            "Mochtroid attached loop");
        Console.WriteLine("  Mochtroid behavior: both pulse loops and the change-detecting install pass.");
    }

    /// <summary>Runs the Mochtroid behavior check and its program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--mochtroid-instruction-mechanics")]
    internal static int MochtroidBehaviorCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyMochtroidBehavior), () => VerifyMochtroidBehavior(rom));
        Suite(nameof(MochtroidAnimationsVerification.VerifyMochtroidAnimations), () => MochtroidAnimationsVerification.VerifyMochtroidAnimations(rom));
        return 0;
    }
}
