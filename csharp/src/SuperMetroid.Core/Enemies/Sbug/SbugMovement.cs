namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>Chooses an Sbug's facing, its directional loop and the action its activation behavior starts.</summary>
internal static class SbugMovement
{
    /// <summary>The facing <paramref name="heading"/> shows: the facing whose eighth of the turn contains it.</summary>
    internal static CompassDirection FacingOf(RotationAngle heading) => SbugMovementDefinitions.Facings.At(heading);

    /// <summary>The directional loop of <paramref name="facing"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="facing"/> is not one of the eight facings.</exception>
    internal static SbugSegment FacingAnimation(CompassDirection facing) =>
        SbugMovementDefinitions.FacingAnimations[facing];

    /// <summary>The action <paramref name="behavior"/> starts.</summary>
    /// <exception cref="InvalidDataException"><paramref name="behavior"/> is outside the three selectors.</exception>
    internal static SbugAction ActivationAction(SbugActivationBehavior behavior) =>
        SbugMovementDefinitions.ActivationActions[behavior];
}
