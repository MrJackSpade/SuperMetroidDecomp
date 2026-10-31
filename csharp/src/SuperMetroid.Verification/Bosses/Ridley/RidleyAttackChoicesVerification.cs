using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.Ridley;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Norfair Ridley's situation-dependent attack table and same-frame selection behavior.</summary>
internal static class RidleyAttackChoicesVerification
{
    /// <summary>Compares all 48 native choices and verifies RNG consumption, branch priority, and chosen-action setup.</summary>
    internal static void VerifyRidleyAttackChoices(SuperMetroidAddressSpace rom)
    {
        RidleyAiFunction Table(int row, int choice) => RidleyAttackChoices.Resolve(row switch
        {
            0 => RidleyAttackSituation.BelowHalfHealth,
            1 => RidleyAttackSituation.AboveHalfHealth,
            2 => RidleyAttackSituation.DamageBoosting,
            3 => RidleyAttackSituation.PogoZone,
            4 => RidleyAttackSituation.SpinJumping,
            _ => RidleyAttackSituation.ZeroHealth,
        }, choice);
        var native = new RidleyAiFunction[6, 8];
        for (int row = 0; row < 6; row++)
        for (int choice = 0; choice < 8; choice++)
        {
            int address = 0xa6b38c + row * 16 + choice * 2;
            native[row, choice] = RidleyNativeWords.AiFunctions.Decode(CartridgeImportSource.Require(rom).ReadCartridgeWord(address),
                "Ridley action");
            AssertEqual(native[row, choice], Table(row, choice), "Every native Ridley action pointer");
        }

        var actual = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        var reference = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        Action<RoomEnemySlot, RidleyEnemyState, SamusState?, ControllerButton, RoomLevelData?> select = actual.RidleyBehavior.SelectNorfairRidleyAttack;
        Action<RoomEnemySlot, RidleyEnemyState, SamusState?, ControllerButton, RoomLevelData?> runReference = reference.RidleyBehavior.RunNorfairRidleyFunction;
        ushort random = 0;
        int advances = 0, actualReads = 0, referenceReads = 0;
        actual.VerificationServices().NextRandom = () => { advances++; return random; };
        actual.VerificationServices().ReadRandom = () => { actualReads++; return random; };
        reference.VerificationServices().ReadRandom = () => { referenceReads++; return random; };
        var standing = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()) { Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(1000, 256)) };
        var low = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()) { Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(1000, 352)) };
        var spinning = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()) { Pose = SamusPoseId.SpinJumpRightPose, Position = FixedPosition<RoomPixels>.FromWhole(new(1000, 256)) };
        // Explicit native branch cases, including spin priority over zero health,
        // health thresholds, absent Samus, and Y immediately before the pogo zone.
        RidleyAttackChoicesCase[] cases =
        [
            new RidleyAttackChoicesCase(0, standing, 5), new RidleyAttackChoicesCase(0, spinning, 4), new RidleyAttackChoicesCase(1, standing, 0),
            new RidleyAttackChoicesCase(8999, standing, 0), new RidleyAttackChoicesCase(9000, standing, 0), new RidleyAttackChoicesCase(14399, low, 0),
            new RidleyAttackChoicesCase(14400, standing, 2), new RidleyAttackChoicesCase(18000, low, 3), new RidleyAttackChoicesCase(18000, null, 0),
            new RidleyAttackChoicesCase(18000, spinning, 4),
        ];
        foreach (var sample in cases)
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            random = (ushort)raw;
            advances = actualReads = referenceReads = 0;
            var state = new RidleyEnemyState { Function = RidleyAiFunction.NorfairSelectAttack, ZeroHealthLungeCount = 0 };
            var expected = new RidleyEnemyState
            {
                Function = native[sample.Row, raw & 7],
                ZeroHealthLungeCount = sample.Row == 5 ? 1 : 0,
            };
            foreach (var slot in new[] { actual.SlotTable.Slots[0], reference.SlotTable.Slots[0] })
            {
                slot.Health = sample.Health;
                slot.Position = slot.Position.WithWhole(new(128, 352));
            }
            runReference(reference.SlotTable.Slots[0], expected, sample.Samus, 0, null);
            select(actual.SlotTable.Slots[0], state, sample.Samus, 0, null);
            AssertEqual(1, advances, "Selector advances RNG exactly once");
            AssertEqual(referenceReads, actualReads, "Chosen setup reads the same current RNG word");
            AssertEqual(expected.Function, state.Function, "Native chosen callback executes on the selection frame");
            AssertEqual(expected.FunctionTimer.RawValue, state.FunctionTimer.RawValue, "Same-frame selected timer");
            AssertEqual(expected.Velocity.X.RawValue, state.Velocity.X.RawValue, "Same-frame selected X motion");
            AssertEqual(expected.Velocity.Y.RawValue, state.Velocity.Y.RawValue, "Same-frame selected Y motion");
            AssertEqual(expected.ZeroHealthLungeCount, state.ZeroHealthLungeCount, "Only a zero-health selection counts a lunge; spin takes priority");
        }
        Console.WriteLine("Ridley choices: all 48 native pointers and 655360 actual selections pass with table reads forbidden, exact RNG and same-frame setup checks.");
    }

}
