namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>What a cue of Golden Torizo's egg does that only the egg behaviour runs: hatch, or start the hatchling's charge.</summary>
internal enum GoldenTorizoEggCueKind
{
    /// <summary>Continues in the egg's facing hatched animation.</summary>
    Hatch,
    /// <summary>Starts a hatched egg charging toward a wall.</summary>
    StartEggCharge,
}
