namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>A save station's confirmation outcome and its same-pass first-frame draw.</summary>
public readonly record struct SaveStationConfirmationResult(
    bool Saving,
    IReadOnlyList<PlmTilemapUpdate> TilemapUpdates);
