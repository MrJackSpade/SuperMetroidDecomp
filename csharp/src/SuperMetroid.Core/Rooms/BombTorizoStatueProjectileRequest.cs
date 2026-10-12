using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Rooms;

/// <summary>One <c>SpawnEprojWithRoomGfx($A993, parameter)</c> hand fragment.</summary>
public readonly record struct BombTorizoStatueProjectileRequest(
    ushort DefinitionPointer,
    ushort Parameter,
    Position16<RoomBlocks> PlmBlock);
