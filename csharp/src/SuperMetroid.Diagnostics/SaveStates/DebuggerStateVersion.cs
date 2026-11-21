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
    /// work-memory image beside it (#1688); every array of values with no identity (enums, structs of values, nullable
    /// values) written as one packed run of a schema, member tables and back-to-back values instead of a framed object per
    /// element (#1696); a type value, such as a key of the room-object visuals' catalog store, written as the identity of
    /// the type it names instead of refused (#2060); the references a packed run's struct elements hold written as places in
    /// the run's table of the objects held, each object written once, instead of every such element framed (#2609); every
    /// object written and restored through the members generated for its type, each level of its fields after their count and
    /// each field after its name, a primary-constructor type's arguments first, delegates by the name of the method each call
    /// makes, and an immutable array of values that pack written as one packed run, as an array of them is (#2602); every
    /// name, of a type, a field, an enum member, a packed run's schema or a method a delegate calls, stored as text the first
    /// time a state names it and by its number every later time, rather than as text at every use (#2602).
    /// </summary>
    Current = 21,
}
