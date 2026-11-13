
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
        int entry = RoomObjectRoomArgumentWords.Decode(RoomObjectKind.MetroidRoomClearance, roomArgument) is MetroidRoomArgument metroids
            ? metroids.Entry : throw new InvalidDataException($"Metroids-cleared argument ${roomArgument:X4} decodes to no entry.");
        MetroidsClearedEvents.ResolveEvent(entry);
        return (ushort)(entry < 9 ? MetroidsClearedFirstNoOp + entry : MetroidsClearedFirstQuotaObserver + (entry - 9) * 16);
    }

}
