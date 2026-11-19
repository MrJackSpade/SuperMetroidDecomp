using SuperMetroid.Core;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Enemies.Animals;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>
/// Dachora's typed speed, acceleration, echo lifetime and palette cycle keep the cartridge's word arithmetic in the
/// slot variables they view.
/// </summary>
internal static class DachoraMotionStateVerification
{
    /// <summary>Samus's air gravity fraction: 7168 sixty-five-thousand-five-hundred-thirty-sixths of a pixel per update.</summary>
    private const ushort AirGravityFraction = 7168;

    /// <summary>
    /// Runs the production speed and fall paths. At the run speed cap, a cleared palette cycle counted down borrows
    /// from its frame as one word does, becoming frame 255 with 255 ticks and loading no palette. Falling, gravity
    /// carries the fraction into the whole word, a negative whole speed included, and a whole speed reaching ten is
    /// replaced by exactly ten pixels with no fraction. Every result is checked against the slot variables, computed
    /// independently as 16-bit words.
    /// </summary>
    internal static void VerifyDachoraMotionState()
    {
        RoomLevelData level = VerificationHelpersVerification.CreateEmptyRoom(new(16, 16));

        (RoomEnemySystem enemies, RoomEnemyPlace place, DachoraEnemyState state) = NewDachora(parameter: 1);
        state.Speed = SignedSixteenSixteen.FromParts(8, 0);
        state.SetGlowCycle(SuperMetroid.Core.Compatibility.DachoraGlowCycle.At(0, 0));
        enemies.DachoraBehavior.AccelerateDachora(place, state, level);
        AssertEqual(255, state.GlowFrame, "a cleared palette cycle's tick borrows from its frame");
        AssertEqual(255, state.GlowTicks, "a cleared palette cycle's ticks wrap");
        AssertEqual(SignedSixteenSixteen.FromParts(8, 0), state.Speed, "the run speed stays at its cap");

        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork());
        samus.Kinematics.YAcceleration = new UnsignedSixteenSixteen(0, AirGravityFraction);
        var frame = new ClockedEnemyAiFrame(
            RoomEnemyFrameFixtures.Standalone(level, 0) with { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, Samus = samus },
            new FrameCounters(0));
        foreach ((short whole, ushort fraction, ushort expectedWhole, ushort expectedFraction) in new VerifyDachoraMotionStateCase[]
                 {
                     new VerifyDachoraMotionStateCase(9, 61440, 10, 0),
                     new VerifyDachoraMotionStateCase(-1, 61440, 0, 3072),
                     new VerifyDachoraMotionStateCase(3, 0, 3, AirGravityFraction),
                 })
        {
            (RoomEnemySystem fallEnemies, RoomEnemyPlace fallPlace, DachoraEnemyState fall) = NewDachora(parameter: 1);
            fall.Action = DachoraAction.Falling;
            fall.Speed = SignedSixteenSixteen.FromParts(whole, fraction);
            fallPlace.Position = fallPlace.Position.WithWhole(new(64, 32));

            fallEnemies.DachoraBehavior.RunDachoraMainAi(fallPlace, frame);

            string context = $"Dachora falling at {whole} and {fraction}/65536 pixels";
            AssertEqual(expectedWhole, fall.Speed.UnsignedWholePixels, $"{context}: whole speed word");
            AssertEqual(expectedFraction, fall.Speed.Fraction, $"{context}: speed fraction word");
            AssertEqual(SignedSixteenSixteen.FromParts(unchecked((short)expectedWhole), expectedFraction), fall.Speed, context);
        }

