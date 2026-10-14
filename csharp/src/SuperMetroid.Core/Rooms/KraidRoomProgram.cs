namespace SuperMetroid.Core.Rooms;

/// <summary>The Kraid room's ceiling, platform and spike PLM programs (#1296), in authored order.</summary>
internal enum KraidRoomProgram
{
    /// <summary>Crumbles a ceiling block, then runs on into setting it to background one (header $B7A3).</summary>
    CrumbleCeilingIntoBackground1,
    /// <summary>Sets a ceiling block to background one (unused header $B79F).</summary>
    SetCeilingToBackground1,
    /// <summary>Crumbles a ceiling block, then runs on into setting it to background two (header $B7AB).</summary>
    CrumbleCeilingIntoBackground2,
    /// <summary>Sets a ceiling block to background two: the first platform variant (header $B7A7).</summary>
    SetCeilingToBackground2,
    /// <summary>Crumbles a ceiling block, then runs on into setting it to background three (header $B7B3).</summary>
    CrumbleCeilingIntoBackground3,
    /// <summary>Sets a ceiling block to background three: the second platform variant (header $B7AF).</summary>
    SetCeilingToBackground3,
    /// <summary>Clears the defeated room's ceiling (header $B7B7).</summary>
    ClearCeiling,
    /// <summary>Loads eleven spike passes, then runs on into the first pass (header $B7BF).</summary>
    CrumbleSpikes,
    /// <summary>One pass: crumbles two spike columns, moving right after each, and repeats while passes remain.</summary>
    CrumbleSpikesPass,
    /// <summary>Clears the defeated room's spikes (header $B7BB).</summary>
    ClearSpikes,
}
