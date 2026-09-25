namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Bounded Kraid ceiling and spike PLM programs at $84:AB6D..ABE2. The
/// $ABD6 move-right callback is machine code, not an instruction list.
/// </summary>
internal static class KraidRoomPlmProgramDefinitions
{
    /// <summary><c>$84:AB6D</c>: ceiling crumble into background one.</summary>
    internal const ushort CrumbleCeilingBackground1 =
        RoomPlmInstructionLists.CrumbleKraidCeilingIntoBackground1;
    /// <summary><c>$84:AB79</c>: ceiling background-one final block.</summary>
    internal const ushort CeilingBackground1 = 0xab79;
    /// <summary><c>$84:AB7F</c>: ceiling crumble into background two.</summary>
    internal const ushort CrumbleCeilingBackground2 =
        RoomPlmInstructionLists.CrumbleKraidCeilingIntoBackground2;
    /// <summary><c>$84:AB8B</c>: ceiling background-two final block.</summary>
    internal const ushort CeilingBackground2 =
        RoomPlmInstructionLists.CrumbleKraidPlatformVariant1;
    /// <summary><c>$84:AB91</c>: ceiling crumble into background three.</summary>
    internal const ushort CrumbleCeilingBackground3 =
        RoomPlmInstructionLists.CrumbleKraidCeilingIntoBackground3;
    /// <summary><c>$84:AB9D</c>: ceiling background-three final block.</summary>
    internal const ushort CeilingBackground3 =
        RoomPlmInstructionLists.CrumbleKraidPlatformVariant2;
    /// <summary><c>$84:ABA3</c>: clear the defeated ceiling.</summary>
    internal const ushort ClearCeiling = RoomPlmInstructionLists.ClearKraidCeiling;
    /// <summary><c>$84:ABA9</c>: eleven two-block spike crumble passes.</summary>
    internal const ushort CrumbleSpikes = RoomPlmInstructionLists.CrumbleKraidSpikes;
    /// <summary><c>$84:ABAC</c>: spike loop body target.</summary>
    internal const ushort SpikeLoopBody = 0xabac;
    /// <summary><c>$84:ABD6</c>: first byte of move-right callback machine code.</summary>
    internal const ushort MoveRightCallback = RoomPlmInstructionCodes.MoveRightOneBlock;
    /// <summary><c>$84:ABDD</c>: clear defeated spikes.</summary>
    internal const ushort ClearSpikes = RoomPlmInstructionLists.ClearKraidSpikes;
    /// <summary><c>$84:ABE3</c>: first byte of following Mother Brain PLM program.</summary>
    internal const ushort EndExclusive = 0xabe3;
    /// <summary><c>$84:ABAB</c>: number of two-block spike crumble passes.</summary>
    internal const byte SpikePassCount = 11;
    /// <summary>Native duration of each Kraid crumble appearance.</summary>
    internal const ushort CrumbleFrameDuration = 3;

    private static readonly IReadOnlyDictionary<ushort, ushort> Words = Build();

    internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
        Words.TryGetValue(address, out value);

    internal static bool TryReadMechanicsByte(ushort address, out byte value)
    {
        if (address == CrumbleSpikes + 2)
        {
            value = SpikePassCount;
            return true;
        }
        value = 0;
        return false;
    }

    internal static IEnumerable<ushort> NativeWordAddresses() => Words.Keys.Order();

    private static IReadOnlyDictionary<ushort, ushort> Build()
    {
        var words = new Dictionary<ushort, ushort>();
        Add(CrumbleCeilingBackground1,
            [3, KraidRoomPlmDrawDefinitions.CrumbleFirst,
             3, KraidRoomPlmDrawDefinitions.CrumbleSecond,
             3, KraidRoomPlmDrawDefinitions.CrumbleThird]);
        Add(CeilingBackground1,
            [3, KraidRoomPlmDrawDefinitions.CeilingBackground1,
             RoomPlmInstructionCodes.Delete]);
        Add(CrumbleCeilingBackground2,
            [3, KraidRoomPlmDrawDefinitions.CrumbleFirst,
             3, KraidRoomPlmDrawDefinitions.CrumbleSecond,
             3, KraidRoomPlmDrawDefinitions.CrumbleThird]);
        Add(CeilingBackground2,
            [3, KraidRoomPlmDrawDefinitions.CeilingBackground2,
             RoomPlmInstructionCodes.Delete]);
        Add(CrumbleCeilingBackground3,
            [3, KraidRoomPlmDrawDefinitions.CrumbleFirst,
             3, KraidRoomPlmDrawDefinitions.CrumbleSecond,
             3, KraidRoomPlmDrawDefinitions.CrumbleThird]);
        Add(CeilingBackground3,
            [3, KraidRoomPlmDrawDefinitions.CeilingBackground3,
             RoomPlmInstructionCodes.Delete]);
        Add(ClearCeiling,
            [1, KraidRoomPlmDrawDefinitions.ClearCeiling,
             RoomPlmInstructionCodes.Delete]);
        Add(CrumbleSpikes,
            [RoomPlmInstructionCodes.SetEightBitTimer]);
        Add(SpikeLoopBody,
            [3, KraidRoomPlmDrawDefinitions.CrumbleFirst,
             3, KraidRoomPlmDrawDefinitions.CrumbleSecond,
             3, KraidRoomPlmDrawDefinitions.CrumbleThird,
             3, KraidRoomPlmDrawDefinitions.SpikeFirst,
             MoveRightCallback,
             3, KraidRoomPlmDrawDefinitions.CrumbleFirst,
             3, KraidRoomPlmDrawDefinitions.CrumbleSecond,
             3, KraidRoomPlmDrawDefinitions.CrumbleThird,
             3, KraidRoomPlmDrawDefinitions.SpikeSecond,
             MoveRightCallback,
             RoomPlmInstructionCodes.DecrementTimerAndGoto, SpikeLoopBody,
             RoomPlmInstructionCodes.Delete]);
        Add(ClearSpikes,
            [1, KraidRoomPlmDrawDefinitions.ClearSpikes,
             RoomPlmInstructionCodes.Delete]);
        return words;

        void Add(ushort start, ushort[] values)
        {
            for (int index = 0; index < values.Length; index++)
            {
                ushort address = checked((ushort)(start + index * 2));
                if (!words.TryAdd(address, values[index]))
                    throw new InvalidDataException(
                        $"Duplicate compiled Kraid instruction ${address:X4}.");
            }
        }
    }
}
