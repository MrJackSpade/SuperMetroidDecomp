namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The way a shot fired in one of the ten directions moves (#2529): its sense along each axis, or none along an axis it
/// does not move along. A diagonal shot moves along both; a straight shot along one.
/// </summary>
/// <param name="Horizontal">The way the shot moves along X, or null when it moves straight up or down.</param>
/// <param name="Vertical">The way the shot moves along Y, or null when it moves straight left or right.</param>
internal sealed partial record SamusProjectileDirectionAxes(HorizontalDirection? Horizontal, VerticalDirection? Vertical)
{
    /// <summary>Whether the shot moves along X: every direction but straight up or down.</summary>
    internal bool MovesHorizontally => Horizontal is not null;

    /// <summary>Whether the shot moves along Y: every direction but straight left or right.</summary>
    internal bool MovesVertically => Vertical is not null;

    /// <summary>Whether the shot moves diagonally, along both axes.</summary>
    internal bool IsDiagonal => MovesHorizontally && MovesVertically;

    /// <summary>Whether the shot moves straight up, facing either way.</summary>
    internal bool IsStraightUp => !MovesHorizontally && Vertical == VerticalDirection.Up;
}
