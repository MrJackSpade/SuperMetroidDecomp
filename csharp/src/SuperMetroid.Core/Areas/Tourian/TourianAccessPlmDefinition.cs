using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>One Tourian access-floor PLM header and its initial instruction list.</summary>
internal readonly record struct TourianAccessPlmDefinition(
    PlmHeaderId HeaderPointer,
    ushort InstructionListPointer);
