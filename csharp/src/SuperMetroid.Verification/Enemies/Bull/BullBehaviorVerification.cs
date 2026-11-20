using SuperMetroid.Core;
using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Bull;
using SuperMetroid.Core.Enemies.Bull.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Bull;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Progression;

/// <summary>Checks Bull's normal loop and its immune-shot reaction animation.</summary>
internal static class BullBehaviorVerification
{
    /// <summary>
    /// The initializer starts the normal loop (four frames, ten updates apiece). An immune shot starts the shot program,
    /// which plays its four three-update frames five times on the native repeat timer and then returns to the normal loop.
    /// </summary>
    internal static void VerifyBullBehavior()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(EnemyDefinitionId.Bull, place.Definition);
        place.Bind(place.HeldOccupant().DefinitionId, default(RoomEnemyDefinition));
        place.SetSpawnWords(EnemyDefinitionId.Bull, words => words);
        enemies.BullBehavior.InitializeBull(place.HeldOccupant(), place);
        BullEnemyState state = enemies.BullStates[0] ?? throw new InvalidDataException("Bull initializer did not publish typed state.");
        AssertEqual(new AnimationPlayback<BullSegment>(BullAnimations.StartingWith(BullSegment.Idle), AnimationPlace.StartOfAnimation), state.Playback, "Bull initializer animation");
        AssertEqual(EnemyInstructionRunner.ExpectedLoop(BullAnimations.Segments, BullSegment.Idle, BullFrameToolingDefinitions.Spritemap),
            EnemyInstructionRunner.ShownFrames(place, EnemyInstructionRunner.LoopUpdates(BullAnimations.Segments, BullSegment.Idle)),
            "Bull normal loop");

        BullBehavior.ResolveBullImmuneShot(place, state, BullMovementDefinitions.ShotAngles.For(SamusProjectileDirection.Right));
        AssertEqual(new AnimationPlayback<BullSegment>(BullAnimations.Shot, AnimationPlace.StartOfAnimation), state.Playback, "Bull immune-shot animation");
        ushort[] shot = BullAnimationsConstants.Spritemaps(BullSegment.Shot).ToArray();
        ushort[] expected = [.. Enumerable.Repeat(shot, 5).SelectMany(frames => frames), BullAnimationsConstants.Spritemaps(BullSegment.Idle).First()];
        AssertEqual(EnemyInstructionRunner.ExpectedFrames(expected, Enumerable.Repeat(3, expected.Length)),
            EnemyInstructionRunner.ShownFrames(place, 61), "Bull shot program repeats five times and returns to normal");

