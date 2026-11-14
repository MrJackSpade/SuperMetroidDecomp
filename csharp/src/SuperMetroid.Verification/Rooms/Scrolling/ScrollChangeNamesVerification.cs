using System.Text;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Verification.Rooms.Scrolling;

/// <summary>
/// Checks that every retail scroll change's name describes what its writes do (#1624): its name is the room whose
/// populations place it, then the screens of that room it closes, opens, and opens top-aligned, so a name cannot drift
/// from the change's data.
/// </summary>
internal static class ScrollChangeNamesVerification
{
    /// <summary>
    /// Finds the room each retail scroll change is placed in from the rooms' state selections, and checks that the
    /// change's name is that room followed by the column and row of each screen its writes close, open and open
    /// top-aligned, in that order and in screen order within each, a screen in a row past the room's marked as below the
    /// room, and that each screen lies in one of the room's columns.
    /// </summary>
    internal static void VerifyScrollChangeNamesDescribeTheirScreens()
    {
        IReadOnlyDictionary<RoomScrollChange, RoomDefinition> placedIn = RoomObjectScrollChangeDefinitions.PlacingRooms;
        AssertEqual(Enum.GetValues<RoomScrollChange>().Length, RoomObjectScrollChangeDefinitions.Changes.Count,
            "every scroll change has writes");
        AssertEqual(RoomObjectScrollChangeDefinitions.Changes.Count, placedIn.Count, "every scroll change is placed in a room");
        foreach ((RoomScrollChange change, ScreenScrollChange[] writes) in RoomObjectScrollChangeDefinitions.Changes)
        {
            RoomDefinition room = placedIn[change];
            AssertEqual(writes.Length, writes.Select(write => write.Screen).Distinct().Count(), $"{change} writes each screen once");
            var name = new StringBuilder(room.Name);
            AppendScreens(name, room, writes, RoomScrollState.Closed, "Closes", "");
            AppendScreens(name, room, writes, RoomScrollState.Open, "Opens", "");
            AppendScreens(name, room, writes, RoomScrollState.OpenAlignedToTop, "Opens", "TopAligned");
            AssertEqual(name.ToString(), change.ToString(), $"{change} names the screens its writes change in {room.Name}");
        }
    }

    /// <summary>Appends the screens <paramref name="writes"/> set to <paramref name="state"/>, if any, under <paramref name="verb"/>.</summary>
    private static void AppendScreens(StringBuilder name, RoomDefinition room, ScreenScrollChange[] writes, RoomScrollState state, string verb, string suffix)
    {
        string[] screens = writes.Where(write => write.State == state).OrderBy(write => write.Screen.Y).ThenBy(write => write.Screen.X).Select(write =>
        {
            AssertTrue(write.Screen.X < room.Size.X, $"scroll write {write.Screen} lies in one of {room.Name}'s columns");
            return $"{write.Screen.X}x{write.Screen.Y}" + (write.Screen.Y < room.Size.Y ? "" : "BelowTheRoom");
        }).ToArray();
        if (screens.Length == 0)
            return;
        name.Append(verb).Append(screens.Length == 1 ? "Screen" : "Screens").Append(string.Join("And", screens)).Append(suffix);
    }
}
