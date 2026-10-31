namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What an actor that takes over a slot inherits from the slot's previous occupant of its kind (#1476). In the original
/// game the values an actor's setup does not write stay as the slot's previous occupant left them, so a new occupant
/// starts those values where the last one stopped: a fresh Beetom touched before it first moves attaches facing the way
/// the previous Beetom in its slot last faced.
/// Each occupant keeps its own state; this service only remembers, per slot, the occupant a successor inherits from.
/// </summary>
/// <remarks>
/// Consulted once per occupant, when its setup runs: <see cref="Inherit"/> hands the setup the slot's previous occupant
/// (none for the slot's first occupant since the service was created) and records the new one for the next. The
/// service's lifetime is the lifetime of the slots it covers; a new set of slots starts with no previous occupants.
/// Which values an occupant inherits, and the evidence for each, is documented on the occupant's own state where it
/// copies them. Evidence: the original setups write only some of an actor's per-slot values, and nothing clears the rest
/// between occupants.
/// </remarks>
/// <typeparam name="TOccupant">The state of one occupant of the kind that inherits.</typeparam>
[CompatibilityService("an actor that takes over a slot starts the values its setup leaves alone from where the slot's previous occupant of its kind left them")]
public sealed class SlotInheritance<TOccupant>
    where TOccupant : class
{
    /// <summary>Each slot's latest occupant, or null while the slot has had none.</summary>
    private readonly TOccupant?[] _occupants;

    /// <summary>Starts a set of slots that have had no occupants.</summary>
    /// <param name="slotCount">The number of slots, at least one.</param>
    public SlotInheritance(int slotCount) =>
        _occupants = new TOccupant?[Ensure.BetweenInclusive(1, int.MaxValue, slotCount)];

    /// <summary>
    /// Sets up the new occupant of a slot from the slot's previous occupant, and records it as the occupant the slot's
    /// next occupant inherits from.
    /// </summary>
    /// <param name="slot">The zero-based slot the new occupant takes over.</param>
    /// <param name="occupy">Builds the new occupant from the previous one, or from none for the slot's first occupant.</param>
    /// <returns>The new occupant.</returns>
    public TOccupant Inherit(int slot, Func<TOccupant?, TOccupant> occupy)
    {
        ArgumentNullException.ThrowIfNull(occupy);
        TOccupant occupant = occupy(_occupants[slot]);
        _occupants[slot] = occupant;
        return occupant;
    }
}
