namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>Chooses an Sbug's directional loop and the function its activation behavior starts.</summary>
internal static class SbugMovement
{
    /// <summary>The directional loop of <paramref name="facing"/>, counter-clockwise from right.</summary>
    /// <exception cref="InvalidDataException"><paramref name="facing"/> is outside the eight selectors.</exception>
    internal static SbugProgram FacingProgram(int facing) =>
        (uint)facing < (uint)SbugMovementDefinitions.FacingPrograms.Length
            ? SbugMovementDefinitions.FacingPrograms[facing]
            : throw new InvalidDataException(
                $"Sbug facing {facing} is outside the eight native selectors.");

    /// <summary>The function <paramref name="behavior"/> starts.</summary>
    /// <exception cref="InvalidDataException"><paramref name="behavior"/> is outside the seven selectors.</exception>
    internal static SbugEnemyFunction ActivationFunction(SbugActivationBehavior behavior) =>
        SbugMovementDefinitions.ActivationFunctions.TryGetValue(behavior, out SbugEnemyFunction function)
            ? function
            : throw new InvalidDataException(
                $"Sbug activation behavior {behavior} is outside the seven native selectors.");
}
