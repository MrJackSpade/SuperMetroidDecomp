using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyRidleyAttackChoices(SuperMetroidAddressSpace rom)
    {
        ReadOnlySpan<RidleyAiFunction> Table(int row) => row switch
        {
            0 => RidleyAttackChoices.BelowHalfHealth,
            1 => RidleyAttackChoices.AboveHalfHealth,
            2 => RidleyAttackChoices.DamageBoosting,
            3 => RidleyAttackChoices.PogoZone,
            4 => RidleyAttackChoices.SpinJumping,
            _ => RidleyAttackChoices.ZeroHealth,
        };
        var native = new RidleyAiFunction[6, 8];
        for (int row = 0; row < 6; row++)
        for (int choice = 0; choice < 8; choice++)
        {
            int address = 0xa6b38c + row * 16 + choice * 2;
            native[row, choice] = (RidleyAiFunction)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
            AssertEqual(native[row, choice], Table(row)[choice], "Every native Ridley action pointer");
        }

        const BindingFlags instance = BindingFlags.Instance | BindingFlags.NonPublic;
        var actual = new RoomEnemySystem();
        var reference = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", instance)!.SetValue(actual, new RidleyAttackReadGuard(rom));
        typeof(RoomEnemySystem).GetField("_bus", instance)!.SetValue(reference, rom);
        var select = typeof(RoomEnemySystem).GetMethod("SelectNorfairRidleyAttack", instance)!
            .CreateDelegate<Action<RoomEnemySlot, RidleyEnemyState, SamusState?, ushort, RoomLevelData?>>(actual);
        var runReference = typeof(RoomEnemySystem).GetMethod("RunNorfairRidleyFunction", instance)!
            .CreateDelegate<Action<RoomEnemySlot, RidleyEnemyState, SamusState?, ushort, RoomLevelData?>>(reference);
        ushort random = 0;
        int advances = 0, actualReads = 0, referenceReads = 0;
        typeof(RoomEnemySystem).GetField("_nextRandom", instance)!.SetValue(actual, (Func<ushort>)(() => { advances++; return random; }));
        typeof(RoomEnemySystem).GetField("_readRandomNumber", instance)!.SetValue(actual, (Func<ushort>)(() => { actualReads++; return random; }));
        typeof(RoomEnemySystem).GetField("_readRandomNumber", instance)!.SetValue(reference, (Func<ushort>)(() => { referenceReads++; return random; }));
        var standing = new SamusState { Pose = SamusPoseIds.FacingRightNormalPose, XPosition = 1000, YPosition = 256 };
        var low = new SamusState { Pose = SamusPoseIds.FacingRightNormalPose, XPosition = 1000, YPosition = 352 };
        var spinning = new SamusState { Pose = SamusPoseIds.SpinJumpRightPose, XPosition = 1000, YPosition = 256 };
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
            foreach (var slot in new[] { actual.Slots[0], reference.Slots[0] })
            {
                slot.Health = sample.Health;
                slot.XPosition = 128;
                slot.YPosition = 352;
            }
            runReference(reference.Slots[0], expected, sample.Samus, 0, null);
            select(actual.Slots[0], state, sample.Samus, 0, null);
            AssertEqual(1, advances, "Selector advances RNG exactly once");
            AssertEqual(referenceReads, actualReads, "Chosen setup reads the same current RNG word");
            AssertEqual(expected.Function, state.Function, "Native chosen callback executes on the selection frame");
            AssertEqual(expected.FunctionTimer, state.FunctionTimer, "Same-frame selected timer");
            AssertEqual(expected.HorizontalVelocity, state.HorizontalVelocity, "Same-frame selected X motion");
            AssertEqual(expected.VerticalVelocity, state.VerticalVelocity, "Same-frame selected Y motion");
            AssertEqual(expected.ZeroHealthLungeCount, state.ZeroHealthLungeCount, "Zero-health lunge counter wraps; spin takes priority");
        }
        Console.WriteLine("Ridley choices: all 48 native pointers and 655360 actual selections pass with table reads forbidden, exact RNG and same-frame setup checks.");
    }

    private sealed class RidleyAttackReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address) => address is >= 0xa6b38c and < 0xa6b3ec
            ? throw new InvalidOperationException("Unexpected migrated Ridley attack-choice read.") : source.ReadByte(address);
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
