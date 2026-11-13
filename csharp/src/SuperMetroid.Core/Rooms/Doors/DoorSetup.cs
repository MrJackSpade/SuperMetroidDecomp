using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// What a door does to the room it leads into while Samus passes through it (#1721): the screens of that room it closes to
/// the camera, opens and opens with the camera aligned to the screen's top, in the order it changes them, and the room
/// event it starts. Every retail setup is one of <see cref="DoorSetupDefinitions"/>; a setup is what it does, so one names
/// each change in each room, whichever door runs it.
/// </summary>
public sealed class DoorSetup
{
    /// <summary>Keeps a setup's name, its screen changes in order and its room event.</summary>
    internal DoorSetup(string name, IReadOnlyList<DoorScreenChange> screenChanges, DoorRoomEvent roomEvent = DoorRoomEvent.None)
    {
        Name = Ensure.NotNull(name);
        ScreenChanges = Ensure.NotNull(screenChanges);
        RoomEvent = roomEvent;
    }

    /// <summary>The setup's name: the room its doors lead into, then what it does there.</summary>
    public string Name { get; }

    /// <summary>The screens the setup closes and opens, in the order it changes them.</summary>
    internal IReadOnlyList<DoorScreenChange> ScreenChanges { get; }

    /// <summary>What the setup starts in the room besides changing its screens.</summary>
    public DoorRoomEvent RoomEvent { get; }

    /// <summary>Gives each of the setup's screens its new scroll state in <paramref name="scrolls"/>, in order.</summary>
    internal void ChangeScreens(RoomScrollGrid scrolls)
    {
        Ensure.NotNull(scrolls);
        foreach (DoorScreenChange change in ScreenChanges)
            scrolls.SetScreen(change.Screen, change.State);
    }

    /// <inheritdoc/>
    public override string ToString() => Name;
}
