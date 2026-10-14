using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.LevelData.SharedSlopeHeightsVerification;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
namespace SuperMetroid.Verification.Bosses.Ridley;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Norfair Ridley's missed grab-lunge exits and recovery paths.</summary>
internal static class RidleyMissedLungeVerification
{
    /// <summary>Checks native height and pass thresholds, room-boundary handoff, tenth death miss, and Power Bomb dodge.</summary>
    internal static void VerifyRidleyMissedLunge()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(NewSlopeHeightNoReadBus());
        Action<RoomEnemySlot, RidleyEnemyState, SamusState?, ushort, RoomLevelData?> run = enemies.RidleyBehavior.RunNorfairRidleyFunction;
        Action<RoomEnemySlot, RidleyEnemyState, bool> move = enemies.CeresRidleyBehavior.IntegrateRidleyMovement;
        var slot = enemies.SlotTable.Slots[0];
        slot.Health = 1000;
        slot.Position = slot.Position.WithWhole(new(160, 400));
        var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(100, 420)) };
        var state = new RidleyEnemyState
        {
            Function = RidleyAiFunction.NorfairGrabApproach,
            FacingDirection = RidleyFacing.Left, FightMode = RidleyFightMode.Fighting,
            MovementMinimum = new Position16<RoomPixels>(32, 64), MovementMaximum = new Position16<RoomPixels>(224, 416),
        };
        run(slot, state, samus, 0, null);
        AssertEqual(RidleyAiFunction.NorfairHoverSetup, state.Function,
            "low missed lunge exits immediately when Ridley Y + 35 reaches Samus Y");
        AssertEqual((ushort)1, state.TailWhipRequest, "missed lunge requests native tail whip");
        ushort lowY = slot.Position.Y.Whole;
        for (int frame = 0; frame < 32; frame++)
        {
            run(slot, state, samus, 0, null);
            move(slot, state, false);
        }
        AssertTrue(slot.Position.Y.Whole < lowY, "missed lunge retreats upward instead of chasing Samus at floor level");
        AssertEqual((ushort)0, state.GrabState, "retreat never falsely grabs Samus");
        void ResetLunge(ushort x, ushort y, RidleyFacing facing = RidleyFacing.Left)
        {
            slot.Position = FixedPosition<RoomPixels>.FromWhole(new(x, y));
            slot.Health = 1000;
            state.Function = RidleyAiFunction.NorfairGrabApproach;
            state.FacingDirection = facing;
            state.Velocity = new EightEightVector<RoomPixels>(new SnesSignedEightEight(0), new SnesSignedEightEight(0));
            state.HitRoomBoundary = false;
            state.ZeroHealthLungeCount = 0;
            state.FightMode = RidleyFightMode.Fighting;
            state.TailWhipRequest = 0;
        }
        // The native Y comparison is >=, and applies even when the claw's X misses.
        ResetLunge(160, 384);
        run(slot, state, samus, 0, null);
        AssertEqual(RidleyAiFunction.NorfairGrabApproach, state.Function, "one pixel above vertical cutoff continues lunge");
        ResetLunge(160, 385);
        run(slot, state, samus, 0, null);
        AssertEqual(RidleyAiFunction.NorfairHoverSetup, state.Function, "exact vertical cutoff abandons lunge");
        foreach (RidleyFacing facing in new[] { RidleyFacing.Left, RidleyFacing.Right })
        {
            int direction = facing == RidleyFacing.Left ? -1 : 1;
            ResetLunge((ushort)(samus.Position.X.Whole + direction * 31), 300, facing);
            run(slot, state, samus, 0, null);
            AssertEqual(RidleyAiFunction.NorfairGrabApproach, state.Function, "31 pixels past Samus remains within native pass allowance");
            ResetLunge((ushort)(samus.Position.X.Whole + direction * 32), 300, facing);
            run(slot, state, samus, 0, null);
            AssertEqual(RidleyAiFunction.NorfairHoverSetup, state.Function, "32 pixels past Samus abandons lunge for either facing");
        }
        // Exercise real integration, not a manually supplied boundary flag.
        ResetLunge(223, 300);
        state.Velocity = state.Velocity.WithX(new SnesSignedEightEight(0x100));
        move(slot, state, false);
        AssertTrue(state.HitRoomBoundary, "exact maximum X counts as a native boundary hit");
        run(slot, state, samus, 0, null);
        AssertEqual(RidleyAiFunction.NorfairHoverSetup, state.Function, "movement boundary result exits the next lunge call");
        state.Velocity = state.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)-0x100)));
        move(slot, state, false);
        AssertTrue(!state.HitRoomBoundary, "moving back inside clears the boundary result");

        ResetLunge(160, 400);
        slot.Health = 0;
        state.ZeroHealthLungeCount = 9;
        run(slot, state, samus, 0, null);
        AssertEqual(RidleyAiFunction.NorfairHoverSetup, state.Function, "ninth zero-health miss still retreats");
        state.Function = RidleyAiFunction.NorfairGrabApproach;
        state.ZeroHealthLungeCount = 10;
        run(slot, state, samus, 0, null);
        AssertEqual(RidleyAiFunction.NorfairDeathExplosions, state.Function, "tenth miss immediately executes death-roar setup");
        AssertEqual(RidleyFightMode.Dying, state.FightMode, "tenth miss enters death mode without requiring a grab");
        AssertEqual((ushort)32, state.FunctionTimer.RawValue, "native death-roar timer is armed");

        var bomb = new SamusPowerBombExplosionState();
        bomb.Arm();
        bomb.Spawn(new(200, 400));
        enemies.Audio.PowerBomb = bomb;
        ResetLunge(160, 400);
        run(slot, state, samus, 0, null);
        AssertEqual(RidleyAiFunction.NorfairReturnToArena, state.Function, "bomb selects native dodge instead of ordinary retreat");
        AssertEqual(RidleyFightMode.PowerBombDodge, state.FightMode, "dodge suppresses repeated combat interruption");
        AssertTrue((short)state.Velocity.X.RawValue < 0 && (short)state.Velocity.Y.RawValue < 0,
            "low right-side bomb immediately accelerates Ridley toward upper-left target");
        bomb.ReleaseFlag();
        run(slot, state, samus, 0, null);
        AssertEqual(RidleyAiFunction.NorfairSelectAttack, state.Function, "ending bomb resumes attack selection");
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
