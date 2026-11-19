using SuperMetroid.Core.Compatibility;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// What Samus's collision with the room's door blocks has left for the game to act on (#2545): the door she entered, which
/// the door transition goes through, and her stand at the elevator's door, which reaches the elevator on the next update.
/// Both belong to the room: the door waits through a message box the same update opened, the pause's last darkening
/// updates and the transition's own updates until the destination loads, and a room left behind takes both with it.
/// </summary>
[RoomLifetime]
internal sealed class RoomDoorContacts
{
    /// <summary>
    /// Samus's stand at the elevator's door, which waits for the elevator. Every gameplay update runs the enemies before
    /// Samus's collision, so the elevator takes a stand her collision makes at the start of the next update's enemies, never
    /// on the update she made it. The one-update delay is gameplay-observable: the elevator leaves an update after she stands
    /// at its door.
    /// </summary>
    internal ElevatorStandAtDoor ElevatorStand { get; } = new();

    /// <summary>
    /// The door Samus's collision entered, until the door transition goes through it or the reserve refill's beginning
    /// forgets it; null while she has entered none. The first door she enters is the one she goes through.
    /// </summary>
    public DoorDefinition? EnteredDoor { get; private set; }

    /// <summary>Acts on one of Samus's door-block contacts: a door she enters, or her stand at the elevator's door.</summary>
    /// <param name="contact">The contact her collision made.</param>
    internal void Meet(DoorContact contact)
    {
        switch (Ensure.NotNull(contact))
        {
            case DoorContact.EntersDoor entered:
                EnteredDoor ??= entered.Door;
                return;
            case DoorContact.StandsAtElevator:
                ElevatorStand.Stand();
                return;
            case DoorContact.BrushesElevator:
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(DoorContact)} {contact}.");
        }
    }

    /// <summary>
    /// Forgets the entered door without going through it (#2534): the AUTO reserves' refill beginning later in the update
    /// replaces the door's game state, so only a later touch of a door enters one again.
    /// </summary>
    internal void ForgetEnteredDoor() => EnteredDoor = null;

    /// <summary>
    /// The door transition goes through <paramref name="door"/>, the entered door, once its destination's assets have
    /// loaded; a destination that fails to load leaves the door entered.
    /// </summary>
    /// <param name="door">The door the transition goes through.</param>
    /// <exception cref="InvalidOperationException">Samus entered no door, or another one, while the destination loaded.</exception>
    internal void GoThroughEnteredDoor(DoorDefinition door)
    {
        DoorDefinition entered = EnteredDoor
            ?? throw new InvalidOperationException("The entered door disappeared while its destination's assets were prepared.");
        if (entered != door)
            throw new InvalidOperationException($"The entered door changed from {door.Name} to {entered.Name} while its destination's assets were prepared.");
        EnteredDoor = null;
    }
}
