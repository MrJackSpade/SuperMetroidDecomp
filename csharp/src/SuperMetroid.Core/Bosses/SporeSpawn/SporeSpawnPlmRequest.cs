using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>One hardcoded bank-$84 ceiling mutation published by Spore Spawn.</summary>
/// <param name="Block">Zero-based room block; the native ceiling request uses column seven, row thirty.</param>
/// <param name="Header">Bank-relative PLM header offset in bank $84, selecting the ceiling clear or crumble operation.</param>
public readonly record struct SporeSpawnPlmRequest(Position16<RoomBlocks> Block, PlmHeaderId Header);
