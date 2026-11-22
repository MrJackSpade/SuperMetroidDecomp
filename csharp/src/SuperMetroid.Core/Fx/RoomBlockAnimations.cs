using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The looping block animations of one room (#1406, #1988, #2053, #2422): one loop for each animation the door-selected room
/// effect lists, in the effect's order (the horizontal spikes, Crocomire's wall among them, the Maridia sand and the
/// Wrecked Ship treadmills), and the treadmill a Wrecked Ship entrance door's setup starts. Selection comes from the
/// selected room effect, never from a particular room. Each room builds its own, so nothing carries into the next room.
/// The room holds each treadmill's wait for the area's boss (<see cref="BlockAnimationLoops.WaitsForAreaBoss"/>).
/// </summary>
/// <remarks>
/// The effect overlay's own animation (lava, acid, rain or spores) is not among them: it is the overlay's, which steps it
/// with the effect's picture under the effect's own freeze (<see cref="RoomFxOverlayState.Step"/>).
/// </remarks>
public sealed partial class RoomBlockAnimations
{
    /// <summary>The loops the room effect lists, in the effect's order.</summary>
    internal readonly List<AnimationPlayer<BlockAnimation, ArtworkFrame, NoAnimationCue>> effectAnimations = [];

    /// <summary>
    /// The loops still waiting for the area's boss to be defeated before their first frame: each starts on the first pass
    /// on which the boss is recorded defeated, and the room never tests the boss for it again.
    /// </summary>
    private readonly HashSet<AnimationPlayer<BlockAnimation, ArtworkFrame, NoAnimationCue>> awaitingAreaBoss = [];

    /// <summary>The treadmill a Wrecked Ship entrance door's setup starts, once it has; it plays after the effect's animations.</summary>
    private AnimationPlayer<BlockAnimation, ArtworkFrame, NoAnimationCue>? doorTreadmill;

    /// <summary>Starts one room's animations from the door-selected room effect, in the effect's order; none when the room has no effect.</summary>
    /// <param name="fx">The room state's FX definition.</param>
    /// <param name="door">The entry door, which selects the room effect.</param>
    /// <exception cref="InvalidOperationException">The effect lists an animation with no loop.</exception>
    public RoomBlockAnimations(RoomFxSelection? fx, DoorDefinition? door)
    {
        if (fx?.Select(door) is not { } effect)
            return;
        foreach (BlockAnimation animation in effect.BlockAnimations)
            effectAnimations.Add(Start(animation));
    }

    /// <summary>
    /// Starts the Wrecked Ship entrance treadmill for a belt pushing <paramref name="direction"/>, as the entrance door's
    /// setup does; it waits for Phantoon's defeat and afterwards loops its treadmill frames.
    /// </summary>
    /// <exception cref="InvalidOperationException">A door's setup has already started the treadmill.</exception>
    public void StartDoorTreadmill(WreckedShipTreadmillDirection direction)
    {
        if (doorTreadmill is not null)
            throw new InvalidOperationException($"A door's setup already started the {doorTreadmill.Playback.PlayingSegment} treadmill.");
        doorTreadmill = Start(BlockAnimationLoops.Treadmill(direction));
    }

    /// <summary>
    /// Runs one pass of every animation, the effect's in its order and then the door's treadmill, each selecting its frame
    /// for the following frame boundary; an animation that waits for the area's boss starts on the first pass
    /// <paramref name="areaBossDefeated"/> holds and shows its first frame on that pass.
    /// </summary>
    public void Step(bool areaBossDefeated, ArtworkFrameSelections frames)
    {
        Ensure.NotNull(frames);
        foreach (AnimationPlayer<BlockAnimation, ArtworkFrame, NoAnimationCue> animation in effectAnimations)
            Pass(animation, areaBossDefeated, frames);
        if (doorTreadmill is not null)
            Pass(doorTreadmill, areaBossDefeated, frames);
    }

    /// <summary>Starts the loop of <paramref name="animation"/>, waiting for the area's boss when its loop does.</summary>
    private AnimationPlayer<BlockAnimation, ArtworkFrame, NoAnimationCue> Start(BlockAnimation animation)
    {
        AnimationPlayer<BlockAnimation, ArtworkFrame, NoAnimationCue> loop = BlockAnimationLoops.Start(animation);
        if (BlockAnimationLoops.WaitsForAreaBoss(animation))
            awaitingAreaBoss.Add(loop);
        return loop;
    }

    /// <summary>Runs one pass of <paramref name="loop"/>, which does nothing while it waits for a boss not yet defeated.</summary>
    private void Pass(AnimationPlayer<BlockAnimation, ArtworkFrame, NoAnimationCue> loop, bool areaBossDefeated, ArtworkFrameSelections frames)
    {
        if (awaitingAreaBoss.Contains(loop))
        {
            if (!areaBossDefeated)
                return;
            awaitingAreaBoss.Remove(loop);
        }
        BlockAnimationLoops.Pass(loop, frames);
    }
}
