using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World;

/// <summary>
/// The Tourian statue room as its statues' enemies read it (#2033, #2382): the room effect's water their particles splash
/// through and how far the entrance statue has sunk. Only the statue room, whose setup unlocks the statues, has one; its
/// load builds it before the room's enemies, which take it as theirs, and its statue sequence lowers the statue.
/// </summary>
[RoomLifetime]
public sealed class TourianStatueRoom : ITourianStatueRoom
{
    /// <summary>The room's effect, whose water surface the liquid step moves.</summary>
    private readonly RoomFxOverlayState _roomFxOverlay;

    /// <summary>Builds the statue room over its effect, the statue standing raised.</summary>
    /// <param name="roomFxOverlay">The room's effect, whose water the particles splash through.</param>
    internal TourianStatueRoom(RoomFxOverlayState roomFxOverlay) => _roomFxOverlay = Ensure.NotNull(roomFxOverlay);

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
    /// <remarks>Read live from the room's effect, which the update's liquid step has already moved.</remarks>
    /// <exception cref="InvalidOperationException">The statue room's effect names no water surface.</exception>
    public int WaterSurface => _roomFxOverlay.Surface ??
        throw new InvalidOperationException("The Tourian statue room's effect names no water surface.");
}
