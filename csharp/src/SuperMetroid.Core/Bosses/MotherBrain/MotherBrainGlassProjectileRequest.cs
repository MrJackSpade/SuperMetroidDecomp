namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One <c>SpawnEprojWithRoomGfx($CEFC, parameter)</c> emitted by glass bytecode.</summary>
public readonly record struct MotherBrainGlassProjectileRequest(
    ushort DefinitionPointer,
    ushort Parameter,
    Position16<RoomBlocks> PlmBlock);
