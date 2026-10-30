namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The terrain a room has past its authored grid (#1469). Blocks counted past the room's last block read, first, the
/// rest of the room's level data that followed the grid, read as blocks, and then solid blocks to the end of the room's
/// level space; every behaviour there starts empty. Room objects may write terrain and behaviour there, and the writes
/// last for the room's lifetime. Probes that ask about a position outside the room's width or height read solid
/// terrain (<see cref="EdgeSolid"/>).
/// </summary>
/// <remarks>
/// <para>
/// Consulted when a room object's setup or draw lands past the room's last block, when a collision probe or a
/// collision extension crosses the room's edge, and when enemy terrain reads index past the room. The original's
/// bottom-edge fill of a door's scroll also read a full column of this space below a short room; what that drew is a
/// presentation quirk, so the display shows only the room's grid and does not consult the service.
/// </para>
/// <para>
/// Evidence: the original game filled the room's whole level space with solid blocks before decompressing the room over
/// its beginning (<c>$82:EA73</c>), and indexed it without a bound on the room's width or height. Two shipped coloured
/// doors are placed off their rooms, so their setup writes land in this space and must persist without disturbing the
/// room; the bottom-edge column fill shows that the decompressed data following a short room's grid is what lies
/// there before the solid fill. The game's terrain probes therefore find these blocks, not empty space.
/// </para>
/// <para>
/// The service is written in its own types: the room module supplies its terrain and behaviour types, the solid block an
/// edge probe reads and the empty behaviour. One instance is built from the installed room level for each room load and
/// owned by that room's level data.
/// </para>
/// </remarks>
/// <typeparam name="TTerrain">The room's terrain block.</typeparam>
/// <typeparam name="TBehavior">The room's block behaviour.</typeparam>
[CompatibilityService("room objects and probes that reach past a room's grid find the level data that followed it, then solid terrain, and their writes there persist")]
public sealed class TerrainBeyondRoom<TTerrain, TBehavior>
    where TTerrain : struct
    where TBehavior : struct
{
    /// <summary>The terrain of each block past the room's grid, in order.</summary>
    private readonly TTerrain[] terrain;

    /// <summary>The behaviour of each block past the room's grid, in order.</summary>
    private readonly TBehavior[] behaviors;

    /// <summary>Builds the terrain past one room's grid.</summary>
    /// <param name="following">The level data that followed the room's grid, read as blocks; no more than <paramref name="count"/>.</param>
    /// <param name="count">How many blocks the room's level space holds past its grid, zero or more.</param>
    /// <param name="edgeSolid">The solid block that fills the rest of the space, and that a probe outside the room reads.</param>
    /// <param name="noBehavior">The empty behaviour every block past the grid starts with.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative or shorter than <paramref name="following"/>.</exception>
    public TerrainBeyondRoom(IReadOnlyList<TTerrain> following, int count, TTerrain edgeSolid, TBehavior noBehavior)
    {
        Ensure.NotNull(following);
        Ensure.BetweenInclusive(following.Count, int.MaxValue, count);
        terrain = new TTerrain[count];
        Array.Fill(terrain, edgeSolid);
        for (int index = 0; index < following.Count; index++)
            terrain[index] = following[index];
        behaviors = new TBehavior[count];
        Array.Fill(behaviors, noBehavior);
        EdgeSolid = edgeSolid;
        NoBehavior = noBehavior;
    }

    /// <summary>The solid block a probe outside the room's width or height reads.</summary>
    public TTerrain EdgeSolid { get; }

    /// <summary>The behaviour a probe outside the room reads with <see cref="EdgeSolid"/>.</summary>
    public TBehavior NoBehavior { get; }

    /// <summary>How many blocks lie past the room's grid.</summary>
    public int Count => terrain.Length;

    /// <summary>The terrain of a block past the grid.</summary>
    /// <param name="index">The block, counted from the first block past the room's last block.</param>
    /// <exception cref="ArgumentOutOfRangeException">The block is past the room's level space.</exception>
    public TTerrain TerrainAt(int index) => terrain[Checked(index)];

    /// <summary>The behaviour of a block past the grid.</summary>
    /// <param name="index">The block, counted from the first block past the room's last block.</param>
    /// <exception cref="ArgumentOutOfRangeException">The block is past the room's level space.</exception>
    public TBehavior BehaviorAt(int index) => behaviors[Checked(index)];

    /// <summary>Replaces the terrain of a block past the grid, for the rest of the room's lifetime.</summary>
    /// <param name="index">The block, counted from the first block past the room's last block.</param>
    /// <param name="block">The terrain written.</param>
    /// <exception cref="ArgumentOutOfRangeException">The block is past the room's level space.</exception>
    public void SetTerrain(int index, TTerrain block) => terrain[Checked(index)] = block;

    /// <summary>Replaces the behaviour of a block past the grid, for the rest of the room's lifetime.</summary>
    /// <param name="index">The block, counted from the first block past the room's last block.</param>
    /// <param name="behavior">The behaviour written.</param>
    /// <exception cref="ArgumentOutOfRangeException">The block is past the room's level space.</exception>
    public void SetBehavior(int index, TBehavior behavior) => behaviors[Checked(index)] = behavior;

    /// <summary>Rejects a block outside the room's level space.</summary>
    private int Checked(int index) =>
        (uint)index < (uint)terrain.Length
            ? index
            : throw new ArgumentOutOfRangeException(nameof(index), index,
                $"The room's level space holds {terrain.Length} blocks past its grid.");
}
