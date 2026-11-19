namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The reward's jump (#2541): Samus's jump, fall and landing, until her landing asks for the shot.</summary>
/// <param name="jump">The jump's actors.</param>
internal sealed class EndingRewardJumpScene(EndingRewardJump jump) : EndingScene
{
    /// <summary>The jump's actors.</summary>
    internal EndingRewardJump Jump { get; } = Ensure.NotNull(jump);
}
