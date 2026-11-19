namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The brief white flash into the final logo (#2541): a white wash over the post-credits text and the landed Samus, full
/// white as it begins and a step fewer each update, fading to none, where it stays.
/// </summary>
/// <param name="jump">The landed jump's actors, which keep running.</param>
/// <param name="flash">The flash's white wash.</param>
internal sealed class EndingWhiteFlashScene(EndingRewardJump jump, WhiteWash flash) : EndingScene
{
    /// <summary>The landed jump's actors.</summary>
    internal EndingRewardJump Jump { get; } = Ensure.NotNull(jump);

    /// <summary>The flash's white wash.</summary>
    internal WhiteWash Flash { get; } = Ensure.NotNull(flash);
}
