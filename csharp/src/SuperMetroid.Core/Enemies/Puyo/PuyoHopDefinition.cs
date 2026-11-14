namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>One hop's fixed mechanics (#1820).</summary>
/// <param name="JumpHeight">How high the hop rises, in pixels; the drop rises none.</param>
/// <param name="XSpeed">The horizontal speed, in 256ths of a pixel per frame, which also paces the launch's curve search.</param>
/// <param name="CurveStep">How far along the speed curve each airborne update moves; the drop, which falls at its own speed, takes no step.</param>
/// <param name="Movement">The airborne behaviour the hop runs.</param>
public readonly record struct PuyoHopDefinition(int JumpHeight, int XSpeed, QuadraticSpeedCurvePlace CurveStep, PuyoAirborneMovement Movement);
