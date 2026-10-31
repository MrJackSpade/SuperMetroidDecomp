namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// A cap that has been opened for good: its door is recorded as opened, and the cap keeps which of the sixteen doors of
/// its window were open at the opening. The window starts at the first door of the opened door's group of eight. It is
/// never checked again.
/// </summary>
public sealed record SpentDoorArgument : PlmRoomArgument
{
    /// <summary>The doors a spent cap's window covers.</summary>
    internal const int WindowDoorCount = 16;

    /// <summary>The doors of each group a window starts at the first of.</summary>
    private const int WindowGroupDoorCount = 8;

    /// <summary>The first door of the window, the first of the opened door's group of eight.</summary>
    internal readonly int FirstDoor;

    /// <summary>The window's doors that were open at the opening, ascending.</summary>
    internal readonly IReadOnlyList<int> OpenedDoors;

    /// <summary>Keeps the window's opened doors as the opening left them.</summary>
    /// <param name="firstDoor">The first door of the window.</param>
    /// <param name="openedDoors">The window's doors open at the opening, ascending.</param>
    internal SpentDoorArgument(int firstDoor, IReadOnlyList<int> openedDoors)
    {
        FirstDoor = Ensure.AtLeastZero(firstDoor);
        OpenedDoors = Ensure.NotNull(openedDoors);
    }

    /// <summary>The first door of the window a cap for <paramref name="door"/> keeps.</summary>
    internal static int FirstDoorOfWindow(int door) => Ensure.AtLeastZero(door) / WindowGroupDoorCount * WindowGroupDoorCount;
}
