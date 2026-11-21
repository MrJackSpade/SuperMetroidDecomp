using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
namespace SuperMetroid.Verification.Bosses.Ridley;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Progression;

/// <summary>Checks Ridley's swoop components and tail state changes through pogo, stab, and target selection.</summary>
internal static class RidleyFlightTailVerification
{
    /// <summary>The native signed sine sample for <paramref name="angle"/> from the $A0:B443 table.</summary>
    private static short ReadRetailSwoopSine(ISnesAddressSpace rom, int angle)
    {
        int address = 0xa0b443 + unchecked((byte)angle) * 2;
        return unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(address));
    }

    /// <summary>Compares retail swoop sine motion and production tail transitions, coordinates, and RNG use.</summary>
    internal static void VerifyRidleyFlightTail()
    {
        Action<RidleyFlightState, TurnStep, RotationAngle, SignedEightEight> update = RidleyFlight.UpdateSwoopVelocity;
        SignedEightEight fullSpeed = SignedEightEight.FromPixels(5);
        var state = new NorfairRidleyState { Flight = { SwoopSpeed = fullSpeed } };
        update(state.Flight, TurnStep.None, RotationAngle.Zero, fullSpeed);
        AssertEqual((ushort)0x500, state.Flight.Velocity.Y.RawValue, "native downward swoop uses full speed");
        state.Flight.SwoopHeading = RotationAngle.FromSteps(120);
        update(state.Flight, TurnStep.None, RotationAngle.HalfTurn, fullSpeed);
        AssertEqual(RotationAngle.FromSteps(120), state.Flight.SwoopHeading, "native CMP/BMI retains right-facing recovery angle");
        // A rightward step from just short of a full turn wraps the heading past zero; the half-turn target's
        // sign-of-difference test then keeps the wrapped heading instead of snapping to the target.
        state.Flight.SwoopHeading = RotationAngle.FromSteps(254);
        update(state.Flight, new TurnStep(RotationAngle.FromSteps(4), RotationDirection.Counterclockwise), RotationAngle.HalfTurn, fullSpeed);
        AssertEqual(RotationAngle.FromSteps(2), state.Flight.SwoopHeading, "swoop heading wraps past a full turn");
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        for (int phase = 0; phase < 256; phase++)
        {
            state.Flight.SwoopHeading = RotationAngle.FromSteps((byte)phase);
            update(state.Flight, TurnStep.None, RotationAngle.FromSteps(phase), fullSpeed);
            ushort NativeComponent(int angle)
            {
                short sample = ReadRetailSwoopSine(rom, angle);
                return unchecked((ushort)(Math.Sign(sample) * (EightEightOracle.Multiply(0x500, Math.Abs((int)sample)))));
            }
            AssertEqual(NativeComponent(phase), state.Flight.Velocity.X.RawValue, "native swoop X table multiplication");
            AssertEqual(NativeComponent(phase + 64), state.Flight.Velocity.Y.RawValue, "native swoop Y table multiplication");
        }
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().ReadRandom = () => 0;
        // Samus's beams and missiles, none fired, as the tail's whip sees them.
        SamusProjectileSystem tailProjectiles = InstalledFixtures.CreateProjectileSystem();
        Action<RoomEnemyPlace, NorfairRidleyState, SamusState> tail = (ridley, ridleyState, target) =>
            enemies.RidleyTail.TickRidleyTail(ridley, ridleyState, target, tailProjectiles);
        // The power bomb explosion of idle bombs, none exploding, which a missed lunge would dodge.
        var powerBomb = SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.StandalonePowerBombExplosion();
        Action<RoomEnemyPlace, NorfairRidleyState, SamusState, GameplayAction, RoomLevelData> run = (ridley, ridleyState, target, input, level) =>
            enemies.NorfairRidleyBehavior.RunNorfairRidleyPhase(ridley, ridleyState, target, input, level, powerBomb);
        var place = enemies.Places.InUpdateOrder[0];
        place.Position = place.Position.WithWhole(new(128, 128));
        // Ridley faces left, the side this fixture's native hip coordinates were taken from.
        state = new NorfairRidleyState { Phase = NorfairRidleyPhase.PogoMoveToHeight, Flight = { FacingDirection = RidleyFacing.Left } };
        state.PhaseTimer.LastFor(1);
        RidleyTailFixtures.BlankLinks(state.Tail);
        foreach (RidleyTailLink link in state.Tail.Links)
        {
            link.Activate();
            link.Angle = RidleyTailAngle.StraightDown.TurnedClockwise(16);
            link.Distance = link.RestDistance;
        }
        run(place, state, SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.StandaloneSamus(), 0, SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.OpenRoom(new(64, 64)));
        AssertEqual(RidleyTailAction.PointDown, state.Tail.Action, "pogo wait runs pointed-tail setup");
        AssertEqual(RidleyTailAngle.StraightDown.TurnedClockwise(8), state.Tail.Base.Angle, "setup immediately advances tail by eight");
        tail(place, state, SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.StandaloneSamus());
        AssertEqual(RidleyTailAction.Pogo, state.Tail.Action, "all pointed segments finish into normal pogo");
        AssertTrue(state.Tail.Links.All(segment => segment.Angle == RidleyTailAngle.StraightDown && !segment.Active), "pogo tail points down and stops");
        // $D10E returns on deactivation before recomputing offsets: retain the
        // previous $F8-angle Mode 7 products (-1,+1), not the new angle's (0,2).
        AssertEqual((ushort)159, state.Tail.Base.Position.X, "pointed root retains pre-stop native hip X");
        AssertEqual((ushort)145, state.Tail.Base.Position.Y, "pointed root retains pre-stop native hip Y");
        state.Flight.Velocity = state.Flight.Velocity.WithY(new SignedEightEight(0));
        tail(place, state, SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.StandaloneSamus());
        AssertEqual(RidleyTailAction.StabSetup, state.Tail.Action, "descending normal pogo starts stab setup");
        tail(place, state, SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.StandaloneSamus());
        AssertEqual(RidleyTailAction.Stab, state.Tail.Action, "stab setup activates downward stab");
        AssertTrue(state.Tail.Links.All(segment => segment.TargetDistance == SignedEightEight.FromParts(10, 0)), "stab sets every segment extension target");
        state.Phase = NorfairRidleyPhase.PogoRise;
        run(place, state, SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.StandaloneSamus(), 0, SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.OpenRoom(new(64, 64)));
        AssertEqual(RidleyTailAction.Neutral, state.Tail.Action, "leaving pogo restores neutral tail");
        AssertEqual(RidleyTailTurn.OfSteps(1), state.Tail.SwingStep, "leaving pogo restores native delta");
        // Samus's beams and missiles as each aim sees them; none fired until the check fires a missile.
        SamusProjectileSystem aimedProjectiles = InstalledFixtures.CreateProjectileSystem();
        Action<NorfairRidleyState, SamusState, int> aim = (state, samus, turnsBeyond) => RidleyTail.AimRidleyTailWhip(state, samus, aimedProjectiles, turnsBeyond);
        state.FightMode = RidleyFightMode.Fighting;
        state.Flight.FacingDirection = RidleyFacing.Left;
        state.Tail.Base.Position = new Position16<RoomPixels>(128, 128);
        state.Tail.Base.Angle = RidleyTailAngle.StraightDown;
        state.Tail.Tip.Position = new Position16<RoomPixels>(128, 128);
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Position = FixedPosition<RoomPixels>.FromWhole(new(128, 200)) };
        aim(state, samus, 0);
        RidleyTailAngle? samusTarget = state.Tail.ClockwiseWhipTarget;
        var projectiles = new SamusProjectileSystem(GameplayAudioRequests.Standalone(), new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftBeamStartFromLastShot(SamusProjectileSystem.PlaceCount), new SamusShotCount(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork(), new global::SuperMetroid.Core.Fx.RoomEarthquake());
        projectiles.Pool.ShotCount.Restore(1);
        projectiles.Shots[0].OccupyAsFired(SamusProjectileType.Of(SamusProjectileFamily.Missile));
        projectiles.Shots[0].HeldShot().Position = projectiles.Shots[0].Position.WithWhole(new(96, 128));
        aimedProjectiles = projectiles;
        aim(state, samus, 0);
        RidleyTailAngle? missileTarget = state.Tail.ClockwiseWhipTarget;
        AssertTrue(missileTarget != samusTarget, "nearby missile changes tail target");
        aimedProjectiles = new SamusProjectileSystem(GameplayAudioRequests.Standalone(), new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftBeamStartFromLastShot(SamusProjectileSystem.PlaceCount), new SamusShotCount(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork(), new global::SuperMetroid.Core.Fx.RoomEarthquake());
        aim(state, new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Position = FixedPosition<RoomPixels>.FromWhole(new(96, 104)) }, 0);
        AssertEqual(missileTarget, state.Tail.ClockwiseWhipTarget, "missile uses its exact position without Samus's 24-pixel offset");
        ushort seed = 0xf0;
        int advances = 0;
        enemies.VerificationServices().ReadRandom = () => seed;
        enemies.VerificationServices().NextRandom = () => { advances++; return 0; };
        Action<RoomEnemyPlace, NorfairRidleyState, SamusState> neutral = (ridley, state, samus) => enemies.RidleyTail.HandleRidleyNeutralTailControl(ridley, state, samus, aimedProjectiles);
        state.Tail.IdleWhipEnabled = true;
        state.Tail.Base.Angle = RidleyTailAngle.StraightDown.TurnedByWholeTurns(16);
        foreach (var segment in state.Tail.Links) segment.Activate();
        state.Tail.ClockwiseWhipTarget = state.Tail.CounterClockwiseWhipTarget = null;
        neutral(place, state, new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Position = FixedPosition<RoomPixels>.FromWhole(new(512, 128)) });
        AssertTrue(state.Tail.ClockwiseWhipTarget is not null, "native random fling admission works outside proximity range");
        AssertEqual(0, advances, "tail controller never advances RNG");
        Action<RoomEnemyPlace, NorfairRidleyState, SamusState> direction = enemies.NorfairRidleyBehavior.Combat.SetRidleyPogoHorizontalDirection;
        state.Flight.MovementMinimum = state.Flight.MovementMinimum.WithX(64);
        global::SuperMetroid.Core.PrivateState.Set(state.Flight, "MovementMaximum", state.Flight.MovementMaximum.WithX(192));
        place.Position = place.Position.WithWholeX(128);
        state.Flight.Velocity = state.Flight.Velocity.WithX(new SignedEightEight(0x100));
        state.PogoBounce = HorizontalDirection.Right;
        seed = 0x1000;
        direction(place, state, new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Position = FixedPosition<RoomPixels>.FromWhole(new(64, 0)) });
        AssertEqual(HorizontalDirection.Left, state.PogoBounce, "pogo bounce turns toward Samus for ordinary seed");
        place.Position = place.Position.WithWholeX(32);
        direction(place, state, samus);
        AssertEqual(HorizontalDirection.Right, state.PogoBounce, "pogo left boundary forces rightward bounce");
        place.Position = place.Position.WithWholeX(192);
        direction(place, state, samus);
        AssertEqual(HorizontalDirection.Left, state.PogoBounce, "pogo right boundary forces leftward bounce");
        AssertEqual(0, advances, "pogo direction does not advance RNG");
        Console.WriteLine("Ridley flight: native swoop magnitude and signed angle comparison confirmed.");
        Console.WriteLine("Ridley tail: real pogo setup, exact segment angles/position, stop, descending stab and neutral recovery confirmed.");
    }

    /// <summary>Runs the focused Ridley flight and tail check.</summary>
    [VerificationCommand("--ridley-flight-tail")]
    internal static int RidleyFlightTailCommand()
    {
        Suite(nameof(VerifyRidleyFlightTail), () => VerifyRidleyFlightTail());
        return 0;
    }
}
