
namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The bank-$84 routine each shared pre-instruction ports, for comparing slots with native PLM RAM.</summary>
internal static class RoomObjectFrameBehaviorNativeRoutines
{
    /// <summary>The first of the Metroids-cleared table's nine one-byte no-op handlers (<c>$84:DAD5</c>).</summary>
    private const ushort MetroidsClearedFirstNoOp = 0xdad5;

    /// <summary>The first of its four sixteen-byte quota observers (<c>$84:DADE</c>).</summary>
    private const ushort MetroidsClearedFirstQuotaObserver = 0xdade;



    /// <summary>The routine a Metroids-cleared PLM installs for <paramref name="roomArgument"/>.</summary>
    internal static ushort MetroidsCleared(ushort roomArgument)
    {
        // Decoding rejects a word that names none of the table's entries.
        _ = RoomObjectRoomArgumentWords.Decode(RoomObjectKind.MetroidRoomClearance, roomArgument) as MetroidRoomArgument
            ?? throw new InvalidDataException($"Metroids-cleared argument ${roomArgument:X4} decodes to no watch.");
        int entry = roomArgument / 2;
        int noOps = RoomObjectRoomArgumentWords.MetroidRoomNoOpEntryCount;
        return (ushort)(entry < noOps ? MetroidsClearedFirstNoOp + entry : MetroidsClearedFirstQuotaObserver + (entry - noOps) * 16);
    }

}