        Console.WriteLine("  Dachora motion state: the palette cycle borrows as one word; gravity carries and the fall cap replaces the speed.");
    }

    /// <summary>
    /// Runs the production echo AI. An echo with updates left counts one off and stays visible on its frame parity;
    /// an echo with none left stays at zero, never wrapping, and is hidden.
    /// </summary>
    internal static void VerifyDachoraEchoLifetime()
    {
        var frame = new ClockedEnemyAiFrame(
            RoomEnemyFrameFixtures.Standalone(RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), 0) with { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false },
            new FrameCounters(0));
        foreach ((int lifetime, ushort expected, bool invisible) in new VerifyDachoraEchoLifetimeCase[] { new VerifyDachoraEchoLifetimeCase(8, 7, false), new VerifyDachoraEchoLifetimeCase(1, 0, false), new VerifyDachoraEchoLifetimeCase(0, 0, true) })
        {
            (RoomEnemySystem enemies, RoomEnemyPlace place) = NewDachoraPlace(parameter: ushort.MaxValue);
            DachoraEchoState state = place.SpeciesState<DachoraEchoState>() ?? throw new InvalidOperationException("A negative parameter makes an echo.");
            state.Lifetime = UpdateCountdown.For(lifetime);

            DachoraBehavior.RunDachoraEchoMainAi(place, frame);

            AssertEqual(expected, state.Lifetime.UpdatesLeft, $"echo lifetime {lifetime} counts down without wrapping");
            AssertEqual(invisible, place.Presence.HasAny(EnemyPresence.Hidden), $"echo lifetime {lifetime} visibility");
        }
        Console.WriteLine("  Dachora echo lifetime: counts down to zero and stays there, hiding the echo.");
    }

    /// <summary>
    /// The body spaces its trail itself (#2147): the launch clears its trail timer, so its first shinespark update places
    /// the first echo, the next places none, and the one after places the second echo, while the echoes keep only their
    /// lives.
    /// </summary>
    internal static void VerifyDachoraTrailSpacing()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        for (int slot = 0; slot < 5; slot++)
        {
            RoomEnemyPlace part = enemies.Places.InUpdateOrder[slot];
            part.SetSpawnParameter1(EnemyDefinitionId.Dachora, slot == 0 ? (ushort)1 : ushort.MaxValue);
            part.Bind(EnemyDefinitionId.Dachora, part.Definition);
            part.Radius = new(8, 8);
        }
        for (int slot = 0; slot < 5; slot++)
            InitializeThroughEntity(enemies.Places.InUpdateOrder[slot]);
        RoomEnemyPlace body = enemies.Places.InUpdateOrder[0];
        DachoraEnemyState state = body.RequireSpeciesState<DachoraEnemyState>();
        DachoraEchoState[] echoes = [.. Enumerable.Range(1, 4).Select(slot => enemies.Places.InUpdateOrder[slot].RequireSpeciesState<DachoraEchoState>())];
        foreach (DachoraEchoState echo in echoes)
            echo.Lifetime = UpdateCountdown.For(3);
        state.TrailTimer = UpdateCountdown.For(5);
        body.Position = body.Position.WithWhole(new(64, 128));
        state.Action = DachoraAction.ChargingShinespark;
        state.StartCharging(1);

        var frame = new ClockedEnemyAiFrame(
            RoomEnemyFrameFixtures.Standalone(RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), 0) with { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false },
            new FrameCounters(0));
        enemies.DachoraBehavior.RunDachoraMainAi(body, frame);
        AssertEqual(DachoraAction.Shinesparking, state.Action, "the charge launches the shinespark");
        AssertEqual(0, state.TrailTimer.UpdatesLeft, "the launch clears the body's trail timer");
        AssertTrue(echoes.All(echo => echo.Lifetime.IsOver), "the launch frees every echo");

        int[][] lives = [[8, 0, 0, 0], [8, 0, 0, 0], [8, 8, 0, 0]];
        int[] timers = [1, 0, 1];
        for (int update = 0; update < lives.Length; update++)
        {
            enemies.DachoraBehavior.RunDachoraMainAi(body, frame);
            AssertEqual(timers[update], state.TrailTimer.UpdatesLeft, $"the body's trail timer after shinespark update {update}");
            AssertEqual(string.Join(",", lives[update]), string.Join(",", echoes.Select(echo => echo.Lifetime.UpdatesLeft)),
                $"the echoes' lives after shinespark update {update}");
        }
        Console.WriteLine("  Dachora trail spacing: the body's own timer places an echo every second shinespark update.");
    }

    /// <summary>An initialized Dachora body with spawn parameter <paramref name="parameter"/> in the first slot of a fresh room.</summary>
    private static NewDachoraResult NewDachora(ushort parameter)
    {
        (RoomEnemySystem enemies, RoomEnemyPlace place) = NewDachoraPlace(parameter);
        return new NewDachoraResult(enemies, place, place.RequireSpeciesState<DachoraEnemyState>());
    }

    /// <summary>An initialized Dachora body or echo with spawn parameter <paramref name="parameter"/> in the first slot of a fresh room.</summary>
    private static (RoomEnemySystem Enemies, RoomEnemyPlace Place) NewDachoraPlace(ushort parameter)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.SetSpawnParameter1(EnemyDefinitionId.Dachora, parameter);
        place.Bind(EnemyDefinitionId.Dachora, place.Definition);
        place.Radius = new(8, 8);
        InitializeThroughEntity(place);
        return (enemies, place);
    }

    /// <summary>
    /// Initializes <paramref name="place"/> through the entity its placement made, as a room's spawn does: a body, or a
    /// speed-boost echo (#2283).
    /// </summary>
    private static void InitializeThroughEntity(RoomEnemyPlace place) =>
        (place.Entity as IEnemyInitialization ?? throw new InvalidOperationException("A Dachora place initializes.")).Initialize();

    /// <summary>Runs Dachora's motion-state and echo-lifetime checks.</summary>
    [VerificationCommand("--dachora-motion-state")]
    internal static int DachoraMotionStateCommand()
    {
        Suite(nameof(VerifyDachoraMotionState), () => VerifyDachoraMotionState());
        Suite(nameof(VerifyDachoraEchoLifetime), () => VerifyDachoraEchoLifetime());
        Suite(nameof(VerifyDachoraTrailSpacing), () => VerifyDachoraTrailSpacing());
        return 0;
    }
}
