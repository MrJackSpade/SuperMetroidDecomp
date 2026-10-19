using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Owtch;
using SuperMetroid.Core.Enemies.Owtch.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.Owtch;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Owtch's facing animations and the direction their cues hand to main AI.</summary>
internal static class OwtchBehaviorVerification
{
    /// <summary>
    /// The initializer starts the left animation for even states and the right one for odd states; each animation's cue
    /// sets main AI's walking direction, and its three-pose shell loop runs on time.
    /// </summary>
    internal static void VerifyOwtchBehavior()
    {
        foreach ((OwtchBehaviorState initial, OwtchProgram program, OwtchProgram loop, OwtchBehaviorState walking) in new[]
                 {
                     (OwtchBehaviorState.MovingRight, OwtchProgram.MovingLeft, OwtchProgram.MovingLeftLoop, OwtchBehaviorState.MovingLeft),
                     (OwtchBehaviorState.MovingLeft, OwtchProgram.MovingRight, OwtchProgram.MovingRightLoop, OwtchBehaviorState.MovingRight),
                 })
        {
            var enemies = new RoomEnemySystem(new EnemySession());
            enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(EnemyDefinitionId.Owtch, slot.Definition);
            slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = OwtchFrameToolingDefinitions.Bank });
            slot.SetSpawnParameter1((ushort)initial);
            OwtchBehavior.InitializeOwtch(slot);
            OwtchEnemyState state = enemies.OwtchStates[0] ?? throw new InvalidOperationException("Owtch has no state.");
            OwtchProgram started = initial == OwtchBehaviorState.MovingLeft ? OwtchProgram.MovingLeft : OwtchProgram.MovingRight;
            AssertEqual(AnimationProgramCursor<OwtchProgram>.Start(started), state.Program, $"Owtch state {initial} starts {started}");

            state.Program = AnimationProgramCursor<OwtchProgram>.Start(program);
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(OwtchAnimations.Programs, loop, OwtchFrameToolingDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(OwtchAnimations.Programs, loop)), $"Owtch {program} loop");
            AssertEqual(walking, state.Behavior, $"Owtch {program} cue sets main AI's direction");
        }
        Console.WriteLine("  Owtch behavior: the initializer picks each facing, the cues set the walking direction and the loops run on time.");
    }

    /// <summary>Runs the Owtch behavior check and its program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--owtch-instruction-mechanics")]
    internal static int OwtchBehaviorCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyOwtchBehavior), () => VerifyOwtchBehavior());
        Suite(nameof(OwtchAnimationsVerification.VerifyOwtchAnimations), () => OwtchAnimationsVerification.VerifyOwtchAnimations(rom));
        return 0;
    }
}
