namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>
/// Fune and Namihe's fireball animations (#641): one segment per animation a spawn, branch or family step enters, in authored order.
/// Their steps are <see cref="FuneNamiheFireballAnimations"/>.
/// </summary>
internal enum FuneNamiheFireballSegment
{
    /// <summary>The left animation.</summary>
    Left,
    /// <summary>The right animation.</summary>
    Right,
}
