namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// How the Baby Metroid flies through Mother Brain's last fight (#2171): where it arrives, the curve and rush that carry it
/// into Mother Brain's head, the speeds of its approaches, where it latches and how it falls when it dies.
/// </summary>
internal static class BabyMetroidCutsceneFlightDefinitions
{
    /// <summary>The half-extent of the rectangle every route and staging target is tested with: four pixels each way.</summary>
    internal static readonly Extent16<RoomPixels> TargetReach = new(4, 4);

    /// <summary>The half-extent of the rectangle about its latch point on her brain that ends its latch onto her.</summary>
    internal static readonly Extent16<RoomPixels> BrainLatchReach = new(8, 8);

    /// <summary>How fast it pulls itself onto her brain to drain her, each update: two pixels an update.</summary>
    internal static readonly SignedEightEight BrainDrainAcceleration = SignedEightEight.FromPixels(2);

    /// <summary>How fast it pulls itself onto Samus to heal her, each update: a sixteenth of a pixel an update.</summary>
    internal static readonly SignedEightEight SamusLatchAcceleration = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(16);

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

    /// <summary>
    /// What a gentle approach divides the remaining distance by: sixteen, the pull of its approaches to Mother Brain, the
    /// ceiling, Samus and its route, and of the stare-down and the legs staging the final charge.
    /// </summary>
    internal const int GentleAccelerationDivisor = 16;

    /// <summary>How far left of Samus the Baby stares Mother Brain down from.</summary>
    internal const int StareDownOffsetX = -4;

    /// <summary>The height the Baby stares Mother Brain down from.</summary>
    internal const int StareDownY = 96;

    /// <summary>The point just past the screen's right edge the Baby flies to after staring Mother Brain down.</summary>
    internal static readonly Position16<RoomPixels> RightEdgePoint = new(272, 64);

    /// <summary>Where the Baby stages its final charge.</summary>
    internal static readonly Position16<RoomPixels> FinalChargeStart = new(305, 160);

    /// <summary>Where the final charge's start aims the Baby.</summary>
    internal static readonly Position16<RoomPixels> FinalChargeAim = new(290, 128);

    /// <summary>What the final charge's start divides the remaining distance by: six, a much firmer pull than sixteen.</summary>
    internal const int ChargeStartDivisor = 6;

    /// <summary>Where on Mother Brain's brain the final charge aims: 32 pixels above it.</summary>
    internal static readonly Offset16<RoomPixels> FinalChargeOffset = new(0, -32);

    /// <summary>What the final charge divides the remaining distance by: four, its firmest pull.</summary>
    internal const int FinalChargeDivisor = 4;

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
