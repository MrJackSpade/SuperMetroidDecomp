namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>
/// Fune and Namihe's fireball programs (#641), one per animation a spawn, branch or family step enters, in authored order.
/// Their steps are <see cref="FuneNamiheFireballAnimations"/>.
/// </summary>
internal enum FuneNamiheFireballProgram
{
    /// <summary>The left animation.</summary>
    Left,
    /// <summary>The right animation.</summary>
    Right,
}
