using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>One hardcoded bank-$84 PLM request issued by Kraid's bank-$A7 AI.</summary>
/// <param name="Block">The room block the PLM is spawned at (native inline byte arguments).</param>
/// <param name="Header">The bank-$84 PLM header.</param>
public readonly record struct KraidPlmRequest(Position16<RoomBlocks> Block, PlmHeaderId Header);
