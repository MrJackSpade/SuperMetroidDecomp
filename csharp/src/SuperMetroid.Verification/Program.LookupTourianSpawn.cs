using System.Collections;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyTourianStatueSpawnOrder(ISnesAddressSpace rom)
    {
        var expected = new List<ushort>();
        int cursor = 0x8f91d7;
        while (rom.ReadByte(cursor) != 0x60)
        {
            AssertTrue(expected.Count < 4, "Native statue setup has bounded spawn count");
            AssertEqual((byte)0xa0, rom.ReadByte(cursor), "Original LDY immediate");
            expected.Add(ReadVerificationWord(rom, cursor + 1));
            AssertEqual((byte)0x22, rom.ReadByte(cursor + 3), "Original JSL spawn");
            AssertEqual((ushort)0x8027, ReadVerificationWord(rom, cursor + 4), "Original animated spawn entry");
            AssertEqual((byte)0x87, rom.ReadByte(cursor + 6), "Original animated spawn bank");
            cursor += 7;
        }
        AssertEqual(4, expected.Count, "Four native calls before RTS");

        var sequence = new TourianStatueSequence();
        typeof(TourianStatueSequence).GetMethod("SpawnAnimatedObjects", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(sequence, null);
        var objects = ((IEnumerable)typeof(TourianStatueSequence)
            .GetField("objects", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(sequence)!).Cast<object>().ToArray();
        AssertEqual(expected.Count, objects.Length, "Production spawn count");
        for (int index = 0; index < objects.Length; index++)
        {
            object tile = objects[index];
            var definition = (TourianStatueAnimatedTileProgramDefinition)tile.GetType().GetField("Definition")!.GetValue(tile)!;
            AssertEqual(expected[index], definition.ObjectPointer, "Native spawn/handler order");
            AssertEqual(ReadVerificationWord(rom, 0x870000 | expected[index]),
                (ushort)tile.GetType().GetField("Pointer")!.GetValue(tile)!, "Original initial program");
            AssertEqual((ushort)1, (ushort)tile.GetType().GetField("Timer")!.GetValue(tile)!, "Spawned instruction timer");
        }
    }
}
