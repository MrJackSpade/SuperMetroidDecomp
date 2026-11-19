namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// What the ending shows with its phase (#2541), each scene owning its own actors: the cast in its running order
/// (<see cref="EndingCastScene"/>), the staff credits (<see cref="EndingCreditsRollScene"/>), the reward's reveal
/// (<see cref="EndingRewardRevealScene"/>), gesture (<see cref="EndingRewardGestureScene"/>), jump
/// (<see cref="EndingRewardJumpScene"/>), shot (<see cref="EndingPostShotScene"/>) and white flash
/// (<see cref="EndingWhiteFlashScene"/>), and the final logo (<see cref="EndingLogoScene"/>) with its typed texts
/// (<see cref="EndingTypingScene"/>). The ending's state shows one at a time and changes it with its phase.
/// </summary>
internal abstract class EndingScene
{
    /// <summary>Only the ending's scenes derive from it.</summary>
    private protected EndingScene()
    {
    }
}
