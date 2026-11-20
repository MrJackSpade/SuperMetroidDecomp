namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Finds an enemy's partner where the room lists it, once, as the enemy is built (#2554): the partner's entity, holding its
/// state, which the enemy then holds as a typed reference for its whole life.
/// </summary>
internal static class EnemyPartners
{
    /// <summary>The <typeparamref name="TEntity"/> holding <paramref name="place"/>, which the room lists as the partner.</summary>
    /// <param name="place">The place the room lists the partner in.</param>
    /// <typeparam name="TEntity">The partner's entity.</typeparam>
    /// <exception cref="InvalidDataException">The room lists another enemy, or none, where the partner belongs.</exception>
    internal static TEntity Listed<TEntity>(RoomEnemyPlace place) where TEntity : class, IEnemyEntity =>
        Ensure.NotNull(place).Entity is TEntity partner
            ? partner
            : throw new InvalidDataException(
                $"Enemy place {place.Ordinal} holds {place.DefinitionId}, where the room lists a {typeof(TEntity).Name} as a partner.");
}
