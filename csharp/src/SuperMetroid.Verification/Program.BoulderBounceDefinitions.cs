using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyCompiledBoulderBounces(SuperMetroidAddressSpace rom)
    {
        var enemies = new RoomEnemySystem();
        var fall = typeof(RoomEnemySystem).GetMethod("RunBoulderFalling", BindingFlags.NonPublic | BindingFlags.Instance)!
            .CreateDelegate<Action<RoomEnemySlot, BoulderEnemyState, RoomLevelData>>(enemies);
        var blocks = new ushort[64];
        Array.Fill(blocks, (ushort)0x8000, 32, 32);
        var level = new RoomLevelData(8, 8, blocks, new byte[64], new ushort[64], new byte[8]);
        RoomEnemySlot actor = enemies.Slots[0];
        var state = new BoulderEnemyState(actor);
        actor.XPosition = 40;
        actor.XRadius = actor.YRadius = 8;
        for (ushort counter = 0; counter <= 2; counter++)
        {
            int address = 0xa686f1 + (counter - 1) * 2;
            ushort expected = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
            AssertEqual(expected, BoulderBounceDefinitions.SpeedIndex(counter), "Boulder native leading-zero/bounce word");
            actor.YPosition = 55;
            actor.YSubposition = 0;
            state.BounceCounter = counter;
            state.VerticalSpeedAccumulator = 0x2000;
            state.Function = BoulderAiFunction.Falling;
            fall(actor, state, level);
            AssertEqual(expected, state.VerticalSpeedAccumulator, "Boulder real collision installs compiled sample without bus");
            AssertEqual(unchecked((ushort)(counter - 1)), state.BounceCounter, "Boulder native bounce underflow");
            AssertEqual(counter == 0 ? BoulderAiFunction.Rolling : BoulderAiFunction.Rebound,
                state.Function, "Boulder last collision enters rolling");
            if (counter == 0)
            {
                AssertEqual(actor.YPosition, state.PreviousYPosition, "Boulder rolling Y handoff");
                AssertEqual(actor.YSubposition, state.PreviousYSubposition, "Boulder rolling subpixel handoff");
            }
        }
        AssertThrows<ArgumentOutOfRangeException>(() => BoulderBounceDefinitions.SpeedIndex(3), "Boulder unsupported bounce count");
        AssertThrows<ArgumentOutOfRangeException>(() => BoulderBounceDefinitions.SpeedIndex(ushort.MaxValue), "Boulder underflow is not another bounce");
        Console.WriteLine("Boulder bounce definitions: all three native words and real solid-floor collisions match without a bus, including zero-to-FFFF rolling handoff.");
    }
}
