using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The partners each part of one multipart species found when it was placed (#1542), held by the species for its
/// room. A part records its <typeparamref name="TLinks"/> as it initializes, from where the room lists its partners
/// (<see cref="RoomEnemyListing"/>), and every later reach to a partner reads them here; a part placed again, by a
/// respawn or a new occupant of its place, records them again. A partner is a place, so one that has died or been
/// deleted is still the one reached.
/// </summary>
/// <typeparam name="TLinks">The species' named partners of one part, or the one partner of a part that has one.</typeparam>
[RoomLifetime]
internal sealed class EnemyPartLinks<TLinks> where TLinks : class
{
    /// <summary>Each placed part's partners, keyed by the part's place.</summary>
    private readonly Dictionary<RoomEnemySlot, TLinks> _links = [];

    /// <summary>Records the partners <paramref name="part"/> found as it was placed, replacing any it found before.</summary>
    internal void Place(RoomEnemySlot part, TLinks links) => _links[Ensure.NotNull(part)] = Ensure.NotNull(links);

    /// <summary>The partners <paramref name="part"/> found when it was placed.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="part"/> has not been placed as this species this room.</exception>
    internal TLinks Of(RoomEnemySlot part) =>
        _links.TryGetValue(Ensure.NotNull(part), out TLinks? links)
            ? links
            : throw new InvalidOperationException(
                $"Enemy {part.SlotIndex} ({part.DefinitionId}) reached its partners before it was placed as a part that has them.");

    /// <summary>The partners <paramref name="part"/> found when it was placed, or null when it has not been placed this room.</summary>
    internal TLinks? OfOrNone(RoomEnemySlot part) => _links.GetValueOrDefault(Ensure.NotNull(part));
}
