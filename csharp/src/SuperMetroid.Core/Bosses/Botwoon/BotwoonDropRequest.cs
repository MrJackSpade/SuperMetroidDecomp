namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Observable request emitted by Botwoon's specialized item-drop tail.</summary>
/// <param name="Position">Wrapping room position, randomized to X 64 through 191 and Y 128 through 191 by the specialized sixteen-drop producer.</param>
/// <param name="ItemDropChancesPointer">The retained head definition's native six-entry item-chance-table pointer, consumed by ordinary pickup selection.</param>
public readonly record struct BotwoonDropRequest(
    Position16<RoomPixels> Position,
    ushort ItemDropChancesPointer);
