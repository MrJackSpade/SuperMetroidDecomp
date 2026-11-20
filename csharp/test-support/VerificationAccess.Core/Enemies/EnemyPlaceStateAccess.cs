using System.Reflection;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Reads the state a place's enemy holds for checks (#2554): each species' entity owns its typed state as its own member,
/// which a check reads as the one member of the type it names.
/// </summary>
internal static class EnemyPlaceStateAccess
{
    /// <summary>The state a place's enemy holds.</summary>
    extension(RoomEnemyPlace self)
    {
        /// <summary>The <typeparamref name="T"/> the place's enemy holds, or null when the place is empty or its enemy holds none.</summary>
        internal T? SpeciesState<T>() where T : class => self.Entity is { } entity ? StateOf<T>(entity) : null;

        /// <summary>The <typeparamref name="T"/> the place's enemy holds.</summary>
        /// <exception cref="InvalidOperationException">The place is empty or its enemy holds no <typeparamref name="T"/>.</exception>
        internal T RequireSpeciesState<T>() where T : class => self.SpeciesState<T>()
            ?? throw new InvalidOperationException($"Place {self.Ordinal}'s enemy holds no {typeof(T).Name}.");

        /// <summary>
        /// The <typeparamref name="T"/> another enemy finds in the place: the state of the enemy holding it or left there by a
        /// deletion, or null once a kill cleared it or no enemy has held it.
        /// </summary>
        internal T? FoundState<T>() where T : class => self.Found switch
        {
            EnemyPlaceFinding.Enemy found => StateOf<T>(found.Entity),
            EnemyPlaceFinding.Killed killed => killed.Remains.ShaktoolSegment as T,
            _ => null,
        };

        /// <summary>
        /// The <typeparamref name="T"/> another enemy finds in the place, or the record a kill cleared there when that is a
        /// <typeparamref name="T"/>.
        /// </summary>
        internal T? FoundOrClearedState<T>() where T : class => self.FoundState<T>();
    }

    /// <summary>The one <typeparamref name="T"/> the fields of <paramref name="entity"/> and its base types hold, or null.</summary>
    /// <exception cref="InvalidOperationException">The entity holds more than one.</exception>
    private static T? StateOf<T>(object entity) where T : class
    {
        var found = new HashSet<T>(ReferenceEqualityComparer.Instance);
        for (Type? type = entity.GetType(); type is not null; type = type.BaseType)
        {
            foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                if (field.GetValue(entity) is T state)
                    found.Add(state);
            }
        }
        return found.Count switch
        {
            0 => null,
            1 => found.First(),
            _ => throw new InvalidOperationException($"{entity.GetType().Name} holds {found.Count} {typeof(T).Name} states; a check reads exactly one."),
        };
    }
}