        // #2350: a bomb that leaves the Bull unhurt kicks it as the original's shot table kicks a shot fired up facing
        // right, the direction the original reads for every bomb.
        RoomEnemyPlace bombed = enemies.Places.InUpdateOrder[1];
        bombed.Bind(EnemyDefinitionId.Bull, bombed.Definition);
        bombed.SetSpawnWords(EnemyDefinitionId.Bull, words => words);
        enemies.BullBehavior.InitializeBull(bombed.HeldOccupant(), bombed);
        var bomb = new SamusBomb(0);
        bomb.OccupyAsPlaced(SamusProjectileType.Of(SamusProjectileFamily.Bomb));
        var context = new EnemyBombContext(bombed.HeldOccupant(), bomb.HeldBomb(), InstalledFixtures.CreateProjectileSystem(), RoomEnemyFrameFixtures.StandaloneSamus(),
            SelectedShot: null, UsesFrameHitboxes: false, new Position16<RoomPixels>(0, 0))
        {
            StruckOccupant = bombed.OccupantStates,
        };
        (enemies.BullBehavior.BombReaction.AfterCommon ?? throw new InvalidOperationException("The Bull has no bomb recoil."))(
            new EnemyBombHitContext(context, bombed.Health));
        AssertEqual(BullMovementDefinitions.ShotAngles.For(SamusProjectileDirection.UpFacingRight), bombed.RequireSpeciesState<BullEnemyState>().Angle,
            "an unhurting bomb kicks the Bull straight up");
        Console.WriteLine("  Bull behavior: the normal loop, the five-repeat immune-shot reaction and the bomb's upward kick run as the cartridge does.");
    }

    /// <summary>
    /// #2065: a Bull's speed stays within 0 through 2880 256ths of a pixel an update, so its moves read it as an ordinary
    /// speed. A flight from rest under the fastest cap, 2815 256ths, accelerates while below it and overshoots once, to
    /// 2880; braking runs the acceleration out with 360 256ths left. An immune shot's kick (six pixels an update, braked
    /// from a pixel) runs the acceleration below zero with 304 256ths left; the same kick while the Bull waits, with no
    /// braking change, brakes onto exactly zero. The speed is never below zero before a decelerating update stops the
    /// flight.
    /// </summary>
    internal static void VerifyBullSpeedStaysInFlightRange()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        RoomEnemyPlace slot = enemies.Places.InUpdateOrder[0];
        slot.Bind(EnemyDefinitionId.Bull, slot.Definition);
        slot.SetSpawnWords(EnemyDefinitionId.Bull, words => words);
        enemies.BullBehavior.InitializeBull(slot.HeldOccupant(), slot);
        BullEnemyState state = slot.RequireSpeciesState<BullEnemyState>();
        SignedEightEight brakingStep = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(24);

        SignedEightEight fastestCap = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(2815);
        state.AccelerationDelta = brakingStep;
        int accelerations = 0;
        while (state.IsBelow(fastestCap))
        {
            state.Accelerate();
            accelerations++;
        }
        AssertEqual(15, accelerations, "a flight under the fastest cap accelerates fifteen times");
        AssertEqual(2880, state.Speed.InTwoHundredFiftySixthsOfPixel, "the last acceleration overshoots the fastest cap to 2880 256ths");
        int Brake()
        {
            while (!state.HasStopped)
            {
                AssertTrue(state.Speed > SignedEightEight.Zero, "a braking Bull flies at a speed above zero");
                state.Decelerate();
            }
            AssertTrue(state.Speed >= SignedEightEight.Zero, "a Bull stops at a speed of zero or more");
            return state.Speed.InTwoHundredFiftySixthsOfPixel;
        }
        AssertEqual(360, Brake(), "braking a flight runs its acceleration out with 360 256ths left");
        AssertTrue(global::SuperMetroid.Core.PrivateState.Get<global::SuperMetroid.Core.Enemies.Bull.BullEnemyState, global::SuperMetroid.Core.Spatial.SignedEightEight>(state, "Acceleration").IsZero, "braking a flight runs its acceleration out exactly");

        BullBehavior.ResolveBullImmuneShot(slot, state, BullMovementDefinitions.ShotAngles.For(SamusProjectileDirection.Right));
        AssertEqual(1536, state.Speed.InTwoHundredFiftySixthsOfPixel, "a kick throws the Bull at six pixels an update");
        state.AccelerationDelta = brakingStep;
        AssertEqual(304, Brake(), "a braked kick runs its acceleration below zero with 304 256ths left");
        AssertEqual(-8, global::SuperMetroid.Core.PrivateState.Get<global::SuperMetroid.Core.Enemies.Bull.BullEnemyState, global::SuperMetroid.Core.Spatial.SignedEightEight>(state, "Acceleration").InTwoHundredFiftySixthsOfPixel, "a braked kick's acceleration ends below zero");

        state.ShotReactionDisableTimer = UpdateCountdown.Ended();
        BullBehavior.ResolveBullImmuneShot(slot, state, BullMovementDefinitions.ShotAngles.For(SamusProjectileDirection.Right));
        state.AccelerationDelta = SignedEightEight.Zero;
        AssertEqual(0, Brake(), "a kick while the Bull waits brakes a pixel an interval onto exactly zero");
        Console.WriteLine("  Bull speed: flights and kicks keep the speed within 0 through 2880 256ths of a pixel an update.");
    }

    /// <summary>Runs the Bull behavior check and program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--bull-instruction-mechanics")]
    internal static int BullInstructionMechanicsCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyBullBehavior), () => VerifyBullBehavior());
        Suite(nameof(VerifyBullSpeedStaysInFlightRange), () => VerifyBullSpeedStaysInFlightRange());
        Suite(nameof(BullAnimationsVerification.VerifyBullAnimations), () => BullAnimationsVerification.VerifyBullAnimations(rom));
        return 0;
    }
}
