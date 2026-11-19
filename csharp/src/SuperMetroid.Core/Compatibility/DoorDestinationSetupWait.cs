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
/// The transition holds the wait as an <see cref="UpdateCountdown"/> of <see cref="Updates"/>.
/// </remarks>
[CompatibilityService("a door transition spends two updates between aligning its camera and moving Samus to the scroll's start, each a whole update of the transition with its own random draw")]
public static class DoorDestinationSetupWait
{
    /// <summary>The updates the wait lasts.</summary>
    public const int Updates = 2;
}
