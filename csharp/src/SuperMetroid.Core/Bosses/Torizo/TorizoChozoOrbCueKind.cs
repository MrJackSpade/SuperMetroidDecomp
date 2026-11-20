namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>What a cue of a Chozo orb does that only the orb behaviour runs: leave the orb's drop.</summary>
internal enum TorizoChozoOrbCueKind
{
    /// <summary>Leaves the drop of a broken Chozo orb, rolled from the orb's chances (<see cref="TorizoChozoOrbDefinitions"/>).</summary>
    SpawnOrbDrops,
}
