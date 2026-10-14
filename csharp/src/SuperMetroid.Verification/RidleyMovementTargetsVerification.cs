using static SuiteTimingVerification;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SharedSlopeHeightsVerification;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;

/// <summary>Checks Ridley's compiled movement targets and health divisors through live motion.</summary>
internal static class RidleyMovementTargetsVerification
{
    /// <summary>Compares 23 native words and exact carry, pogo, side, release, and grab acceleration.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Ridley movement targets")]
    internal static void VerifyRidleyMovementTargets(SuperMetroidAddressSpace rom)
    {
        ushort[] Facings(int address, Func<RidleyFacing, ushort> compiled)
        {
            AssertEqual(3, Enum.GetValues<RidleyFacing>().Length, "Ridley facing domain");
            return Table(address, index => compiled((RidleyFacing)index));
        }
        ushort[] Table(int address, Func<int, ushort> compiled, int count = 3)
        {
            var native = new ushort[count];
            for (int i = 0; i < native.Length; i++)
            {
                native[i] = CartridgeImportSource.Require(rom).ReadCartridgeWord(address + i * 2);
                AssertEqual(native[i], compiled(i), "Native Ridley target/divisor record");
            }
            return native;
        }
        var descending = Facings(EnemyRomTablePointers.Ridley.DescendingPogoTargetXWords, RidleyMovementTargets.DescendingPogoX);
        var ascending = Facings(EnemyRomTablePointers.Ridley.AscendingPogoTargetXWords, RidleyMovementTargets.AscendingPogoX);
        var ground = Facings(EnemyRomTablePointers.Ridley.GroundAttackTargetXWords, RidleyMovementTargets.GroundAttackX);
        var carry = Facings(EnemyRomTablePointers.Ridley.CarryAnchorXWords, RidleyMovementTargets.CarryAnchorX);
        var release = Facings(EnemyRomTablePointers.Ridley.CarryReleaseXWords, RidleyMovementTargets.CarryReleaseX);
        var hover = Table(EnemyRomTablePointers.Ridley.HoverMovementDivisorIndexWords, RidleyMovementTargets.HoverDivisorIndexes, 4);
        var grab = Table(EnemyRomTablePointers.Ridley.HealthMovementDivisorIndexWords, RidleyMovementTargets.GrabDivisorIndexes, 4);
        var enemies = new RoomEnemySystem();
        const BindingFlags instance = BindingFlags.NonPublic | BindingFlags.Instance;
        enemies.BindFixtureBus(NewSlopeHeightNoReadBus());
        Action<RoomEnemySlot, RidleyEnemyState> beginCarry = RidleyBehavior.BeginNorfairRidleyCarry;
        Func<RidleyEnemyState, int> readDivisor = RidleyBehavior.ReadRidleyHealthMovementDivisorIndex;
        Action<RoomEnemySlot, RidleyEnemyState, SamusState?, bool> pogo = enemies.RidleyBehavior.TickNorfairRidleyPogo;
        Action<RoomEnemySlot, RidleyEnemyState, SamusState?> moveSide = enemies.RidleyBehavior.TickNorfairRidleyGroundAttackMoveToSide;
        Action<RoomEnemySlot, RidleyEnemyState, SamusState?, ushort, RoomLevelData?> run = enemies.RidleyBehavior.RunNorfairRidleyFunction;
        var slot = enemies.SlotTable.Slots[0];
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
            state.HealthStage = (ushort)raw;
            state.FacingDirection = Enum.GetValues<RidleyFacing>()[Math.Min(raw, 2)];
            state.Velocity = new EightEightVector<RoomPixels>(new SnesSignedEightEight(0), new SnesSignedEightEight(0));
            slot.Position = slot.Position.WithWhole(new((ushort)(ushort.MaxValue - raw), (ushort)raw));
            beginCarry(slot, state);
            ushort x = carry[Math.Min(raw, 2)];
            ushort y = unchecked((short)(raw - 320)) < 0 ? (ushort)256 : unchecked((ushort)(raw - 64));
            AssertEqual(x, state.Target.X, "Real carry anchor facing selection");
            AssertEqual(y, state.Target.Y, "Real carry height selection");
            AssertEqual(Expected(slot.Position.X.Whole, x, 0), state.Velocity.X.RawValue, "Carry target drives exact X acceleration");
            AssertEqual(Expected(slot.Position.Y.Whole, y, 0), state.Velocity.Y.RawValue, "Carry target drives exact Y acceleration");
            // Setup stores 32 and then falls through into the shared timer decrement.
            AssertEqual((ushort)31, state.FunctionTimer.RawValue, "Carry setup timer");
            AssertEqual(RidleyAiFunction.NorfairCarryMoveToAnchor, state.Function, "Carry setup phase");
            AssertEqual((int)hover[Math.Min(raw, 3)], readDivisor(state), "Every health word preserves existing divisor clamp");
        }
        foreach (RidleyFacing facing in Enum.GetValues<RidleyFacing>())
        foreach (ushort x in new ushort[] { 0, 128, 0xffff })
        for (ushort health = 0; health < 4; health++)
        {
            slot.Position = slot.Position.WithWhole(new(x, 352));
            state.FacingDirection = facing;
            state.HealthStage = health;
            foreach (bool down in new[] { false, true })
            {
                state.Velocity = new EightEightVector<RoomPixels>(new SnesSignedEightEight(0), new SnesSignedEightEight(0));
                pogo(slot, state, null, down);
                ushort target = (down ? descending : ascending)[(int)facing];
                AssertEqual(Expected(x, target, hover[health]), state.Velocity.X.RawValue, "Pogo side target and health divisor reach motion");
            }
            state.Velocity = new EightEightVector<RoomPixels>(new SnesSignedEightEight(0), new SnesSignedEightEight(0));
            moveSide(slot, state, null);
            AssertEqual(Expected(x, ground[(int)facing], 0), state.Velocity.X.RawValue, "Ground side target reaches motion");
            state.Velocity = new EightEightVector<RoomPixels>(new SnesSignedEightEight(0), new SnesSignedEightEight(0));
            state.Function = RidleyAiFunction.NorfairCarryRelease;
            state.FunctionTimer = NativeCountdown.Start(1);
            run(slot, state, null, 0, null);
            AssertEqual(Expected(x, release[(int)facing], 0), state.Velocity.X.RawValue, "Carry release target reaches motion");
        }
        // Grab approach legitimately reads Samus pose data and the not-yet-migrated
        // claw offsets, so forbid only its migrated health-table dependency here.
        enemies.BindFixtureBus(NewRidleyGrabDivisorReadGuard(rom));
        Action<RoomEnemySlot, RidleyEnemyState, SamusState?> approach = enemies.RidleyBehavior.TickNorfairRidleyGrabApproach;
        var samus = new SamusState { Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(256, 256)) };
        for (int health = 0; health <= ushort.MaxValue; health++)
        {
            state.HealthStage = (ushort)health;
            state.FacingDirection = Enum.GetValues<RidleyFacing>()[health % 3];
            state.Velocity = new EightEightVector<RoomPixels>(new SnesSignedEightEight(0), new SnesSignedEightEight(0));
            // Start ahead of the lunge stopping boundary for each facing.
            ushort startX = state.FacingDirection == RidleyFacing.Right ? (ushort)128 : (ushort)384;
            slot.Position = slot.Position.WithWhole(new(startX, 128));
            state.HitRoomBoundary = false;
            approach(slot, state, samus);
            ushort targetX = (ushort)(state.FacingDirection == RidleyFacing.Left ? 240 : 272);
            int divisor = grab[Math.Min(health, 3)];
            AssertEqual(Expected(startX, targetX, divisor), state.Velocity.X.RawValue, "Actual grab health divisor drives X acceleration");
            AssertEqual(Expected(128, 252, divisor), state.Velocity.Y.RawValue, "Actual grab health divisor drives Y acceleration");
        }
        Console.WriteLine("Ridley targets: 23 native words, 65536 carry/health cases, 192 side-target motions and 65536 grab approaches pass with migrated reads forbidden.");
    }

    /// <summary>Forbids live reads of the migrated grab health-divisor table.</summary>
    private static CartridgeReadGuard NewRidleyGrabDivisorReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0xa6bb4e and < 0xa6bb56,
                _ => "Unexpected migrated Ridley grab divisor read.")
            .RejectWrites(_ => "Unexpected Ridley target bus write.");

    /// <summary>Runs Ridley target and acceleration comparisons against the cartridge.</summary>
    [VerificationCommand("--ridley-movement-target-fixture")]
    internal static int RidleyMovementTargetFixtureCommand()
    {
        Suite(nameof(VerifyRidleyMovementTargets), () => VerifyRidleyMovementTargets(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
