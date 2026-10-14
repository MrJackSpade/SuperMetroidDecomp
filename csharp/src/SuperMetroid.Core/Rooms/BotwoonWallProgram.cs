namespace SuperMetroid.Core.Rooms;

/// <summary>Botwoon's wall PLM programs (#1296), in authored order.</summary>
internal enum BotwoonWallProgram
{
    /// <summary>Loads nine rows, opens the first two scroll cells, then runs on into the first row (header $B79B).</summary>
    Crumble,
    /// <summary>One row: queues the crumble sound, crumbles the block, moves down and repeats while rows remain.</summary>
    CrumbleRow,
    /// <summary>Clears the defeated room's wall (header $B797).</summary>
    Clear,
}
