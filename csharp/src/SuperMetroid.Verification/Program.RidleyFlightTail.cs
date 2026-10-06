using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyRidleyFlightTail()
    {
        var update = typeof(RoomEnemySystem).GetMethod("UpdateRidleySwoopVelocity", BindingFlags.Static | BindingFlags.NonPublic)!
            .CreateDelegate<Action<RidleyEnemyState, int, int, int>>();
        var state = new RidleyEnemyState { SwoopSpeedMagnitude = 0x500 };
        update(state, 0, 0, 0x500);
        AssertEqual((ushort)0x500, state.VerticalVelocity, "native downward swoop uses full speed");
        state.SwoopAngleAccumulator = 0x7800;
        update(state, 0, short.MinValue, 0x500);
        AssertEqual((ushort)0x7800, state.SwoopAngleAccumulator, "native CMP/BMI retains right-facing recovery angle");
        var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        for (int phase = 0; phase < 256; phase++)
        {
            state.SwoopAngleAccumulator = (ushort)(phase << 8);
            update(state, 0, phase << 8, 0x500);
            ushort NativeComponent(int angle)
            {
                int address = 0xa0b443 + (angle & 255) * 2;
                short sample = unchecked((short)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8));
                return unchecked((ushort)(Math.Sign(sample) * (0x500 * Math.Abs((int)sample) >> 8)));
            }
            AssertEqual(NativeComponent(phase), state.HorizontalVelocity, "native swoop X table multiplication");
            AssertEqual(NativeComponent(phase + 64), state.VerticalVelocity, "native swoop Y table multiplication");
        }
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, new SlopeHeightNoReadBus());
        typeof(RoomEnemySystem).GetField("_readRandomNumber", flags)!.SetValue(enemies, (Func<ushort>)(() => 0));
        var tail = typeof(RoomEnemySystem).GetMethod("TickRidleyTail", flags)!
            .CreateDelegate<Action<RoomEnemySlot, RidleyEnemyState, SamusState?>>(enemies);
        var run = typeof(RoomEnemySystem).GetMethod("RunNorfairRidleyFunction", flags)!
            .CreateDelegate<Action<RoomEnemySlot, RidleyEnemyState, SamusState?, ushort, RoomLevelData?>>(enemies);
        var slot = enemies.Slots[0];
        slot.XPosition = slot.YPosition = 128;
        state = new RidleyEnemyState
        {
            Function = RidleyAiFunction.NorfairFireballMoveToHeight, FunctionTimer = 0,
            TailSegments = Enumerable.Range(0, 7).Select(index => new RidleyTailSegment
            {
                Active = true, StaggerAngle = ushort.MaxValue, MovementDirection = 0,
                Angle = 0x3ff0, Distance = RidleyTailDefinitions.RestDistance(index),
            }).ToArray(),
        };
        run(slot, state, null, 0, null);
        AssertEqual(RidleyTailDefinitions.PointDown, state.TailFunctionIndex, "pogo wait runs pointed-tail setup");
        AssertEqual((ushort)0x3ff8, state.TailSegments[0].Angle, "setup immediately advances tail by eight");
        tail(slot, state, null);
        AssertEqual(RidleyTailDefinitions.Pogo, state.TailFunctionIndex, "all pointed segments finish into normal pogo");
        AssertTrue(state.TailSegments.All(segment => segment.Angle == 0x4000 && !segment.Active), "pogo tail points down and stops");
        AssertEqual((ushort)160, state.TailSegments[0].XPosition, "pointed root has native facing-left hip X");
        AssertEqual((ushort)146, state.TailSegments[0].YPosition, "pointed root has native two-pixel Y offset");
        state.VerticalVelocity = 0;
        tail(slot, state, null);
        AssertEqual(RidleyTailDefinitions.StabSetup, state.TailFunctionIndex, "descending normal pogo starts stab setup");
        tail(slot, state, null);
        AssertEqual(RidleyTailDefinitions.Stab, state.TailFunctionIndex, "stab setup activates downward stab");
        AssertTrue(state.TailSegments.All(segment => segment.TargetDistance == 0x0a00), "stab sets every segment extension target");
        state.Function = RidleyAiFunction.NorfairFireballRecover;
        run(slot, state, null, 0, null);
        AssertEqual(RidleyTailDefinitions.Neutral, state.TailFunctionIndex, "leaving pogo restores neutral tail");
        AssertEqual((ushort)1, state.TailAngleDelta, "leaving pogo restores native delta");
        var aim = typeof(RoomEnemySystem).GetMethod("AimCeresRidleyTailWhip", flags)!
            .CreateDelegate<Action<RidleyEnemyState, SamusState?, byte>>(enemies);
        state.FightMode = 1;
        state.FacingDirection = 0;
        state.TailSegments[0].XPosition = state.TailSegments[0].YPosition = 128;
        state.TailSegments[0].Angle = 0x4000;
        state.TailSegments[6].XPosition = state.TailSegments[6].YPosition = 128;
        var samus = new SamusState { XPosition = 128, YPosition = 200 };
        aim(state, samus, 0);
        ushort samusTarget = state.TailWhipTargetClockwiseAngle;
        var projectiles = new SamusProjectileSystem();
        typeof(SamusProjectileSystem).GetProperty("ProjectileCounter")!.SetValue(projectiles, (ushort)1);
        projectiles.Slots[0].Type = (ushort)SamusProjectileFamily.Missile;
        projectiles.Slots[0].XPosition = 96;
        projectiles.Slots[0].YPosition = 128;
        typeof(RoomEnemySystem).GetField("_samusProjectilesForEnemyFrame", flags)!.SetValue(enemies, projectiles);
        aim(state, samus, 0);
        ushort missileTarget = state.TailWhipTargetClockwiseAngle;
        AssertTrue(missileTarget != samusTarget, "nearby missile changes tail target");
        typeof(RoomEnemySystem).GetField("_samusProjectilesForEnemyFrame", flags)!.SetValue(enemies, null);
        aim(state, new SamusState { XPosition = 96, YPosition = 104 }, 0);
        AssertEqual(missileTarget, state.TailWhipTargetClockwiseAngle, "missile uses its exact position without Samus's 24-pixel offset");
        ushort seed = 0xf0;
        int advances = 0;
        typeof(RoomEnemySystem).GetField("_readRandomNumber", flags)!.SetValue(enemies, (Func<ushort>)(() => seed));
        typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(enemies, (Func<ushort>)(() => { advances++; return 0; }));
        var neutral = typeof(RoomEnemySystem).GetMethod("HandleCeresRidleyNeutralTailControl", flags)!
            .CreateDelegate<Action<RoomEnemySlot, RidleyEnemyState, SamusState?>>(enemies);
        state.IdleTailWhipEnabled = 1;
        state.TailSegments[0].Angle = 0x5000;
        foreach (var segment in state.TailSegments) segment.Active = true;
        state.TailWhipTargetClockwiseAngle = state.TailWhipTargetCounterClockwiseAngle = ushort.MaxValue;
        neutral(slot, state, new SamusState { XPosition = 512, YPosition = 128 });
        AssertTrue(state.TailWhipTargetClockwiseAngle != ushort.MaxValue, "native random fling admission works outside proximity range");
        AssertEqual(0, advances, "tail controller never advances RNG");
        var direction = typeof(RoomEnemySystem).GetMethod("SetRidleyPogoHorizontalDirection", flags)!
            .CreateDelegate<Action<RoomEnemySlot, RidleyEnemyState, SamusState?>>(enemies);
        state.MinimumX = 64;
        state.MaximumX = 192;
        slot.XPosition = 128;
        state.HorizontalVelocity = 0x100;
        seed = 0x1000;
        direction(slot, state, new SamusState { XPosition = 64 });
        AssertEqual(unchecked((ushort)-0x100), state.HorizontalVelocity, "pogo bounce turns toward Samus for ordinary seed");
        slot.XPosition = 32;
        direction(slot, state, samus);
        AssertEqual((ushort)0x100, state.HorizontalVelocity, "pogo left boundary forces rightward bounce");
        slot.XPosition = 192;
        direction(slot, state, samus);
        AssertEqual(unchecked((ushort)-0x100), state.HorizontalVelocity, "pogo right boundary forces leftward bounce");
        AssertEqual(0, advances, "pogo direction does not advance RNG");
        Console.WriteLine("Ridley flight: native swoop magnitude and signed angle comparison confirmed.");
        Console.WriteLine("Ridley tail: real pogo setup, exact segment angles/position, stop, descending stab and neutral recovery confirmed.");
    }
}
