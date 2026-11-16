using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.Ridley;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Ridley's compiled movement targets and health divisors through live motion.</summary>
internal static class RidleyMovementTargetsVerification
{
    /// <summary>Compares 23 native words and exact carry, pogo, side, release, and grab acceleration.</summary>
    internal static void VerifyRidleyMovementTargets(CartridgeImportAddressSpace rom)
    {
        ushort[] Facings(int address, Func<RidleyFacing, int> compiled)
        {
            AssertEqual(3, Enum.GetValues<RidleyFacing>().Length, "Ridley facing domain");
            return Table(address, index => compiled(RidleyNativeWords.Facings.Decode(index, "Ridley facing index")));
        }
        ushort[] Table(int address, Func<int, int> compiled, int count = 3)
        {
            var native = new ushort[count];
            for (int i = 0; i < native.Length; i++)
            {
                native[i] = CartridgeImportSource.Require(rom).ReadCartridgeWord(address + i * 2);
                AssertEqual(native[i], compiled(i), "Native Ridley target/divisor record");
            }
            return native;
        }
        var behind = Facings(EnemyRomTablePointers.Ridley.DescendingPogoTargetXWords, RidleyMovementTargets.WallBehindX);
        var ahead = Facings(EnemyRomTablePointers.Ridley.AscendingPogoTargetXWords, RidleyMovementTargets.WallAheadX);
        var pogoSide = Facings(EnemyRomTablePointers.Ridley.GroundAttackTargetXWords, RidleyMovementTargets.PogoSideX);
        NorfairRidleyHealthBand[] bands = RidleyNativeWords.HealthBandsInStageOrder;
        var carry = Facings(EnemyRomTablePointers.Ridley.CarryAnchorXWords, RidleyMovementTargets.CarryAnchorX);
        var release = Facings(EnemyRomTablePointers.Ridley.CarryReleaseXWords, RidleyMovementTargets.CarryReleaseX);
        // The cartridge stores each divisor as its index into a table counting the divisors down from the largest.
        var hover = Table(EnemyRomTablePointers.Ridley.HoverMovementDivisorIndexWords,
            stage => RidleyInertiaDefinitions.LargestDivisor - bands[stage].HoverDivisor, 4);
        var grab = Table(EnemyRomTablePointers.Ridley.HealthMovementDivisorIndexWords,
            stage => RidleyInertiaDefinitions.LargestDivisor - bands[stage].GrabDivisor, 4);
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        Action<RoomEnemyPlace, RidleyEnemyState> beginCarry = NorfairRidleyCombat.BeginNorfairRidleyCarry;
        Func<RidleyEnemyState, int> readDivisor = NorfairRidleyCombat.ReadRidleyHealthMovementDivisor;
        Action<RoomEnemyPlace, RidleyEnemyState, SamusState, RidleyWallSweepTarget> sweep = enemies.RidleyBehavior.Combat.TickNorfairRidleyWallSweep;
        Action<RoomEnemyPlace, RidleyEnemyState, SamusState> moveSide = enemies.RidleyBehavior.Combat.TickNorfairRidleyPogoMoveToSide;
        // The power bomb explosion of idle bombs, none exploding, which a missed lunge would dodge.
        var powerBomb = SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.StandalonePowerBombExplosion();
        Action<RoomEnemyPlace, RidleyEnemyState, SamusState, GameplayAction, RoomLevelData> run = (ridley, ridleyState, target, input, level) =>
            enemies.RidleyBehavior.RunNorfairRidleyPhase(ridley, ridleyState, target, input, level, powerBomb);
        var place = enemies.Places.InUpdateOrder[0];
        var state = new RidleyEnemyState();
        // Independent closed form of native inertia from zero velocity: target selection
        // must change actual output velocity, not just expose a matching catalog value.
        // Toward a lower target, $A6:D559 starts the reversal SBC chain with carry set; the
        // reversal acceleration of 8 borrows, so the chain removes 8 + 1 + 2 * step. Toward a
        // higher target the single ADC carries in the unsigned CMP result (position >= target),
        // which is set when the signed distance is negative only because the words wrapped.
        static ushort Expected(ushort position, ushort target, int divisor)
        {
            int distance = unchecked((short)(position - target));
            if (distance == 0) return 0;
            int step = Math.Max(1, Math.Abs(distance) / divisor);
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
            state.HealthBand = bands[raw % 4];
            state.FacingDirection = Enum.GetValues<RidleyFacing>()[Math.Min(raw, 2)];
            state.Velocity = new EightEightVector<RoomPixels>(new SignedEightEight(0), new SignedEightEight(0));
            place.Position = place.Position.WithWhole(new((ushort)(ushort.MaxValue - raw), (ushort)raw));
            beginCarry(place, state);
            ushort x = carry[Math.Min(raw, 2)];
            ushort y = unchecked((short)(raw - 320)) < 0 ? (ushort)256 : unchecked((ushort)(raw - 64));
            AssertEqual(x, state.Target.X, "Real carry anchor facing selection");
            AssertEqual(y, state.Target.Y, "Real carry height selection");
            AssertEqual(Expected(place.Position.X.Whole, x, 16), state.Velocity.X.RawValue, "Carry target drives exact X acceleration");
            AssertEqual(Expected(place.Position.Y.Whole, y, 16), state.Velocity.Y.RawValue, "Carry target drives exact Y acceleration");
            // Setup stores 32 and then falls through into the shared timer decrement.
            AssertEqual(31, state.PhaseTimer, "Carry setup timer");
            AssertEqual(NorfairRidleyPhase.CarryMoveToAnchor, state.NorfairPhase, "Carry setup phase");
            AssertEqual(RidleyInertiaDefinitions.LargestDivisor - hover[raw % 4], readDivisor(state), "Every health band reads its native hover divisor");
        }
        foreach (RidleyFacing facing in Enum.GetValues<RidleyFacing>())
        foreach (ushort x in new ushort[] { 0, 128, 0xffff })
        for (int health = 0; health < 4; health++)
        {
            place.Position = place.Position.WithWhole(new(x, 352));
            state.FacingDirection = facing;
            state.HealthBand = bands[health];
            foreach (RidleyWallSweepTarget wall in Enum.GetValues<RidleyWallSweepTarget>())
            {
                state.Velocity = new EightEightVector<RoomPixels>(new SignedEightEight(0), new SignedEightEight(0));
                sweep(place, state, SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.StandaloneSamus(), wall);
                ushort target = (wall == RidleyWallSweepTarget.WallBehind ? behind : ahead)[RidleyNativeWords.Facings.Word(facing)];
                AssertEqual(Expected(x, target, RidleyInertiaDefinitions.LargestDivisor - hover[health]), state.Velocity.X.RawValue, "Wall sweep target and health divisor reach motion");
            }
            state.Velocity = new EightEightVector<RoomPixels>(new SignedEightEight(0), new SignedEightEight(0));
            moveSide(place, state, SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.StandaloneSamus());
            AssertEqual(Expected(x, pogoSide[RidleyNativeWords.Facings.Word(facing)], 16), state.Velocity.X.RawValue, "Pogo side target reaches motion");
            state.Velocity = new EightEightVector<RoomPixels>(new SignedEightEight(0), new SignedEightEight(0));
            state.NorfairPhase = NorfairRidleyPhase.CarryRelease;
            state.PhaseTimer = 1;
            run(place, state, SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.StandaloneSamus(), 0, SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.OpenRoom(new(64, 64)));
            AssertEqual(Expected(x, release[RidleyNativeWords.Facings.Word(facing)], 16), state.Velocity.X.RawValue, "Carry release target reaches motion");
        }
        // Grab approach legitimately reads Samus pose data and the not-yet-migrated
        // claw offsets, so forbid only its migrated health-table dependency here.
        // The power bomb explosion of idle bombs, none exploding, which a missed lunge would dodge.
        var idlePowerBomb = SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.StandalonePowerBombExplosion();
        Action<RoomEnemyPlace, RidleyEnemyState, SamusState> approach = (ridley, ridleyState, target) =>
            NorfairRidleyCombat.TickNorfairRidleyGrabApproach(ridley, ridleyState, target, idlePowerBomb);
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(256, 256)) };
        for (int health = 0; health <= ushort.MaxValue; health++)
        {
            state.HealthBand = bands[health % 4];
            state.FacingDirection = Enum.GetValues<RidleyFacing>()[health % 3];
            state.Velocity = new EightEightVector<RoomPixels>(new SignedEightEight(0), new SignedEightEight(0));
            // Start ahead of the lunge stopping boundary for each facing.
            ushort startX = state.FacingDirection == RidleyFacing.Right ? (ushort)128 : (ushort)384;
            place.Position = place.Position.WithWhole(new(startX, 128));
            state.HitRoomBoundary = false;
            approach(place, state, samus);
            ushort targetX = (ushort)(state.FacingDirection == RidleyFacing.Left ? 240 : 272);
            int divisor = RidleyInertiaDefinitions.LargestDivisor - grab[health % 4];
            AssertEqual(Expected(startX, targetX, divisor), state.Velocity.X.RawValue, "Actual grab health divisor drives X acceleration");
            AssertEqual(Expected(128, 252, divisor), state.Velocity.Y.RawValue, "Actual grab health divisor drives Y acceleration");
        }
        Console.WriteLine("Ridley targets: 23 native words, 65536 carry/health-band cases, 144 side-target motions and 65536 grab approaches pass with migrated reads forbidden.");
    }


    /// <summary>Runs Ridley target and acceleration comparisons against the cartridge.</summary>
    [VerificationCommand("--ridley-movement-target-fixture")]
    internal static int RidleyMovementTargetFixtureCommand()
    {
        Suite(nameof(VerifyRidleyMovementTargets), () => VerifyRidleyMovementTargets(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
