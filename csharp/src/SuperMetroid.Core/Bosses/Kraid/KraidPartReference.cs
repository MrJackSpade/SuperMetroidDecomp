using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's reference to one of his parts (#1877, #2146): the enemy that joined as that part when it initialized. A part
/// that initializes again, as a respawned fingernail does, joins the same enemy again.
/// </summary>
/// <param name="name">The part's name, for the failures that name it.</param>
internal sealed class KraidPartReference(string name)
{
    /// <summary>The enemy that joined as the part, or none before it initialized.</summary>
    private PopulatedEnemyPlace? _joined;

    /// <summary>Joins <paramref name="place"/> as the part, as it initializes.</summary>
    /// <exception cref="InvalidDataException">Another enemy already initialized as the part.</exception>
    internal void Join(RoomEnemyOccupant occupant)
    {
        RoomEnemyPlace place = Ensure.NotNull(occupant).Place;
        if (_joined is { } held && !ReferenceEquals(held.Place, place))
            throw new InvalidDataException($"Kraid's {name} initialized as enemy {place.Ordinal}, but enemy {held.Place.Ordinal} already is.");
        _joined = occupant.History;
    }

    /// <summary>The enemy that joined as the part.</summary>
    /// <exception cref="InvalidOperationException">The part has not initialized.</exception>
    internal RoomEnemyPlace Place => Joined.Place;

    /// <summary>The part last spawned in the place, which a death of Kraid kills whether it holds the place or has left it.</summary>
    /// <exception cref="InvalidOperationException">The part has not initialized.</exception>
    internal RoomEnemyOccupant LatestOccupant => Joined.LatestOccupant;

    /// <summary>The history of the part's place, from the part's initialization on.</summary>
    /// <exception cref="InvalidOperationException">The part has not initialized.</exception>
    private PopulatedEnemyPlace Joined => _joined ?? throw new InvalidOperationException($"Kraid's {name} is reached before it initialized.");
}
