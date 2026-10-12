using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using static SuiteTimingVerification;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Hardware;
using static SharedSlopeHeightsVerification;

/// <summary>Represents ridley flight tail verification.</summary>
internal static class RidleyFlightTailVerification
{
    /// <summary>The native signed sine sample for <paramref name="angle"/> from the $A0:B443 table.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the $A0:B443 sine table used by Ridley's swoop")]
    private static short ReadRetailSwoopSine(ISnesAddressSpace rom, int angle)
    {
        int address = 0xa0b443 + unchecked((byte)angle) * 2;
        return unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(address));
    }

    /// <summary>Verifies ridley flight tail.</summary>
    internal static void VerifyRidleyFlightTail()
    {
        Action<RidleyEnemyState, int, int, int> update = CeresRidleyBehavior.UpdateRidleySwoopVelocity;
        var state = new RidleyEnemyState { SwoopSpeedMagnitude = 0x500 };
        update(state, 0, 0, 0x500);
        AssertEqual((ushort)0x500, state.Velocity.Y.RawValue, "native downward swoop uses full speed");
        state.SwoopAngleAccumulator = 0x7800;
        update(state, 0, short.MinValue, 0x500);
        AssertEqual((ushort)0x7800, state.SwoopAngleAccumulator, "native CMP/BMI retains right-facing recovery angle");
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        for (int phase = 0; phase < 256; phase++)
        {
            state.SwoopAngleAccumulator = (ushort)SnesEightEight.FromWhole(phase);
            update(state, 0, SnesEightEight.FromWhole(phase), 0x500);
            ushort NativeComponent(int angle)
            {
                short sample = ReadRetailSwoopSine(rom, angle);
                return unchecked((ushort)(Math.Sign(sample) * (SnesEightEight.Multiply(0x500, Math.Abs((int)sample)))));
            }
            AssertEqual(NativeComponent(phase), state.Velocity.X.RawValue, "native swoop X table multiplication");
            AssertEqual(NativeComponent(phase + 64), state.Velocity.Y.RawValue, "native swoop Y table multiplication");
        }
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(NewSlopeHeightNoReadBus());
        enemies.VerificationServices().ReadRandom = (Func<ushort>)(() => 0);
        Action<RoomEnemySlot, RidleyEnemyState, SamusState?> tail = enemies.RidleyTail.TickRidleyTail;
        Action<RoomEnemySlot, RidleyEnemyState, SamusState?, ushort, RoomLevelData?> run = enemies.RidleyBehavior.RunNorfairRidleyFunction;
        var slot = enemies.SlotTable.Slots[0];
        slot.Position = slot.Position.WithWhole(new(128, 128));
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
        AssertEqual(RidleyTailFunction.PointDown, state.TailFunctionIndex, "pogo wait runs pointed-tail setup");
        AssertEqual((ushort)0x3ff8, state.TailSegments[0].Angle, "setup immediately advances tail by eight");
        tail(slot, state, null);
        AssertEqual(RidleyTailFunction.Pogo, state.TailFunctionIndex, "all pointed segments finish into normal pogo");
        AssertTrue(state.TailSegments.All(segment => segment.Angle == 0x4000 && !segment.Active), "pogo tail points down and stops");
        // $D10E returns on deactivation before recomputing offsets: retain the
        // previous $F8-angle Mode 7 products (-1,+1), not the new angle's (0,2).
        AssertEqual((ushort)159, state.TailSegments[0].Position.X, "pointed root retains pre-stop native hip X");
        AssertEqual((ushort)145, state.TailSegments[0].Position.Y, "pointed root retains pre-stop native hip Y");
        state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(0));
        tail(slot, state, null);
        AssertEqual(RidleyTailFunction.StabSetup, state.TailFunctionIndex, "descending normal pogo starts stab setup");
        tail(slot, state, null);
        AssertEqual(RidleyTailFunction.Stab, state.TailFunctionIndex, "stab setup activates downward stab");
        AssertTrue(state.TailSegments.All(segment => segment.TargetDistance == 0x0a00), "stab sets every segment extension target");
        state.Function = RidleyAiFunction.NorfairFireballRecover;
        run(slot, state, null, 0, null);
        AssertEqual(RidleyTailFunction.Neutral, state.TailFunctionIndex, "leaving pogo restores neutral tail");
        AssertEqual((ushort)1, state.TailAngleDelta, "leaving pogo restores native delta");
        Action<RidleyEnemyState, SamusState?, byte> aim = enemies.RidleyTail.AimCeresRidleyTailWhip;
        state.FightMode = RidleyFightMode.Fighting;
        state.FacingDirection = RidleyFacing.Left;
        state.TailSegments[0].Position = new Position16<RoomPixels>(128, 128);
        state.TailSegments[0].Angle = 0x4000;
        state.TailSegments[6].Position = new Position16<RoomPixels>(128, 128);
        var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(128, 200)) };
        aim(state, samus, 0);
        ushort samusTarget = state.TailWhipTargetClockwiseAngle;
        var projectiles = new SamusProjectileSystem();
        typeof(ProjectileSlotTable).GetProperty("ProjectileCounter")!.SetValue(projectiles.SlotTable, (ushort)1);
        projectiles.Slots[0].Type = (ushort)SamusProjectileFamily.Missile;
        projectiles.Slots[0].Position = projectiles.Slots[0].Position.WithWhole(new(96, 128));
        enemies.FrameContext._samusProjectilesForEnemyFrame = projectiles;
        aim(state, samus, 0);
        ushort missileTarget = state.TailWhipTargetClockwiseAngle;
        AssertTrue(missileTarget != samusTarget, "nearby missile changes tail target");
        enemies.FrameContext._samusProjectilesForEnemyFrame = null;
        aim(state, new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(96, 104)) }, 0);
        AssertEqual(missileTarget, state.TailWhipTargetClockwiseAngle, "missile uses its exact position without Samus's 24-pixel offset");
        ushort seed = 0xf0;
        int advances = 0;
        enemies.VerificationServices().ReadRandom = (Func<ushort>)(() => seed);
        enemies.VerificationServices().NextRandom = (Func<ushort>)(() => { advances++; return 0; });
        Action<RoomEnemySlot, RidleyEnemyState, SamusState?> neutral = enemies.RidleyTail.HandleCeresRidleyNeutralTailControl;
        state.IdleTailWhipEnabled = 1;
        state.TailSegments[0].Angle = 0x5000;
        foreach (var segment in state.TailSegments) segment.Active = true;
        state.TailWhipTargetClockwiseAngle = state.TailWhipTargetCounterClockwiseAngle = ushort.MaxValue;
        neutral(slot, state, new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(512, 128)) });
        AssertTrue(state.TailWhipTargetClockwiseAngle != ushort.MaxValue, "native random fling admission works outside proximity range");
        AssertEqual(0, advances, "tail controller never advances RNG");
        Action<RoomEnemySlot, RidleyEnemyState, SamusState?> direction = enemies.RidleyBehavior.SetRidleyPogoHorizontalDirection;
        state.MovementMinimum = state.MovementMinimum.WithX(64);
        state.MovementMaximum = state.MovementMaximum.WithX(192);
        slot.Position = slot.Position.WithWholeX(128);
        state.Velocity = state.Velocity.WithX(new SnesSignedEightEight(0x100));
        seed = 0x1000;
        direction(slot, state, new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(64, 0)) });
        AssertEqual(unchecked((ushort)-0x100), state.Velocity.X.RawValue, "pogo bounce turns toward Samus for ordinary seed");
        slot.Position = slot.Position.WithWholeX(32);
        direction(slot, state, samus);
        AssertEqual((ushort)0x100, state.Velocity.X.RawValue, "pogo left boundary forces rightward bounce");
        slot.Position = slot.Position.WithWholeX(192);
        direction(slot, state, samus);
        AssertEqual(unchecked((ushort)-0x100), state.Velocity.X.RawValue, "pogo right boundary forces leftward bounce");
        AssertEqual(0, advances, "pogo direction does not advance RNG");
        Console.WriteLine("Ridley flight: native swoop magnitude and signed angle comparison confirmed.");
        Console.WriteLine("Ridley tail: real pogo setup, exact segment angles/position, stop, descending stab and neutral recovery confirmed.");
    }

    /// <summary>Ridley flight tail command.</summary>
    [VerificationCommand("--ridley-flight-tail")]
    internal static int RidleyFlightTailCommand()
    {
        Suite(nameof(VerifyRidleyFlightTail), () => VerifyRidleyFlightTail());
        return 0;
    }
}
