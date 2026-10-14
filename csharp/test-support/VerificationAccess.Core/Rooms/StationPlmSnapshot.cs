using SuperMetroid.Core.Rooms;

/// <summary>One active station PLM as a verification snapshot.</summary>
internal readonly record struct StationPlmSnapshot(
    int NativeSlotIndex,
    int BlockIndex,
    ushort RoomArgument,
    StationKind Kind,
    bool Triggered,
    SaveStationPhase SavePhase,
    bool SaveStationLockedOut);
