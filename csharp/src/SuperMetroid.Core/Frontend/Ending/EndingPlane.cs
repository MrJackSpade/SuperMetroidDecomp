namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The ending's one-screen background planes, by what each shows over the ending's course (#1404).</summary>
internal enum EndingPlane
{
    /// <summary>The credits roll, and later the result and copyright panels.</summary>
    Credits,
    /// <summary>The waiting backdrop, and later the final text's upper screen.</summary>
    Text,
    /// <summary>The final text's lower screen.</summary>
    TextLower,
    /// <summary>The Super Metroid logo.</summary>
    Logo,
    /// <summary>The wavy Samus backdrop.</summary>
    WavySamus,
}
