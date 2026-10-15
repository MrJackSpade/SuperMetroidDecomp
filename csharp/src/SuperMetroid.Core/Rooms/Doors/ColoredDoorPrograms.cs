namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The bank-$84 instruction lists and draw a coloured door runs through its lifetime (#633).</summary>
internal readonly record struct ColoredDoorPrograms(
    ushort InitialList, ushort ClosedBlueList, ushort HitList, ushort OpeningList, ushort ColoredClosedDraw);
