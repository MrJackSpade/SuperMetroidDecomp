using System.Collections.Frozen;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification's native view of the retail scroll programs: where each is stored in bank $8F and its native encoding.</summary>
internal static class RoomObjectScrollProgramDefinitionsAccess
{
    /// <summary>The room whose state populations place each retail scroll change, from the rooms' state selections.</summary>
    private static readonly Lazy<FrozenDictionary<RoomScrollChange, RoomDefinition>> PlacedIn = new(FindPlacingRooms);

    /// <summary>The retail scroll programs by their bank-$8F addresses.</summary>
    extension(RoomObjectScrollChangeDefinitions)
    {
        /// <summary>Every retail scroll program's bank-$8F address, ascending.</summary>
        internal static IEnumerable<ushort> Pointers => RoomScrollProgramNative.Addresses.Values.Order();

        /// <summary>Each retail scroll change and the one room whose state populations place it.</summary>
        internal static IReadOnlyDictionary<RoomScrollChange, RoomDefinition> PlacingRooms => PlacedIn.Value;

        /// <summary>The program stored at bank-$8F address <paramref name="pointer"/>.</summary>
        /// <exception cref="InvalidDataException">No retail scroll program is stored there.</exception>
        internal static RoomScrollChange ProgramAt(ushort pointer) =>
            RoomScrollProgramNative.Addresses.Where(entry => entry.Value == pointer).Select(entry => (RoomScrollChange?)entry.Key).SingleOrDefault()
                ?? throw new InvalidDataException($"No retail scroll program is stored at $8F:{pointer:X4}.");

        /// <summary>Applies the ordered writes of the program stored at <paramref name="pointer"/>, each to its screen of the room.</summary>
        internal static void ApplyAt(ushort pointer, Action<Position16<RoomScreens>, RoomScrollState> write) =>
            RoomObjectScrollChanges.Apply(RoomObjectScrollChangeDefinitions.ProgramAt(pointer), write);

        /// <summary>
        /// The native encoding of the program stored at <paramref name="pointer"/>: each write's cell number, its screen's row
        /// times the placing room's width plus its column, and state, then the terminator.
        /// </summary>
        /// <exception cref="InvalidDataException">A write's column lies outside the placing room.</exception>
        internal static ReadOnlyMemory<byte> Get(ushort pointer)
        {
            RoomScrollChange change = RoomObjectScrollChangeDefinitions.ProgramAt(pointer);
            int width = PlacedIn.Value[change].Size.X;
            var bytes = new List<byte>();
            RoomObjectScrollChanges.Apply(change, (screen, state) =>
            {
                if (screen.X >= width)
                    throw new InvalidDataException($"{change} writes column {screen.X}, outside its {width}-screen-wide room.");
                bytes.Add(checked((byte)(screen.Y * width + screen.X)));
                bytes.Add(state.ToStorageByte());
            });
            bytes.Add(0x80);
            return bytes.ToArray();
        }
    }

    /// <summary>Finds the room each retail scroll change is placed in from the rooms' state selections.</summary>
    /// <exception cref="InvalidDataException">A change is placed in two rooms.</exception>
    private static FrozenDictionary<RoomScrollChange, RoomDefinition> FindPlacingRooms()
    {
        var placedIn = new Dictionary<RoomScrollChange, RoomDefinition>();
        foreach ((RoomDefinition room, RoomStateChoice choice) in RoomStateSelectionDefinitions.Choices)
        {
            foreach (RoomStateSettings state in choice.Branches.Select(branch => branch.State).Prepend(choice.Default))
            {
                foreach (RoomObjectPlacement placement in state.ObjectPopulation.Placements.ToArray())
                {
                    if (placement.CompiledScrollChange is not { } change)
                        continue;
                    if (placedIn.TryGetValue(change, out RoomDefinition? earlier) && earlier.Name != room.Name)
                        throw new InvalidDataException($"{change} is placed in both {earlier.Name} and {room.Name}.");
                    placedIn[change] = room;
                }
            }
        }
        return placedIn.ToFrozenDictionary();
    }
}
