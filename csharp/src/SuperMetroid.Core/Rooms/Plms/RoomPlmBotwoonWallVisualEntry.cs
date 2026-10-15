namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The editable nine-tile appearance of Botwoon's defeated wall.</summary>
/// <param name="Id">Compiled clear-frame visual identity; no crumble-frame identities are owned by this entry.</param>
/// <param name="Blocks">Nine visual metatile/flip words in the native clear run's order; construction copies a changed array.</param>
public sealed record RoomPlmBotwoonWallVisualEntry(string Id, ushort[] Blocks);
