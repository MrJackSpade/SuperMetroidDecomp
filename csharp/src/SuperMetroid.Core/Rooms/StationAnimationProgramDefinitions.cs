namespace SuperMetroid.Core.Rooms;

/// <summary>Immutable timing and draw-list selections for the four station animation families.</summary>
internal static class StationAnimationProgramDefinitions
{
    /// <summary>Map station idle cycle at $84:AD66, after its $84:AD62 entry opcode.</summary>
    internal const ushort MapIdle = 0xad66;
    /// <summary>Map station acquired cycle at $84:AD76.</summary>
    internal const ushort MapAcquired = 0xad76;
    /// <summary>Energy station idle/recharged cycle at $84:ADC6.</summary>
    internal const ushort Energy = 0xadc6;
    /// <summary>Missile station idle/recharged cycle at $84:AE50.</summary>
    internal const ushort Missile = 0xae50;

    internal readonly record struct Frame(ushort Duration, ushort DrawPointer);

    private static readonly IReadOnlyDictionary<ushort, Frame[]> Programs =
        new Dictionary<ushort, Frame[]>
        {
            [MapIdle] =
            [
                new(6, 0x9f25), new(6, 0x9f31), new(6, 0x9f3d),
            ],
            [MapAcquired] =
            [
                new(2, 0x9f25), new(2, 0x9f31), new(2, 0x9f3d),
            ],
            [Energy] =
            [
                new(6, 0x9f6d), new(6, 0x9f79), new(6, 0x9f85),
            ],
            [Missile] =
            [
                new(6, 0x9f91), new(6, 0x9f9d), new(6, 0x9fa9),
            ],
            [RoomPlmInstructionLists.SaveStationIdleDraw] =
            [
                new(1, 0x9a3f),
            ],
            [RoomPlmInstructionLists.SaveStationAnimationFirstFrame] =
            [
                new(4, 0x9a9f),
            ],
            [RoomPlmInstructionLists.SaveStationAnimationSecondFrame] =
            [
                new(4, 0x9a6f),
            ],
        };

    internal static IEnumerable<(ushort Address, ushort Value)> NativeWords()
    {
        foreach ((ushort list, Frame[] frames) in Programs)
        for (int index = 0; index < frames.Length; index++)
        {
            yield return (checked((ushort)(list + 4 * index)), frames[index].Duration);
            yield return (checked((ushort)(list + 4 * index + 2)), frames[index].DrawPointer);
        }
    }

    internal static Frame Resolve(ushort list, int frameIndex)
    {
        if (!Programs.TryGetValue(list, out Frame[]? frames) ||
            (uint)frameIndex >= (uint)frames.Length)
            throw new InvalidDataException(
                $"Station animation $84:{list:X4} frame {frameIndex} is not compiled.");
        return frames[frameIndex];
    }
}
