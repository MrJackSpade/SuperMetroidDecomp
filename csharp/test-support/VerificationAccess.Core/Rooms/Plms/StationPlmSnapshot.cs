namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>One active station PLM as a verification snapshot.</summary>
/// <param name="ProcessingPosition">The station's place in the PLM table's processing order, zero first.</param>
/// <param name="BlockIndex">The room block the station is placed at.</param>
/// <param name="RoomArgument">The room argument the station was placed with.</param>
/// <param name="Kind">Which station it is.</param>
/// <param name="Triggered">Whether Samus has triggered the station.</param>
/// <param name="SavePhase">The save station's phase.</param>
/// <param name="SaveStationLockedOut">Whether the save station is locked out.</param>
internal readonly record struct StationPlmSnapshot(
    int ProcessingPosition,
    int BlockIndex,
    ushort RoomArgument,
    StationKind Kind,
    bool Triggered,
    SaveStationPhase SavePhase,
    bool SaveStationLockedOut);
