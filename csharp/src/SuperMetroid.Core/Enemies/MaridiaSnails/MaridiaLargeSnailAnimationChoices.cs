using A = SuperMetroid.Core.Enemies.MaridiaSnails.MaridiaLargeSnailSegment;

namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>Chooses Oum's animation by its facing and action (#1624, #1810).</summary>
internal static class MaridiaLargeSnailAnimationChoices
{
    /// <summary>The idle animation facing left or right.</summary>
    internal static A Idle(bool facingLeft) => facingLeft ? A.FacingLeftIdle : A.FacingRightIdle;

    /// <summary>The attack facing left or right.</summary>
    internal static A Attacking(bool facingLeft) => facingLeft ? A.FacingLeftAttacking : A.FacingRightAttacking;

    /// <summary>The forward roll an idle snail starts, with <paramref name="idle"/>'s facing.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="idle"/> is not an idle animation.</exception>
    internal static A RollingFrom(A idle) => idle switch
    {
        A.FacingRightIdle => A.FacingRightRollingForwards,
        A.FacingLeftIdle => A.FacingLeftRollingForwards,
        A.FacingRightRollingForwards or A.FacingLeftRollingForwards or A.FacingRightAttacking or A.FacingLeftAttacking or
            A.FacingRightRollingBackwards or A.FacingLeftRollingBackwards or _ =>
            throw new InvalidOperationException($"Oum starts rolling only from idle, not {idle}."),
    };

    /// <summary>The forward roll a finished attack resumes, with <paramref name="attack"/>'s facing.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="attack"/> is not an attack.</exception>
    internal static A RollingAfter(A attack) => attack is A.FacingLeftAttacking or A.FacingRightAttacking
        ? RollingForwards(facingLeft: attack == A.FacingLeftAttacking)
        : throw new InvalidOperationException($"Oum resumes rolling only after an attack, not {attack}.");

    /// <summary>The forward roll facing left or right.</summary>
    private static A RollingForwards(bool facingLeft) => facingLeft ? A.FacingLeftRollingForwards : A.FacingRightRollingForwards;

    /// <summary>
    /// The roll in the other direction with <paramref name="roll"/>'s facing, as a roll that meets a wall reverses: a
    /// forward roll rolls backwards and a backward roll forwards.
    /// </summary>
    /// <exception cref="InvalidOperationException"><paramref name="roll"/> is not a roll.</exception>
    internal static A Reversed(A roll) => roll switch
    {
        A.FacingRightRollingForwards => A.FacingRightRollingBackwards,
        A.FacingLeftRollingForwards => A.FacingLeftRollingBackwards,
        A.FacingRightRollingBackwards => A.FacingRightRollingForwards,
        A.FacingLeftRollingBackwards => A.FacingLeftRollingForwards,
        A.FacingRightIdle or A.FacingLeftIdle or A.FacingRightAttacking or A.FacingLeftAttacking or _ =>
            throw new InvalidOperationException($"Only a rolling Oum reverses its roll, not one {roll}."),
    };
}
