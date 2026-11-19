namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The staff credits (#2541): their scroll, which the credits' setup starts.</summary>
/// <param name="scroll">The staff credits' scroll.</param>
internal sealed class EndingCreditsRollScene(CreditsScroll scroll) : EndingScene
{
    /// <summary>The staff credits' scroll.</summary>
    internal CreditsScroll Scroll { get; } = Ensure.NotNull(scroll);
}
