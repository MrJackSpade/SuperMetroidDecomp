using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyRidleyMovementTargets(SuperMetroidAddressSpace rom)
    {
        ushort[] Table(int address, Func<int, ushort> compiled, int count = 3)
        {
            var native = new ushort[count];
            for (int i = 0; i < native.Length; i++)
            {
                native[i] = (ushort)(rom.ReadByte(address + i * 2) | rom.ReadByte(address + i * 2 + 1) << 8);
                AssertEqual(native[i], compiled(i), "Native Ridley target/divisor record");
            }
            return native;
        }
        var descending = Table(EnemyRomTablePointers.Ridley.DescendingPogoTargetXWords, RidleyMovementTargets.DescendingPogoX);
        var ascending = Table(EnemyRomTablePointers.Ridley.AscendingPogoTargetXWords, RidleyMovementTargets.AscendingPogoX);
        var ground = Table(EnemyRomTablePointers.Ridley.GroundAttackTargetXWords, RidleyMovementTargets.GroundAttackX);
        var carry = Table(EnemyRomTablePointers.Ridley.CarryAnchorXWords, RidleyMovementTargets.CarryAnchorX);
        var release = Table(EnemyRomTablePointers.Ridley.CarryReleaseXWords, RidleyMovementTargets.CarryReleaseX);
        var hover = Table(EnemyRomTablePointers.Ridley.HoverMovementDivisorIndexWords, RidleyMovementTargets.HoverDivisorIndexes, 4);
        var grab = Table(EnemyRomTablePointers.Ridley.HealthMovementDivisorIndexWords, RidleyMovementTargets.GrabDivisorIndexes, 4);
        var enemies = new RoomEnemySystem();
        const BindingFlags instance = BindingFlags.NonPublic | BindingFlags.Instance;
        typeof(RoomEnemySystem).GetField("_bus", instance)!.SetValue(enemies, new SlopeHeightNoReadBus());
        var beginCarry = typeof(RoomEnemySystem).GetMethod("BeginNorfairRidleyCarry", BindingFlags.NonPublic | BindingFlags.Static)!
            .CreateDelegate<Action<RoomEnemySlot, RidleyEnemyState>>();
        var readDivisor = typeof(RoomEnemySystem).GetMethod("ReadRidleyHealthMovementDivisorIndex", BindingFlags.NonPublic | BindingFlags.Static)!
            .CreateDelegate<Func<RidleyEnemyState, int>>();
        var pogo = typeof(RoomEnemySystem).GetMethod("TickNorfairRidleyPogo", instance)!
            .CreateDelegate<Action<RoomEnemySlot, RidleyEnemyState, SamusState?, bool>>(enemies);
        var moveSide = typeof(RoomEnemySystem).GetMethod("TickNorfairRidleyGroundAttackMoveToSide", instance)!
            .CreateDelegate<Action<RoomEnemySlot, RidleyEnemyState, SamusState?>>(enemies);
        var run = typeof(RoomEnemySystem).GetMethod("RunNorfairRidleyFunction", instance)!
            .CreateDelegate<Action<RoomEnemySlot, RidleyEnemyState, SamusState?, ushort, RoomLevelData?>>(enemies);
        var slot = enemies.Slots[0];
        var state = new RidleyEnemyState();
        // Independent closed form of native inertia from zero velocity: target selection
        // must change actual output velocity, not just expose a matching catalog value.
        // Toward a lower target, $A6:D559 starts the reversal SBC chain with carry set; the
        // reversal acceleration of 8 borrows, so the chain removes 8 + 1 + 2 * step. Toward a
        // higher target the single ADC carries in the unsigned CMP result (position >= target),
        // which is set when the signed distance is negative only because the words wrapped.
        static ushort Expected(ushort position, ushort target, int index)
        {
            int distance = unchecked((short)(position - target));
            if (distance == 0) return 0;
            int step = Math.Max(1, Math.Abs(distance) / (16 - index));
            if (distance > 0)
            {
                ushort lowered = unchecked((ushort)(-9 - 2 * step));
                return unchecked((short)(lowered + 1280)) < 0 ? unchecked((ushort)-1280) : lowered;
            }
            ushort raised = unchecked((ushort)(step + (position >= target ? 1 : 0)));
            return unchecked((short)(raised - 1280)) >= 0 ? (ushort)1280 : raised;
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
            // Setup stores 32 and then falls through into the shared timer decrement.
            AssertEqual((ushort)31, state.FunctionTimer, "Carry setup timer");
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
            moveSide(slot, state, null);
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
        var samus = new SamusState { Pose = SamusPoseId.FacingRightNormalPose, XPosition = 256, YPosition = 256 };
        for (int health = 0; health <= ushort.MaxValue; health++)
        {
            state.HealthStage = (ushort)health;
            state.FacingDirection = (ushort)(health % 3);
            state.HorizontalVelocity = state.VerticalVelocity = 0;
            // Start ahead of the lunge stopping boundary for each facing.
            ushort startX = state.FacingDirection == 2 ? (ushort)128 : (ushort)384;
            slot.XPosition = startX;
            slot.YPosition = 128;
            state.HitRoomBoundary = false;
            approach(slot, state, samus);
            ushort targetX = (ushort)(state.FacingDirection == 0 ? 240 : 272);
            int divisor = grab[Math.Min(health, 3)];
            AssertEqual(Expected(startX, targetX, divisor), state.HorizontalVelocity, "Actual grab health divisor drives X acceleration");
            AssertEqual(Expected(128, 252, divisor), state.VerticalVelocity, "Actual grab health divisor drives Y acceleration");
        }
        Console.WriteLine("Ridley targets: 23 native words, 65536 carry/health cases, 192 side-target motions and 65536 grab approaches pass with migrated reads forbidden.");
    }

    private sealed class RidleyGrabDivisorReadGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address) => address is >= 0xa6bb4e and < 0xa6bb56
            ? throw new InvalidOperationException("Unexpected migrated Ridley grab divisor read.") : source.ReadByte(address);
        public void WriteByte(int address, byte value) => throw new InvalidOperationException("Unexpected Ridley target bus write.");
    }
}
