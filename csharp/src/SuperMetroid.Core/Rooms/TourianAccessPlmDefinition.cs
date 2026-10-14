namespace SuperMetroid.Core.Rooms;

/// <summary>One Tourian access-floor PLM header and its initial instruction list.</summary>
internal readonly record struct TourianAccessPlmDefinition(
    PlmHeaderId HeaderPointer,
    ushort InstructionListPointer);
