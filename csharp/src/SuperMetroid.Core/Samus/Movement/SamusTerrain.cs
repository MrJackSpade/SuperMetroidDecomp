using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// The terrain Samus's movement, collision, grapple and projectiles meet (#633, #2553): a room's level, whose reacting
/// blocks reach the room's objects (<see cref="RoomTerrain"/>), or a plain terrain of air, solid blocks and slopes alone,
/// which holds no block that reacts (<see cref="PlainTerrain"/>). These two are the only terrains.
/// </summary>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
internal abstract class SamusTerrain
{
    /// <summary>Creates the terrain of <paramref name="level"/>.</summary>
    /// <param name="level">The collision level the scans probe.</param>
    private protected SamusTerrain(RoomLevelData level) => Level = Ensure.NotNull(level);

    /// <summary>The collision level the scans probe.</summary>
    internal RoomLevelData Level { get; }

    /// <summary>
    /// The block met at the cell <paramref name="block"/> a read found: the block it stands for
    /// (<see cref="RoomLevelData.StandingBlockFor"/>), carrying its reaction when this terrain's blocks reach room objects;
    /// null when the cell behaves as air.
    /// </summary>
    /// <param name="block">A block the read found in the grid, or the edge-solid block outside it.</param>
    /// <exception cref="InvalidDataException">A link leads outside the room's grid, or the chain is cyclic.</exception>
    internal MetBlock? Meet(RoomCollisionBlock block) => Level.StandingBlockFor(block) is { } standing ? Met(standing) : null;

    /// <summary>The block met at <paramref name="block"/> of the grid (<see cref="Meet(RoomCollisionBlock)"/>).</summary>
    /// <param name="block">A block of the grid.</param>
    internal MetBlock? MeetAt(Position16<RoomBlocks> block) => Meet(Level.GetCollisionBlock(block));

    /// <summary>
    /// The block met at <paramref name="block"/>, which a collision probe outside the room's grid meets as the edge-solid
    /// block (<see cref="Meet(RoomCollisionBlock)"/>, <see cref="RoomLevelData.GetCollisionBlockOrEdgeSolid"/>).
    /// </summary>
    /// <param name="block">The block the probe reads.</param>
    internal MetBlock? MeetAtOrEdgeSolid(Position16<RoomBlocks> block) => Meet(Level.GetCollisionBlockOrEdgeSolid(block));

    /// <summary>The met block of <paramref name="standing"/> in this terrain, with the reaction this terrain's blocks carry.</summary>
    /// <param name="standing">The block a met cell stands for.</param>
    private protected abstract MetBlock Met(StandingBlock standing);
}
