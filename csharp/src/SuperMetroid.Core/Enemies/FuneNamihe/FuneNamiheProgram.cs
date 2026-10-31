namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>
/// The Fune's and Namihe's idle and spitting animations as typed programs (#641), one per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="FuneNamiheAnimations"/>.
/// </summary>
internal enum FuneNamiheProgram
{
    /// <summary>The fune idle left animation.</summary>
    FuneIdleLeft,
    /// <summary>The fune active left animation.</summary>
    FuneActiveLeft,
    /// <summary>The fune idle right animation.</summary>
    FuneIdleRight,
    /// <summary>The fune active right animation.</summary>
    FuneActiveRight,
    /// <summary>The namihe idle left animation.</summary>
    NamiheIdleLeft,
    /// <summary>The namihe active left animation.</summary>
    NamiheActiveLeft,
    /// <summary>The namihe idle right animation.</summary>
    NamiheIdleRight,
    /// <summary>The namihe active right animation.</summary>
    NamiheActiveRight,
}
