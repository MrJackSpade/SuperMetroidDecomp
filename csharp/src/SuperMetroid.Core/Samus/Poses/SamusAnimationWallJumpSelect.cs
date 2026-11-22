namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// Continues a wall jump with its ordinary, Space Jump or Screw Attack frames: a body fully submerged without the Gravity
/// Suit takes the ordinary frames, otherwise Screw Attack takes priority over Space Jump.
/// </summary>
/// <param name="OrdinaryFrame">The first frame of the ordinary wall-jump spin.</param>
/// <param name="SpaceJumpFrame">The first frame of the Space Jump wall-jump spin.</param>
/// <param name="ScrewAttackFrame">The first frame of the Screw Attack wall-jump spin.</param>
internal sealed partial record SamusAnimationWallJumpSelect(
    SamusAnimationFrame OrdinaryFrame,
    SamusAnimationFrame SpaceJumpFrame,
    SamusAnimationFrame ScrewAttackFrame) : SamusAnimationCue
{
    /// <inheritdoc/>
    internal override TResult Match<TContext, TResult>(SamusAnimationCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).WallJumpSelect(this, context);
}
