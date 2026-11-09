using SuperMetroid.Core;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.AssetExtraction.Progression;

/// <summary>
/// The single conversion between a <see cref="LockedDoor"/> or an <see cref="ItemLocation"/> and the number room data
/// gives it (#1553): the operand of its room-object placement and its bit in the saved opened-door or collected-item
/// table. The progression table codec, SpaceTime's overwrite, the legacy save translation and the checks that compare
/// with native state convert through it; nothing in the game knows these numbers.
/// </summary>
[EncodingOwner("room-data door and item location numbering")]
public static partial class RoomPlacementNumbers
{
    /// <summary>How many numbers each of the opened-door and collected-item tables holds, 0 through 511.</summary>
    public const int NumberCount = 512;

    /// <summary>The number room data gives <paramref name="door"/>.</summary>
    /// <exception cref="ArgumentException"><paramref name="door"/> is not a retail door.</exception>
    public static int Number(this LockedDoor door) => Index.DoorNumbers.TryGetValue(Ensure.NotNull(door), out int number)
        ? number
        : throw new ArgumentException($"{door} is not a retail door.", nameof(door));

    /// <summary>The number room data gives <paramref name="location"/>.</summary>
    /// <exception cref="ArgumentException"><paramref name="location"/> is not a retail item location.</exception>
    public static int Number(this ItemLocation location) => Index.ItemNumbers.TryGetValue(Ensure.NotNull(location), out int number)
        ? number
        : throw new ArgumentException($"{location} is not a retail item location.", nameof(location));

    /// <summary>The door room data numbers <paramref name="number"/>, when one has that number.</summary>
    /// <returns>False for a number of the table that no door has.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="number"/> is outside the table.</exception>
    public static bool TryDoor(int number, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out LockedDoor? door) =>
        Index.Doors.TryGetValue(Ensure.BetweenInclusive(0, NumberCount - 1, number), out door);

    /// <summary>The item location room data numbers <paramref name="number"/>, when one has that number.</summary>
    /// <returns>False for a number of the table that no item location has.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="number"/> is outside the table.</exception>
    public static bool TryItem(int number, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out ItemLocation? location) =>
        Index.Items.TryGetValue(Ensure.BetweenInclusive(0, NumberCount - 1, number), out location);

    /// <summary>The door room data numbers <paramref name="number"/>.</summary>
    /// <exception cref="InvalidDataException">No door has <paramref name="number"/>.</exception>
    public static LockedDoor Door(int number) => TryDoor(number, out LockedDoor? door)
        ? door
        : throw new InvalidDataException($"Door number {number} names no retail door.");

    /// <summary>The item location room data numbers <paramref name="number"/>.</summary>
    /// <exception cref="InvalidDataException">No item location has <paramref name="number"/>.</exception>
    public static ItemLocation Item(int number) => TryItem(number, out ItemLocation? location)
        ? location
        : throw new InvalidDataException($"Item number {number} names no retail item location.");

    /// <summary>The lookups built once from the generated numbering.</summary>
    private static class Index
    {
        /// <summary>Each door by its number.</summary>
        internal static readonly Dictionary<int, LockedDoor> Doors = NumberedDoors.ToDictionary(entry => entry.Number, entry => entry.Placement);

        /// <summary>Each door's number.</summary>
        internal static readonly Dictionary<LockedDoor, int> DoorNumbers = NumberedDoors.ToDictionary(entry => entry.Placement, entry => entry.Number);

        /// <summary>Each item location by its number.</summary>
        internal static readonly Dictionary<int, ItemLocation> Items = NumberedItems.ToDictionary(entry => entry.Number, entry => entry.Placement);

        /// <summary>Each item location's number.</summary>
        internal static readonly Dictionary<ItemLocation, int> ItemNumbers = NumberedItems.ToDictionary(entry => entry.Placement, entry => entry.Number);
    }
}
