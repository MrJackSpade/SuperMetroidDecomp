using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// One cross-bank PLM request made by the Chozo-statue enemy AI. Hardcoded requests carry
/// explicit room-block coordinates; ordinary requests use the block selected by the preceding
/// pixel probe. Retaining the header pointer keeps this seam consumable by bank $84 later.
/// </summary>
public readonly record struct ChozoStatuePlmRequest(
    PlmHeaderId HeaderPointer,
    Position16<RoomBlocks> Block,
    bool IsHardcoded);
