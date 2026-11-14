namespace SuperMetroid.Core.Enemies;

/// <summary>One vertical enemy terrain probe: the target center and edge, direction, and the block of the span it tests (#633).</summary>
internal readonly record struct EnemyVerticalProbe
{
    /// <summary>
    /// The enemy's center after the move: its current column and the moved row, wrapping at sixteen bits as the
    /// game's position arithmetic does.
    /// </summary>
    public required Position16<RoomPixels> TargetCenter { get; init; }

    /// <summary>The leading edge after the move: the enemy's column and the moved row offset by its vertical radius.</summary>
    public required Position16<RoomPixels> TargetEdge { get; init; }

    /// <summary>Moving up.</summary>
    public required bool MovingUp { get; init; }

    /// <summary>Which block of the enemy's horizontal span the probe tests.</summary>
    public required EnemyTerrainSpanBlock SpanBlock { get; init; }
}
