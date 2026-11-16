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
        Action<RidleyEnemyState, RidleySwoopTurn, RotationAngle, SignedEightEight> update = CeresRidleyFlight.UpdateRidleySwoopVelocity;
        SignedEightEight fullSpeed = SignedEightEight.FromPixels(5);
        var state = new RidleyEnemyState { SwoopSpeed = fullSpeed };
        update(state, RidleySwoopTurn.None, RotationAngle.Zero, fullSpeed);
        AssertEqual((ushort)0x500, state.Velocity.Y.RawValue, "native downward swoop uses full speed");
        state.SwoopAngleAccumulator = RotationAngle.FromSteps(120);
        update(state, RidleySwoopTurn.None, RotationAngle.HalfTurn, fullSpeed);
        AssertEqual(RotationAngle.FromSteps(120), state.SwoopAngleAccumulator, "native CMP/BMI retains right-facing recovery angle");
        // A rightward step from just short of a full turn wraps the heading past zero; the half-turn target's
        // sign-of-difference test then keeps the wrapped heading instead of snapping to the target.
        state.SwoopAngleAccumulator = RotationAngle.FromSteps(254);
        update(state, new RidleySwoopTurn(RotationAngle.FromSteps(4), RotationDirection.Counterclockwise), RotationAngle.HalfTurn, fullSpeed);
        AssertEqual(RotationAngle.FromSteps(2), state.SwoopAngleAccumulator, "swoop heading wraps past a full turn");
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        for (int phase = 0; phase < 256; phase++)
        {
            state.SwoopAngleAccumulator = RotationAngle.FromSteps((byte)phase);
            update(state, RidleySwoopTurn.None, RotationAngle.FromSteps(phase), fullSpeed);
            ushort NativeComponent(int angle)
            {
                short sample = ReadRetailSwoopSine(rom, angle);
                return unchecked((ushort)(Math.Sign(sample) * (EightEight.Multiply(0x500, Math.Abs((int)sample)))));
            }
            AssertEqual(NativeComponent(phase), state.Velocity.X.RawValue, "native swoop X table multiplication");
            AssertEqual(NativeComponent(phase + 64), state.Velocity.Y.RawValue, "native swoop Y table multiplication");
        }
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().ReadRandom = () => 0;
        Action<RoomEnemyPlace, RidleyEnemyState, SamusState> tail = enemies.RidleyTail.TickRidleyTail;
        Action<RoomEnemyPlace, RidleyEnemyState, SamusState, GameplayAction, RoomLevelData> run = enemies.RidleyBehavior.RunNorfairRidleyPhase;
        var place = enemies.Places.InUpdateOrder[0];
        place.Position = place.Position.WithWhole(new(128, 128));
        state = new RidleyEnemyState
        {
            Phase = RidleyPhase.NorfairPogoMoveToHeight, PhaseTimer = 0,
            TailSegments = Enumerable.Range(0, 7).Select(index => new RidleyTailSegment
            {
                Active = true, StaggerAngle = null, Clockwise = false,
                Angle = RidleyTailAngle.StraightDown.TurnedClockwise(16), Distance = RidleyTailReach.RestDistance(index),
            }).ToArray(),
        };
        run(place, state, SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.StandaloneSamus(), 0, SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.OpenRoom(new(64, 64)));
        AssertEqual(RidleyTailAction.PointDown, state.TailAction, "pogo wait runs pointed-tail setup");
        AssertEqual(RidleyTailAngle.StraightDown.TurnedClockwise(8), state.TailSegments[0].Angle, "setup immediately advances tail by eight");
        tail(place, state, SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.StandaloneSamus());
        AssertEqual(RidleyTailAction.Pogo, state.TailAction, "all pointed segments finish into normal pogo");
        AssertTrue(state.TailSegments.All(segment => segment.Angle == RidleyTailAngle.StraightDown && !segment.Active), "pogo tail points down and stops");
        // $D10E returns on deactivation before recomputing offsets: retain the
        // previous $F8-angle Mode 7 products (-1,+1), not the new angle's (0,2).
        AssertEqual((ushort)159, state.TailSegments[0].Position.X, "pointed root retains pre-stop native hip X");
        AssertEqual((ushort)145, state.TailSegments[0].Position.Y, "pointed root retains pre-stop native hip Y");
        state.Velocity = state.Velocity.WithY(new SignedEightEight(0));
        tail(place, state, SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.StandaloneSamus());
        AssertEqual(RidleyTailAction.StabSetup, state.TailAction, "descending normal pogo starts stab setup");
        tail(place, state, SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.StandaloneSamus());
        AssertEqual(RidleyTailAction.Stab, state.TailAction, "stab setup activates downward stab");
        AssertTrue(state.TailSegments.All(segment => segment.TargetDistance == SignedEightEight.FromParts(10, 0)), "stab sets every segment extension target");
        state.Phase = RidleyPhase.NorfairPogoRise;
        run(place, state, SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.StandaloneSamus(), 0, SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.OpenRoom(new(64, 64)));
        AssertEqual(RidleyTailAction.Neutral, state.TailAction, "leaving pogo restores neutral tail");
        AssertEqual(1, state.TailAngleDelta, "leaving pogo restores native delta");
        Action<RidleyEnemyState, SamusState, int> aim = (state, samus, turnsBeyond) => enemies.RidleyTail.AimCeresRidleyTailWhip(state, samus, turnsBeyond);
        state.FightMode = RidleyFightMode.Fighting;
        state.FacingDirection = RidleyFacing.Left;
        state.TailSegments[0].Position = new Position16<RoomPixels>(128, 128);
        state.TailSegments[0].Angle = RidleyTailAngle.StraightDown;
        state.TailSegments[6].Position = new Position16<RoomPixels>(128, 128);
        var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), new SuperMetroid.Core.Samus.Artwork.SamusArtworkBinding()) { Position = FixedPosition<RoomPixels>.FromWhole(new(128, 200)) };
        aim(state, samus, 0);
        RidleyTailAngle? samusTarget = state.ClockwiseWhipTarget;
        var projectiles = new SamusProjectileSystem(new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftMuzzleSubpixelCarry(SamusProjectileSystem.PlaceCount));
        projectiles.Pool.ShotCount.Restore(1);
        projectiles.Shots[0].Type = SamusProjectileType.Of(SamusProjectileFamily.Missile);
        projectiles.Shots[0].Position = projectiles.Shots[0].Position.WithWhole(new(96, 128));
        enemies.FrameContext.SeedFrame(enemies.FrameContext.FrameCounter, projectiles, new SuperMetroid.Core.Samus.Colours.SamusColours());
        aim(state, samus, 0);
        RidleyTailAngle? missileTarget = state.ClockwiseWhipTarget;
        AssertTrue(missileTarget != samusTarget, "nearby missile changes tail target");
        enemies.FrameContext.SeedFrame(enemies.FrameContext.FrameCounter, new SamusProjectileSystem(new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftMuzzleSubpixelCarry(SamusProjectileSystem.PlaceCount)), new SuperMetroid.Core.Samus.Colours.SamusColours());
        aim(state, new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), new SuperMetroid.Core.Samus.Artwork.SamusArtworkBinding()) { Position = FixedPosition<RoomPixels>.FromWhole(new(96, 104)) }, 0);
        AssertEqual(missileTarget, state.ClockwiseWhipTarget, "missile uses its exact position without Samus's 24-pixel offset");
        ushort seed = 0xf0;
        int advances = 0;
        enemies.VerificationServices().ReadRandom = () => seed;
        enemies.VerificationServices().NextRandom = () => { advances++; return 0; };
        Action<RoomEnemyPlace, RidleyEnemyState, SamusState> neutral = enemies.RidleyTail.HandleCeresRidleyNeutralTailControl;
        state.IdleTailWhipEnabled = true;
        state.TailSegments[0].Angle = RidleyTailAngle.StraightDown.TurnedByWholeTurns(16);
        foreach (var segment in state.TailSegments) segment.Active = true;
        state.ClockwiseWhipTarget = state.CounterClockwiseWhipTarget = null;
        neutral(place, state, new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), new SuperMetroid.Core.Samus.Artwork.SamusArtworkBinding()) { Position = FixedPosition<RoomPixels>.FromWhole(new(512, 128)) });
        AssertTrue(state.ClockwiseWhipTarget is not null, "native random fling admission works outside proximity range");
        AssertEqual(0, advances, "tail controller never advances RNG");
        Action<RoomEnemyPlace, RidleyEnemyState, SamusState> direction = enemies.RidleyBehavior.Combat.SetRidleyPogoHorizontalDirection;
        state.MovementMinimum = state.MovementMinimum.WithX(64);
        state.MovementMaximum = state.MovementMaximum.WithX(192);
        place.Position = place.Position.WithWholeX(128);
        state.Velocity = state.Velocity.WithX(new SignedEightEight(0x100));
        seed = 0x1000;
        direction(place, state, new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), new SuperMetroid.Core.Samus.Artwork.SamusArtworkBinding()) { Position = FixedPosition<RoomPixels>.FromWhole(new(64, 0)) });
        AssertEqual(unchecked((ushort)-0x100), state.Velocity.X.RawValue, "pogo bounce turns toward Samus for ordinary seed");
        place.Position = place.Position.WithWholeX(32);
        direction(place, state, samus);
        AssertEqual((ushort)0x100, state.Velocity.X.RawValue, "pogo left boundary forces rightward bounce");
        place.Position = place.Position.WithWholeX(192);
        direction(place, state, samus);
        AssertEqual(unchecked((ushort)-0x100), state.Velocity.X.RawValue, "pogo right boundary forces leftward bounce");
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
