using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>One Chozo terrain PLM header paired with its first program.</summary>
internal readonly record struct ChozoStatuePlmDefinition(
    PlmHeaderId HeaderPointer,
    PlmProgramStart Program);
