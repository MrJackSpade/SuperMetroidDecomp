namespace SuperMetroid.Core.Enemies;

/// <summary>
/// How a part of a multipart enemy finds its partners (#1542). A room's population lists every part of a multipart
/// enemy together, in that enemy's own order: a Dragon's wing right after its body, a Dachora's four echoes after
/// it, a Kzan's base after its top. Each part, when it is placed, looks up its partners here by where the population
/// lists them relative to itself and keeps them as references; no behaviour reaches another enemy by its place after
/// that. A reference names the place, not its current occupant, so a partner that has died or been deleted is still
/// the one reached, as the game expects.
/// </summary>
internal static class RoomEnemyListing
{
    /// <summary>The place the population lists <paramref name="distance"/> places after <paramref name="part"/>.</summary>
    /// <param name="enemies">The room's enemy places.</param>
    /// <param name="part">The part looking for its partner.</param>
    /// <param name="distance">How many places after it the partner is listed, one or more.</param>
    /// <exception cref="InvalidDataException">No place is listed that far after <paramref name="part"/>.</exception>
    internal static RoomEnemyPlace ListedAfter(this EnemyPlaces enemies, RoomEnemyPlace part, int distance) =>
        enemies.ListedAfterOrNone(part, distance) ?? throw new InvalidDataException(
            $"Enemy {part.Ordinal} ({part.DefinitionId}) has no place listed {distance} after it for its partner.");

    /// <summary>
    /// The place the population lists <paramref name="distance"/> places after <paramref name="part"/>, or null when
    /// <paramref name="part"/> is listed too late to have one.
    /// </summary>
    /// <param name="enemies">The room's enemy places.</param>
    /// <param name="part">The part looking for its partner.</param>
    /// <param name="distance">How many places after it the partner is listed, one or more.</param>
    internal static RoomEnemyPlace? ListedAfterOrNone(this EnemyPlaces enemies, RoomEnemyPlace part, int distance)
    {
        Ensure.NotNull(enemies);
        Ensure.NotNull(part);
        _ = Ensure.BetweenInclusive(1, EnemyPlaceOrder.Capacity - 1, distance);
        int place = part.Ordinal + distance;
        return place >= EnemyPlaceOrder.Capacity ? null : enemies.InUpdateOrder[place];
    }

    /// <summary>The <paramref name="count"/> places the population lists directly after <paramref name="part"/>, nearest first.</summary>
    /// <param name="enemies">The room's enemy places.</param>
    /// <param name="part">The part looking for its partners.</param>
    /// <param name="count">How many partners follow it, one or more.</param>
    /// <exception cref="InvalidDataException">Fewer than <paramref name="count"/> places are listed after <paramref name="part"/>.</exception>
    internal static RoomEnemyPlace[] ListedRunAfter(this EnemyPlaces enemies, RoomEnemyPlace part, int count)
    {
        _ = Ensure.BetweenInclusive(1, EnemyPlaceOrder.Capacity - 1, count);
        var partners = new RoomEnemyPlace[count];
        for (int partner = 0; partner < count; partner++)
            partners[partner] = enemies.ListedAfter(part, partner + 1);
        return partners;
    }

    /// <summary>The place the population lists <paramref name="distance"/> places before <paramref name="part"/>.</summary>
    /// <param name="enemies">The room's enemy places.</param>
    /// <param name="part">The part looking for its partner.</param>
    /// <param name="distance">How many places before it the partner is listed, one or more.</param>
    /// <exception cref="InvalidDataException">No place is listed that far before <paramref name="part"/>.</exception>
    internal static RoomEnemyPlace ListedBefore(this EnemyPlaces enemies, RoomEnemyPlace part, int distance) =>
        enemies.ListedBeforeOrNone(part, distance) ?? throw new InvalidDataException(
            $"Enemy {part.Ordinal} ({part.DefinitionId}) has no place listed {distance} before it for its partner.");

    /// <summary>
    /// The place the population lists <paramref name="distance"/> places before <paramref name="part"/>, or null when
    /// <paramref name="part"/> is listed too early to have one.
    /// </summary>
    /// <param name="enemies">The room's enemy places.</param>
    /// <param name="part">The part looking for its partner.</param>
    /// <param name="distance">How many places before it the partner is listed, one or more.</param>
    internal static RoomEnemyPlace? ListedBeforeOrNone(this EnemyPlaces enemies, RoomEnemyPlace part, int distance)
    {
        Ensure.NotNull(enemies);
        Ensure.NotNull(part);
        _ = Ensure.BetweenInclusive(1, EnemyPlaceOrder.Capacity - 1, distance);
        int place = part.Ordinal - distance;
        return place < 0 ? null : enemies.InUpdateOrder[place];
    }

    /// <summary>
    /// Whether <paramref name="place"/> lies among the places the room's population, or a spawn since, has filled
    /// (<see cref="EnemyPlaces.EnemyCount"/>).
    /// </summary>
    /// <param name="enemies">The room's enemy places.</param>
    /// <param name="place">The place in question.</param>
    internal static bool IsFilled(this EnemyPlaces enemies, RoomEnemyPlace place) =>
        Ensure.NotNull(place).Ordinal < Ensure.NotNull(enemies).EnemyCount;

    /// <summary>
    /// Whether room loading has initialized <paramref name="enemy"/> (<see cref="IRoomLoadProgress"/>). Loading
    /// initializes the room's population in the order it lists it, so its progress is measured by that order.
    /// </summary>
    /// <param name="progress">How far room loading has progressed.</param>
    /// <param name="enemy">The enemy whose initialization is asked about.</param>
    internal static bool HasInitialized(this IRoomLoadProgress progress, RoomEnemyPlace enemy) =>
        Ensure.NotNull(progress).HasInitializedEnemyPlace(Ensure.NotNull(enemy).Ordinal);
}
