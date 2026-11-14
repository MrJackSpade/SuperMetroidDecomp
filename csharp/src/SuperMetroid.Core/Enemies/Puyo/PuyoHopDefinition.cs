namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>One hop's fixed mechanics (#1820).</summary>
/// <param name="JumpHeight">How high the hop rises, in pixels; the drop rises none.</param>
/// <param name="XSpeed">The horizontal speed, in 256ths of a pixel per frame, which also paces the launch's curve search.</param>
/// <param name="CurveStep">How far along the speed curve each airborne update moves; the drop, which falls at its own speed, takes no step.</param>
/// <param name="Movement">The airborne behaviour the hop runs.</param>
/// <param name="DropSpeed">
/// How fast the Puyo drops when a wall or ceiling cuts this hop short, and for the drop itself how fast it falls: the drop
/// that follows a cut hop keeps falling at the cut hop's speed until it lands.
/// </param>
public readonly record struct PuyoHopDefinition(int JumpHeight, int XSpeed, QuadraticSpeedCurvePlace CurveStep, PuyoAirborneMovement Movement,
    SignedSixteenSixteen DropSpeed);
