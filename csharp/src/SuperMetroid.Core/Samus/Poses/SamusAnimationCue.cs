namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// The cue that closes a run of delays in a Samus animation: where the animation goes next, or which
/// transitional pose it publishes. Each kind is its own record; the animation acts on the kind.
/// </summary>
internal abstract partial record SamusAnimationCue : SamusPoseAnimationStep
{
    /// <summary>Allows only the cues of this namespace's records (#2553).</summary>
    private protected SamusAnimationCue()
    {
    }

    /// <summary>What <paramref name="cases"/> makes of the cue, through the case it is.</summary>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <typeparam name="TResult">What the cue is made into.</typeparam>
    /// <param name="cases">What each case of the cue is made into.</param>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract TResult Match<TContext, TResult>(SamusAnimationCueCases<TContext, TResult> cases, TContext context);

    /// <summary>
    /// Whether the animation can go on from the cue to the frame after it, the first frame of the next segment: a hold, the
    /// breathing's low-energy continuation and the drained fall's handover do. Every other cue moves the animation itself
    /// or ends it, and a frame after one of them is no step of the animation
    /// (<see cref="Compatibility.FramesBeyondAnimation{TAnimation, TFrame, TPlace, TStep}"/>).
    /// </summary>
    internal virtual bool ContinuesToNextSegment => false;

    /// <summary>The transitional poses the cue can publish; none for a cue that publishes no pose.</summary>
    internal virtual IReadOnlyList<SamusPoseId> PublishedPoses => [];
}

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
