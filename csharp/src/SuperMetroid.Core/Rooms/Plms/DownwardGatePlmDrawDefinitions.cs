namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Downward-gate physical draws. Six vertical
/// five-cell frames fill one additional shootable row per step; the top tile is
/// stationary at the endpoints and moving in between. Four trigger colors use
/// consecutive descending tiles, mirrored on the right. Gameplay evaluates cells
/// directly; temporary DTOs preserve the artwork import/export interface.
/// </summary>
internal static class DownwardGatePlmDrawDefinitions
{
    /// <summary>The visual block of the column's top while the gate rests at either end.</summary>
    internal const int StationaryTopBlock = 0x0d6;
    /// <summary>The visual block of the column's top while the gate moves.</summary>
    internal const int MovingTopBlock = 0x0d7;
    /// <summary>The visual block of the column's other cells, shootable where filled and air where open.</summary>
    internal const int BodyBlock = 0x0ff;
    /// <summary>The visual block of a left trigger's solid neighbour.</summary>
    internal const int NeighbourBlock = 0x0d6;
    /// <summary>The visual block of the first trigger colour; each colour's frame steps one block down.</summary>
    internal const int TriggerFirstBlock = 0x0db;
}
