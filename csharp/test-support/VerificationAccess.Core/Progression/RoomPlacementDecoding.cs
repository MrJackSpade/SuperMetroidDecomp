using SuperMetroid.Core.Progression;

namespace SuperMetroid.AssetExtraction.Progression;

/// <summary>
/// The doors, item locations and story events room data and native state number, for checks that name them by those
/// numbers or compare with native state (#1553). The game reads no such number.
/// </summary>
internal static class RoomPlacementDecoding
{
    /// <summary>Each story event by the number the cartridge gives it.</summary>
    private static readonly Dictionary<int, StoryEvent> StoryEvents = Enum.GetValues<StoryEvent>().ToDictionary(storyEvent => storyEvent.Number());

    /// <summary>The door room data numbers <paramref name="number"/>.</summary>
    /// <exception cref="InvalidDataException">No door has <paramref name="number"/>.</exception>
    internal static LockedDoor Door(int number) => RoomPlacementNumbers.TryDoor(number, out LockedDoor? door)
        ? door
        : throw new InvalidDataException($"Door number {number} names no retail door.");

    /// <summary>The item location room data numbers <paramref name="number"/>.</summary>
    /// <exception cref="InvalidDataException">No item location has <paramref name="number"/>.</exception>
    internal static ItemLocation Item(int number) => RoomPlacementNumbers.TryItem(number, out ItemLocation? location)
        ? location
        : throw new InvalidDataException($"Item number {number} names no retail item location.");

    /// <summary>The story event the cartridge numbers <paramref name="number"/>.</summary>
    /// <exception cref="InvalidDataException">No story event uses <paramref name="number"/>.</exception>
    internal static StoryEvent StoryEvent(int number) => StoryEvents.TryGetValue(number, out StoryEvent storyEvent)
        ? storyEvent
        : throw new InvalidDataException($"Event number {number} names no story event.");
}
