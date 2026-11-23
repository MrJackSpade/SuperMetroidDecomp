namespace Engine.Shared;

/// <summary>Where a weapon points, relative to the way its holder faces. Held apart from <see cref="Facing"/>.</summary>
public enum Aim
{
    /// <summary>Straight ahead.</summary>
    Forward,

    /// <summary>Ahead and upward.</summary>
    DiagonalUp,

    /// <summary>Straight up.</summary>
    Up,

    /// <summary>Ahead and downward.</summary>
    DiagonalDown,

    /// <summary>Straight down.</summary>
    Down,
}
