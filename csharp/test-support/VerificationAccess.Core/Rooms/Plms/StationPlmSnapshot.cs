namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>One active station PLM as a verification snapshot.</summary>
/// <param name="ProcessingPosition">The station's place in the PLM table's processing order, zero first.</param>
internal readonly record struct StationPlmSnapshot(
    int ProcessingPosition,
    int BlockIndex,
    ushort RoomArgument,
    StationKind Kind,
    bool Triggered,
    SaveStationPhase SavePhase,
    bool SaveStationLockedOut);
