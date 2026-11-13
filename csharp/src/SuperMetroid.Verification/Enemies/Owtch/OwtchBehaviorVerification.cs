using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Owtch;
using SuperMetroid.Core.Enemies.Owtch.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Owtch;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Owtch's facing animations and the direction their cues hand to main AI.</summary>
internal static class OwtchBehaviorVerification
{
    /// <summary>
    /// The initializer starts the left animation for even states and the right one for odd states; each animation's cue
    /// sets main AI's walking direction, and its three-pose shell loop runs on time.
    /// </summary>
    internal static void VerifyOwtchBehavior()
    {
        foreach ((OwtchBehaviorState initial, OwtchSegment program, OwtchSegment loop, OwtchBehaviorState walking) in new[]
                 {
                     new VerifyOwtchBehaviorCase(OwtchBehaviorState.MovingRight, OwtchSegment.MovingLeft, OwtchSegment.MovingLeftLoop, OwtchBehaviorState.MovingLeft),
                     new VerifyOwtchBehaviorCase(OwtchBehaviorState.MovingLeft, OwtchSegment.MovingRight, OwtchSegment.MovingRightLoop, OwtchBehaviorState.MovingRight),
                 })
        {
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(EnemyDefinitionId.Owtch, slot.Definition);
            slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
            slot.SetSpawnParameter1(EnemyDefinitionId.Owtch, OwtchNativeWords.Behaviors.Word(initial));
            OwtchBehavior.InitializeOwtch(slot);
            OwtchEnemyState state = enemies.OwtchStates[0] ?? throw new InvalidOperationException("Owtch has no state.");
            OwtchSegment started = initial == OwtchBehaviorState.MovingLeft ? OwtchSegment.MovingLeft : OwtchSegment.MovingRight;
            AssertEqual(new AnimationPlayback<OwtchSegment>(OwtchAnimations.StartingWith(started), AnimationPlace.StartOfAnimation), state.Playback, $"Owtch state {initial} starts {started}");

            state.Playback = new(OwtchAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(OwtchAnimations.Segments, loop, OwtchFrameToolingDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(OwtchAnimations.Segments, loop)), $"Owtch {program} loop");
            AssertEqual(walking, state.Behavior, $"Owtch {program} cue sets main AI's direction");
        }
        Console.WriteLine("  Owtch behavior: the initializer picks each facing, the cues set the walking direction and the loops run on time.");
    }

    /// <summary>Runs the Owtch behavior check and its program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--owtch-instruction-mechanics")]
    internal static int OwtchBehaviorCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyOwtchBehavior), () => VerifyOwtchBehavior());
        Suite(nameof(OwtchAnimationsVerification.VerifyOwtchAnimations), () => OwtchAnimationsVerification.VerifyOwtchAnimations(rom));
        return 0;
    }
}
