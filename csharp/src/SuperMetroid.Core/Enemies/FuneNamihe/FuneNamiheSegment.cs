namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>
/// The Fune's and Namihe's idle and spitting animations as typed programs (#641), one per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="FuneNamiheAnimations"/>.
/// </summary>
internal enum FuneNamiheSegment
{
    /// <summary>The Fune idle left animation.</summary>
    FuneIdleLeft,
    /// <summary>The Fune active left animation.</summary>
    FuneActiveLeft,
    /// <summary>The Fune idle right animation.</summary>
    FuneIdleRight,
    /// <summary>The Fune active right animation.</summary>
    FuneActiveRight,
    /// <summary>The Namihe idle left animation.</summary>
    NamiheIdleLeft,
    /// <summary>The Namihe active left animation.</summary>
    NamiheActiveLeft,
    /// <summary>The Namihe idle right animation.</summary>
    NamiheIdleRight,
    /// <summary>The Namihe active right animation.</summary>
    NamiheActiveRight,
}
