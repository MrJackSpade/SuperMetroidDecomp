using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>One vertical enemy terrain probe: the target column and edge, direction, and remaining span (#633).</summary>
internal readonly record struct EnemyVerticalProbe
{
    /// <summary>Target center.</summary>
    public required ushort TargetCenter { get; init; }

    /// <summary>Target edge.</summary>
    public required ushort TargetEdge { get; init; }

    /// <summary>Moving up.</summary>
    public required bool MovingUp { get; init; }

    /// <summary>Remaining.</summary>
    public required int Remaining { get; init; }

    /// <summary>Span minus one.</summary>
    public required int SpanMinusOne { get; init; }
}
