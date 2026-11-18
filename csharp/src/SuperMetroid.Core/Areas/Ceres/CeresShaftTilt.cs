using static SuperMetroid.Core.Areas.Ceres.CeresShaftRotationDefinitions;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// How far the Ceres elevator shaft's floor is turned from level (#1895, #2426): a whole number of steps, each a 256th of
/// a radian, clockwise positive, from <see cref="FullAnticlockwise"/> through <see cref="FullClockwise"/>. The shaft turns
/// one step at a time and holds each tilt for the updates its distance from level gives it.
/// </summary>
/// <remarks>
/// The step is a 256th of a radian, not a 256th of a turn (the unit of <see cref="RotationAngle"/>): the shaft's turn
/// shows each tilt as the turn whose sine is its steps in 256ths, so the full tilt of 34 steps leans the shaft about
/// seven and a half degrees.
/// </remarks>
public readonly record struct CeresShaftTilt
{
    /// <summary>Holds a tilt of <paramref name="stepsFromLevel"/>, within the shaft's full tilt either way.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="stepsFromLevel"/> lies past the shaft's full tilt.</exception>
    private CeresShaftTilt(int stepsFromLevel) =>
        StepsFromLevel = Ensure.BetweenInclusive(-FullTiltSteps, FullTiltSteps, stepsFromLevel);

    /// <summary>The level shaft, which the elevator door's setup leaves and the turn starts from.</summary>
    public static CeresShaftTilt Level { get; } = new(0);

    /// <summary>The shaft's full clockwise tilt, where its forward sweep turns back.</summary>
    public static CeresShaftTilt FullClockwise { get; } = new(FullTiltSteps);

    /// <summary>The shaft's full anticlockwise tilt, where its backward sweep turns forward again.</summary>
    public static CeresShaftTilt FullAnticlockwise { get; } = new(-FullTiltSteps);

    /// <summary>The steps from level, clockwise positive: -34 through 34.</summary>
    public int StepsFromLevel { get; }

    /// <summary>
    /// The steps from the full anticlockwise tilt, 0 through 68: the place in the shaft's turn the following room's main
    /// inherits (<see cref="RoomMainCarryover"/>).
    /// </summary>
    public int StepsFromFullAnticlockwise => StepsFromLevel + FullTiltSteps;

    /// <summary>How far the shaft's floor is turned clockwise on the screen, in radians.</summary>
    public double Radians => StepsFromLevel / StepsPerRadian;

    /// <summary>The updates the shaft holds this tilt, counting the update that turns it there.</summary>
    public int HoldUpdates => TiltHoldFrames[Math.Abs(StepsFromLevel)];

    /// <summary>The tilt one step further clockwise.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The tilt is the full clockwise tilt.</exception>
    public CeresShaftTilt OneStepClockwise => new(StepsFromLevel + 1);

    /// <summary>The tilt one step further anticlockwise.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The tilt is the full anticlockwise tilt.</exception>
    public CeresShaftTilt OneStepAnticlockwise => new(StepsFromLevel - 1);

    /// <inheritdoc/>
    public override string ToString() => $"{StepsFromLevel} steps from level";
}
