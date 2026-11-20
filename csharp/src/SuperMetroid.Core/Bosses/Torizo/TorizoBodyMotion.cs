namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// How a Bomb or Golden Torizo's body moves itself each update before its animation steps (#1931). A cue of its animation
/// starts a motion or stops it, and a Torizo with no motion holds still. Both statues move by the same motions; what
/// differs between them comes from their definition (<see cref="TorizoInitializationDefinition"/>).
/// </summary>
internal enum TorizoBodyMotion
{
    /// <summary>Holds still: the body does not move itself, as the StopMoving cue and its setup leave it.</summary>
    HoldingStill,

    /// <summary>
    /// Walks on: once its turn timer runs out the Torizo turns toward Samus, and while the ground under it has given way
    /// it starts to fall.
    /// </summary>
    Walking,

    /// <summary>Stands its ground while it attacks or is stunned, settling onto the floor and shaking the room as a fall lands.</summary>
    Standing,

    /// <summary>Follows a leap's or a fall's arc until it lands, then plays the animation its leap lands into.</summary>
    Leaping,
}
