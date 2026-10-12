using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Rooms;

/// <summary>One <c>SpawnEprojWithRoomGfx($CEFC, parameter)</c> emitted by glass bytecode.</summary>
public readonly record struct MotherBrainGlassProjectileRequest(
    ushort DefinitionPointer,
    ushort Parameter,
    Position16<RoomBlocks> PlmBlock);
