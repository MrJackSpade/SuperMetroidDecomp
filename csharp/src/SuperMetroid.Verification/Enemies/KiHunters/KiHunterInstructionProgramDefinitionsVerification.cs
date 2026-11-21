using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Input;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core;
namespace SuperMetroid.Verification.Enemies.KiHunters;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks KiHunter body and wing instruction programs, acid spawning, and compiled visual selectors.</summary>
internal static class KiHunterInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge oracle for KiHunter instruction verification.</summary>
    internal static void VerifyKiHunterInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyKiHunterInstructionProgramDefinitions), () => VerifyKiHunterInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Exercises thirteen flight, swoop, wing, ground, and spit lists while forbidding migrated bank-$A8 reads.</summary>
    internal static void VerifyKiHunterInstructionProgramDefinitions(
        CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyFlying), () => VerifyFlying(KiHunterSegment.FlyingLeft, movingRight: false));
        Suite(nameof(VerifyFlying), () => VerifyFlying(KiHunterSegment.FlyingRight, movingRight: true));
        Suite(nameof(VerifySwoop), () => VerifySwoop(KiHunterSegment.SwoopLeft, movingRight: false));
        Suite(nameof(VerifySwoop), () => VerifySwoop(KiHunterSegment.SwoopRight, movingRight: true));
        Suite(nameof(VerifyWingLoop), () => VerifyWingLoop(KiHunterSegment.WingsLeft, "left"));
        Suite(nameof(VerifyWingLoop), () => VerifyWingLoop(KiHunterSegment.WingsRight, "right"));
        Suite(nameof(VerifyDetachedWings), () => VerifyDetachedWings());
        Suite(nameof(VerifyJump), () => VerifyJump(KiHunterSegment.JumpLeft, "left"));
        Suite(nameof(VerifyJump), () => VerifyJump(KiHunterSegment.JumpRight, "right"));
        Suite(nameof(VerifyLanding), () => VerifyLanding(KiHunterSegment.LandLeft, "left"));
        Suite(nameof(VerifyLanding), () => VerifyLanding(KiHunterSegment.LandRight, "right"));
        Suite(nameof(VerifySpit), () => VerifySpit(KiHunterSegment.SpitLeft, movingRight: false));
        Suite(nameof(VerifySpit), () => VerifySpit(KiHunterSegment.SpitRight, movingRight: true));
        Suite(nameof(VerifySwoopAngularMotion), () => VerifySwoopAngularMotion(swoopLeft: false));
        Suite(nameof(VerifySwoopAngularMotion), () => VerifySwoopAngularMotion(swoopLeft: true));

        Suite(nameof(KiHunterAnimationsVerification.VerifyKiHunterAnimations), () => KiHunterAnimationsVerification.VerifyKiHunterAnimations(rom));

        Console.WriteLine("KiHunter: flight, swoop, wing, detached-wing, jump, landing and spit programs with their cues pass.");

        void VerifyFlying(KiHunterSegment program, bool movingRight)
        {
            (RoomEnemySystem enemies, RoomEnemyPlace body, _, KiHunterBodyState state, _) =
                CreatePair();
            state.Heading = movingRight ? HorizontalDirection.Right : HorizontalDirection.Left;
            Install(body, program);
            RunFrames(enemies, body, 7);
            AssertEqual(program, Playing(state.Playback),
                $"KiHunter {(movingRight ? "right" : "left")} flight remains in steady list");
        }

        void VerifySwoop(KiHunterSegment program, bool movingRight)
        {
            (RoomEnemySystem enemies, RoomEnemyPlace body, _, KiHunterBodyState state, _) =
                CreatePair();
            state.Heading = movingRight ? HorizontalDirection.Right : HorizontalDirection.Left;
            Install(body, program);
            RunFrames(enemies, body, 55);
            KiHunterSegment idle = movingRight
                ? KiHunterSegment.FlyingRight
                : KiHunterSegment.FlyingLeft;
            AssertEqual(idle, Playing(state.Playback),
                $"KiHunter {(movingRight ? "right" : "left")} swoop returns to flight");
        }

        void VerifyWingLoop(KiHunterSegment program, string direction)
        {
            (RoomEnemySystem enemies, _, RoomEnemyPlace wings, _, KiHunterWingState wingState) = CreatePair();
            Install(wings, program);
            RunFrames(enemies, wings, 7);
            AssertEqual(program, Playing(wingState.Playback),
                $"KiHunter {direction} wing loop remains bounded");
        }

        void VerifyDetachedWings()
        {
            (RoomEnemySystem enemies, _, RoomEnemyPlace wings, _, KiHunterWingState wingState) = CreatePair();
            Install(wings, KiHunterSegment.DetachedWings);
            RunFrames(enemies, wings, 3);
            AssertAsleep(wingState.Playback, KiHunterSegment.DetachedWings, "KiHunter detached wings reach terminal sleep");
        }

        void VerifyJump(KiHunterSegment program, string direction)
        {
            (RoomEnemySystem enemies, RoomEnemyPlace body, _, KiHunterBodyState state, _) =
                CreatePair();
            Install(body, program);
            RunFrames(enemies, body, 40);
            AssertEqual(KiHunterAction.GroundJump, state.Action,
                $"KiHunter {direction} jump installs ground-jump owner");
            AssertAsleep(state.Playback, program, $"KiHunter {direction} jump reaches terminal sleep");
        }

        void VerifyLanding(KiHunterSegment program, string direction)
        {
            (RoomEnemySystem enemies, RoomEnemyPlace body, _, KiHunterBodyState state, _) =
                CreatePair();
            Install(body, program);
            RunFrames(enemies, body, 45);
            AssertEqual(KiHunterAction.GroundWait, state.Action,
                $"KiHunter {direction} landing installs ground-wait owner");
            AssertAsleep(state.Playback, program, $"KiHunter {direction} landing reaches terminal sleep");
        }

        void VerifySpit(KiHunterSegment program, bool movingRight)
        {
            (RoomEnemySystem enemies, RoomEnemyPlace body, _, KiHunterBodyState state, _) =
                CreatePair();
            Install(body, program);
            RunFrames(enemies, body, 90);
            AssertEqual(KiHunterAction.GroundWait, state.Action,
                $"KiHunter {(movingRight ? "right" : "left")} spit restores ground wait");
            AssertEqual(24, state.WaitTimer.UpdatesLeft,
                $"KiHunter {(movingRight ? "right" : "left")} spit wait timer");
            AssertTrue(enemies.Audio.SoundRequests.Any(request => request.SoundEffect == EnemySound.AcidSpit),
                $"KiHunter {(movingRight ? "right" : "left")} spit sound");
            // The acid is one kind whichever way it is spat; its velocity is its way (#2278).
            AssertEqual(1, enemies.Projectiles.EnemyProjectiles.Count(projectile => projectile.Kind == RoomEnemyProjectileKind.KiHunterAcidSpit &&
                    projectile.Velocity.X.IsNegative != movingRight),
                $"KiHunter {(movingRight ? "right" : "left")} acid spawn");
            AssertAsleep(state.Playback, program, $"KiHunter {(movingRight ? "right" : "left")} spit reaches terminal sleep");
        }

        // Drives the swoop through production main AI over open terrain. Each update adds the 16.16 angular
        // acceleration with the fraction carrying into the whole word, caps the whole word alone (the fraction keeps
        // accumulating beneath the cap), steps the angle by the whole word, and ends the swoop on the update the
        // angle reaches a full turn (rightward) or falls below a half turn (leftward). The expectation is the native
        // split-word arithmetic, computed here independently of the production fixed-point type.
        void VerifySwoopAngularMotion(bool swoopLeft)
        {
            (RoomEnemySystem enemies, RoomEnemyPlace body, _, KiHunterBodyState state, _) = CreatePair();
            SignedSixteenSixteen acceleration = SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel((swoopLeft
                ? -KiHunterMotionDefinitions.SwoopAcceleration
                : KiHunterMotionDefinitions.SwoopAcceleration).FractionsOfStep);
            ushort accelerationWhole = acceleration.UnsignedWholePixels;
            ushort accelerationFraction = acceleration.Fraction;
            ushort cap = swoopLeft ? unchecked((ushort)-2) : (ushort)2;
            ushort angle = swoopLeft ? (ushort)255 : (ushort)128;
            state.Action = KiHunterAction.Swooping;
            state.AngularVelocity = AngularSpeed.Zero;
            // A swoop heading left turns toward decreasing angles (#2338).
            state.Heading = swoopLeft ? HorizontalDirection.Left : HorizontalDirection.Right;
            state.SwoopAngle = RotationAngle.FromSteps(angle);
            state.SwoopTargetX = body.Position.X.Whole;
            state.SwoopOriginY = body.Position.Y.Whole;
            state.SwoopRadius = new Extent16<RoomPixels>(16, 16);
            RoomLevelData level = VerificationHelpersVerification.CreateEmptyRoom(new(32, 32));
            var frame = new ClockedEnemyAiFrame(
                RoomEnemyFrameFixtures.Standalone(level, 0) with { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, Samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Position = FixedPosition<RoomPixels>.FromWhole(Position16<RoomPixels>.Zero) } },
                new FrameCounters(SuperMetroid.Core.Compatibility.ShortFrameCount.Zero));

            ushort velocityWhole = 0;
            ushort velocityFraction = 0;
            bool cappedKeepingFraction = false;
            bool ended = false;
            bool swiped = false;
            for (int update = 1; update <= 200 && !ended; update++)
            {
                // The game swipes once the angle it starts an update at has passed its trigger, a flag keeping it from
                // restarting the swipe (#2473): below 240 for a leftward swoop, at or past 144 for a rightward one.
                swiped |= swoopLeft ? angle < 240 : angle >= 144;
                uint fractionSum = (uint)velocityFraction + accelerationFraction;
                velocityWhole = unchecked((ushort)(velocityWhole + accelerationWhole + fractionSum / 65536));
                velocityFraction = unchecked((ushort)fractionSum);
                bool capped = swoopLeft
                    ? unchecked((short)(velocityWhole - cap)) < 0
                    : unchecked((short)(velocityWhole - cap)) >= 0;
                if (capped)
                {
                    velocityWhole = cap;
                    cappedKeepingFraction |= velocityFraction != 0;
                }
                angle = unchecked((ushort)(angle + velocityWhole));
                ended = swoopLeft
                    ? unchecked((short)(angle - 128)) < 0
                    : unchecked((short)(angle - 256)) >= 0;

                enemies.KiHunterBehavior.RunKiHunterBodyMainAi(body, frame);
                string side = swoopLeft ? "left" : "right";
                AssertEqual(SignedSixteenSixteen.FromParts(unchecked((short)velocityWhole), velocityFraction), SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(state.AngularVelocity.FractionsOfStep),
                    $"KiHunter {side} swoop update {update} angular velocity carries and caps only its whole part");
                AssertEqual(RotationAngle.WrappedSteps(angle), state.SwoopAngle, $"KiHunter {side} swoop update {update} steps the angle by the whole velocity");
                AssertEqual(ended ? KiHunterAction.FlyingPatrol : swiped ? KiHunterAction.Swiping : KiHunterAction.Swooping, state.Action,
                    $"KiHunter {side} swoop update {update} ends only when the native angle test ends it, and swipes once its trigger is passed");
                if (!ended)
                {
                    AssertEqual(swiped, state.Playback.PlayingSegment == (swoopLeft ? KiHunterSegment.SwoopLeft : KiHunterSegment.SwoopRight),
                        $"KiHunter {side} swoop update {update} plays the swipe only once its trigger is passed");
                }
            }
            AssertTrue(ended, $"KiHunter {(swoopLeft ? "left" : "right")} swoop ends");
            AssertTrue(cappedKeepingFraction, $"KiHunter {(swoopLeft ? "left" : "right")} swoop caps a velocity whose fraction is kept");
        }

        CreatePairResult CreatePair()
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            Action<RoomEnemyPlace> initializeBody = fixturePlace => enemies.KiHunterBehavior.InitializeKiHunter(fixturePlace);
            Action<RoomEnemyPlace> initializeWings = fixturePlace => enemies.KiHunterBehavior.InitializeKiHunterWings(fixturePlace.HeldOccupant(), fixturePlace);

            RoomEnemyPlace body = enemies.Places.InUpdateOrder[0];
            body.Bind(EnemyDefinitionId.KihunterGreen, body.KeptDefinition);
            body.Bind(body.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
            // The header's collision half-extents: the terrain movers scan the rows the body spans.
            body.Radius = CompiledRoomEnemyDefinitions.Instance.ReadEnemyDefinition(EnemyDefinitionId.KihunterGreen).Radius;
            body.Position = body.Position.WithWhole(new(0x0080, 0x0080));
            RoomEnemyPlace wings = enemies.Places.InUpdateOrder[1];
            wings.Bind(EnemyDefinitionId.KihunterGreenWings, wings.KeptDefinition);
            wings.Bind(wings.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
            // A winged pair's population entries, as the room load gives them: the body keeps its wings, and the wing
            // entry's detachment health is never reached by these unhurt programs.
            body.SetSpawnSettings(new KiHunterSpawnSettings(KiHunterWings.Winged));
            wings.SetSpawnSettings(new KiHunterWingsSpawnSettings(DetachHealth: 0));
            initializeBody(body);
            initializeWings(wings);
            return new CreatePairResult(enemies, body, wings,
                (enemies.KiHunterBodyStates[0] ?? throw new InvalidOperationException("Verification requires the initialized KiHunter body state.")), (enemies.KiHunterWingStates[1] ?? throw new InvalidOperationException("Verification requires the initialized KiHunter wing state.")));
        }

        static void Install(RoomEnemyPlace place, KiHunterSegment program)
        {
            var playback = new AnimationPlayback<KiHunterSegment>(KiHunterAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);
            if (place.SpeciesState<KiHunterWingState>() is { } wings)
                wings.Playback = playback;
            else
                place.RequireSpeciesState<KiHunterBodyState>().Playback = playback;
            place.AnimationClock.Restart();
        }

        // The playback rests past the last beat of an animation that holds there.
        static void AssertAsleep(AnimationPlayback<KiHunterSegment> playback, KiHunterSegment program, string message)
        {
            AssertEqual(program, Playing(playback), message);
            AssertTrue(playback.Animation.End is AnimationEnd.Hold &&
                playback.Place == AnimationPlace.HoldingIn(playback.Animation.Phases.Count - 1, playback.Animation.Phases[^1].BeatCount), message);
        }

        // The segment the playback's current phase plays.
        static KiHunterSegment Playing(AnimationPlayback<KiHunterSegment> playback) =>
            playback.Animation.Phases[playback.Place.Phase].Segment;

        static void RunFrames(RoomEnemySystem enemies, RoomEnemyPlace place, int frames)
        {
            object?[] arguments = [place, null, null, Position16<RoomPixels>.Zero, default(GameplayAction)];
            for (int frame = 0; frame < frames; frame++)
                EnemyInstructionRunner.Process(arguments);
        }
    }

    /// <summary>Runs the KiHunter instruction check against the local retail cartridge image.</summary>
    [VerificationCommand("--kihunter-instruction-mechanics")]
    internal static int KihunterInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyKiHunterInstructionProgramDefinitions), () => VerifyKiHunterInstructionProgramDefinitions());
        return 0;
    }
}
