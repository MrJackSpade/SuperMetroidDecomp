using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.World;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>The bus, level, roomObjects and gate block index of one create downward gate fixture result.</summary>
/// <param name="Bus">The bus.</param>
/// <param name="Level">The level.</param>
/// <param name="RoomObjects">The roomObjects.</param>
/// <param name="GateBlockIndex">The gate block index.</param>
internal readonly record struct CreateDownwardGateFixtureResult(TestAddressSpace Bus, RoomLevelData Level, RoomObjectSystem RoomObjects, int GateBlockIndex);
