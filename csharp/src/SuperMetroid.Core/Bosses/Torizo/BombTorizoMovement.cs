using static SuperMetroid.Core.Bosses.Torizo.BombTorizoMovementDefinitions;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Bomb Torizo's posture displacements and walking velocities, selected by step. The first half of each table's steps
/// is the left-facing movement; the second half mirrors it horizontally for the right facing.
/// </summary>
internal static class BombTorizoMovement
{
    /// <summary>The number of posture steps: the left-facing steps, then their mirrored right-facing copies.</summary>
    internal static int PostureStepCount => LeftPostureX.Length * 2;

    /// <summary>The number of walking steps: the left-facing steps, then their mirrored right-facing copies.</summary>
    internal static int WalkStepCount => LeftWalkVelocities.Length * 2;

    /// <summary>Returns the frame-relative posture displacement of a posture step.</summary>
    /// <exception cref="InvalidDataException"><paramref name="step"/> is not a posture step.</exception>
    internal static Offset16<RoomPixels> Posture(int step)
    {
        if ((uint)step >= (uint)PostureStepCount)
            throw new InvalidDataException($"Bomb Torizo posture step {step} is outside its {PostureStepCount} steps.");

        int horizontal = LeftPostureX[step % LeftPostureX.Length];
        return new Offset16<RoomPixels>(
            (short)(step < LeftPostureX.Length ? horizontal : -horizontal), PostureY[step % PostureY.Length]);
    }

    /// <summary>Returns the signed whole-pixel walking velocity of a walking step.</summary>
    /// <exception cref="InvalidDataException"><paramref name="step"/> is not a walking step.</exception>
    internal static int WalkVelocity(int step)
    {
        if ((uint)step >= (uint)WalkStepCount)
            throw new InvalidDataException($"Bomb Torizo walk step {step} is outside its {WalkStepCount} steps.");

        int velocity = LeftWalkVelocities[step % LeftWalkVelocities.Length];
        return step < LeftWalkVelocities.Length ? velocity : -velocity;
    }
}
