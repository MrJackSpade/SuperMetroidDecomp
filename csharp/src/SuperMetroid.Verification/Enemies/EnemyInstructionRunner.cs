using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Verification.Enemies;

/// <summary>
/// Typed entry to the enemy instruction interpreter for fixtures that keep their call as an argument row:
/// slot, Samus, level, camera and controller input. A null value-type element is its default, as it was when
/// these rows were passed to <see cref="System.Reflection.MethodBase.Invoke(object, object[])"/>; an element
/// of the wrong type is rejected.
/// </summary>
internal static class EnemyInstructionRunner
{
    /// <summary>Processes the slot's instruction list for one frame.</summary>
    internal static void Process(RoomEnemySystem enemies, object?[] arguments) =>
        enemies.Instructions.ProcessInstructions(
            arguments[0] as RoomEnemySlot ?? throw new ArgumentException("Element 0 must be the enemy slot.", nameof(arguments)),
            Optional<SamusState>(arguments, 1),
            Optional<RoomLevelData>(arguments, 2),
            Value<Position16<RoomPixels>>(arguments, 3),
            Value<ushort>(arguments, 4));

    /// <summary>Accepts a nullable fixture reference at an argument index and reports mismatched runtime types.</summary>
    private static T? Optional<T>(object?[] arguments, int index) where T : class => arguments[index] switch
    {
        null => null,
        T value => value,
        object other => throw new ArgumentException($"Element {index} is {other.GetType()}, not {typeof(T)}.", nameof(arguments)),
    };

    /// <summary>Restores reflection's default-value behavior for a null fixture value type while rejecting wrong types.</summary>
    private static T Value<T>(object?[] arguments, int index) where T : struct => arguments[index] switch
    {
        null => default,
        T value => value,
        object other => throw new ArgumentException($"Element {index} is {other.GetType()}, not {typeof(T)}.", nameof(arguments)),
    };
}
