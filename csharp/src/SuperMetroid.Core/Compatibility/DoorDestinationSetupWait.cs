namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// A door transition's destination-setup wait (#1631): once the transition's camera has aligned with the doorway, it
/// spends two updates before moving Samus and the camera to where the door's scroll starts. The game spent them
/// correcting an upward door's top row of the source room and setting up the destination room, work this room load
/// does at once, but the count is gameplay-observable: each of the two is a whole update of the transition, with its blank
/// frame, its effects and its own random draw, so every later update of the transition, and everything after it, sees the
/// random sequence and the frame counters two updates further on.
/// </summary>
/// <remarks>
/// Started on the update the transition's camera finishes aligning, spent once by each of the transition's following
/// updates, and over after the second, whose successor moves Samus to the scroll's start. Evidence: the door transition
/// runs one stage per update, and between aligning the camera and its scrolling setup it runs the upward door's
/// correction and the room's setup as one stage each; the Ridley door-entry check matches the recorded random number on
/// each of the six loading updates after the source fade, the two of this wait among them.
/// </remarks>
[CompatibilityService("a door transition spends two updates between aligning its camera and moving Samus to the scroll's start, each a whole update of the transition with its own random draw")]
public readonly record struct DoorDestinationSetupWait
{
    /// <summary>The updates the wait lasts.</summary>
    public const int Updates = 2;

    /// <summary>A wait with <paramref name="updatesLeft"/> updates still to spend.</summary>
    private DoorDestinationSetupWait(int updatesLeft) => UpdatesLeft = updatesLeft;

    /// <summary>The wait a transition starts once its camera has aligned, none of its updates spent yet.</summary>
    public static DoorDestinationSetupWait Started { get; } = new(Updates);

    /// <summary>The wait's updates still to spend, from <see cref="Updates"/> down to zero.</summary>
    public int UpdatesLeft { get; }

    /// <summary>Whether the wait is over, so the transition moves on to the door's scroll.</summary>
    public bool IsOver => UpdatesLeft == 0;

    /// <summary>The wait after one more of its updates is spent.</summary>
    /// <exception cref="InvalidOperationException">The wait is already over.</exception>
    public DoorDestinationSetupWait Spend() => IsOver
        ? throw new InvalidOperationException("The door's destination-setup wait is already over.")
        : new(UpdatesLeft - 1);
}
