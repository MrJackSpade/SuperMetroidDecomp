using static SuperMetroid.Core.DefinitionRun;
namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres escape shaft's rotation: the point it turns about, how far each step tilts the shaft and how long the shaft
/// holds it.
/// </summary>
public static class CeresShaftRotationDefinitions
{
    /// <summary>
    /// The room point the shaft's turning floor always rotates about, set both by the elevator door's setup, which shows
    /// the shaft level, and by the room's main behaviour as it turns.
    /// </summary>
    public static Position16<RoomPixels> RotationCenter { get; } = new(128, 1008);

    /// <summary>The steps of the shaft's full tilt either way from level (#1895).</summary>
    public const int FullTiltSteps = 34;

    /// <summary>
    /// The steps from level that tilt the shaft one radian: each step tilts it a further 256th of a radian (the shaft shows
    /// a tilt as the turn whose sine is its steps in 256ths).
    /// </summary>
    public const double StepsPerRadian = 256;

    /// <summary>The updates the shaft holds level, once the elevator door's destruction releases it, before its first turn.</summary>
    public const int LevelHoldFrames = 60;

    /// <summary>
    /// The updates the shaft holds each tilt, by its steps from level either way, from level through the full tilt
    /// (#1955): the update that turns the shaft to a tilt is the first it holds. The holds lengthen away from level, so
    /// the turn slows toward each end of its sweep, and the shaft rests at the full tilt before it turns back.
    /// </summary>
    public static IReadOnlyList<int> TiltHoldFrames { get; } =
    [
        .. Repeated(2, 5), .. Repeated(3, 4), .. Repeated(4, 4), .. Repeated(5, 3), 6, 6, 7, 7, 8, 8,
        9, 10, 11, 12, 13, 15, 17, 19, 21, 23, 25, 27, 61,
    ];
}
