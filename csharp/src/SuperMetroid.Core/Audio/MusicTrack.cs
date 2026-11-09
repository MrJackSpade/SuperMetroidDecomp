namespace SuperMetroid.Core.Audio;

/// <summary>
/// One song of the loaded music data set, named for its part in the set. Every data set holds the four shared songs
/// (the fanfares and the elevator and statue-hall tunes, the same in every set) and up to four songs of its own; a room
/// or scene asks for a song of whichever set is loaded, and the clip library finds each set's songs by these names
/// (<see cref="MusicTrackKey"/>).
/// </summary>
/// <remarks>Values are arbitrary; the import pipeline decodes the game's track numbering into these names once.</remarks>
public enum MusicTrack
{
    /// <summary>Samus's fanfare, played as a game begins.</summary>
    SamusFanfare,
    /// <summary>The fanfare of a permanent item collected.</summary>
    ItemFanfare,
    /// <summary>The elevator tune, also played in item rooms and in a boss's room once the boss has fallen.</summary>
    Elevator,
    /// <summary>The tense statue-hall tune: the hall before Tourian's statues, Ridley's lair before he rises, and the game-over menu.</summary>
    StatueHall,
    /// <summary>The loaded set's first own song: an area's theme, a boss's battle, or a scene's music.</summary>
    FirstSetSong,
    /// <summary>The loaded set's second own song.</summary>
    SecondSetSong,
    /// <summary>The loaded set's third own song.</summary>
    ThirdSetSong,
    /// <summary>The loaded set's fourth own song, which only the Ceres set holds.</summary>
    FourthSetSong,
}
