namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// How far and which way Spore Spawn's sway phase turns each movement update (#2334): a whole step of the turn, two once
/// a hit low on his health quickens it, toward increasing angles (counterclockwise) or decreasing ones (clockwise). The
/// first damaging hit of each opening reverses it.
/// </summary>
/// <param name="PerUpdate">How far the phase turns each movement update.</param>
/// <param name="Direction">Which way the phase turns.</param>
internal readonly record struct SporeSpawnSwayStep(RotationAngle PerUpdate, RotationDirection Direction)
{
    /// <summary>The step before his descent or his fight's start sets one: no turn, which a quickening turns counterclockwise.</summary>
    internal static SporeSpawnSwayStep None => new(RotationAngle.Zero, RotationDirection.Counterclockwise);

    /// <summary><paramref name="phase"/> turned one movement update's worth.</summary>
    internal RotationAngle Turn(RotationAngle phase) =>
        Direction == RotationDirection.Counterclockwise ? phase + PerUpdate : phase - PerUpdate;

    /// <summary>The same step turning the other way.</summary>
    internal SporeSpawnSwayStep Reversed() =>
        this with { Direction = Direction == RotationDirection.Clockwise ? RotationDirection.Counterclockwise : RotationDirection.Clockwise };

    /// <summary>The step turning the same way by <paramref name="perUpdate"/>.</summary>
    internal SporeSpawnSwayStep By(RotationAngle perUpdate) => this with { PerUpdate = perUpdate };
}
