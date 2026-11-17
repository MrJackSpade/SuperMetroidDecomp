namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>
/// The Fune's and Namihe's idle and spitting animations (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="FuneNamiheAnimations"/>.
/// </summary>
internal enum FuneNamiheSegment
{
    /// <summary>The Fune facing left with its mouth closed, held.</summary>
    FuneIdleLeft,
    /// <summary>
    /// The Fune facing left opening its mouth to spit a fireball left with its sound, then closing it again before it
    /// idles.
    /// </summary>
    FuneActiveLeft,
    /// <summary>The Fune facing right with its mouth closed, held.</summary>
    FuneIdleRight,
    /// <summary>
    /// The Fune facing right opening its mouth to spit a fireball right with its sound, then closing it again before it
    /// idles.
    /// </summary>
    FuneActiveRight,
    /// <summary>The Namihe facing left with its mouth closed, held.</summary>
    NamiheIdleLeft,
    /// <summary>
    /// The Namihe facing left opening its mouth wide to spit a fireball left with its sound, then closing it again
    /// before it idles.
    /// </summary>
    NamiheActiveLeft,
    /// <summary>The Namihe facing right with its mouth closed, held.</summary>
    NamiheIdleRight,
    /// <summary>
    /// The Namihe facing right opening its mouth wide to spit a fireball right with its sound, then closing it again
    /// before it idles.
    /// </summary>
    NamiheActiveRight,
}
