namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The opening's background colours its cross-fader moves as one (#1512), named for what each holds on screen. Which
/// colours each is, the import alone knows.
/// </summary>
public enum IntroBackgroundFade
{
    /// <summary>The narration's colours with the first of the next set, faded out as a scene replaces the narration.</summary>
    GameplayBackground,

    /// <summary>The narration's colours, cleared and faded back in as the narration returns from a scene.</summary>
    ClearedBackground,

    /// <summary>The narration portrait's colours, faded out as a scene replaces the narration and back in as it returns.</summary>
    Environment,

    /// <summary>The Mother Brain room's lit colours, faded in with the room.</summary>
    NarrationText,

    /// <summary>The Mother Brain room's background colours, faded in with the room.</summary>
    NarrationBackground,

    /// <summary>The laboratory's background colours, faded in with the laboratory.</summary>
    DiscoveryBackground,
}
