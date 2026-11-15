namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// How far and which way Ridley's swoop heading turns each update toward its target heading (#1936). His heading grows
/// counterclockwise on screen: straight down, then toward the right.
/// </summary>
/// <param name="PerUpdate">How far the heading turns each update.</param>
/// <param name="Direction">
/// Which way it turns, and so on which side of the target heading a turn stops at it; a heading that does not turn stops
/// at its target as a counterclockwise one does.
/// </param>
internal readonly record struct RidleySwoopTurn(RotationAngle PerUpdate, RotationDirection Direction)
{
    /// <summary>A heading that holds still, settling straight onto its target the way a counterclockwise turn does.</summary>
    internal static RidleySwoopTurn None => new(RotationAngle.Zero, RotationDirection.Counterclockwise);

    /// <summary><paramref name="heading"/> turned one update's worth.</summary>
    internal RotationAngle Turn(RotationAngle heading) =>
        Direction == RotationDirection.Counterclockwise ? heading + PerUpdate : heading - PerUpdate;
}
