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
        Action<RidleyEnemyState, int, int, int> update = CeresRidleyFlight.UpdateRidleySwoopVelocity;
        var state = new RidleyEnemyState { SwoopSpeedMagnitude = 0x500 };
        update(state, 0, 0, 0x500);
        AssertEqual((ushort)0x500, state.Velocity.Y.RawValue, "native downward swoop uses full speed");
        state.SwoopAngleAccumulator = RotationAngle.FromTableIndex(120);
        update(state, 0, short.MinValue, 0x500);
        AssertEqual(RotationAngle.FromTableIndex(120), state.SwoopAngleAccumulator, "native CMP/BMI retains right-facing recovery angle");
        // A rightward step from just short of a full turn wraps the heading past zero; the half-turn target's
        // sign-of-difference test then keeps the wrapped heading instead of snapping to the target.
        state.SwoopAngleAccumulator = RotationAngle.FromTableIndex(254);
        update(state, RotationAngle.FromTableIndex(4).RawValue, short.MinValue, 0x500);
        AssertEqual(RotationAngle.FromTableIndex(2), state.SwoopAngleAccumulator, "swoop heading wraps past a full turn");
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        for (int phase = 0; phase < 256; phase++)
        {
            state.SwoopAngleAccumulator = RotationAngle.FromTableIndex((byte)phase);
            update(state, 0, EightEight.FromWhole(phase), 0x500);
            ushort NativeComponent(int angle)
            {
                short sample = ReadRetailSwoopSine(rom, angle);
                return unchecked((ushort)(Math.Sign(sample) * (EightEight.Multiply(0x500, Math.Abs((int)sample)))));
            }
            AssertEqual(NativeComponent(phase), state.Velocity.X.RawValue, "native swoop X table multiplication");
            AssertEqual(NativeComponent(phase + 64), state.Velocity.Y.RawValue, "native swoop Y table multiplication");
        }
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().ReadRandom = () => 0;
        Action<RoomEnemySlot, RidleyEnemyState, SamusState?> tail = enemies.RidleyTail.TickRidleyTail;
        Action<RoomEnemySlot, RidleyEnemyState, SamusState?, ControllerButton, RoomLevelData?> run = enemies.RidleyBehavior.RunNorfairRidleyFunction;
        var slot = enemies.SlotTable.Slots[0];
        slot.Position = slot.Position.WithWhole(new(128, 128));
        state = new RidleyEnemyState
        {
            Function = RidleyAiFunction.NorfairFireballMoveToHeight, FunctionTimer = WrappingCountdown.Start(0),
            TailSegments = Enumerable.Range(0, 7).Select(index => new RidleyTailSegment
            {
                Active = true, StaggerAngle = null, Clockwise = false,
                Angle = 0x3ff0, Distance = RidleyTailReach.RestDistance(index),
            }).ToArray(),
        };
        run(slot, state, null, 0, null);
        AssertEqual(RidleyTailFunction.PointDown, state.TailFunctionIndex, "pogo wait runs pointed-tail setup");
        AssertEqual(0x3ff8, state.TailSegments[0].Angle, "setup immediately advances tail by eight");
        tail(slot, state, null);
        AssertEqual(RidleyTailFunction.Pogo, state.TailFunctionIndex, "all pointed segments finish into normal pogo");
        AssertTrue(state.TailSegments.All(segment => segment.Angle == 0x4000 && !segment.Active), "pogo tail points down and stops");
        // $D10E returns on deactivation before recomputing offsets: retain the
        // previous $F8-angle Mode 7 products (-1,+1), not the new angle's (0,2).
        AssertEqual((ushort)159, state.TailSegments[0].Position.X, "pointed root retains pre-stop native hip X");
        AssertEqual((ushort)145, state.TailSegments[0].Position.Y, "pointed root retains pre-stop native hip Y");
        state.Velocity = state.Velocity.WithY(new SignedEightEight(0));
        tail(slot, state, null);
        AssertEqual(RidleyTailFunction.StabSetup, state.TailFunctionIndex, "descending normal pogo starts stab setup");
        tail(slot, state, null);
        AssertEqual(RidleyTailFunction.Stab, state.TailFunctionIndex, "stab setup activates downward stab");
        AssertTrue(state.TailSegments.All(segment => segment.TargetDistance == 0x0a00), "stab sets every segment extension target");
        state.Function = RidleyAiFunction.NorfairFireballRecover;
        run(slot, state, null, 0, null);
        AssertEqual(RidleyTailFunction.Neutral, state.TailFunctionIndex, "leaving pogo restores neutral tail");
        AssertEqual(1, state.TailAngleDelta, "leaving pogo restores native delta");
        Action<RidleyEnemyState, SamusState?, byte> aim = (state, samus, angle) => enemies.RidleyTail.AimCeresRidleyTailWhip(state, samus, new ByteAngle(angle));
        state.FightMode = RidleyFightMode.Fighting;
        state.FacingDirection = RidleyFacing.Left;
        state.TailSegments[0].Position = new Position16<RoomPixels>(128, 128);
        state.TailSegments[0].Angle = 0x4000;
        state.TailSegments[6].Position = new Position16<RoomPixels>(128, 128);
        var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(128, 200)) };
        aim(state, samus, 0);
        int? samusTarget = state.ClockwiseWhipTarget;
        var projectiles = new SamusProjectileSystem(new BeamChargeState(), SamusProjectileResidue.None);
        projectiles.SlotTable.ProjectileCounter = 1;
        projectiles.Slots[0].Type = SamusProjectileType.Of(SamusProjectileFamily.Missile);
        projectiles.Slots[0].Position = projectiles.Slots[0].Position.WithWhole(new(96, 128));
        enemies.FrameContext.SeedSamusProjectiles(projectiles);
        aim(state, samus, 0);
        int? missileTarget = state.ClockwiseWhipTarget;
        AssertTrue(missileTarget != samusTarget, "nearby missile changes tail target");
        enemies.FrameContext.SeedSamusProjectiles(null);
        aim(state, new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(96, 104)) }, 0);
        AssertEqual(missileTarget, state.ClockwiseWhipTarget, "missile uses its exact position without Samus's 24-pixel offset");
        ushort seed = 0xf0;
        int advances = 0;
        enemies.VerificationServices().ReadRandom = () => seed;
        enemies.VerificationServices().NextRandom = () => { advances++; return 0; };
        Action<RoomEnemySlot, RidleyEnemyState, SamusState?> neutral = enemies.RidleyTail.HandleCeresRidleyNeutralTailControl;
        state.IdleTailWhipEnabled = true;
        state.TailSegments[0].Angle = 0x5000;
        foreach (var segment in state.TailSegments) segment.Active = true;
        state.ClockwiseWhipTarget = state.CounterClockwiseWhipTarget = null;
        neutral(slot, state, new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(512, 128)) });
        AssertTrue(state.ClockwiseWhipTarget is not null, "native random fling admission works outside proximity range");
        AssertEqual(0, advances, "tail controller never advances RNG");
        Action<RoomEnemySlot, RidleyEnemyState, SamusState?> direction = enemies.RidleyBehavior.Combat.SetRidleyPogoHorizontalDirection;
        state.MovementMinimum = state.MovementMinimum.WithX(64);
        state.MovementMaximum = state.MovementMaximum.WithX(192);
        slot.Position = slot.Position.WithWholeX(128);
        state.Velocity = state.Velocity.WithX(new SignedEightEight(0x100));
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

    /// <summary>Runs the focused Ridley flight and tail check.</summary>
    [VerificationCommand("--ridley-flight-tail")]
    internal static int RidleyFlightTailCommand()
    {
        Suite(nameof(VerifyRidleyFlightTail), () => VerifyRidleyFlightTail());
        return 0;
    }
}
