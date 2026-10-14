using SuperMetroid.Core.Enemies;

/// <summary>
/// Typed entry to the native extended-hitbox walker for collision tables whose fixtures keep their query as an
/// argument row: slot, target position, target radius, shot selection, and the callback the walk reports. An
/// element of the wrong type is rejected.
/// </summary>
internal static class ExtendedHitboxWalker
{
    /// <summary>Walks the slot's extended hitboxes and stores the reported callback in the row's last element.</summary>
    internal static bool Find(RoomEnemySystem enemies, object?[] arguments)
    {
        bool found = enemies.Collision.TryFindExtendedHitboxCallback(
            arguments[0] as RoomEnemySlot ?? throw new ArgumentException("Element 0 must be the enemy slot.", nameof(arguments)),
            Value<Position16<RoomPixels>>(arguments, 1),
            Value<Extent16<RoomPixels>>(arguments, 2),
            Value<bool>(arguments, 3),
            out ushort callback);
        arguments[4] = callback;
        return found;
    }

    /// <summary>Reads a typed value from an argument row, treating null as the struct default and rejecting a mismatched type.</summary>
    private static T Value<T>(object?[] arguments, int index) where T : struct => arguments[index] switch
    {
        null => default,
        T value => value,
        object other => throw new ArgumentException($"Element {index} is {other.GetType()}, not {typeof(T)}.", nameof(arguments)),
    };
}
