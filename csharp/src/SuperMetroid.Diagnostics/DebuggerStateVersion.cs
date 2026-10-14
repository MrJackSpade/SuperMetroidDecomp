namespace SuperMetroid.Desktop;

/// <summary>
/// The on-disk debugger-state schema, stored as a 32-bit header word. A build reads only its own
/// version and object layout (#1270): older states are rejected, never converted.
/// </summary>
internal enum DebuggerStateVersion
{
    /// <summary>
    /// Named delegate signatures, the installed-content identity with its named component table,
    /// and the build's exact object-graph layout, with the SPC player inside the game's audio
    /// session rather than beside the game.
    /// </summary>
    Current = 6,
}
