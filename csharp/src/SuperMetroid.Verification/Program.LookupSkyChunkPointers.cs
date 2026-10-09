using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Checks Land's native scrolling-sky pointer table and its camera-driven VRAM row transfers.</summary>
    /// <param name="rom">Address space used as the source of the original pointer words.</param>
    private static void VerifyLandSkyChunkPointers(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifySkyChunkPointers), () => VerifySkyChunkPointers(rom, 0x88ad9c, RoomMainCallback.ScrollingSkyLand));

    /// <summary>Checks Ocean's native scrolling-sky pointer table and its camera-driven VRAM row transfers.</summary>
    /// <param name="rom">Address space used as the source of the original pointer words.</param>
    private static void VerifyOceanSkyChunkPointers(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifySkyChunkPointers), () => VerifySkyChunkPointers(rom, 0x88ada6, RoomMainCallback.ScrollingSkyOcean));

    /// <summary>Validates native sky pointers, rejects unsupported table entries, and compares row-transfer sources across camera positions.</summary>
    /// <param name="rom">Address space containing the original sky pointer table and chunk data.</param>
    /// <param name="table">Bank-local address of the Land or Ocean pointer table.</param>
    /// <param name="callback">Room callback selecting the corresponding scrolling-sky behavior.</param>
    private static void VerifySkyChunkPointers(SuperMetroidAddressSpace rom, int table, RoomMainCallback callback)
    {
        ushort Original(int index) => ReadVerificationWord(rom, 0x880000 | ((table + 2 * index) & 0xffff));
        for (int index = 0; index <= 256; index++)
        {
            if (index < 9 || index == 255)
                AssertEqual(Original(index), ScrollingSkyChunkPointerDefinitions.Get(table, index),
                    "Sky original pointer including adjacent data/code");
            else
                AssertThrows<ArgumentOutOfRangeException>(() => ScrollingSkyChunkPointerDefinitions.Get(table, index),
                    "Sky rejects unreachable indices");
        }
        foreach (int index in new[] { int.MinValue, -1, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => ScrollingSkyChunkPointerDefinitions.Get(table, index),
                "Sky int index endpoints reject");
        foreach (int invalidTable in new[] { int.MinValue, 0, table - 1, table + 1, int.MaxValue })
        foreach (int index in new[] { -1, 0, 8, 255 })
        {
            var exception = AssertThrows<ArgumentOutOfRangeException>(
                () => ScrollingSkyChunkPointerDefinitions.Get(invalidTable, index), "Unknown sky table rejects first");
            AssertEqual("pointerTable", exception.ParamName!, "Table rejection precedes index handling");
        }

        // Exercise the actual caller's mask, signed row probes, source offset and wrap
        // over its complete ushort camera input, using only original ROM pointers.
        for (int camera = 0; camera <= ushort.MaxValue; camera++)
        {
            var queue = new VramWriteQueue();
            new ScrollingSkyState().ProcessFrame((ushort)camera, false, queue, callback);
            AssertEqual(4, queue.Entries.Count, "Sky publishes four row transfers");
            ushort upper = unchecked((ushort)((camera & 0x07f8) - 16));
            ushort lower = unchecked((ushort)((camera & 0x07f8) + 240));
            int upperSource = 0x8a0000 | unchecked((ushort)(Original(upper >> 8) + (upper & 255) * 8));
            int lowerSource = 0x8a0000 | unchecked((ushort)(Original(lower >> 8) + (lower & 255) * 8));
            AssertEqual(upperSource, queue.Entries[0].SourceAddress, "Sky upper source from native pointer");
            AssertEqual(upperSource + 64, queue.Entries[1].SourceAddress, "Sky upper continuation");
            AssertEqual(lowerSource, queue.Entries[2].SourceAddress, "Sky lower source from native pointer");
            AssertEqual(lowerSource + 64, queue.Entries[3].SourceAddress, "Sky lower continuation");
        }
    }
}
