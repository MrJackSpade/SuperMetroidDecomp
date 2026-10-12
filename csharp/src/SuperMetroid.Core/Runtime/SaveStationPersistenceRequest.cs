using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

/// <summary>Confirmed native save-point identity handed to the selected-slot SRAM owner.</summary>
public readonly record struct SaveStationPersistenceRequest(
    AreaId AreaIndex,
    ushort StationIndex);
