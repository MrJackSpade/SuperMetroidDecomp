using System.Text;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>
/// Checks that every retail door setup's name describes what it does (#1624, #1721): the room its doors lead into, then,
/// for a setup that only changes screens, the screens of that room it closes, opens and opens top-aligned, so a name
/// cannot drift from the setup's screen changes.
/// </summary>
internal static class DoorSetupNamesVerification
{
    /// <summary>Each scroll state a door setup gives a screen, in the order its name lists them, with the words that name it.</summary>
    private static readonly (RoomScrollState State, string Verb, string Suffix)[] NamedStates =
    [
        (RoomScrollState.Closed, "Closes", ""),
        (RoomScrollState.Open, "Opens", ""),
        (RoomScrollState.OpenAlignedToTop, "Opens", "TopAligned"),
    ];

    /// <summary>
    /// Runs every retail door setup's screen changes over the room its doors lead into and checks its name: that room,
    /// then the column and row of each screen it closes, opens and opens top-aligned (a screen past the room's rows
    /// marked as below the room). A setup that only changes screens has nothing between the two, so its name is its room
    /// and change alone and two setups cannot make one change in one room; a setup that also starts a room event names
    /// that event there.
    /// </summary>
    internal static void VerifyDoorSetupNamesDescribeTheirScreens()
    {
        Dictionary<DoorSetup, DoorDefinition[]> doorsRunning = DoorDefinitions.Definitions
            .Where(door => door.Setup != DoorSetupDefinitions.None)
            .GroupBy(door => door.Setup)
            .ToDictionary(group => group.Key, group => group.ToArray());
        AssertEqual(DoorIdentityWords.SetupRoutines.WordsByIdentity.Count - 1, doorsRunning.Count,
            "every door setup the cartridge has but none is run by a door");

        var described = new Dictionary<DoorSetup, (RoomDefinition Room, string Screens)>();
        foreach ((DoorSetup setup, DoorDefinition[] doors) in doorsRunning)
        {
            RoomDefinition[] rooms = [.. doors.Select(door => door.DestinationRoom).Distinct()];
            AssertEqual(1, rooms.Length, $"{setup}'s doors lead into one room");
            described[setup] = (rooms[0], ScreensChanged(setup, rooms[0]));
        }

        foreach ((DoorSetup setup, (RoomDefinition room, string screens)) in described)
        {
            string name = setup.Name;
            AssertTrue(name.Length >= room.Name.Length + screens.Length
                && name.StartsWith(room.Name, StringComparison.Ordinal)
                && name.EndsWith(screens, StringComparison.Ordinal),
                $"{setup} is named for {room.Name}, then the screens it changes ({screens})");
            string between = name[room.Name.Length..^screens.Length];
            if (setup.RoomEvent != DoorRoomEvent.None)
            {
                AssertTrue(between.Length > 0 && !between.Contains("Opens", StringComparison.Ordinal)
                    && !between.Contains("Closes", StringComparison.Ordinal),
                    $"{setup} names its room event between {room.Name} and its screens");
                continue;
            }
            AssertTrue(screens.Length > 0, $"{setup} changes a screen of {room.Name}");
            AssertEqual("", between, $"{setup} names only {room.Name} and its screens");
        }
    }

    /// <summary>
    /// The screens <paramref name="setup"/> changes in <paramref name="room"/>, as its name lists them: its screen changes
    /// run over the room's kept cells filled with each scroll state in turn, so every change shows in a run whose cells it
    /// changes.
    /// </summary>
    private static string ScreensChanged(DoorSetup setup, RoomDefinition room)
    {
        var written = new SortedDictionary<int, RoomScrollState>();
        foreach ((RoomScrollState fill, _, _) in NamedStates)
        {
            RoomScrollGrid grid = RoomScrollGrid.LoadNativeTable(
                Enumerable.Repeat(fill.ToStorageByte(), RoomScrollGrid.KeptCellCount).ToArray(), room.Size);
            setup.ChangeScreens(grid);
            RoomScrollState[] cells = grid.KeptStates;
            for (int index = 0; index < cells.Length; index++)
            {
                if (cells[index] == fill)
                    continue;
                if (written.TryGetValue(index, out RoomScrollState earlier))
                    AssertEqual(earlier, cells[index], $"{setup} leaves cell {index} in one state");
                written[index] = cells[index];
            }
        }
        var name = new StringBuilder();
        foreach ((RoomScrollState state, string verb, string suffix) in NamedStates)
        {
            string[] screens = [.. written.Where(write => write.Value == state).Select(write =>
                $"{write.Key % room.Size.X}x{write.Key / room.Size.X}" + (write.Key < room.Size.X * room.Size.Y ? "" : "BelowTheRoom"))];
            if (screens.Length > 0)
                name.Append(verb).Append(screens.Length == 1 ? "Screen" : "Screens").Append(string.Join("And", screens)).Append(suffix);
        }
        return name.ToString();
    }
}
