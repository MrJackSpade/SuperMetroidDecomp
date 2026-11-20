namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>What a cue of the Torizos' drool and statue fragments does that only the behaviour starting their falls runs: start a fall.</summary>
internal enum TorizoFallCueKind
{
    /// <summary>Starts Bomb Torizo's drool falling.</summary>
    StartDroolFall,
    /// <summary>Starts a statue fragment falling.</summary>
    StartFragmentFall,
}
