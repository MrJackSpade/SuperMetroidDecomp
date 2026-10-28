namespace SuperMetroid.Core.Enemies.Stoke;

/// <summary>
/// The direction word stored at Stoke WRAM <c>$0FB0,x</c>. Stoke's turn routine deliberately
/// leaves this word unchanged until the newly selected animation executes command
/// <c>$8990</c> or <c>$899D</c>; the enum therefore describes cartridge storage, not merely
/// the sign of the current velocity.
/// </summary>
public enum StokeDirection
{
    /// <summary>Native direction word zero: left-facing latch installed with the moving-left function by instruction $A2:8990.</summary>
    Left = 0,
    /// <summary>Native direction word one: right-facing latch installed with the moving-right function by instruction $A2:899D.</summary>
    Right = 1,
}
