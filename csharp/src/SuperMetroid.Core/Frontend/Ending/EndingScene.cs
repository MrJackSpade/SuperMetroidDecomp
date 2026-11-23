using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// What the ending shows with its phase (#2541), each scene owning its own actors: the cast in its running order
/// (<see cref="EndingCastScene"/>), the staff credits (<see cref="EndingCreditsRollScene"/>), the reward's reveal
/// (<see cref="EndingRewardRevealScene"/>), gesture (<see cref="EndingRewardGestureScene"/>), jump
/// (<see cref="EndingRewardJumpScene"/>), shot (<see cref="EndingPostShotScene"/>) and white flash
/// (<see cref="EndingWhiteFlashScene"/>), and the final logo (<see cref="EndingLogoScene"/>) with its typed texts
/// (<see cref="EndingTypingScene"/>). The ending's state shows one at a time and changes it with its phase. Each scene
/// steps itself and draws its own actors (<see cref="EndingActorScene"/>) and, when it has one, its own screen (#2571), so neither the state nor the
/// presentation switches over scene types.
/// </summary>
internal abstract partial class EndingScene
{
    /// <summary>Only the ending's scenes, and its scenes with actors, derive from it.</summary>
    private protected EndingScene()
    {
    }

    /// <summary>The ending's phase while the scene shows (#2553): the scene's own, so no phase runs on a scene it is not.</summary>
    internal abstract EndingCreditsPhase Phase { get; }

    /// <summary>Runs one update of the scene's phase on the ending.</summary>
    /// <param name="ending">The ending showing the scene.</param>
    /// <returns>Whether the update goes on past the phase to the ending's colour effects and stars.</returns>
    internal abstract bool Step(EndingCreditsState ending);

    /// <summary>
    /// The scene's screen: its own for the credits, the logo, its typed texts and the reward's shot, the ending's shared
    /// layering for the cast, the reward's reveal, gesture and jump, and the white flash.
    /// </summary>
    /// <param name="ending">The ending showing the scene.</param>
    /// <param name="paint">The frame's paint.</param>
    internal abstract LayeredScene CaptureScreen(EndingCreditsState ending, FramePaint paint);
}
