namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The escaping Etecoons' typed programs (#641), one per bank-$B3 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="EscapeEtecoonAnimations"/>.
/// </summary>
internal enum EscapeEtecoonProgram
{
    /// <summary><c>InstList_EtecoonEscape_RunningLeft_LowTide_0</c> at $B3:E556.</summary>
    RunningLeftLowTide,
    /// <summary>The loop at $B3:E55A.</summary>
    RunningLeftLowTideLoop,
    /// <summary><c>InstList_EtecoonEscape_RunningLeft_HighTide</c> at $B3:E56E.</summary>
    RunningLeftHighTide,
    /// <summary><c>InstList_EtecoonEscape_RunningRight_LowTide_0</c> at $B3:E582.</summary>
    RunningRightLowTide,
    /// <summary>The loop at $B3:E586.</summary>
    RunningRightLowTideLoop,
    /// <summary><c>InstList_EtecoonEscape_RunningRight_HighTide</c> at $B3:E59A.</summary>
    RunningRightHighTide,
    /// <summary><c>InstList_EtecoonEscape_RunningForEscape_0</c> at $B3:E5AE.</summary>
    RunningForEscape,
    /// <summary>The loop at $B3:E5B2.</summary>
    RunningForEscapeLoop,
    /// <summary><c>InstList_EtecoonEscape_Stationary</c> at $B3:E5C6.</summary>
    Stationary,
    /// <summary><c>InstList_EtecoonEscape_ExpressGratitudeThenEscape_0</c> at $B3:E5DA.</summary>
    ExpressGratitudeThenEscape,
    /// <summary>The loop at $B3:E5E0.</summary>
    ExpressGratitudeThenEscapeLoop,
}
