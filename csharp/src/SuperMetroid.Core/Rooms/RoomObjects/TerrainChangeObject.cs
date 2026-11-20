using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A room object that changes the terrain at its block on an authored cadence and is then gone (#1554): a boss room's
/// crumbling ceiling, wall, platform or spikes, a bridge or wall clear, a row of Mother Brain's mural. It waits its
/// change's first delay, then draws each frame in turn for that frame's updates, moving its origin and requesting the
/// frame's sound as it draws, and leaves the room on the update after its last frame. Each draw lands on the same
/// update, and the object leaves on the same update, as the game's timed draws and its delete did. The change plays on
/// the shared animation player (#2480).
/// </summary>
/// <param name="change">The change's authored timing.</param>
internal class TerrainChangeObject(TerrainChange change) : RoomObjectBehaviour
{
    /// <summary>The change's frames playing, from its first delay; none shown before its first frame draws.</summary>
    private AnimationPlayer<RoomObjectFrameRun, TerrainChangeFrame, NoAnimationCue> _frames = Playing(change);

    /// <inheritdoc/>
    internal sealed override void Update(RoomObjectPlace place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        bool beforeFirstFrame = _frames.Shown is null;
        if (_frames.StepToNewFrame() is { } frame)
        {
            if (beforeFirstFrame)
                Begin(context);
            if (frame.MoveBefore != Offset16<RoomBlocks>.Zero)
                context.MoveOrigin(place, frame.MoveBefore);
            if (frame.SoundBefore is { } sound)
                context.QueueSound(sound, frame.SoundPriority);
            context.Draw(place, frame.Draw);
            return;
        }
        if (!_frames.HasStopped)
            return;
        End(context);
        context.RoomObjects.DeleteObject(place);
    }

    /// <summary>
    /// Makes <paramref name="change"/> the change the object makes, from its first delay: an object that turns into
    /// another change, such as a gate that closes again.
    /// </summary>
    private protected void Restart(TerrainChange change) => _frames = Playing(change);

    /// <summary>Runs as the change draws its first frame, before that frame's move, sound and draw; nothing by default.</summary>
    /// <param name="context">The update.</param>
    private protected virtual void Begin(RoomObjectStepContext context)
    {
    }

    /// <summary>Runs on the update the change ends, just before the object leaves the room; nothing by default.</summary>
    /// <param name="context">The update.</param>
    private protected virtual void End(RoomObjectStepContext context)
    {
    }

    /// <summary>
    /// <paramref name="change"/>'s frames, each for its own updates, its first drawing on the update its first delay runs
    /// out on and its end on the update after its last frame's updates run out.
    /// </summary>
    private static AnimationPlayer<RoomObjectFrameRun, TerrainChangeFrame, NoAnimationCue> Playing(TerrainChange change)
    {
        AnimationSegments<RoomObjectFrameRun, TerrainChangeFrame, NoAnimationCue> frames =
            RoomObjectFrameRuns.Of(Ensure.NotNull(change).Frames, frame => frame.Updates);
        return AnimationPlayer<RoomObjectFrameRun, TerrainChangeFrame, NoAnimationCue>.StartingAfter(
            frames, RoomObjectFrameRuns.Once(frames), change.FirstFrameDelay);
    }
}
