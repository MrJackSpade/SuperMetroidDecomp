using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Verification.Harness;

/// <summary>
/// Typed entry to the native extended-hitbox walker for collision tables whose fixtures keep their query as an
/// argument row: slot, target position, target radius, shot selection, and the reaction the walk reports. An
/// element of the wrong type is rejected.
/// </summary>
internal static class ExtendedHitboxWalker
{
    /// <summary>Walks the slot's extended hitboxes and stores the reported reaction in the row's last element.</summary>
    internal static bool Find(object?[] arguments)
    {
        bool found = EnemyCollision.TryFindFrameHitboxReaction(
            arguments[0] as RoomEnemySlot ?? throw new ArgumentException("Element 0 must be the enemy slot.", nameof(arguments)),
            Value<Position16<RoomPixels>>(arguments, 1),
            Value<Extent16<RoomPixels>>(arguments, 2),
            (Value<bool>(arguments, 3)) ? HitboxContactKind.Shot : HitboxContactKind.Touch,
            out EnemyReaction? reaction);
        arguments[4] = reaction;
        return found;
    }

    /// <summary>The reaction <see cref="Find"/> stored in the row's last element, or null when the walk selected none.</summary>
    internal static EnemyReaction? Reaction(object?[] arguments) => arguments[4] switch
    {
        null => null,
        EnemyReaction reaction => reaction,
        object other => throw new ArgumentException($"Element 4 is {other.GetType()}, not {typeof(EnemyReaction)}.", nameof(arguments)),
    };

    /// <summary>
    /// The reaction the native callback word a reference walk stored in the row's last element names in
    /// <paramref name="bank"/>'s code, decoded as a shot or touch word by the row's shot selection.
    /// </summary>
    internal static EnemyReaction? NativeReaction(int bank, object?[] arguments) =>
        EnemyReactionAccess.Of(bank, Value<ushort>(arguments, 4), Value<bool>(arguments, 3));

    /// <summary>Reads a typed value from an argument row, treating null as the struct default and rejecting a mismatched type.</summary>
    private static T Value<T>(object?[] arguments, int index) where T : struct => arguments[index] switch
    {
        null => default,
        T value => value,
        object other => throw new ArgumentException($"Element {index} is {other.GetType()}, not {typeof(T)}.", nameof(arguments)),
    };
}
