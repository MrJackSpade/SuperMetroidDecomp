namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A room object that changes the terrain at its block on an authored cadence and is then gone (#1554): a boss room's
/// crumbling ceiling, wall, platform or spikes, a bridge or wall clear, a row of Mother Brain's mural. It waits its
/// change's first delay, then draws each frame in turn for that frame's updates, moving its origin and requesting the
/// frame's sound as it draws, and leaves the room on the update after its last frame. Each draw lands on the same
/// update, and the object leaves on the same update, as the original game's timed draws and its delete did.
/// </summary>
/// <param name="change">The change's authored timing.</param>
internal class TerrainChangeObject(TerrainChange change) : RoomObjectBehaviour
{
    /// <summary>The change's authored timing.</summary>
    internal TerrainChange Change { get; } = Ensure.NotNull(change);

    /// <summary>How many of the change's frames have drawn.</summary>
    private int _framesDrawn;

    /// <summary>The updates left before the next frame draws, or before the change ends after its last frame.</summary>
    private int _updatesBeforeNextStep = Ensure.BetweenInclusive(1, int.MaxValue, change.FirstFrameDelay);

    /// <inheritdoc/>
    internal sealed override void Update(RoomObjectSlot place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        _updatesBeforeNextStep--;
        if (_updatesBeforeNextStep != 0)
            return;
        if (_framesDrawn == Change.Frames.Count)
        {
            context.RoomObjects.DeleteObject(place);
            return;
        }
        if (_framesDrawn == 0)
            Begin(context);
        TerrainChangeFrame frame = Change.Frames[_framesDrawn];
        _framesDrawn++;
        if (frame.MoveBefore != Offset16<RoomBlocks>.Zero)
            context.MoveOrigin(place, frame.MoveBefore.X, frame.MoveBefore.Y);
        if (frame.SoundBefore is { } sound)
            context.QueueSound(sound, SoundRequestPriority.Normal);
        context.Draw(place, frame.Draw);
        _updatesBeforeNextStep = frame.Updates;
    }

    /// <summary>Runs as the change draws its first frame, before that frame's move, sound and draw; nothing by default.</summary>
    /// <param name="context">The update.</param>
    private protected virtual void Begin(RoomObjectStepContext context)
    {
    }
}
