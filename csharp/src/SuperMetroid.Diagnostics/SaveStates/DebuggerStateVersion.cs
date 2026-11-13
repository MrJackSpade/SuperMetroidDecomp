namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// The on-disk debugger-state schema, stored as a 32-bit value in the file's header. A build reads only its own
/// version and object layout (#1270): older states are rejected, never converted.
/// </summary>
internal enum DebuggerStateVersion
{
    /// <summary>
    /// Named delegate signatures, the installed-content identity with its named component table,
    /// and the build's exact object-graph layout, with the music player inside the game's audio
    /// session rather than beside the game, and the room, its state and the game state named in the header instead of
    /// addressed or numbered; the host's bound content named by its path instead of held, and enums stored by member
    /// name (#1507); the header's frame number a whole update count rather than a wrapping 16-bit counter (#1607); the objects
    /// the game's static members hold, catalog identities among them, named by their path from the member instead of
    /// copied (#1623); every hash-keyed collection held as its comparers and entries, which a restore adds to a new
    /// collection, instead of its capturing process's bucket layout (#1681); the game itself as the graph's root, with no
    /// work-memory image beside it (#1688).
    /// </summary>
    Current = 15,
}
