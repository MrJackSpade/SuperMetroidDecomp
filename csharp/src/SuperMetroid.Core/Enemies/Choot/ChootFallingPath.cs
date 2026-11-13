namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>
/// The five Choot falling paths.
/// Its values are arbitrary.
/// </summary>
internal enum ChootFallingPath
{
    /// <summary>Normal falling motion.</summary>
    Normal = 1,
    /// <summary>Wide falling motion.</summary>
    Wide,
    /// <summary>Very wide falling motion.</summary>
    VeryWide,

    /// <summary>
    /// The normal path with ten extra copies of
    /// normal frame 27 at physical frames 28..37 and normal frame 62 at
    /// physical frames 73..82. Physical frame 93 ends the loop.
    /// All 188 physical frames match this expansion of the authored
    /// path; the loop Y distance is separate. The bounded
    /// mapping is applied by <see cref="ChootFallingPathDefinitions"/>.
    /// Investigation: #625 / #658.
    /// </summary>
    Slow,

    /// <summary>
    /// The normal path with thirty extra copies of
    /// normal frame 27 at physical frames 28..57 and normal frame 62 at
    /// physical frames 93..122. Physical frame 133 ends the loop.
    /// All 268 physical frames match this expansion of the authored
    /// path; the loop Y distance is separate. The bounded
    /// mapping is applied by <see cref="ChootFallingPathDefinitions"/>.
    /// Investigation: #625 / #659.
    /// </summary>
    VerySlow,
}
