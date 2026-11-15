namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The Ceres escape shaft's rotation steps: how far each tilts the shaft and how long the shaft holds it.</summary>
public static class CeresShaftRotationDefinitions
{

    /// <summary>The steps of the shaft's full tilt either way from level (#1895).</summary>
    public const int FullTiltSteps = 34;

    /// <summary>The steps from level that tilt the shaft one radian: each step tilts it a further 256th of a radian.</summary>
    public const double StepsPerRadian = 256;

    /// <summary>
    /// The updates the shaft holds each tilt, by its steps from level either way, from level through the full tilt
    /// (#1955): the update that turns the shaft to a tilt is the first it holds. The holds lengthen away from level, so
    /// the turn slows toward each end of its sweep, and the shaft rests at the full tilt before it turns back.
    /// </summary>
    public static IReadOnlyList<int> TiltHoldFrames { get; } =
    [
        2, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 6, 6, 7, 7, 8, 8,
        9, 10, 11, 12, 13, 15, 17, 19, 21, 23, 25, 27, 61,
    ];
}
