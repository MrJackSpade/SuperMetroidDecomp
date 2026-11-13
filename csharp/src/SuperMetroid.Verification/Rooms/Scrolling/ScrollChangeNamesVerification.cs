using System.Text;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.World.RoomStates;

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
    /// top-aligned, in that order and in screen order within each.
    /// </summary>
    internal static void VerifyScrollChangeNamesDescribeTheirScreens()
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
                    if (placedIn.TryGetValue(change, out RoomDefinition? earlier))
                        AssertEqual(earlier.Name, room.Name, $"{change} is placed in one room");
                    placedIn[change] = room;
                }
            }
        }
        AssertEqual(Enum.GetValues<RoomScrollChange>().Length, RoomObjectScrollChangeDefinitions.Changes.Count,
            "every scroll change has writes");
        AssertEqual(RoomObjectScrollChangeDefinitions.Changes.Count, placedIn.Count, "every scroll change is placed in a room");
        foreach ((RoomScrollChange change, RoomScrollWrite[] writes) in RoomObjectScrollChangeDefinitions.Changes)
        {
            RoomDefinition room = placedIn[change];
            AssertEqual(writes.Length, writes.Select(write => write.Index).Distinct().Count(), $"{change} writes each screen once");
            var name = new StringBuilder(room.Name);
            AppendScreens(name, room, writes, RoomScrollState.RedBoundary, "Closes", "");
            AppendScreens(name, room, writes, RoomScrollState.Green, "Opens", "");
            AppendScreens(name, room, writes, RoomScrollState.Blue, "Opens", "TopAligned");
            AssertEqual(name.ToString(), change.ToString(), $"{change} names the screens its writes change in {room.Name}");
        }
    }

    /// <summary>Appends the screens <paramref name="writes"/> set to <paramref name="state"/>, if any, under <paramref name="verb"/>.</summary>
    private static void AppendScreens(StringBuilder name, RoomDefinition room, RoomScrollWrite[] writes, RoomScrollState state, string verb, string suffix)
    {
        string[] screens = writes.Where(write => write.State == state).OrderBy(write => write.Index).Select(write =>
        {
            AssertTrue(write.Index < room.Size.X * room.Size.Y, $"scroll write {write.Index} lies within {room.Name}'s screens");
            return $"{write.Index % room.Size.X}x{write.Index / room.Size.X}";
        }).ToArray();
        if (screens.Length == 0)
            return;
        name.Append(verb).Append(screens.Length == 1 ? "Screen" : "Screens").Append(string.Join("And", screens)).Append(suffix);
    }
}
