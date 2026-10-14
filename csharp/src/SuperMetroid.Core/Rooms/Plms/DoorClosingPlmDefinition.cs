namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>One direction-selected fallback door-closing PLM definition.</summary>
internal readonly record struct DoorClosingPlmDefinition(
    PlmHeaderId Header,
    ushort InitialInstructionList);
