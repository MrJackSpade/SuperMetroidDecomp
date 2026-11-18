using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A block reacting to Samus or her weapons (#1554): a crumble, a shot, Super Missile, Power Bomb, bomb or Grapple block
/// breaking up and perhaps growing back, a Speed Booster block crumbling, or a bombed block's reveal. It waits its
/// reaction's first delay, then shows each image in turn for that image's updates, requesting the image's sound as it
/// shows and restoring the block's saved terrain where an image asks for it, and leaves the room on the update after its
/// last image. Each image it shows tells the room's block reactions whether its block now stands broken open. The
/// reaction plays on the shared animation player (#2480).
/// </summary>
/// <param name="change">The reaction's authored timing.</param>
internal class BlockReactionObject(BlockReactionChange change) : RoomObjectBehaviour
{
    /// <summary>The reaction's authored timing.</summary>
    internal BlockReactionChange Change { get; } = Ensure.NotNull(change);

    /// <summary>
    /// The reaction's images playing, each for its own updates: its first shows on the update its first delay runs out
    /// on, and it ends on the update after its last image's updates run out.
    /// </summary>
    private readonly AnimationPlayer<RoomObjectFrameRun, BlockReactionFrame, NoAnimationCue> _images = Playing(change);

    /// <inheritdoc/>
    internal sealed override void Update(RoomObjectPlace place, RoomObjectStepContext context)
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
        if (frame.Draw is { } draw)
        {
            context.Draw(place, draw);
        }
        else
        {
            RestoreBlock(place, context);
        }
        // The owner of the room's block reactions learns as each image shows whether the block now stands broken open,
        // which a room object waiting on a block's breakup observes.
        context.RoomObjects.BlockReactions.RecordImageShown(place.Block, frame.Draw);
    }

    /// <summary>Restores the block's saved terrain where an image asks for it.</summary>
    /// <param name="place">The reaction's place.</param>
    /// <param name="context">The update.</param>
    private protected virtual void RestoreBlock(RoomObjectPlace place, RoomObjectStepContext context) => context.DrawRestoredBlock(place);

    /// <summary><paramref name="change"/>'s images, played from its first delay.</summary>
    private static AnimationPlayer<RoomObjectFrameRun, BlockReactionFrame, NoAnimationCue> Playing(BlockReactionChange change)
    {
        AnimationSegments<RoomObjectFrameRun, BlockReactionFrame, NoAnimationCue> images =
            RoomObjectFrameRuns.Of(Ensure.NotNull(change).Frames, image => image.Updates);
        return AnimationPlayer<RoomObjectFrameRun, BlockReactionFrame, NoAnimationCue>.StartingAfter(
            images, RoomObjectFrameRuns.Once(images), change.FirstFrameDelay);
    }
}
