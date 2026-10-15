namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>One Chozo statue PLM layout's visual blocks in native run order.</summary>
/// <param name="Id">Compiled Chozo hand or slope-access draw-layout identity.</param>
/// <param name="Blocks">Visual metatile/flip words flattened in native run order and word order within each run; changed arrays are copied by the catalog.</param>
public sealed record RoomPlmChozoStatueVisualEntry(string Id, ushort[] Blocks);
