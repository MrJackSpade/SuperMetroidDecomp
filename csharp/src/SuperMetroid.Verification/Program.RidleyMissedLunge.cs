using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyRidleyMissedLunge()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, new SlopeHeightNoReadBus());
        var run = typeof(RoomEnemySystem).GetMethod("RunNorfairRidleyFunction", flags)!
            .CreateDelegate<Action<RoomEnemySlot, RidleyEnemyState, SamusState?, ushort, RoomLevelData?>>(enemies);
        var move = typeof(RoomEnemySystem).GetMethod("IntegrateRidleyMovement", flags)!
            .CreateDelegate<Action<RoomEnemySlot, RidleyEnemyState, bool>>(enemies);
        var slot = enemies.Slots[0];
        slot.Health = 1000;
        slot.XPosition = 160;
        slot.YPosition = 400;
        var samus = new SamusState { XPosition = 100, YPosition = 420 };
        var state = new RidleyEnemyState
        {
            Function = RidleyAiFunction.NorfairGrabApproach,
            FacingDirection = 0, FightMode = 1,
            MinimumX = 32, MaximumX = 224, MinimumY = 64, MaximumY = 416,
        };
        run(slot, state, samus, 0, null);
        AssertEqual(RidleyAiFunction.NorfairHoverSetup, state.Function,
            "low missed lunge exits immediately when Ridley Y + 35 reaches Samus Y");
        AssertEqual((ushort)1, state.TailWhipRequest, "missed lunge requests native tail whip");
        ushort lowY = slot.YPosition;
        for (int frame = 0; frame < 32; frame++)
        {
            run(slot, state, samus, 0, null);
            move(slot, state, false);
        }
        AssertTrue(slot.YPosition < lowY, "missed lunge retreats upward instead of chasing Samus at floor level");
        AssertEqual((ushort)0, state.GrabState, "retreat never falsely grabs Samus");
        void ResetLunge(ushort x, ushort y, ushort facing = 0)
        {
            slot.XPosition = x;
            slot.YPosition = y;
            slot.XSubposition = slot.YSubposition = 0;
            slot.Health = 1000;
            state.Function = RidleyAiFunction.NorfairGrabApproach;
            state.FacingDirection = facing;
            state.HorizontalVelocity = state.VerticalVelocity = 0;
            state.HitRoomBoundary = false;
            state.ZeroHealthLungeCount = 0;
            state.FightMode = 1;
            state.TailWhipRequest = 0;
        }
        // The native Y comparison is >=, and applies even when the claw's X misses.
        ResetLunge(160, 384);
        run(slot, state, samus, 0, null);
        AssertEqual(RidleyAiFunction.NorfairGrabApproach, state.Function, "one pixel above vertical cutoff continues lunge");
        ResetLunge(160, 385);
        run(slot, state, samus, 0, null);
        AssertEqual(RidleyAiFunction.NorfairHoverSetup, state.Function, "exact vertical cutoff abandons lunge");
        foreach (ushort facing in new ushort[] { 0, 2 })
        {
            int direction = facing == 0 ? -1 : 1;
            ResetLunge((ushort)(samus.XPosition + direction * 31), 300, facing);
            run(slot, state, samus, 0, null);
            AssertEqual(RidleyAiFunction.NorfairGrabApproach, state.Function, "31 pixels past Samus remains within native pass allowance");
            ResetLunge((ushort)(samus.XPosition + direction * 32), 300, facing);
            run(slot, state, samus, 0, null);
            AssertEqual(RidleyAiFunction.NorfairHoverSetup, state.Function, "32 pixels past Samus abandons lunge for either facing");
        }
        // Exercise real integration, not a manually supplied boundary flag.
        ResetLunge(223, 300);
        state.HorizontalVelocity = 0x100;
        move(slot, state, false);
        AssertTrue(state.HitRoomBoundary, "exact maximum X counts as a native boundary hit");
        run(slot, state, samus, 0, null);
        AssertEqual(RidleyAiFunction.NorfairHoverSetup, state.Function, "movement boundary result exits the next lunge call");
        state.HorizontalVelocity = unchecked((ushort)-0x100);
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
        AssertEqual((ushort)0xffff, state.FightMode, "tenth miss enters death mode without requiring a grab");
        AssertEqual((ushort)32, state.FunctionTimer, "native death-roar timer is armed");

        var bomb = new SamusPowerBombExplosionState();
        bomb.Arm();
        bomb.Spawn(200, 400);
        typeof(RoomEnemySystem).GetField("_audioPowerBomb", flags)!.SetValue(enemies, bomb);
        ResetLunge(160, 400);
        run(slot, state, samus, 0, null);
        AssertEqual(RidleyAiFunction.NorfairReturnToArena, state.Function, "bomb selects native dodge instead of ordinary retreat");
        AssertEqual((ushort)2, state.FightMode, "dodge suppresses repeated combat interruption");
        AssertTrue((short)state.HorizontalVelocity < 0 && (short)state.VerticalVelocity < 0,
            "low right-side bomb immediately accelerates Ridley toward upper-left target");
        bomb.ReleaseFlag();
        run(slot, state, samus, 0, null);
        AssertEqual(RidleyAiFunction.NorfairSelectAttack, state.Function, "ending bomb resumes attack selection");
        AssertEqual((ushort)1, state.FightMode, "ending dodge restores ordinary fight mode");
        Console.WriteLine("Ridley missed low lunge: native exit, tail request and upward retreat confirmed.");
        Console.WriteLine("Native pass/height thresholds, movement boundary handoff, tenth death lunge and bomb recovery confirmed.");
    }
}
