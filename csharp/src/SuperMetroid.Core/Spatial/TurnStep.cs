namespace SuperMetroid.Core.Spatial;

/// <summary>
/// How far and which way an angle turns each update (#2470): a rotation angle's worth toward increasing angles
/// (counterclockwise) or decreasing ones (clockwise). Unlike an <see cref="AngularSpeed"/> it keeps its way round when it
/// turns no distance, as Ridley's held swoop heading settles onto its target the way a counterclockwise turn does and
/// Spore Spawn's still sway turns counterclockwise once a hit quickens it.
/// </summary>
/// <param name="PerUpdate">How far the angle turns each update.</param>
/// <param name="Direction">Which way it turns.</param>
public readonly partial record struct TurnStep(RotationAngle PerUpdate, RotationDirection Direction)
{
    /// <summary>No turn at all, the way round a counterclockwise turn goes.</summary>
    public static TurnStep None => new(RotationAngle.Zero, RotationDirection.Counterclockwise);

    /// <summary><paramref name="angle"/> turned one update's worth.</summary>
    public RotationAngle Turn(RotationAngle angle) =>
        Direction == RotationDirection.Counterclockwise ? angle + PerUpdate : angle - PerUpdate;

    /// <summary>The same step turning the other way round.</summary>
    public TurnStep Reversed() => this with { Direction = Direction.Reversed() };

    /// <summary>The step turning the same way by <paramref name="perUpdate"/>.</summary>
    public TurnStep By(RotationAngle perUpdate) => this with { PerUpdate = perUpdate };
}
