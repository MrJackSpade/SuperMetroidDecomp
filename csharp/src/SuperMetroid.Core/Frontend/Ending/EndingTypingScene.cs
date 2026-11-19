namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The final logo while one of the post-credits texts types beneath it (#2541): the item percentage, then the final
/// message, which the ending stays on.
/// </summary>
/// <param name="logo">The logo's actor.</param>
/// <param name="typing">The text that types now.</param>
internal sealed class EndingTypingScene(EndingLogo logo, EndingTypewriter typing) : EndingScene
{
    /// <summary>The logo's actor.</summary>
    internal EndingLogo Logo { get; } = Ensure.NotNull(logo);

    /// <summary>The text that types now.</summary>
    internal EndingTypewriter Typing { get; } = Ensure.NotNull(typing);
}
