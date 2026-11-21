namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>One hop's fixed mechanics (#1820).</summary>
/// <param name="JumpHeight">How high the hop rises, in pixels; the drop rises none.</param>
/// <param name="XPixels">The whole pixels the hop moves Samus-ward or away each update; the hop moves by whole pixels only.</param>
/// <param name="LaunchSearchStep">
/// How far along the speed curve each pass of the launch's search steps: past the curve's start, so the search moves on,
/// which the definition requires once as it is built.
/// </param>
/// <param name="CurveStep">How far along the speed curve each airborne update moves; the drop, which falls at its own speed, takes no step.</param>
/// <param name="Movement">The airborne behaviour the hop runs.</param>
/// <param name="DropSpeed">
/// How fast the Puyo drops when a wall or ceiling cuts this hop short, and for the drop itself how fast it falls: the drop
/// that follows a cut hop keeps falling at the cut hop's speed until it lands.
/// </param>
public readonly record struct PuyoHopDefinition(int JumpHeight, int XPixels, QuadraticSpeedCurvePlace LaunchSearchStep,
    QuadraticSpeedCurvePlace CurveStep, PuyoAirborneMovement Movement, SignedSixteenSixteen DropSpeed)
{
    /// <summary>How far along the speed curve each pass of the launch's search steps, past the curve's start.</summary>
    public QuadraticSpeedCurvePlace LaunchSearchStep { get; } = LaunchSearchStep.CompareTo(QuadraticSpeedCurvePlace.Start) > 0
        ? LaunchSearchStep
        : throw new ArgumentOutOfRangeException(nameof(LaunchSearchStep), LaunchSearchStep, "A Puyo hop's launch search steps past the curve's start.");
}
