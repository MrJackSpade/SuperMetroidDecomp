using System.Runtime.CompilerServices;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Desktop;

/// <summary>Migrates one drained retired field of a restored instance.</summary>
internal delegate void RetiredFieldMigration(object instance, string serializedName, object? value);

/// <summary>
/// Exact historical fields that no longer exist in the current layout. The reader drains
/// each one wherever a legacy capture contains it and validates the remaining layout through
/// the ordinary count-based migrations, as though the retired field had never existed.
/// </summary>
internal static class DebuggerRetiredFieldDefinitions
{
    // Legacy values drained from a restored instance, for migrations that run after the
    // owning graph is complete. Weak keys keep this from retaining restored state.
    private static readonly ConditionalWeakTable<object, Dictionary<string, object?>> LegacyValues = new();

    // Declared before the inventory, whose initializer reads it.
    private static readonly Type CartridgePaletteTransitionType =
        typeof(SuperMetroidRuntime).Assembly.GetType("SuperMetroid.Core.Frontend.CartridgePaletteTransition", throwOnError: true)!;

    // An explicit inventory, not permission to discard arbitrary unknown fields.
    private static readonly Dictionary<(Type DeclaringType, string Name), RetiredFieldMigration> Retired = new()
    {
        // The Ceres getaway now runs at room main, after Samus, so `$90:E119` installs
        // its handler directly. A capture taken between the old deferred request and its
        // promotion has no current equivalent: promotion also displaced Samus's movement
        // owners, which a field migration cannot reproduce.
        [(typeof(SamusCeresRidleyEjectionState), "<IsPending>k__BackingField")] = (_, _, value) =>
        {
            if (value is not bool pending)
                throw new InvalidDataException("Legacy Ceres Ridley ejection pending flag is not a Boolean.");
            if (pending)
                throw new InvalidDataException(
                    "Legacy snapshot was captured between Ceres Ridley's ejection request and its " +
                    "promotion; that deferred state has no current representation.");
        },
        // RoomMainASMVar1 ($07E1) is now one shared word. These private copies seed it once
        // the runtime graph is complete; see DebuggerStateFieldMigrations.
        [(typeof(SuperMetroidRuntime), "_ceresFallingDebrisTimer")] = Remember,
        [(typeof(SuperMetroidRuntime), "_escapeDiagonalFrames")] = Remember,
        [(typeof(CeresElevatorShaftRoomMainState), "<RotationIndex>k__BackingField")] = Remember,
        [(typeof(MaridiaElevatubeRoomMainState), "<PositionSubposition>k__BackingField")] = Remember,
        // PaletteChangeNumerator ($7E:C400) is now one shared counter. A legacy fade keeps
        // its saved progress in a counter of its own; Kraid's restarts from the shared one.
        [(CartridgePaletteTransitionType, "transitionNumber")] = (instance, _, value) =>
        {
            if (value is not int transitionNumber)
                throw new InvalidDataException("Legacy palette transition number is not an integer.");
            var counter = new GradualColorChangeCounter();
            typeof(GradualColorChangeCounter).GetProperty(nameof(GradualColorChangeCounter.Numerator))!
                .SetValue(counter, checked((ushort)transitionNumber));
            CartridgePaletteTransitionType.GetField("numerator",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(instance, counter);
            if (transitionNumber != 0)
                Console.Error.WriteLine("WARNING: Legacy palette fade was captured mid-transition; it resumes on its own counter rather than the shared PaletteChangeNumerator.");
        },
        // CameraDistanceIndex ($0941) is now one shared word on the enemy system. Kraid's
        // private copy seeds it; null meant an already-defeated room, which leaves it zero.
        [(typeof(KraidEnemyState), "<CameraDistanceIndex>k__BackingField")] = (instance, name, value) =>
            Remember(instance, name, value ?? (ushort)0),
        [(typeof(KraidEnemyState), "<RoomBackgroundFadeStep>k__BackingField")] = (_, _, value) =>
        {
            if (value is not ushort step)
                throw new InvalidDataException("Legacy Kraid background fade step is not a word.");
            if (step != 0)
                Console.Error.WriteLine("WARNING: Legacy Kraid background fade was captured mid-transition; it restarts from the shared PaletteChangeNumerator.");
        },
    };

    /// <summary>Returns the migration of a retired serialized field.</summary>
    internal static bool TryGetMigration(Type declaringType, string serializedName, out RetiredFieldMigration migrate) =>
        Retired.TryGetValue((declaringType, serializedName), out migrate!);

    /// <summary>Returns a retired word drained from <paramref name="instance"/>, if its capture had one.</summary>
    internal static bool TryGetLegacyWord(object instance, string serializedName, out ushort value)
    {
        value = 0;
        if (!LegacyValues.TryGetValue(instance, out Dictionary<string, object?>? values) ||
            !values.TryGetValue(serializedName, out object? stored))
            return false;
        value = stored as ushort? ?? throw new InvalidDataException(
            $"Legacy {instance.GetType().FullName}.{serializedName} is not a word.");
        return true;
    }

    private static void Remember(object instance, string serializedName, object? value) =>
        LegacyValues.GetOrCreateValue(instance)[serializedName] = value;
}
