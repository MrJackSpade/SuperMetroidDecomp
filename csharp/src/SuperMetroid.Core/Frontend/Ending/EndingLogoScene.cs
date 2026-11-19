namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The final logo (#2541): its actor, from the end of the white flash on, while it lands and fades in and while the
/// item-percentage text moves down below it.
/// </summary>
/// <param name="logo">The logo's actor.</param>
internal sealed class EndingLogoScene(EndingLogo logo) : EndingScene
{
    /// <summary>The logo's actor.</summary>
    internal EndingLogo Logo { get; } = Ensure.NotNull(logo);
}
