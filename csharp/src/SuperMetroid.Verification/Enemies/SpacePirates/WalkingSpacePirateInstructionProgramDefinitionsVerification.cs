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
            expectedSegment: WalkingSpacePirateSegment.WalkingLeft,
            expectedHeading: HorizontalDirection.Left));
        Suite(nameof(VerifyProgram), () => VerifyProgram(
            WalkingSpacePirateSegment.WalkingRight,
            frames: 81,
            expectedSegment: WalkingSpacePirateSegment.WalkingRight,
            expectedHeading: HorizontalDirection.Right));
        Suite(nameof(VerifyProgram), () => VerifyProgram(
            WalkingSpacePirateSegment.FlinchFacingLeft,
            frames: 17,
            expectedSegment: WalkingSpacePirateSegment.WalkingLeft,
            expectedHeading: HorizontalDirection.Left));
        Suite(nameof(VerifyProgram), () => VerifyProgram(
            WalkingSpacePirateSegment.FlinchFacingRight,
            frames: 17,
            expectedSegment: WalkingSpacePirateSegment.WalkingRight,
            expectedHeading: HorizontalDirection.Right));
        Suite(nameof(VerifyProgram), () => VerifyProgram(
            WalkingSpacePirateSegment.LookingFacingLeft,
            frames: 125,
            expectedSegment: WalkingSpacePirateSegment.WalkingRight,
            expectedHeading: HorizontalDirection.Right));
        Suite(nameof(VerifyProgram), () => VerifyProgram(
            WalkingSpacePirateSegment.LookingFacingRight,
            frames: 125,
            expectedSegment: WalkingSpacePirateSegment.WalkingLeft,
            expectedHeading: HorizontalDirection.Left));
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
            (RoomEnemySystem enemies, RoomEnemyPlace place,
                WalkingSpacePirateEnemyState state, SamusState samus) = CreateSystem();
            samus.Position = samus.Position.WithWholeX(movingRight ? (ushort)0x0180 : (ushort)0x0080);
            state.Playback = new(WalkingSpacePirateAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);
            place.AnimationClock.StepOnNextUpdate();
            RunFrames(enemies, place, samus, 113);
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
            HorizontalDirection expectedHeading)
        {
            (RoomEnemySystem enemies, RoomEnemyPlace place,
                WalkingSpacePirateEnemyState state, SamusState samus) = CreateSystem();
            state.Playback = new(WalkingSpacePirateAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);
            place.AnimationClock.StepOnNextUpdate();
            RunFrames(enemies, place, samus, frames);
            // The walk loop has shown its first pose and stands before its second: the walk's third beat, after its
            // direction cue (#2166).
            AssertEqual(new AnimationPlayback<WalkingSpacePirateSegment>(WalkingSpacePirateAnimations.StartingWith(program), PlaceOf(program, expectedSegment, 2)), state.Playback,
                $"walking Pirate program {program} playback");
            AssertEqual(WalkingSpacePirateAction.Walking, state.Action,
                $"walking Pirate program {program} function");
            AssertEqual(expectedHeading, state.Heading,
                $"walking Pirate program {program} walks its way (#2165)");
        }

        // The place in the animation starting at start whose next beat is the segment's beat.
        static AnimationPlace PlaceOf(WalkingSpacePirateSegment start, WalkingSpacePirateSegment segment, int beat) =>
            AnimationSegments<WalkingSpacePirateSegment, EnemyFrameId, WalkingSpacePirateCue>.PlaceOf(WalkingSpacePirateAnimations.StartingWith(start), segment, beat);

        WalkingSpacePirateInstructionProgramDefinitionsCreateSystemResult CreateSystem()
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
            place.Bind(EnemyDefinitionId.PirateGreyWalking, place.Definition);
            place.Bind(place.DefinitionId, default(RoomEnemyDefinition));
            // A left-facing Pirate's population entry with no options and no patrol reach, as the room load gives it.
            place.SetSpawnSettings(new WalkingSpacePirateSpawnSettings(WalkingSpacePirateOptions.None, PatrolReach: 0));
            place.Position = place.Position.WithWhole(new(0x0100, 0x0100));
            WalkingSpacePirateBehavior.InitializeWalkingSpacePirate(place);
            var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
            {
                Position = FixedPosition<RoomPixels>.FromWhole(new(0x0080, 0x0100)),
            };
            return new WalkingSpacePirateInstructionProgramDefinitionsCreateSystemResult(enemies, place, (enemies.WalkingSpacePirateStates[0] ?? throw new InvalidOperationException("Verification requires the initialized WalkingSpacePirate state.")), samus);
        }

        static void RunFrames(
            RoomEnemySystem enemies,
            RoomEnemyPlace place,
            SamusState samus,
            int frames)
        {
            object?[] arguments =
                [place, samus, null, Position16<RoomPixels>.Zero, default(GameplayAction)];
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
