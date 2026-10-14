namespace SuperMetroid.Core.Game;

/// <summary>One facing-specific body fragment placement and animation selector.</summary>
internal readonly record struct RidleyExplosionBodyPartDefinition(
    Offset16<RoomPixels> Offset,
    ushort InstructionList);
