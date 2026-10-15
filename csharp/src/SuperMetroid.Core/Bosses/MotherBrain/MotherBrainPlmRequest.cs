using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One literal <c>SpawnHardcodedPLM</c> call issued by Mother Brain's bank-$A9 AI.</summary>
/// <param name="Block">Foreground block coordinate, measured in sixteen-pixel room blocks.</param>
/// <param name="Header">Low-word PLM definition-header pointer in fixed bank $84, consumed by the room PLM allocator.</param>
public readonly record struct MotherBrainPlmRequest(Position16<RoomBlocks> Block, PlmHeaderId Header);
