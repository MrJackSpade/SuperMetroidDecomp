namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The opening's sprite colours its cross-fader moves as one (#1512), named for what each holds on screen. Which colours
/// each is, the import alone knows.
/// </summary>
public enum IntroSpriteFade
{
    /// <summary>The flashback Samus's beam colours.</summary>
    Samus,

    /// <summary>The Mother Brain scene's object colours, faded in with the room.</summary>
    NarrationPortrait,

    /// <summary>The Mother Brain scene's actors' colours, faded in with the room.</summary>
    NarrationObjects,

    /// <summary>The laboratory scientists' colours, faded in with the laboratory.</summary>
    DiscoveryScientists,
}
