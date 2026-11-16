namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// How the Baby Metroid flies through Mother Brain's last fight (#2171): where it arrives, the curve and rush that carry it
/// into Mother Brain's head, the speeds of its approaches, where it latches and how it falls when it dies.
/// </summary>
internal static class BabyMetroidCutsceneFlightDefinitions
{
    /// <summary>The whole room pixel the Baby arrives at and waits on before its dash: X 320, Y 96.</summary>
    internal static readonly Position16<RoomPixels> ArrivalPoint = new(320, 96);

    /// <summary>The heading the Baby's curve toward Mother Brain's head starts on.</summary>
    internal static readonly RotationAngle CurveStartHeading = RotationAngle.FromSteps(216);

    /// <summary>The heading the Baby's curve toward Mother Brain's head turns to.</summary>
    internal static readonly RotationAngle CurveTargetHeading = RotationAngle.FromSteps(176);

    /// <summary>How far the curve's heading turns clockwise each update: a step and a half.</summary>
    internal static readonly RotationAngle CurveTurn = RotationAngle.FromSteps(1, 128);

    /// <summary>The heading the Baby's last rush into Mother Brain's face turns to.</summary>
    internal static readonly RotationAngle FaceTargetHeading = RotationAngle.FromSteps(130);

    /// <summary>How far the rush into Mother Brain's face turns clockwise each update: six steps.</summary>
    internal static readonly RotationAngle FaceTurn = RotationAngle.FromSteps(6);

    /// <summary>The speed the Baby's curve starts and holds: ten pixels an update.</summary>
    internal static readonly SignedEightEight CurveSpeed = SignedEightEight.FromPixels(10);

    /// <summary>The speed the rush into Mother Brain's face builds to: fourteen pixels an update.</summary>
    internal static readonly SignedEightEight FaceSpeed = SignedEightEight.FromPixels(14);

    /// <summary>How much the curve's speed changes each update toward its target: an eighth of a pixel an update.</summary>
    internal static readonly SignedEightEight CurveSpeedStep = SignedEightEight.FromParts(0, 32);

    /// <summary>
    /// The extra speed a reversal adds while the Baby is off screen on its approaches to Mother Brain, the ceiling, Samus
    /// and the final charge: four pixels an update.
    /// </summary>
    internal static readonly SignedEightEight ApproachWrongWaySpeed = SignedEightEight.FromPixels(4);

    /// <summary>
    /// The extra speed a reversal adds while the Baby is off screen as it stares Mother Brain down and flies to the final
    /// charge's start: a sixteenth of a pixel an update.
    /// </summary>
    internal static readonly SignedEightEight StagingWrongWaySpeed = SignedEightEight.FromParts(0, 16);

    /// <summary>The fastest the Baby approaches a point, on each axis, in either direction: five pixels an update.</summary>
    internal static readonly SignedEightEight ApproachMaximumSpeed = SignedEightEight.FromPixels(5);

    /// <summary>Places the latched Baby 24 pixels above Mother Brain's brain anchor.</summary>
    internal static readonly Offset16<RoomPixels> BrainLatchOffset = new(0, -24);

    /// <summary>Places the latched Baby 20 pixels above Samus's center.</summary>
    internal static readonly Offset16<RoomPixels> SamusLatchOffset = new(0, -20);

    /// <summary>The drag the Baby's death fall takes from its sideways speed each update.</summary>
    internal static readonly SignedEightEight DeathFallDrag = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(32);

    /// <summary>The gravity the Baby's death fall adds to its vertical speed each update.</summary>
    internal static readonly SignedEightEight DeathFallGravity = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(2);
}
