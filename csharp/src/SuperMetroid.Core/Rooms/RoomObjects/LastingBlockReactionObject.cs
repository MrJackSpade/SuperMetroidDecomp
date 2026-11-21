using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A block reaction whose setup saved no terrain (#1554, #2554): a permanent shot block breaking up, a bombed block's reveal,
/// or a reaction whose setup made the whole change. It waits its reaction's first delay, then draws each image in turn for
/// that image's updates, requesting the image's sound as it shows, and leaves the room on the update after its last image.
/// Each image it shows tells the room's block reactions whether its block now stands broken open. The reaction plays on
/// the shared animation player (#2480).
/// </summary>
/// <param name="change">The reaction's authored timing.</param>
internal sealed class LastingBlockReactionObject(LastingBlockReactionChange change) : RoomObjectBehaviour
{
    /// <summary>The reaction's authored timing.</summary>
    internal LastingBlockReactionChange Change { get; } = Ensure.NotNull(change);

    /// <summary>
    /// The reaction's images playing, each for its own updates: its first shows on the update its first delay runs out
    /// on, and it ends on the update after its last image's updates run out.
    /// </summary>
    private readonly AnimationPlayer<RoomObjectFrameRun, LastingBlockReactionFrame, NoAnimationCue> _images = Playing(change);

    /// <inheritdoc/>
    internal override void Update(RoomObjectPlace place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        if (_images.StepToNewFrame() is not { } frame)
        {
            if (_images.HasStopped)
                context.RoomObjects.DeleteObject(place);
            return;
        }
        if (frame.SoundBefore is { } sound)
            context.QueueSound(sound, frame.SoundPriority);
        context.Draw(place, frame.Draw);
        // The owner of the room's block reactions learns as each image shows whether the block now stands broken open,
        // which a room object waiting on a block's breakup observes.
        context.RoomObjects.BlockReactions.RecordImageShown(place.Block, frame.Image);
    }

    /// <summary><paramref name="change"/>'s images, played from its first delay.</summary>
    private static AnimationPlayer<RoomObjectFrameRun, LastingBlockReactionFrame, NoAnimationCue> Playing(LastingBlockReactionChange change)
    {
        AnimationSegments<RoomObjectFrameRun, LastingBlockReactionFrame, NoAnimationCue> images =
            RoomObjectFrameRuns.Of(Ensure.NotNull(change).Frames, image => image.Updates);
        return AnimationPlayer<RoomObjectFrameRun, LastingBlockReactionFrame, NoAnimationCue>.StartingAfter(
            images, RoomObjectFrameRuns.Once(images), change.FirstFrameDelay);
    }
}
