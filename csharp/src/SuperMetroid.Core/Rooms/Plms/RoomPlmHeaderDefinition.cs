namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// One PLM a retail room population places. Its setup and first program are the PLM family's own knowledge (#1414: the
/// cartridge's setup and instruction-list pointers this record carried are verification's, not the simulation's).
/// </summary>
/// <param name="Header">The PLM.</param>
public readonly record struct RoomPlmHeaderDefinition(PlmHeaderId Header);
