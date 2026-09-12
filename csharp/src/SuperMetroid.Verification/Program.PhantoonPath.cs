using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCompiledPhantoonPath(SuperMetroidAddressSpace rom)
    {
        for (int index = 0; index < 534; index++)
        {
            var delta = PhantoonPathDefinitions.Step(index);
            AssertEqual(unchecked((sbyte)rom.ReadByte(0xa7e3d2 + index * 2)), delta.X, "Exact Phantoon path X byte");
            AssertEqual(unchecked((sbyte)rom.ReadByte(0xa7e3d3 + index * 2)), delta.Y, "Exact Phantoon path Y byte");
        }
        var enemies = new RoomEnemySystem();
        var body = enemies.Slots[0];
        var eye = enemies.Slots[1];
        var step = typeof(RoomEnemySystem).GetMethod("StepPhantoonFigureEight", BindingFlags.Static | BindingFlags.NonPublic)!
            .CreateDelegate<Action<RoomEnemySlot, RoomEnemySlot>>();
        foreach (ushort origin in new ushort[] { 0, 128, 0xffff })
        foreach (bool reverse in new[] { false, true })
        for (int speed = 0; speed < 8; speed++)
        for (int index = 0; index < 534; index++)
        {
            body.VariableA = (ushort)index;
            body.VariableB = 0x8000;
            body.VariableC = unchecked((ushort)(reverse ? -speed : speed));
            body.VariableD = 1;
            body.XPosition = body.YPosition = origin;
            body.XSubposition = 0x1234;
            body.YSubposition = 0x5678;
            eye.VariableC = reverse ? (ushort)1 : (ushort)0;
            // Fast-stage +/-0.0625 leaves the whole word unchanged, except the
            // native reverse cap clamps -7 to -6. Independently tested speed logic
            // supplies the step count; the path oracle reads only original bytes.
            int count = reverse && speed == 7 ? 6 : speed;
            int cursor = index, x = origin, y = origin;
            for (int i = 0; i < count; i++)
            {
                int dx = unchecked((sbyte)rom.ReadByte(0xa7e3d2 + cursor * 2));
                int dy = unchecked((sbyte)rom.ReadByte(0xa7e3d3 + cursor * 2));
                x += reverse ? -dx : dx;
                y += reverse ? -dy : dy;
                cursor = (cursor + (reverse ? 533 : 1)) % 534;
            }
            step(body, eye);
            AssertEqual(unchecked((ushort)x), body.XPosition, "Real Phantoon exact world X across path steps");
            AssertEqual(unchecked((ushort)y), body.YPosition, "Real Phantoon exact world Y across path steps");
            AssertEqual((ushort)cursor, body.VariableA, "Real Phantoon path cursor wraps at both ends");
            AssertEqual(unchecked((ushort)(reverse ? -count : count)), body.VariableC, "Real Phantoon whole speed determining path steps");
            AssertEqual((ushort)0x1234, body.XSubposition, "Discrete path leaves fractional X intact");
            AssertEqual((ushort)0x5678, body.YSubposition, "Discrete path leaves fractional Y intact");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => PhantoonPathDefinitions.Step(-1), "Negative Phantoon path cursor");
        AssertThrows<ArgumentOutOfRangeException>(() => PhantoonPathDefinitions.Step(534), "Phantoon path end boundary");
        Console.WriteLine("Phantoon path: 1068 exact bytes and 25632 real multi-step movements match without a bus, including both cursor and world-coordinate wraps.");
    }
}
