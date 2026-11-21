using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// What the ending shows with its phase (#2541), each scene owning its own actors: the cast in its running order
/// (<see cref="EndingCastScene"/>), the staff credits (<see cref="EndingCreditsRollScene"/>), the reward's reveal
/// (<see cref="EndingRewardRevealScene"/>), gesture (<see cref="EndingRewardGestureScene"/>), jump
/// (<see cref="EndingRewardJumpScene"/>), shot (<see cref="EndingPostShotScene"/>) and white flash
/// (<see cref="EndingWhiteFlashScene"/>), and the final logo (<see cref="EndingLogoScene"/>) with its typed texts
/// (<see cref="EndingTypingScene"/>). The ending's state shows one at a time and changes it with its phase. Each scene
/// steps itself and draws its own actors and, when it has one, its own screen (#2571), so neither the state nor the
/// presentation switches over scene types.
/// </summary>
internal abstract partial class EndingScene
{
    /// <summary>Only the ending's scenes derive from it.</summary>
    private protected EndingScene()
    {
    }

    /// <summary>
    /// The ending's phase the scene runs (#2560): the scene carries it, so the ending's phase and the scene it shows cannot
    /// disagree.
    /// </summary>
    internal abstract EndingCreditsPhase Phase { get; }

    /// <summary>Runs one update of the scene's phase on the ending.</summary>
    /// <param name="ending">The ending showing the scene.</param>
    /// <returns>Whether the update goes on past the phase to the ending's colour effects and stars.</returns>
    /// <exception cref="InvalidOperationException">The ending runs a phase the scene does not run.</exception>
    internal abstract bool Step(EndingCreditsState ending);

    /// <summary>Places the scene's actors among the frame's sprites: the scene's own, or the cast each on its role's layer.</summary>
    /// <param name="ending">The ending showing the scene.</param>
    /// <param name="sprites">The frame's sprites.</param>
    /// <param name="shows">Whether the sprites of a layer are placed.</param>
    internal abstract void DrawActors(EndingCreditsState ending, ScreenSprites sprites, Func<EndingSpriteLayer, bool> shows);

    /// <summary>
    /// The scene's screen: its own for the credits, the logo, its typed texts and the reward's shot, the ending's shared
    /// layering for the cast, the reward's reveal, gesture and jump, and the white flash.
    /// </summary>
    /// <param name="ending">The ending showing the scene.</param>
    /// <param name="paint">The frame's paint.</param>
    internal abstract LayeredScene CaptureScreen(EndingCreditsState ending, FramePaint paint);
}
