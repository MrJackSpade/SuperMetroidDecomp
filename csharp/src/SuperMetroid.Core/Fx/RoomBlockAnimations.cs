using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The looping block animations of one room (#1406, #1988, #2053): one player for each animation the door-selected room
/// effect lists, in the effect's order (the horizontal spikes, Crocomire's wall among them, the Maridia sand and the
/// Wrecked Ship treadmills), and the treadmill a Wrecked Ship entrance door's setup starts. Selection comes from the
/// selected room effect, never from a particular room. Each room builds its own, so nothing carries into the next room.
/// </summary>
/// <remarks>
/// The effect overlay's own animation (lava, acid, rain or spores) is not among them: it is the overlay's, which steps it
/// with the effect's picture under the effect's own freeze (<see cref="RoomFxOverlayState.Step"/>).
/// </remarks>
[RoomLifetime]
public sealed class RoomBlockAnimations
{
    /// <summary>The animations the room effect lists, in the effect's order.</summary>
    internal readonly List<BlockAnimationPlayer> effectAnimations = [];

    /// <summary>The treadmill a Wrecked Ship entrance door's setup starts, once it has; it plays after the effect's animations.</summary>
    private readonly BlockAnimationPlayer doorTreadmill = new();

    /// <summary>Starts one room's animations from the door-selected room effect, in the effect's order; none when the room has no effect.</summary>
    /// <param name="fx">The room state's FX definition.</param>
    /// <param name="door">The entry door, which selects the room effect.</param>
    /// <exception cref="InvalidOperationException">The effect lists an animation with no loop.</exception>
    public RoomBlockAnimations(RoomFxSelection? fx, DoorDefinition? door)
    {
        if (fx?.Select(door) is not { } effect)
            return;
        foreach (BlockAnimation animation in effect.BlockAnimations)
        {
            var player = new BlockAnimationPlayer();
            player.Play(BlockAnimationLoops.Of(animation));
            effectAnimations.Add(player);
        }
    }

    /// <summary>
    /// Starts the Wrecked Ship entrance treadmill for a belt pushing <paramref name="direction"/>, as the entrance door's
    /// setup does; it waits for Phantoon's defeat and afterwards loops its treadmill frames.
    /// </summary>
    /// <exception cref="InvalidOperationException">A door's setup has already started the treadmill.</exception>
    public void StartDoorTreadmill(WreckedShipTreadmillDirection direction) =>
        doorTreadmill.Play(BlockAnimationLoops.Of(BlockAnimationLoops.Treadmill(direction)));

    /// <summary>
    /// Runs one pass of every animation, the effect's in its order and then the door's treadmill, each selecting its frame
    /// for the following frame boundary; an animation that waits for the area's boss starts on the first pass
    /// <paramref name="areaBossDefeated"/> holds (<see cref="BlockAnimationStart.OnceAreaBossIsDefeated"/>).
    /// </summary>
    public void Step(bool areaBossDefeated, ArtworkFrameSelections frames)
    {
        foreach (BlockAnimationPlayer animation in effectAnimations) animation.Step(areaBossDefeated, frames);
        doorTreadmill.Step(areaBossDefeated, frames);
    }
}
