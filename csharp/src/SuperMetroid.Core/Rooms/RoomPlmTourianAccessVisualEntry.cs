namespace SuperMetroid.Core.Rooms;

/// <summary>One editable Tourian access-floor frame in cartridge draw-run order.</summary>
/// <param name="Id">Case-sensitive compiled identity: <c>crumble-empty-row</c>, <c>crumble-frame-0</c> through <c>crumble-frame-2</c>, or <c>clear-six-rows</c>.</param>
/// <param name="Blocks">Mutable row-major array of twelve-bit metatile/parent-flip words: four 16-by-16 cells for a single-row frame, or twenty-four for the six-row clear; collision bits are excluded.</param>
public sealed record RoomPlmTourianAccessVisualEntry(string Id, ushort[] Blocks);
