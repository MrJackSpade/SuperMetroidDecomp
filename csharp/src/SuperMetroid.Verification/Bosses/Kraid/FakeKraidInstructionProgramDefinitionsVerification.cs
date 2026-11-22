using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.Kraid;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Fake Kraid's typed programs and their cues through its real initializer, walks and spits.</summary>
internal static class FakeKraidInstructionProgramDefinitionsVerification
{
    /// <summary>Enough walk decisions that the spit decision cannot expire during a sixty-update run.</summary>
    private const int UnexpiredSpitDecisions = 256;

    /// <summary>Loads the retail cartridge oracle for Fake Kraid instruction verification.</summary>
    internal static void VerifyFakeKraidInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyFakeKraidInstructionProgramDefinitions), () => VerifyFakeKraidInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks the typed programs against the cartridge and exercises both initial facings, four walk programs and two spit programs.</summary>
    internal static void VerifyFakeKraidInstructionProgramDefinitions(
        CartridgeImportAddressSpace rom)
    {

        int population = EnemyRomTablePointers.Kraid.FakeKraidPopulationRecord;
        EnemyDefinitionId populationDefinition = EnemyDefinitionIdsConstants.FromHeaderPointer(LittleEndianBytes.Word(FakeKraidInstructionProgramDefinitionsRomByte(rom, population), FakeKraidInstructionProgramDefinitionsRomByte(rom, population + 1)));
        ushort extraProperties = LittleEndianBytes.Word(FakeKraidInstructionProgramDefinitionsRomByte(rom, population + 10), FakeKraidInstructionProgramDefinitionsRomByte(rom, population + 11));
        AssertEqual(EnemyDefinitionId.FakeKraid, populationDefinition,
            "retail Fake Kraid population matches the installed visual family");
        AssertTrue(!SuperMetroid.AssetExtraction.Enemies.EnemyPropertyWordCodec.Decode(0, extraProperties).CollidesByFrameHitboxes,
            "retail Fake Kraid uses ordinary OAM composition");

        Suite(nameof(FakeKraidAnimationsVerification.VerifyFakeKraidAnimations), () => FakeKraidAnimationsVerification.VerifyFakeKraidAnimations(rom));
        var level = new RoomLevelData(new Extent16<RoomBlocks>(32, 32), new RoomLevelPlanes(new RoomTerrainBlock[1024], new RoomBlockBehavior[1024], new RoomBlockAppearance[1024]), RoomLevelData.NothingBeyondGrid(), []);

        Suite(nameof(VerifyInitializer), () => VerifyInitializer(movingRight: false));
        Suite(nameof(VerifyInitializer), () => VerifyInitializer(movingRight: true));

        ExerciseWalk(
            FakeKraidSegment.StepForwardsFacingLeft,
            movingRight: false,
            walking: HorizontalDirection.Left);
        ExerciseWalk(
            FakeKraidSegment.StepBackwardsFacingLeft,
            movingRight: false,
            walking: HorizontalDirection.Right);
        ExerciseWalk(
            FakeKraidSegment.StepForwardsFacingRight,
            movingRight: true,
            walking: HorizontalDirection.Right);
        ExerciseWalk(
            FakeKraidSegment.StepBackwardsFacingRight,
            movingRight: true,
            walking: HorizontalDirection.Left);
        ExerciseSpit(
            FakeKraidSegment.FireSpitFacingLeft,
            movingRight: false);
        ExerciseSpit(
            FakeKraidSegment.FireSpitFacingRight,
            movingRight: true);
        ExerciseWalkTimerExpiry(
            FakeKraidSegment.StepForwardsFacingLeft,
            movingRight: false,
            walking: HorizontalDirection.Left);
        ExerciseWalkTimerExpiry(
            FakeKraidSegment.StepBackwardsFacingRight,
            movingRight: true,
            walking: HorizontalDirection.Left);

        Console.WriteLine("Fake Kraid: both initial facings, four walk and two spit programs with their cues, and the walk timer's expiry at zero pass.");

        void VerifyInitializer(bool movingRight)
        {
            RoomEnemySystem enemies = CreateSystem(
                movingRight,
                out RoomEnemyPlace place,
                out _,
                out FakeKraidEnemyState state);
            AssertEqual(
                new AnimationPlayback<FakeKraidSegment>(FakeKraidAnimations.StartingWith(movingRight
                    ? FakeKraidSegment.StepForwardsFacingRight
                    : FakeKraidSegment.StepForwardsFacingLeft), AnimationPlace.StartOfAnimation),
                state.Playback,
                $"Fake Kraid initializer selects {(movingRight ? "right" : "left")} program");
        }

        void ExerciseWalk(FakeKraidSegment program, bool movingRight, HorizontalDirection walking)
        {
            RoomEnemySystem enemies = CreateSystem(
                movingRight,
                out RoomEnemyPlace place,
                out SamusState samus,
                out FakeKraidEnemyState state);
            state.Playback = new AnimationPlayback<FakeKraidSegment>(FakeKraidAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);
            place.AnimationClock.StepOnNextUpdate();
            state.Walking = walking;
            state.Facing = movingRight ? HorizontalDirection.Right : HorizontalDirection.Left;
            state.WalkTurnWait.LastFor(0x0100);
            state.SpitWait.LastFor(0x0100);

            RunFrames(enemies, place, state, samus, level, 60);

            AssertEqual(0x00ff, state.WalkTurnWait.UpdatesLeft,
                $"Fake Kraid {program} executes its walk callback");
            AssertEqual(0x00ff, state.SpitWait.UpdatesLeft,
                $"Fake Kraid {program} executes its action callback");
        }

        void ExerciseSpit(FakeKraidSegment program, bool movingRight)
        {
            RoomEnemySystem enemies = CreateSystem(
                movingRight,
                out RoomEnemyPlace place,
                out SamusState samus,
                out FakeKraidEnemyState state);
            state.Playback = new AnimationPlayback<FakeKraidSegment>(FakeKraidAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);
            place.AnimationClock.StepOnNextUpdate();
            state.WalkTurnWait.LastFor(0x0100);
            state.SpitWait.LastFor(0x0100);

            RunFrames(enemies, place, state, samus, level, 60);

            // The instruction runner moves no projectile, so every spit the program spawned is still in the pool.
            AssertEqual(2, enemies.Projectiles.EnemyProjectiles.Count(projectile =>
                    projectile.IsActive && projectile.Kind == RoomEnemyProjectileKind.FakeKraidSpit),
                $"Fake Kraid {program} spawns its projectile pair");
            AssertEqual((SoundEffectId?)KraidSoundDefinitions.FakeKraidCry, enemies.Audio.LatestSound,
                $"Fake Kraid {program} publishes its cry");
        }

        void ExerciseWalkTimerExpiry(FakeKraidSegment program, bool movingRight, HorizontalDirection walking)
        {
            // A walk on its last step turns round on the walk callback without stepping, and starts a walk of seven
            // through ten steps (seven for the fixture's zero random word).
            RoomEnemySystem enemies = CreateSystem(
                movingRight,
                out RoomEnemyPlace place,
                out SamusState samus,
                out FakeKraidEnemyState state);
            state.Playback = new AnimationPlayback<FakeKraidSegment>(FakeKraidAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);
            place.AnimationClock.StepOnNextUpdate();
            state.Walking = walking;
            state.Facing = movingRight ? HorizontalDirection.Right : HorizontalDirection.Left;
            state.WalkTurnWait.LastFor(1);
            state.SpitWait.LastFor(UnexpiredSpitDecisions);
            ushort startX = place.Position.X.Whole;

            RunFrames(enemies, place, state, samus, level, 60);

            AssertEqual(walking == HorizontalDirection.Right ? HorizontalDirection.Left : HorizontalDirection.Right, state.Walking,
                $"Fake Kraid {program} turns round on its walk's last step");
            AssertEqual(startX, place.Position.X.Whole,
                $"Fake Kraid {program} does not step on the reversing callback");
            AssertEqual(7, state.WalkTurnWait.UpdatesLeft,
                $"Fake Kraid {program} starts its next walk from the random word");
        }

        RoomEnemySystem CreateSystem(
            bool movingRight,
            out RoomEnemyPlace place,
            out SamusState samus,
            out FakeKraidEnemyState state)
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
            enemies.VerificationServices().ReadRandom = () => 0;
            place = enemies.Places.InUpdateOrder[0];
            place.Bind(EnemyDefinitionId.FakeKraid, place.KeptDefinition);
            place.Bind(place.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
            // The header's collision half-extents: the terrain movers scan the rows the body spans.
            place.Radius = CompiledRoomEnemyDefinitions.Instance.ReadEnemyDefinition(EnemyDefinitionId.FakeKraid).Radius;
            place.Position = place.Position.WithWhole(new(0x0100, 0x0100));
            samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
            {
                Position = FixedPosition<RoomPixels>.FromWhole(new(movingRight ? (ushort)0x0140 : (ushort)0x00c0, place.Position.Y.Whole)),
            };
            // Fake Kraid's setup builds the state his animation, which the check plays, runs on.
            state = enemies.FakeKraidBehavior.InitializeFakeKraid(place, samus);
            return enemies;
        }

        static void RunFrames(
            RoomEnemySystem enemies,
            RoomEnemyPlace place,
            FakeKraidEnemyState state,
            SamusState samus,
            RoomLevelData level,
            int frames)
        {
            var context = new EnemyAnimationCueContext { Samus = samus, Level = level, Camera = Position16<RoomPixels>.Zero, ControllerInput = default };
            for (int frame = 0; frame < frames; frame++)
                enemies.FakeKraidBehavior.AdvanceFakeKraidTypedAnimation(place.HeldOccupant(), place, state, context);
        }
    }

    /// <summary>Reads native bank-$A6 bytes only for the independent instruction comparison.</summary>
    private static byte FakeKraidInstructionProgramDefinitionsRomByte(CartridgeImportAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Runs the Fake Kraid instruction check against the local retail cartridge.</summary>
    [VerificationCommand("--fake-kraid-instruction-mechanics")]
    internal static int FakeKraidInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyFakeKraidInstructionProgramDefinitions), () => VerifyFakeKraidInstructionProgramDefinitions());
        return 0;
    }
}
