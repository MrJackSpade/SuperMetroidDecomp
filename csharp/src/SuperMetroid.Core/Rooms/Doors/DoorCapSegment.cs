namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// One run of a door cap's pictures and sounds (<see cref="DoorCapSequenceDefinitions"/>, #2480), shared by every colour
/// and facing: the cap's paint and facing choose what each picture draws.
/// </summary>
internal enum DoorCapSegment
{
    /// <summary>The closed cap for one update, as a cap waits, locks or closes again.</summary>
    Closed,

    /// <summary>
    /// The yellow downward cap's closed image for three updates as it starts to wait (#2480).
    /// </summary>
    YellowDownClosed,

    /// <summary>One flash of a hit short of opening: the blue image for three updates, then the cap for four.</summary>
    Flash,

    /// <summary>The sound each hit on a red cap makes as its flash starts.</summary>
    RedHitSounding,

    /// <summary>A coloured cap opening: the open sound, its three opening images six updates each, the cleared opening for one.</summary>
    Opening,

    /// <summary>The yellow left cap opening: the open sound, its three opening images four updates each, the cleared opening for 92.</summary>
    YellowLeftOpening,

    /// <summary>A shot grey cap opening: the open sound, its three opening images four updates each, the cleared opening for one.</summary>
    GreyOpening,

    /// <summary>A shot blue cap opening: the open sound, its three opening images six updates each, the cleared opening for 94.</summary>
    BlueOpening,

    /// <summary>
    /// A cap closing behind Samus: the cleared opening, then its three opening images in reverse, two updates each, the
    /// close sound as the second of them draws.
    /// </summary>
    Closing,

    /// <summary>Bomb Torizo's door held open behind Samus: the cleared opening for two updates.</summary>
    BombTorizoHeldOpen,

    /// <summary>
    /// Bomb Torizo's door closing once Samus has Bombs: the cleared opening for forty updates, then its three opening images
    /// in reverse, two updates each, the close sound as the first of them draws.
    /// </summary>
    BombTorizoClosing,
}
