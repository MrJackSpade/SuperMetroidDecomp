namespace SuperMetroid.Core.Game;

/// <summary>One randomized room-coordinate placement record for bank-$86 misc dust.</summary>
/// <param name="RandomMask">Per-axis masks applied to the random word's low (X) and high (Y) bytes; each is the scatter span minus one.</param>
/// <param name="Base">Signed offset added before the masked random scatter.</param>
internal readonly record struct MiscDustPlacementDefinition(
    Extent16<RoomPixels> RandomMask,
    Offset16<RoomPixels> Base);
