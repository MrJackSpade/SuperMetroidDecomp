using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Rooms;

/// <summary>A save station's confirmation outcome and its same-pass first-frame draw.</summary>
public readonly record struct SaveStationConfirmationResult(
    bool Saving,
    IReadOnlyList<PlmTilemapUpdate> TilemapUpdates);
