namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>One active station PLM as a verification snapshot.</summary>
/// <param name="ProcessingPosition">The station's place in the PLM table's processing order, zero first.</param>
/// <param name="Block">The room block the station is placed at.</param>
/// <param name="RoomArgument">The word of the argument the station was placed with, or null for a station placed without one.</param>
/// <param name="Kind">Which station it is.</param>
/// <param name="Triggered">Whether Samus has triggered the station.</param>
/// <param name="SavePhase">The save station's phase.</param>
/// <param name="SaveStationLockedOut">Whether the save station is locked out.</param>
internal readonly record struct StationObjectSnapshot(
    int ProcessingPosition,
    Position16<RoomBlocks> Block,
    ushort? RoomArgument,
    StationKind Kind,
    bool Triggered,
    SaveStationPhase SavePhase,
    bool SaveStationLockedOut);
