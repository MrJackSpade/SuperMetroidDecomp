namespace SuperMetroid.Core.Rooms;

/// <summary>One linked-block restoration image in cartridge run order.</summary>
/// <param name="Id">Compiled bomb or contact-crumble restoration identity for a horizontal, vertical, or square layout.</param>
/// <param name="Blocks">Metatile/flip words flattened by native run, with two words per run; changed arrays are copied by the catalog.</param>
public sealed record RoomPlmLinkedRestoreVisualEntry(string Id, ushort[] Blocks);
