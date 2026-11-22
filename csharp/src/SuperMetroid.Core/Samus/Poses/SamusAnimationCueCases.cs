namespace SuperMetroid.Core.Samus.Poses;

/// <summary>What each case of a <see cref="SamusAnimationCue"/> is made into (#2553).</summary>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
/// <typeparam name="TResult">What each case is made into.</typeparam>
/// <param name="Hold">Makes a <see cref="SamusAnimationHold"/> into a result.</param>
/// <param name="LoopUnlessLowEnergy">Makes a <see cref="SamusAnimationLoopUnlessLowEnergy"/> into a result.</param>
/// <param name="InstallDrainedFall">Makes a <see cref="SamusAnimationInstallDrainedFall"/> into a result.</param>
/// <param name="TurnTransition">Makes a <see cref="SamusAnimationTurnTransition"/> into a result.</param>
/// <param name="ItemAirborneTransition">Makes a <see cref="SamusAnimationItemAirborneTransition"/> into a result.</param>
/// <param name="WallJumpSelect">Makes a <see cref="SamusAnimationWallJumpSelect"/> into a result.</param>
/// <param name="VerticalSpeedTransition">Makes a <see cref="SamusAnimationVerticalSpeedTransition"/> into a result.</param>
/// <param name="ItemTransition">Makes a <see cref="SamusAnimationItemTransition"/> into a result.</param>
/// <param name="Transition">Makes a <see cref="SamusAnimationTransition"/> into a result.</param>
/// <param name="Repeat">Makes a <see cref="SamusAnimationRepeat"/> into a result.</param>
/// <param name="Restart">Makes a <see cref="SamusAnimationRestart"/> into a result.</param>
internal sealed partial record SamusAnimationCueCases<TContext, TResult>(Func<SamusAnimationHold, TContext, TResult> Hold, Func<SamusAnimationLoopUnlessLowEnergy, TContext, TResult> LoopUnlessLowEnergy, Func<SamusAnimationInstallDrainedFall, TContext, TResult> InstallDrainedFall, Func<SamusAnimationTurnTransition, TContext, TResult> TurnTransition, Func<SamusAnimationItemAirborneTransition, TContext, TResult> ItemAirborneTransition, Func<SamusAnimationWallJumpSelect, TContext, TResult> WallJumpSelect, Func<SamusAnimationVerticalSpeedTransition, TContext, TResult> VerticalSpeedTransition, Func<SamusAnimationItemTransition, TContext, TResult> ItemTransition, Func<SamusAnimationTransition, TContext, TResult> Transition, Func<SamusAnimationRepeat, TContext, TResult> Repeat, Func<SamusAnimationRestart, TContext, TResult> Restart);
