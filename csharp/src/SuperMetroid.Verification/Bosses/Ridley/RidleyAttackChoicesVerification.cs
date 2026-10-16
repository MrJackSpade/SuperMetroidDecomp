using SuperMetroid.Core.Input;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Bosses.Ridley;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Norfair Ridley's situation-dependent attack table and same-frame selection behavior.</summary>
internal static class RidleyAttackChoicesVerification
{
    /// <summary>Compares all 48 native choices and verifies RNG consumption, branch priority, and chosen-action setup.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Ridley attack choices")]
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
            native[row, choice] = (RidleyAiFunction)CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(native[row, choice], Table(row, choice), "Every native Ridley action pointer");
        }

        const BindingFlags instance = BindingFlags.Instance | BindingFlags.NonPublic;
        var actual = new RoomEnemySystem();
        var reference = new RoomEnemySystem();
        actual.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        reference.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        Action<RoomEnemySlot, RidleyEnemyState, SamusState?, SnesButton, RoomLevelData?> select = actual.RidleyBehavior.SelectNorfairRidleyAttack;
        Action<RoomEnemySlot, RidleyEnemyState, SamusState?, SnesButton, RoomLevelData?> runReference = reference.RidleyBehavior.RunNorfairRidleyFunction;
        ushort random = 0;
        int advances = 0, actualReads = 0, referenceReads = 0;
        actual.VerificationServices().NextRandom = (Func<ushort>)(() => { advances++; return random; });
        actual.VerificationServices().ReadRandom = (Func<ushort>)(() => { actualReads++; return random; });
        reference.VerificationServices().ReadRandom = (Func<ushort>)(() => { referenceReads++; return random; });
        var standing = new SamusState { Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(1000, 256)) };
        var low = new SamusState { Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(1000, 352)) };
        var spinning = new SamusState { Pose = SamusPoseId.SpinJumpRightPose, Position = FixedPosition<RoomPixels>.FromWhole(new(1000, 256)) };
        // Explicit native branch cases, including spin priority over zero health,
        // health thresholds, absent Samus, and Y immediately before the pogo zone.
        (ushort Health, SamusState? Samus, int Row)[] cases =
        [
            (0, standing, 5), (0, spinning, 4), (1, standing, 0),
            (8999, standing, 0), (9000, standing, 0), (14399, low, 0),
            (14400, standing, 2), (18000, low, 3), (18000, null, 0),
            (18000, spinning, 4),
        ];
        foreach (var sample in cases)
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            random = (ushort)raw;
            advances = actualReads = referenceReads = 0;
            var state = new RidleyEnemyState { Function = RidleyAiFunction.NorfairSelectAttack, ZeroHealthLungeCount = ushort.MaxValue };
            var expected = new RidleyEnemyState
            {
                Function = native[sample.Row, raw & 7],
                ZeroHealthLungeCount = sample.Row == 5 ? (ushort)0 : ushort.MaxValue,
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
            AssertEqual(expected.ZeroHealthLungeCount, state.ZeroHealthLungeCount, "Zero-health lunge counter wraps; spin takes priority");
        }
        Console.WriteLine("Ridley choices: all 48 native pointers and 655360 actual selections pass with table reads forbidden, exact RNG and same-frame setup checks.");
    }

}
