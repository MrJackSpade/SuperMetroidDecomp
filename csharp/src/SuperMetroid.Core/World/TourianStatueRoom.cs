using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.World;

/// <summary>
/// The Tourian statue room as its statues' enemies read it (#2033, #2382): the room effect's water their particles splash
/// through, how far the entrance statue has sunk and the statues' shared animation state. Only the statue room, whose setup
/// unlocks the statues, has one; its load builds it before the room's room objects, whose grey door waits for the statue
/// to finish (#2486), and before its enemies, which take it as theirs, and its statue sequence lowers the statue.
/// </summary>
[RoomLifetime]
public sealed class TourianStatueRoom : ITourianStatueRoom, ITourianStatueProgress
{
    /// <summary>The room effect's water surface, which the liquid step moves.</summary>
    private readonly RoomLiquidSurface _water;

    /// <summary>Builds the statue room over its effect's water, the statue standing raised.</summary>
    /// <param name="water">The room effect's water surface, which the particles splash through (#2486, #2554).</param>
    internal TourianStatueRoom(RoomLiquidSurface water) => _water = Ensure.NotNull(water);

    /// <inheritdoc/>
    public TourianStatueAnimation Animation { get; } = new();

    /// <inheritdoc/>
    public bool IsFinished => Animation.Finished;

    /// <summary>
    /// How far the statues have lowered, zero through the authored distance and the few quarter-steps that first reach it,
    /// exposed as a negative background vertical offset. The statue sequence lowers it.
    /// </summary>
    internal SignedSixteenSixteen Descent { get; set; }

    /// <inheritdoc/>
    /// <remarks>
    /// The whole pixels of the statues' descent upward, which descends by a quarter pixel per frame. The upward distance's
    /// whole pixels round toward negative infinity, so the first quarter-pixel step already reads -1. The descent stops at
    /// its authored distance, so the offset stays between minus that distance and zero.
    /// </remarks>
    public int StatueOffset => (-Descent).WholePixels;

    /// <inheritdoc/>
    /// <remarks>Read live from the room effect's water, which the update's liquid step has already moved.</remarks>
    public int WaterSurface => _water.Line;
}
