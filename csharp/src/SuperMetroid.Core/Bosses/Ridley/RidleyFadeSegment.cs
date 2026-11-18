namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The authored colour sequences of the Ridley encounters that the shared animation player plays (#2409), each the images
/// of one of their colour runs (<see cref="RidleyFadeDefinitions"/>) in the order they show, with how long each shows.
/// </summary>
internal enum RidleyFadeSegment
{
    /// <summary>
    /// Both Ridleys' eyes lit from the dark: each eye shade for one update, the darkest first, then his own eye colours
    /// held before his body begins to fade in.
    /// </summary>
    EyesFadingIn,

    /// <summary>
    /// Both Ridleys' bodies brought out of black: each body shade for two updates, ending on his own colours, which his
    /// fight then shows.
    /// </summary>
    BodyFadingIn,

    /// <summary>Lower Norfair Ridley's arena brought out of black as he holds his roar: each of its colourings for three updates.</summary>
    ArenaRevealing,

    /// <summary>
    /// The self-destruct warning's EMERGENCY title flashing through its alarm colours, one colouring for each of the
    /// alarm's steps, its first after its last.
    /// </summary>
    AlarmCycling,

    /// <summary>
    /// The self-destruct warning's first cycle through its alarm colours, from its second colouring to its last: the
    /// cycle counts on from its first colouring before it shows any, then cycles whole (<see cref="AlarmCycling"/>).
    /// </summary>
    AlarmFirstCycle,
}
