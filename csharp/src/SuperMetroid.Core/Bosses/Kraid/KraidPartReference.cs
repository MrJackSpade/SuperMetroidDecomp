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
    private RoomEnemyPlace? _joined;

    /// <summary>Joins <paramref name="place"/> as the part, as it initializes.</summary>
    /// <exception cref="InvalidDataException">Another enemy already initialized as the part.</exception>
    internal void Join(RoomEnemyPlace place)
    {
        Ensure.NotNull(place);
        if (_joined is { } held && !ReferenceEquals(held, place))
            throw new InvalidDataException($"Kraid's {name} initialized as enemy {place.Ordinal}, but enemy {held.Ordinal} already is.");
        _joined = place;
    }

    /// <summary>The enemy that joined as the part.</summary>
    /// <exception cref="InvalidOperationException">The part has not initialized.</exception>
    internal RoomEnemyPlace Place => _joined ?? throw new InvalidOperationException($"Kraid's {name} is reached before it initialized.");

    /// <summary>Whether <paramref name="place"/> is the enemy that joined as the part.</summary>
    internal bool Holds(RoomEnemyPlace place) => _joined is not null && ReferenceEquals(_joined, place);
}
