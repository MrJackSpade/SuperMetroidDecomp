namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>A cartridge memory image's floor (#1467): its map, each word's low byte, and its tile pixels, each word's high byte.</summary>
/// <param name="Map">The floor's cells, row by row.</param>
/// <param name="Pixels">The tiles' pixels, one colour index each, tile after tile.</param>
internal sealed record FloorMemory(byte[] Map, byte[] Pixels);
