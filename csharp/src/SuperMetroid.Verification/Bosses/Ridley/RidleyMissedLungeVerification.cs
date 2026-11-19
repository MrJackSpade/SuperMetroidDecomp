using SuperMetroid.Core.Bosses;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
namespace SuperMetroid.Verification.Bosses.Ridley;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Norfair Ridley's missed grab-lunge exits and recovery paths.</summary>
internal static class RidleyMissedLungeVerification
{
    /// <summary>Checks native height and pass thresholds, room-boundary handoff, tenth death miss, and Power Bomb dodge.</summary>
    internal static void VerifyRidleyMissedLunge()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        // The power bomb explosion of the pass's bombs, which a missed lunge dodges while it is armed.
        var bomb = new SamusPowerBombExplosionState();
        Action<RoomEnemyPlace, NorfairRidleyState, SamusState, GameplayAction, RoomLevelData> run = (ridley, ridleyState, target, input, level) =>
            enemies.NorfairRidleyBehavior.RunNorfairRidleyPhase(ridley, ridleyState, target, input, level, bomb);
        Action<RoomEnemyPlace, RidleyFlightState, RidleyWallImpact> move = enemies.RidleyBody.Flight.IntegrateMovement;
        var place = enemies.Places.InUpdateOrder[0];
        place.Health = 1000;
        place.Position = place.Position.WithWhole(new(160, 400));
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Position = FixedPosition<RoomPixels>.FromWhole(new(100, 420)) };
        var state = new NorfairRidleyState { Phase = NorfairRidleyPhase.GrabApproach, FightMode = RidleyFightMode.Fighting, Flight = { FacingDirection = RidleyFacing.Left, MovementMinimum = new Offset16<RoomPixels>(32, 64), MovementMaximum = new Offset16<RoomPixels>(224, 416) } };
        run(place, state, samus, 0, SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.OpenRoom(new(64, 64)));
        AssertEqual(NorfairRidleyPhase.HoverSetup, state.Phase,
            "low missed lunge exits immediately when Ridley Y + 35 reaches Samus Y");
        AssertTrue(state.Tail.Whip.IsWaiting, "missed lunge requests native tail whip");
        ushort lowY = place.Position.Y.Whole;
        for (int frame = 0; frame < 32; frame++)
        {
            run(place, state, samus, 0, SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.OpenRoom(new(64, 64)));
            move(place, state.Flight, RidleyWallImpact.None);
        }
        AssertTrue(place.Position.Y.Whole < lowY, "missed lunge retreats upward instead of chasing Samus at floor level");
        AssertEqual(RidleyGrabState.Released, state.GrabState, "retreat never falsely grabs Samus");
        void ResetLunge(ushort x, ushort y, RidleyFacing facing = RidleyFacing.Left)
        {
            place.Position = FixedPosition<RoomPixels>.FromWhole(new(x, y));
            place.Health = 1000;
            state.Phase = NorfairRidleyPhase.GrabApproach;
            state.Flight.FacingDirection = facing;
            state.Flight.Velocity = new PixelVelocity<RoomPixels>(new SignedEightEight(0), new SignedEightEight(0));
            state.Flight.HitRoomBoundary = false;
            state.ZeroHealthLungeCount = 0;
            state.FightMode = RidleyFightMode.Fighting;
            _ = state.Tail.Whip.Take();
        }
        // The native Y comparison is >=, and applies even when the claw's X misses.
        ResetLunge(160, 384);
        run(place, state, samus, 0, SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.OpenRoom(new(64, 64)));
        AssertEqual(NorfairRidleyPhase.GrabApproach, state.Phase, "one pixel above vertical cutoff continues lunge");
        ResetLunge(160, 385);
        run(place, state, samus, 0, SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.OpenRoom(new(64, 64)));
        AssertEqual(NorfairRidleyPhase.HoverSetup, state.Phase, "exact vertical cutoff abandons lunge");
        foreach (RidleyFacing facing in new[] { RidleyFacing.Left, RidleyFacing.Right })
        {
            int direction = facing == RidleyFacing.Left ? -1 : 1;
            ResetLunge((ushort)(samus.Position.X.Whole + direction * 31), 300, facing);
            run(place, state, samus, 0, SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.OpenRoom(new(64, 64)));
            AssertEqual(NorfairRidleyPhase.GrabApproach, state.Phase, "31 pixels past Samus remains within native pass allowance");
            ResetLunge((ushort)(samus.Position.X.Whole + direction * 32), 300, facing);
            run(place, state, samus, 0, SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.OpenRoom(new(64, 64)));
            AssertEqual(NorfairRidleyPhase.HoverSetup, state.Phase, "32 pixels past Samus abandons lunge for either facing");
        }
        // Exercise real integration, not a manually supplied boundary flag.
        ResetLunge(223, 300);
        state.Flight.Velocity = state.Flight.Velocity.WithX(new SignedEightEight(0x100));
        move(place, state.Flight, RidleyWallImpact.None);
        AssertTrue(state.Flight.HitRoomBoundary, "exact maximum X counts as a native boundary hit");
        run(place, state, samus, 0, SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.OpenRoom(new(64, 64)));
        AssertEqual(NorfairRidleyPhase.HoverSetup, state.Phase, "movement boundary result exits the next lunge call");
        state.Flight.Velocity = state.Flight.Velocity.WithX(new SignedEightEight(unchecked((ushort)-0x100)));
        move(place, state.Flight, RidleyWallImpact.None);
        AssertTrue(!state.Flight.HitRoomBoundary, "moving back inside clears the boundary result");

        ResetLunge(160, 400);
        place.Health = 0;
        state.ZeroHealthLungeCount = 9;
        run(place, state, samus, 0, SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.OpenRoom(new(64, 64)));
        AssertEqual(NorfairRidleyPhase.HoverSetup, state.Phase, "ninth zero-health miss still retreats");
        state.Phase = NorfairRidleyPhase.GrabApproach;
        state.ZeroHealthLungeCount = 10;
        run(place, state, samus, 0, SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.OpenRoom(new(64, 64)));
        AssertEqual(NorfairRidleyPhase.DeathRoar, state.Phase, "tenth miss immediately executes death-roar setup");
        AssertEqual(RidleyFightMode.Dying, state.FightMode, "tenth miss enters death mode without requiring a grab");
        AssertEqual(32, state.PhaseTimer.CartridgeCount, "native death-roar timer is armed");

        bomb.Arm();
        bomb.Spawn(new(200, 400));
        ResetLunge(160, 400);
        run(place, state, samus, 0, SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.OpenRoom(new(64, 64)));
        AssertEqual(NorfairRidleyPhase.PowerBombDodge, state.Phase, "bomb selects native dodge instead of ordinary retreat");
        AssertEqual(RidleyFightMode.PowerBombDodge, state.FightMode, "dodge suppresses repeated combat interruption");
        AssertTrue((short)state.Flight.Velocity.X.RawValue < 0 && (short)state.Flight.Velocity.Y.RawValue < 0,
            "low right-side bomb immediately accelerates Ridley toward upper-left target");
        bomb.ReleasePowerBombLock();
        run(place, state, samus, 0, SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.OpenRoom(new(64, 64)));
        AssertEqual(NorfairRidleyPhase.SelectAttack, state.Phase, "ending bomb resumes attack selection");
        AssertEqual(RidleyFightMode.Fighting, state.FightMode, "ending dodge restores ordinary fight mode");
        Console.WriteLine("Ridley missed low lunge: native exit, tail request and upward retreat confirmed.");
        Console.WriteLine("Native pass/height thresholds, movement boundary handoff, tenth death lunge and bomb recovery confirmed.");
    }

    /// <summary>Runs the missed-lunge and retreat verification.</summary>
    [VerificationCommand("--ridley-missed-lunge")]
    internal static int RidleyMissedLungeCommand()
    {
        Suite(nameof(VerifyRidleyMissedLunge), () => VerifyRidleyMissedLunge());
        return 0;
    }
}
