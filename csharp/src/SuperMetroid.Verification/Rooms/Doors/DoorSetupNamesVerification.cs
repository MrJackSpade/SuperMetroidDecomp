using System.Text;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>
/// Checks that every retail door setup's name describes what it does (#1624): the room its doors lead into, then, for a
/// setup that only changes screens, the screens of that room its writes close, open and open top-aligned, so a name
/// cannot drift from the setup's writes.
/// </summary>
internal static class DoorSetupNamesVerification
{
    /// <summary>Each scroll state a door setup writes, in the order its name lists them, with the words that name it.</summary>
    private static readonly (RoomScrollState State, string Verb, string Suffix)[] NamedStates =
    [
        (RoomScrollState.RedBoundary, "Closes", ""),
        (RoomScrollState.Green, "Opens", ""),
        (RoomScrollState.Blue, "Opens", "TopAligned"),
    ];

    /// <summary>
    /// Runs every retail door setup's scroll writes over the room its doors lead into and checks its name: that room,
    /// then the column and row of each screen its writes close, open and open top-aligned (a screen past the room's
    /// rows marked as below the room). A setup that only changes screens has nothing between the two, so its name is
    /// its room and change alone and two members cannot make one change in one room; a setup that does more names its
    /// effect there.
    /// </summary>
    internal static void VerifyDoorSetupNamesDescribeTheirScreens()
    {
        Dictionary<DoorSetupCode, DoorDefinition[]> doorsRunning = DoorDefinitions.Definitions
            .Where(door => door.SetupCode != DoorSetupCode.None)
            .GroupBy(door => door.SetupCode)
            .ToDictionary(group => group.Key, group => group.ToArray());
        AssertEqual(Enum.GetValues<DoorSetupCode>().Length - 1, doorsRunning.Count, "every door setup but none is run by a door");

        var described = new Dictionary<DoorSetupCode, (RoomDefinition Room, string Screens)>();
        foreach ((DoorSetupCode setup, DoorDefinition[] doors) in doorsRunning)
        {
            RoomDefinition[] rooms = [.. doors.Select(door => door.DestinationRoom).Distinct()];
            AssertEqual(1, rooms.Length, $"{setup}'s doors lead into one room");
            described[setup] = (rooms[0], ScreensWritten(setup, doors[0], rooms[0]));
        }

        foreach ((DoorSetupCode setup, (RoomDefinition room, string screens)) in described)
        {
            string name = setup.ToString();
            AssertTrue(name.Length >= room.Name.Length + screens.Length
                && name.StartsWith(room.Name, StringComparison.Ordinal)
                && name.EndsWith(screens, StringComparison.Ordinal),
                $"{setup} is named for {room.Name}, then the screens it changes ({screens})");
            string between = name[room.Name.Length..^screens.Length];
            if (!DoorScrollChanges.Contains(setup))
            {
                AssertTrue(between.Length > 0 && !between.Contains("Opens", StringComparison.Ordinal)
                    && !between.Contains("Closes", StringComparison.Ordinal),
                    $"{setup} names what it does besides its screens between {room.Name} and them");
                continue;
            }
            AssertTrue(screens.Length > 0, $"{setup} changes a screen of {room.Name}");
            AssertEqual("", between, $"{setup} names only {room.Name} and its screens");
        }
    }

    /// <summary>
    /// The screens <paramref name="setup"/> changes in <paramref name="room"/>, as its name lists them: its scroll writes
    /// run over the room's kept cells filled with each scroll state in turn, so every write shows in a run whose cells it
    /// changes.
    /// </summary>
    private static string ScreensWritten(DoorSetupCode setup, DoorDefinition door, RoomDefinition room)
    {
        var written = new SortedDictionary<int, RoomScrollState>();
        foreach ((RoomScrollState fill, _, _) in NamedStates)
        {
            RoomScrollGrid grid = RoomScrollGrid.LoadNativeTable(
                Enumerable.Repeat(fill.ToStorageByte(), RoomScrollGrid.KeptCellCount).ToArray(), room.Size);
            DoorSetupCodeInterpreter.ApplyScrollWrites(setup, door.Name, grid);
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
