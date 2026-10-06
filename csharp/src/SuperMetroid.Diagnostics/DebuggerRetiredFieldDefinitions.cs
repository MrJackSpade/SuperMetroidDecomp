using SuperMetroid.Core.Game;

namespace SuperMetroid.Desktop;

/// <summary>
/// Exact historical fields that no longer exist in the current layout. A legacy snapshot
/// carries every retired field of its type in addition to the current fields; each drained
/// value is checked by its migration, which fails when the value cannot be represented.
/// </summary>
internal static class DebuggerRetiredFieldDefinitions
{
    // An explicit inventory, not permission to discard arbitrary unknown fields.
    private static readonly Dictionary<(Type DeclaringType, string Name), Action<object?>> Retired = new()
    {
        // The Ceres getaway now runs at room main, after Samus, so `$90:E119` installs
        // its handler directly. A capture taken between the old deferred request and its
        // promotion has no current equivalent: promotion also displaced Samus's movement
        // owners, which a field migration cannot reproduce.
        [(typeof(SamusCeresRidleyEjectionState), "<IsPending>k__BackingField")] = value =>
        {
            if (value is not bool pending)
                throw new InvalidDataException("Legacy Ceres Ridley ejection pending flag is not a Boolean.");
            if (pending)
                throw new InvalidDataException(
                    "Legacy snapshot was captured between Ceres Ridley's ejection request and its " +
                    "promotion; that deferred state has no current representation.");
        },
    };

    /// <summary>Returns how many retired fields a legacy layout of <paramref name="type"/> carries.</summary>
    internal static int CountFor(Type type) =>
        Retired.Keys.Count(key => key.DeclaringType == type);

    /// <summary>Returns the migration of a retired serialized field.</summary>
    internal static bool TryGetMigration(Type declaringType, string serializedName, out Action<object?> migrate) =>
        Retired.TryGetValue((declaringType, serializedName), out migrate!);
}
