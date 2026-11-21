namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>What the ending's colour loads are (<see cref="EndingColourLoad"/>).</summary>
internal static class EndingColourLoadDefinitions
{
    /// <summary>The loads of a scene's whole screen of colours, which start that scene: the escape's, the explosion's, the credits' and the post-credits'.</summary>
    internal static readonly EndingColourLoad[] StartingScenes =
        [EndingColourLoad.Escape, EndingColourLoad.Explosion, EndingColourLoad.Credits, EndingColourLoad.PostCredits];
}
