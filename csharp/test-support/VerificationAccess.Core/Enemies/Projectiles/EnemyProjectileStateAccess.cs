using System.Reflection;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Reads the state a projectile's behaviour carries for checks (#2554): a family's behaviour holds its projectile's state
/// as its own member, which a check reads as the one member of the type it names.
/// </summary>
internal static class EnemyProjectileStateAccess
{
    /// <summary>The state a projectile's behaviour carries.</summary>
    extension(EnemyProjectile self)
    {
        /// <summary>The <typeparamref name="T"/> the projectile's behaviour carries.</summary>
        /// <exception cref="InvalidOperationException">The behaviour carries no <typeparamref name="T"/>, or more than one.</exception>
        internal T StateOf<T>() where T : class => BehaviourState<T>(self.Behavior);

        /// <summary>The <typeparamref name="T"/> the projectile's behaviour carries, or null when it carries none.</summary>
        internal T? StateOrNull<T>() where T : class => StatesOf<T>(self.Behavior).SingleOrDefault();

        /// <summary>The kind of motion the projectile moves by, the one its behaviour names, or null while it moves by none.</summary>
        internal Enum? MotionKind => self.Motion is { } motion ? StatesOfValue<Enum>(motion).Single() : null;
    }

    /// <summary>The one <typeparamref name="T"/> <paramref name="behaviour"/> holds.</summary>
    private static T BehaviourState<T>(object behaviour) where T : class
    {
        T[] states = [.. StatesOf<T>(behaviour)];
        return states.Length == 1 ? states[0] : throw new InvalidOperationException(
            $"{behaviour.GetType().Name} holds {states.Length} {typeof(T).Name} states; a check reads exactly one.");
    }

    /// <summary>Every <typeparamref name="T"/> value the fields of <paramref name="owner"/> and its base types hold.</summary>
    private static IEnumerable<T> StatesOfValue<T>(object owner)
    {
        for (Type? type = owner.GetType(); type is not null; type = type.BaseType)
        {
            foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                if (field.GetValue(owner) is T value)
                    yield return value;
            }
        }
    }

    /// <summary>Every <typeparamref name="T"/> the fields of <paramref name="behaviour"/> and its base types hold, once each.</summary>
    private static IEnumerable<T> StatesOf<T>(object behaviour) where T : class
    {
        var found = new HashSet<T>(ReferenceEqualityComparer.Instance);
        for (Type? type = behaviour.GetType(); type is not null; type = type.BaseType)
        {
            foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                if (field.GetValue(behaviour) is T state)
                    found.Add(state);
            }
        }
        return found;
    }
}
