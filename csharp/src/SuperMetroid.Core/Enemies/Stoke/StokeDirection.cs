namespace SuperMetroid.Core.Enemies.Stoke;

/// <summary>
/// The direction word stored for Stoke. Stoke's turn routine deliberately
/// leaves this word unchanged until the newly selected animation executes
/// its resume-moving cue; the enum therefore describes the stored word, not merely
/// the sign of the current velocity.
/// </summary>
public enum StokeDirection
{
    /// <summary>Direction word zero: left-facing latch installed with the moving-left function by the left resume cue.</summary>
    Left = 0,
    /// <summary>Direction word one: right-facing latch installed with the moving-right function by the right resume cue.</summary>
    Right = 1,
}
