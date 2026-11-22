namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What an actor that takes over a place inherits from the place's previous occupant of its kind (#1476). In the
/// game the values an actor's setup does not write stay as the place's previous occupant left them, so a new occupant
/// starts those values where the last one stopped: a fresh Beetom touched before it first moves attaches facing the way
/// the previous Beetom in its place last faced.
/// Each occupant keeps its own state; this service only remembers, per place, the occupant a successor inherits from.
/// </summary>
/// <remarks>
/// Consulted once per occupant, when its setup runs: <see cref="Inherit"/> hands the setup the place's previous occupant
/// (none for the place's first occupant since the service was created) and records the new one for the next. The
/// service's lifetime is the lifetime of the places it covers; a new set of places starts with no previous occupants.
/// Which values an occupant inherits, and the evidence for each, is documented on the occupant's own state where it
/// copies them. Evidence: the original setups write only some of an actor's per-place values, and nothing clears the rest
/// between occupants.
/// </remarks>
/// <typeparam name="TOccupant">The state of one occupant of the kind that inherits.</typeparam>
public sealed partial class PlaceInheritance<TOccupant>
    where TOccupant : class
{
    /// <summary>Each place's latest occupant, or null while the place has had none.</summary>
    private readonly TOccupant?[] _occupants;

    /// <summary>Starts a set of places that have had no occupants.</summary>
    /// <param name="placeCount">The number of places, at least one.</param>
    public PlaceInheritance(int placeCount) =>
        _occupants = new TOccupant?[Ensure.BetweenInclusive(1, int.MaxValue, placeCount)];

    /// <summary>
    /// Sets up the new occupant of a place from the place's previous occupant, and records it as the occupant the place's
    /// next occupant inherits from.
    /// </summary>
    /// <param name="place">The zero-based place the new occupant takes over.</param>
    /// <param name="occupy">Builds the new occupant from the previous one, or from none for the place's first occupant.</param>
    /// <returns>The new occupant.</returns>
    public TOccupant Inherit(int place, Func<TOccupant?, TOccupant> occupy)
    {
        TOccupant occupant = Ensure.NotNull(occupy)(_occupants[place]);
        _occupants[place] = occupant;
        return occupant;
    }
}
