namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>One Bomb Torizo hand frame's visible blocks in cartridge run order.</summary>
/// <param name="Id">Compiled Bomb Torizo hand draw-frame identity.</param>
/// <param name="Blocks">Visual metatile/flip words flattened by native run, then by word within each run; changed payloads are copied by the catalog.</param>
public sealed record RoomPlmBombTorizoHandVisualEntry(string Id, ushort[] Blocks);
