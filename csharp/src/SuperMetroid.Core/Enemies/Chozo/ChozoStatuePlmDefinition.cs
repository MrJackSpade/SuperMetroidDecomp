using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>One Chozo terrain PLM header paired with its native initial list.</summary>
internal readonly record struct ChozoStatuePlmDefinition(
    PlmHeaderId HeaderPointer,
    ushort InstructionListPointer);
