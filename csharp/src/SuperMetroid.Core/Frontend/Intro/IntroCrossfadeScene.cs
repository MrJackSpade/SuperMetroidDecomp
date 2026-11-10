namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// What the opening's crossfades fade between (#1512, #1597): the narration and each flashback scene. Each fades as one,
/// at its own level, and no two hold the same colours. Which colours each is, the import alone knows.
/// </summary>
public enum IntroCrossfadeScene
{
    /// <summary>The narration pages: their text, backdrop and portrait, with the flashback Samus's beam colours.</summary>
    Narration,

    /// <summary>The Mother Brain and SR388 flashback rooms, their objects and their actors.</summary>
    Flashback,

    /// <summary>The laboratory and its scientists.</summary>
    Laboratory,
}
