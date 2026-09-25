namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Crocomire's five finite room-object instruction lists at $84:AFCA..AFE7.
/// Each list draws its physical layout once, then deletes its PLM.
/// </summary>
internal static class CrocomireArenaPlmProgramDefinitions
{
    /// <summary><c>$84:AFCA</c>: clear the ten-block bridge.</summary>
    internal const ushort ClearBridge = RoomPlmInstructionLists.ClearCrocomireBridge;
    /// <summary><c>$84:AFD0</c>: crumble one bridge block.</summary>
    internal const ushort CrumbleBridgeBlock = RoomPlmInstructionLists.CrumbleCrocomireBridgeBlock;
    /// <summary><c>$84:AFD6</c>: clear one bridge block.</summary>
    internal const ushort ClearBridgeBlock = RoomPlmInstructionLists.ClearCrocomireBridgeBlock;
    /// <summary><c>$84:AFDC</c>: clear the invisible wall.</summary>
    internal const ushort ClearInvisibleWall = RoomPlmInstructionLists.ClearCrocomireInvisibleWall;
    /// <summary><c>$84:AFE2</c>: create the invisible wall.</summary>
    internal const ushort CreateInvisibleWall = RoomPlmInstructionLists.CreateCrocomireInvisibleWall;
    /// <summary><c>$84:AFE8</c>: first byte of the following save-station program.</summary>
    internal const ushort EndExclusive = 0xafe8;

    private static readonly Dictionary<ushort, ushort> Words = Build();

    internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
        Words.TryGetValue(address, out value);

    internal static IEnumerable<ushort> NativeWordAddresses() => Words.Keys.Order();

    private static Dictionary<ushort, ushort> Build()
    {
        var words = new Dictionary<ushort, ushort>();
        Add(ClearBridge, CrocomireArenaPlmDrawDefinitions.ClearBridge);
        Add(CrumbleBridgeBlock,
            CrocomireArenaPlmDrawDefinitions.CrumbleBridgeBlock);
        Add(ClearBridgeBlock,
            CrocomireArenaPlmDrawDefinitions.ClearBridgeBlock);
        Add(ClearInvisibleWall,
            CrocomireArenaPlmDrawDefinitions.ClearInvisibleWall);
        Add(CreateInvisibleWall,
            CrocomireArenaPlmDrawDefinitions.CreateInvisibleWall);
        return words;

        void Add(ushort start, ushort drawPointer)
        {
            if (!words.TryAdd(start, 1) ||
                !words.TryAdd(checked((ushort)(start + 2)), drawPointer) ||
                !words.TryAdd(checked((ushort)(start + 4)),
                    RoomPlmInstructionCodes.Delete))
                throw new InvalidDataException(
                    $"Crocomire PLM program at ${start:X4} overlaps another list.");
        }
    }
}
