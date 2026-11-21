namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>
/// A live eye beam (#2554): the eye body that cast it, with the state that body holds, whose position and aim the beam's
/// cone follows, and the beam's lifetime from its request on.
/// </summary>
/// <param name="body">The eye body that cast the beam.</param>
/// <param name="eye">The state the eye body holds.</param>
internal sealed class MorphBallEyeBeamCast(RoomEnemyPlace body, MorphBallEyeEnemyState eye)
{
    /// <summary>The eye body that cast the beam.</summary>
    internal RoomEnemyPlace Body { get; } = Ensure.NotNull(body);

    /// <summary>The state the eye body holds.</summary>
    internal MorphBallEyeEnemyState Eye { get; } = Ensure.NotNull(eye);

    /// <summary>Where the beam stands in its lifetime: pending its initialization from its request until the next beam step starts it.</summary>
    internal MorphBallEyeBeamStage Stage { get; set; } = MorphBallEyeBeamStage.PendingInitialization;

    /// <summary>The lifetime phase the beam's stage shows as.</summary>
    internal MorphBallEyeBeamPhase Phase => Stage.Phase;
}
