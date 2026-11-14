using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Input;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Enemies.SpacePirates;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Enemies.Artwork;

/// <summary>Checks walking Space Pirate movement, flinch, look, and laser lists against bank B2.</summary>
internal static class WalkingSpacePirateInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge as the oracle for Pirate instruction words and frames.</summary>
    internal static void VerifyWalkingSpacePirateInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyWalkingSpacePirateInstructionProgramDefinitions), () => VerifyWalkingSpacePirateInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks the typed programs against the cartridge and exercises every program and both three-laser attacks.</summary>
    internal static void VerifyWalkingSpacePirateInstructionProgramDefinitions(
        CartridgeImportAddressSpace rom)
    {
        Suite(nameof(WalkingSpacePirateAnimationsVerification.VerifyWalkingSpacePirateAnimations), () => WalkingSpacePirateAnimationsVerification.VerifyWalkingSpacePirateAnimations(rom));
        Suite(nameof(VerifyProgram), () => VerifyProgram(
            WalkingSpacePirateSegment.WalkingLeft,
            frames: 81,
            expectedSegment: WalkingSpacePirateSegment.WalkingLeftLoop,
            expectedAction: WalkingSpacePirateAction.WalkingLeft));
        Suite(nameof(VerifyProgram), () => VerifyProgram(
            WalkingSpacePirateSegment.WalkingRight,
            frames: 81,
            expectedSegment: WalkingSpacePirateSegment.WalkingRightLoop,
            expectedAction: WalkingSpacePirateAction.WalkingRight));
        Suite(nameof(VerifyProgram), () => VerifyProgram(
            WalkingSpacePirateSegment.FlinchFacingLeft,
            frames: 17,
            expectedSegment: WalkingSpacePirateSegment.WalkingLeftLoop,
            expectedAction: WalkingSpacePirateAction.WalkingLeft));
        Suite(nameof(VerifyProgram), () => VerifyProgram(
            WalkingSpacePirateSegment.FlinchFacingRight,
            frames: 17,
            expectedSegment: WalkingSpacePirateSegment.WalkingRightLoop,
            expectedAction: WalkingSpacePirateAction.WalkingRight));
        Suite(nameof(VerifyProgram), () => VerifyProgram(
            WalkingSpacePirateSegment.LookingFacingLeft,
            frames: 125,
            expectedSegment: WalkingSpacePirateSegment.WalkingRightLoop,
            expectedAction: WalkingSpacePirateAction.WalkingRight));
        Suite(nameof(VerifyProgram), () => VerifyProgram(
            WalkingSpacePirateSegment.LookingFacingRight,
            frames: 125,
            expectedSegment: WalkingSpacePirateSegment.WalkingLeftLoop,
            expectedAction: WalkingSpacePirateAction.WalkingLeft));
        Suite(nameof(VerifyAttack), () => VerifyAttack(movingRight: false));
        Suite(nameof(VerifyAttack), () => VerifyAttack(movingRight: true));

        Console.WriteLine("Walking space pirate: programs and their cues pass through real behavior.");

        void VerifyAttack(bool movingRight)
        {
            WalkingSpacePirateSegment program = movingRight
                ? WalkingSpacePirateSegment.FireLasersRight
                : WalkingSpacePirateSegment.FireLasersLeft;
            // The movement choice continues the attack at its own start, which has run its function cue and shown its
            // first pose again.
            var expected = new AnimationPlayback<WalkingSpacePirateSegment>(WalkingSpacePirateAnimations.StartingWith(program), PlaceOf(program, program, 2));
            (RoomEnemySystem enemies, RoomEnemySlot slot,
                WalkingSpacePirateEnemyState state, SamusState samus) = CreateSystem();
            samus.Position = samus.Position.WithWholeX(movingRight ? (ushort)0x0180 : (ushort)0x0080);
            state.Playback = new(WalkingSpacePirateAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);
            slot.AnimationClock.StepOnNextUpdate();
            RunFrames(enemies, slot, samus, 113);
            AssertEqual(expected, state.Playback,
                $"walking Pirate {(movingRight ? "right" : "left")} attack loops");
            AssertEqual(3, enemies.Projectiles.EnemyProjectiles.Count(p => p.IsActive && p.Kind == RoomEnemyProjectileKind.PirateMotherBrainLaser),
                $"walking Pirate {(movingRight ? "right" : "left")} laser count");
            AssertEqual(EnemySoundEffectDefinitions.PirateMotherBrainLaserSound, (enemies.Audio.LatestSound ?? throw new InvalidOperationException("Verification requires a pending space pirate sound.")),
                $"walking Pirate {(movingRight ? "right" : "left")} laser sound");
        }

        void VerifyProgram(
            WalkingSpacePirateSegment program,
            int frames,
            WalkingSpacePirateSegment expectedSegment,
            WalkingSpacePirateAction expectedAction)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot,
                WalkingSpacePirateEnemyState state, SamusState samus) = CreateSystem();
            state.Playback = new(WalkingSpacePirateAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);
            slot.AnimationClock.StepOnNextUpdate();
            RunFrames(enemies, slot, samus, frames);
            // The walk loop has shown its first pose and stands before its second.
            AssertEqual(new AnimationPlayback<WalkingSpacePirateSegment>(WalkingSpacePirateAnimations.StartingWith(program), PlaceOf(program, expectedSegment, 1)), state.Playback,
                $"walking Pirate program {program} playback");
            AssertEqual(expectedAction, state.Action,
                $"walking Pirate program {program} function");
        }

        // The place in the animation starting at start whose next beat is the segment's beat.
        static AnimationPlace PlaceOf(WalkingSpacePirateSegment start, WalkingSpacePirateSegment segment, int beat) =>
            AnimationSegments<WalkingSpacePirateSegment, EnemyFrameId, WalkingSpacePirateCue>.PlaceOf(WalkingSpacePirateAnimations.StartingWith(start), segment, beat);

        WalkingSpacePirateInstructionProgramDefinitionsCreateSystemResult CreateSystem()
        {
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(EnemyDefinitionId.PirateGreyWalking, slot.Definition);
            slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
            // A left-facing Pirate's population entry with no options and no patrol reach, as the room load gives it.
            slot.SetSpawnSettings(new WalkingSpacePirateSpawnSettings(WalkingSpacePirateOptions.None, PatrolReach: 0));
            slot.Position = slot.Position.WithWhole(new(0x0100, 0x0100));
            WalkingSpacePirateBehavior.InitializeWalkingSpacePirate(slot);
            var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
            {
                Position = FixedPosition<RoomPixels>.FromWhole(new(0x0080, 0x0100)),
            };
            return new WalkingSpacePirateInstructionProgramDefinitionsCreateSystemResult(enemies, slot, (enemies.WalkingSpacePirateStates[0] ?? throw new InvalidOperationException("Verification requires the initialized WalkingSpacePirate state.")), samus);
        }

        static void RunFrames(
            RoomEnemySystem enemies,
            RoomEnemySlot slot,
            SamusState samus,
            int frames)
        {
            object?[] arguments =
                [slot, samus, null, Position16<RoomPixels>.Zero, ControllerButton.None];
            for (int frame = 0; frame < frames; frame++)
                EnemyInstructionRunner.Process(arguments);
        }
    }

    /// <summary>Runs walking Pirate instruction checks from the verification command dispatcher.</summary>
    [VerificationCommand("--walking-space-pirate-instruction-mechanics")]
    internal static int WalkingSpacePirateInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyWalkingSpacePirateInstructionProgramDefinitions), () => VerifyWalkingSpacePirateInstructionProgramDefinitions());
        return 0;
    }
}
