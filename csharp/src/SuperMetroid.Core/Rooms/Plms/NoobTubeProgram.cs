namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The n00b tube's four PLM programs (#1296), in place of its bank-$84 lists at $84:D4D4..D524.</summary>
internal enum NoobTubeProgram
{
    /// <summary>Skips to the broken tube once its event is set; otherwise draws the intact tube and waits for a power bomb.</summary>
    Main,
    /// <summary>After a power bomb: waits for the player's accepted input.</summary>
    WaitForInput,
    /// <summary>Breaks the tube: crack, shards, bubbles, earthquake and the broken event.</summary>
    Break,
    /// <summary>A tube broken on an earlier visit: restores water physics and deletes the PLM.</summary>
    AlreadyBroken,
}
