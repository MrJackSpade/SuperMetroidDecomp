using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One hardcoded bank-$84 wall mutation emitted by Shitroid's room script.</summary>
/// <param name="Block">The room block the PLM is spawned at (native inline byte arguments).</param>
/// <param name="Header">The bank-$84 PLM header.</param>
public readonly record struct ShitroidPlmRequest(Position16<RoomBlocks> Block, PlmHeaderId Header);
