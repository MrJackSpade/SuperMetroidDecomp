using SuperMetroid.Core.Rooms;
namespace SuperMetroid.Core.Game;

/// <summary>One literal <c>SpawnHardcodedPLM</c> call made by bank $A4's Crocomire AI.</summary>
/// <param name="Block">The room block the PLM is spawned at (native inline byte arguments).</param>
/// <param name="Header">The bank-$84 PLM header.</param>
public readonly record struct CrocomirePlmRequest(Position16<RoomBlocks> Block, PlmHeaderId Header);
