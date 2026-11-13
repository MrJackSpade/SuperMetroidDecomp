namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>Chooses an Sbug's facing, its directional loop and the function its activation behavior starts.</summary>
internal static class SbugMovement
{
    /// <summary>The facing <paramref name="heading"/> shows: the facing whose eighth of the turn contains it.</summary>
    internal static SbugFacing FacingOf(RotationAngle heading) =>
        SbugMovementDefinitions.FacingsCounterClockwise.At(
            heading.TurnedBySteps(-SbugDefinitions.FirstFacingStart).Sector(RotationAngle.StepsPerTurn / SbugDefinitions.StepsPerFacing));

    /// <summary>The facing a reversal of the custom heading turns <paramref name="facing"/> to.</summary>
    /// <exception cref="InvalidDataException"><paramref name="facing"/> is not one of the eight facings.</exception>
    internal static SbugFacing ReversedFacing(SbugFacing facing) =>
        SbugMovementDefinitions.ReversedFacings.TryGetValue(facing, out SbugFacing reversed)
            ? reversed
            : throw new InvalidDataException($"Sbug facing {facing} is not one of the eight facings.");

    /// <summary>The directional loop of <paramref name="facing"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="facing"/> is not one of the eight facings.</exception>
    internal static SbugSegment FacingAnimation(SbugFacing facing) =>
        SbugMovementDefinitions.FacingAnimations.TryGetValue(facing, out SbugSegment program)
            ? program
            : throw new InvalidDataException($"Sbug facing {facing} is not one of the eight facings.");

    /// <summary>The function <paramref name="behavior"/> starts.</summary>
    /// <exception cref="InvalidDataException"><paramref name="behavior"/> is outside the seven selectors.</exception>
    internal static SbugEnemyFunction ActivationFunction(SbugActivationBehavior behavior) =>
        SbugMovementDefinitions.ActivationFunctions.TryGetValue(behavior, out SbugEnemyFunction function)
            ? function
            : throw new InvalidDataException(
                $"Sbug activation behavior {behavior} is not one of the seven activation behaviours.");
}
