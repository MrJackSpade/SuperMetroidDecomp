using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyRidleyMovementTargets(SuperMetroidAddressSpace rom)
    {
        ushort[] Table(int address, ReadOnlySpan<ushort> compiled)
        {
            var native = new ushort[compiled.Length];
            for (int i = 0; i < native.Length; i++)
            {
                native[i] = (ushort)(rom.ReadByte(address + i * 2) | rom.ReadByte(address + i * 2 + 1) << 8);
                AssertEqual(native[i], compiled[i], "Native Ridley target/divisor record");
            }
            return native;
        }
        var descending = Table(EnemyRomTablePointers.Ridley.DescendingPogoTargetXWords, RidleyMovementTargets.DescendingPogoX);
        var ascending = Table(EnemyRomTablePointers.Ridley.AscendingPogoTargetXWords, RidleyMovementTargets.AscendingPogoX);
        var ground = Table(EnemyRomTablePointers.Ridley.GroundAttackTargetXWords, RidleyMovementTargets.GroundAttackX);
        var carry = Table(EnemyRomTablePointers.Ridley.CarryAnchorXWords, RidleyMovementTargets.CarryAnchorX);
        var release = Table(EnemyRomTablePointers.Ridley.CarryReleaseXWords, RidleyMovementTargets.CarryReleaseX);
        var hover = Table(EnemyRomTablePointers.Ridley.HoverMovementDivisorIndexWords, RidleyMovementTargets.HoverDivisorIndexes);
        var grab = Table(EnemyRomTablePointers.Ridley.HealthMovementDivisorIndexWords, RidleyMovementTargets.GrabDivisorIndexes);
        var enemies = new RoomEnemySystem();
        const BindingFlags instance = BindingFlags.NonPublic | BindingFlags.Instance;
        typeof(RoomEnemySystem).GetField("_bus", instance)!.SetValue(enemies, new SlopeHeightNoReadBus());
        var beginCarry = typeof(RoomEnemySystem).GetMethod("BeginNorfairRidleyCarry", BindingFlags.NonPublic | BindingFlags.Static)!
            .CreateDelegate<Action<RoomEnemySlot, RidleyEnemyState>>();
        var readDivisor = typeof(RoomEnemySystem).GetMethod("ReadRidleyHealthMovementDivisorIndex", instance)!
            .CreateDelegate<Func<RidleyEnemyState, int>>(enemies);
        var pogo = typeof(RoomEnemySystem).GetMethod("TickNorfairRidleyPogo", instance)!
            .CreateDelegate<Action<RoomEnemySlot, RidleyEnemyState, SamusState?, bool>>(enemies);
        var moveSide = typeof(RoomEnemySystem).GetMethod("TickNorfairRidleyGroundAttackMoveToSide", instance)!
            .CreateDelegate<Action<RoomEnemySlot, RidleyEnemyState>>(enemies);
        var run = typeof(RoomEnemySystem).GetMethod("RunNorfairRidleyFunction", instance)!
            .CreateDelegate<Action<RoomEnemySlot, RidleyEnemyState, SamusState?, ushort, RoomLevelData?>>(enemies);
        var slot = enemies.Slots[0];
        var state = new RidleyEnemyState();
        // Independent zero-velocity specialization of native inertia: target selection
        // must change actual output velocity, not just expose a matching catalog value.
        static ushort Expected(ushort position, ushort target, int index)
        {
            int distance = unchecked((short)(position - target));
            if (distance == 0) return 0;
            int step = Math.Max(1, Math.Abs(distance) / (16 - index));
            return unchecked((ushort)Math.Clamp(distance > 0 ? -8 - step * 2 : step, -1280, 1280));
        }
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            state.FacingDirection = state.HealthStage = (ushort)raw;
            state.HorizontalVelocity = state.VerticalVelocity = 0;
            slot.XPosition = (ushort)(ushort.MaxValue - raw);
            slot.YPosition = (ushort)raw;
            beginCarry(slot, state);
            ushort x = carry[Math.Min(raw, 2)];
            ushort y = unchecked((short)(raw - 320)) < 0 ? (ushort)256 : unchecked((ushort)(raw - 64));
            AssertEqual(x, state.TargetX, "Real carry anchor facing selection");
            AssertEqual(y, state.TargetY, "Real carry height selection");
            AssertEqual(Expected(slot.XPosition, x, 0), state.HorizontalVelocity, "Carry target drives exact X acceleration");
            AssertEqual(Expected(slot.YPosition, y, 0), state.VerticalVelocity, "Carry target drives exact Y acceleration");
            AssertEqual((ushort)32, state.FunctionTimer, "Carry setup timer");
            AssertEqual(RidleyAiFunction.NorfairCarryMoveToAnchor, state.Function, "Carry setup phase");
            AssertEqual((int)hover[Math.Min(raw, 3)], readDivisor(state), "Every health word preserves existing divisor clamp");
        }
        foreach (ushort facing in new ushort[] { 0, 1, 2, 0xffff })
        foreach (ushort x in new ushort[] { 0, 128, 0xffff })
        for (ushort health = 0; health < 4; health++)
        {
            slot.XPosition = x;
            slot.YPosition = 352;
            state.FacingDirection = facing;
            state.HealthStage = health;
            foreach (bool down in new[] { false, true })
            {
                state.HorizontalVelocity = state.VerticalVelocity = 0;
                pogo(slot, state, null, down);
                ushort target = (down ? descending : ascending)[Math.Min(facing, (ushort)2)];
                AssertEqual(Expected(x, target, hover[health]), state.HorizontalVelocity, "Pogo side target and health divisor reach motion");
            }
            state.HorizontalVelocity = state.VerticalVelocity = 0;
            moveSide(slot, state);
            AssertEqual(Expected(x, ground[Math.Min(facing, (ushort)2)], 0), state.HorizontalVelocity, "Ground side target reaches motion");
            state.HorizontalVelocity = state.VerticalVelocity = 0;
            state.Function = RidleyAiFunction.NorfairCarryRelease;
            state.FunctionTimer = 1;
            run(slot, state, null, 0, null);
            AssertEqual(Expected(x, release[Math.Min(facing, (ushort)2)], 0), state.HorizontalVelocity, "Carry release target reaches motion");
        }
        // Grab approach legitimately reads Samus pose data and the not-yet-migrated
        // claw offsets, so forbid only its migrated health-table dependency here.
        typeof(RoomEnemySystem).GetField("_bus", instance)!.SetValue(enemies, new RidleyGrabDivisorReadGuard(rom));
        var approach = typeof(RoomEnemySystem).GetMethod("TickNorfairRidleyGrabApproach", instance)!
            .CreateDelegate<Action<RoomEnemySlot, RidleyEnemyState, SamusState?>>(enemies);
        var samus = new SamusState { Pose = SamusPoseIds.FacingRightNormalPose, XPosition = 1000, YPosition = 256 };
        for (int health = 0; health <= ushort.MaxValue; health++)
        {
            state.HealthStage = (ushort)health;
            state.FacingDirection = (ushort)(health % 3);
            state.HorizontalVelocity = state.VerticalVelocity = 0;
            slot.XPosition = 128;
            slot.YPosition = 352;
            approach(slot, state, samus);
            ushort targetX = (ushort)(state.FacingDirection == 0 ? 984 : 1016);
            int divisor = grab[Math.Min(health, 3)];
            AssertEqual(Expected(128, targetX, divisor), state.HorizontalVelocity, "Actual grab health divisor drives X acceleration");
            AssertEqual(Expected(352, 252, divisor), state.VerticalVelocity, "Actual grab health divisor drives Y acceleration");
        }
        Console.WriteLine("Ridley targets: 23 native words, 65536 carry/health cases, 192 side-target motions and 65536 grab approaches pass with migrated reads forbidden.");
    }

    private sealed class RidleyGrabDivisorReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address) => address is >= 0xa6bb4e and < 0xa6bb56
            ? throw new InvalidOperationException("Unexpected migrated Ridley grab divisor read.") : source.ReadByte(address);
        public void WriteByte(int address, byte value) => throw new InvalidOperationException("Unexpected Ridley target bus write.");
    }
}
