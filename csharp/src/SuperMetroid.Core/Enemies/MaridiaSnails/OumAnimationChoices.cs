using A = SuperMetroid.Core.Enemies.MaridiaSnails.OumSegment;

namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>Chooses Oum's animation by its facing and action (#1624, #1810, #2145).</summary>
internal static partial class OumAnimationChoices
{
    /// <summary>The idle animation facing left or right.</summary>
    internal static A Idle(HorizontalDirection facing) => facing == HorizontalDirection.Left ? A.FacingLeftIdle : A.FacingRightIdle;

    /// <summary>The attack facing left or right.</summary>
    internal static A Attacking(HorizontalDirection facing) => facing == HorizontalDirection.Left ? A.FacingLeftAttacking : A.FacingRightAttacking;

    /// <summary>The forward roll facing left or right, as a roll from idle or after an attack runs.</summary>
    internal static A RollingForwards(HorizontalDirection facing) => facing == HorizontalDirection.Left ? A.FacingLeftRollingForwards : A.FacingRightRollingForwards;

    /// <summary>The backward roll facing left or right, as a wall reverses a forward roll.</summary>
    internal static A RollingBackwards(HorizontalDirection facing) => facing == HorizontalDirection.Left ? A.FacingLeftRollingBackwards : A.FacingRightRollingBackwards;
}
