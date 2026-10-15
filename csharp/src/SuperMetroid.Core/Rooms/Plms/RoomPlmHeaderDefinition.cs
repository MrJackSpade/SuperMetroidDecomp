namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Native setup and first-instruction pointers for one retail room PLM header.</summary>
public readonly record struct RoomPlmHeaderDefinition(
    PlmHeaderId Header, ushort Setup, ushort InitialInstruction);
