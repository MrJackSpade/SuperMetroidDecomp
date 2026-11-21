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
    /// <summary>The room's effect, whose water surface the liquid step moves.</summary>
    private readonly RoomFxOverlayState _roomFxOverlay;

    /// <summary>Builds the statue room over its effect, the statue standing raised.</summary>
    /// <param name="roomFxOverlay">The room's effect, whose water the particles splash through.</param>
    /// <exception cref="InvalidDataException">The room's effect, as its load selected it, names no water surface.</exception>
    internal TourianStatueRoom(RoomFxOverlayState roomFxOverlay)
    {
        _roomFxOverlay = Ensure.NotNull(roomFxOverlay);
        // The statue room is composed only over water (#2486): its load rejects an effect without a surface, which no liquid
        // step takes away, so the statues' particles always have a surface to cross.
        if (!roomFxOverlay.Surface.HasValue)
            throw new InvalidDataException("The Tourian statue room's effect names no water surface.");
    }

    /// <inheritdoc/>
    public TourianStatueAnimation Animation { get; } = new();

    /// <inheritdoc/>
    public bool IsFinished => Animation.Finished;

    /// <summary>
    /// How far the statues have sunk into the room, zero through the authored distance and the few quarter-steps that
    /// first reach it, which the display shows as the statues' placement. The statue sequence lowers it.
    /// </summary>
    internal SignedSixteenSixteen Descent { get; set; }

    /// <inheritdoc/>
    /// <remarks>
    /// The whole pixels of the statues' descent upward, which descends by <see cref="TourianStatueSequenceDefinitions.DescentStep"/> each frame. The upward distance's
    /// whole pixels round toward negative infinity, so the first step already reads -1. The descent stops at
    /// its authored distance, so the offset stays between minus that distance and zero.
    /// </remarks>
    public int StatueOffset => (-Descent).WholePixels;

    /// <inheritdoc/>
    /// <remarks>
    /// Read live from the room's effect, which the update's liquid step has already moved. The effect keeps a liquid's
    /// surface once its load has given it one; the surface word the game's water tests read is its line
    /// (<see cref="Compatibility.AbsentWaterSurfaceLine"/>).
    /// </remarks>
    public int WaterSurface => Compatibility.AbsentWaterSurfaceLine.Line(_roomFxOverlay.Surface);
}
