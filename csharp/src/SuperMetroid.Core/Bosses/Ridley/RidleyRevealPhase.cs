namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The phases of the reveal both of Ridley's encounters open with (#2227): he waits for Samus's door, holds, fades his eyes
/// and then his body in, roars and takes off. His lift-off ends the reveal; the encounter's own phases
/// (<see cref="CeresRidleyPhase"/>, <see cref="NorfairRidleyPhase"/>) run from then on. The state stores the member; its
/// values are arbitrary.
/// </summary>
public enum RidleyRevealPhase
{
    /// <summary>Waits for the room-entry door transition before starting Ridley's reveal.</summary>
    WaitForDoorTransition,
    /// <summary>Counts the initial reveal delay before fading the eyes.</summary>
    InitialDelay,
    /// <summary>Advances the eye-colour fade before the body's.</summary>
    FadeInEyes,
    /// <summary>Advances the body-colour fade and prepares the roar.</summary>
    FadeInBody,
    /// <summary>Waits before starting the opening roar.</summary>
    WaitBeforeRoar,
    /// <summary>Holds the opening roar, in Lower Norfair while his arena is revealed, before he takes off.</summary>
    WaitBeforeLiftoff,
    /// <summary>Holds still while his take-off plays, until its lift-off ends the reveal.</summary>
    TakingOff,
}
