using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Bosses.Phantoon;

/// <summary>Compares compiled Phantoon figure-eight deltas with retail bytes and real movement.</summary>
internal static class PhantoonPathVerification
{
    /// <summary>Checks all 534 path steps plus reverse, speed, cursor-wrap, and world-coordinate cases.</summary>
    internal static void VerifyCompiledPhantoonPath(SuperMetroidAddressSpace rom)
    {
        for (int index = 0; index < 534; index++)
        {
            var delta = PhantoonPathDefinitions.Step(index);
            AssertEqual(unchecked((sbyte)rom.ReadCartridgeByte(0xa7e3d2 + index * 2)), delta.X, "Exact Phantoon path X byte");
            AssertEqual(unchecked((sbyte)rom.ReadCartridgeByte(0xa7e3d3 + index * 2)), delta.Y, "Exact Phantoon path Y byte");
        }
        var enemies = new RoomEnemySystem(new EnemySession());
        var body = enemies.SlotTable.Slots[0];
        var eye = enemies.SlotTable.Slots[1];
        Action<RoomEnemySlot, PhantoonPartState, PhantoonPartState> step = PhantoonPatterns.StepPhantoonFigureEight;
        var bodyWords = new PhantoonPartState();
        var eyeWords = new PhantoonPartState();
        foreach (ushort origin in new ushort[] { 0, 128, 0xffff })
        foreach (bool reverse in new[] { false, true })
        for (int speed = 0; speed < 8; speed++)
        for (int index = 0; index < 534; index++)
        {
            bodyWords.PathCursor = (ushort)index;
            bodyWords.SpeedFraction = 0x8000;
            bodyWords.SpeedWhole = unchecked((ushort)(reverse ? -speed : speed));
            bodyWords.SpeedPhase = 1;
            body.Position = new(new(origin, 0x1234), new(origin, 0x5678));
            eyeWords.FigureEightReversed = reverse;
            // Fast-stage +/-0.0625 leaves the whole word unchanged, except the
            // native reverse cap clamps -7 to -6. Independently tested speed logic
            // supplies the step count; the path oracle reads only original bytes.
            int count = reverse && speed == 7 ? 6 : speed;
            int cursor = index, x = origin, y = origin;
            for (int i = 0; i < count; i++)
            {
                int dx = unchecked((sbyte)rom.ReadCartridgeByte(0xa7e3d2 + cursor * 2));
                int dy = unchecked((sbyte)rom.ReadCartridgeByte(0xa7e3d3 + cursor * 2));
                x += reverse ? -dx : dx;
                y += reverse ? -dy : dy;
                cursor = (cursor + (reverse ? 533 : 1)) % 534;
            }
            step(body, bodyWords, eyeWords);
            AssertEqual(unchecked((ushort)x), body.Position.X.Whole, "Real Phantoon exact world X across path steps");
            AssertEqual(unchecked((ushort)y), body.Position.Y.Whole, "Real Phantoon exact world Y across path steps");
            AssertEqual((ushort)cursor, bodyWords.PathCursor, "Real Phantoon path cursor wraps at both ends");
            AssertEqual(unchecked((ushort)(reverse ? -count : count)), bodyWords.SpeedWhole, "Real Phantoon whole speed determining path steps");
            AssertEqual((ushort)0x1234, body.Position.X.Fraction, "Discrete path leaves fractional X intact");
            AssertEqual((ushort)0x5678, body.Position.Y.Fraction, "Discrete path leaves fractional Y intact");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => PhantoonPathDefinitions.Step(-1), "Negative Phantoon path cursor");
        AssertThrows<ArgumentOutOfRangeException>(() => PhantoonPathDefinitions.Step(534), "Phantoon path end boundary");
        Console.WriteLine("Phantoon path: 1068 exact bytes and 25632 real multi-step movements match without a bus, including both cursor and world-coordinate wraps.");
    }
}
