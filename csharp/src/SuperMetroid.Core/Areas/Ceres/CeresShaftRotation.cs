using static SuperMetroid.Core.Areas.Ceres.CeresShaftRotationDefinitions;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Turns the Ceres escape shaft (<see cref="CeresShaftRotationDefinitions"/>): each tilt's hold and angle.
/// </summary>
public static class CeresShaftRotation
{
    /// <summary>The shaft's hold and tilt at <paramref name="tiltSteps"/> steps from level, clockwise positive.</summary>
    /// <exception cref="InvalidDataException"><paramref name="tiltSteps"/> lies past the shaft's full tilt.</exception>
    public static CeresShaftRotationDefinition Read(int tiltSteps)
    {
        if (Math.Abs(tiltSteps) > FullTiltSteps)
            throw new InvalidDataException($"Ceres shaft tilt {tiltSteps} lies past its full tilt of {FullTiltSteps} steps.");
        return new(TiltHoldFrames[Math.Abs(tiltSteps)], tiltSteps / StepsPerRadian);
    }
}
