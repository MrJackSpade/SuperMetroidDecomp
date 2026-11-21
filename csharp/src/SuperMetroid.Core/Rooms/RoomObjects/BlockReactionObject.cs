using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A block reacting to Samus or her weapons (#1554): a crumble, a shot, Super Missile, Power Bomb, bomb or Grapple block
/// breaking up and perhaps growing back, a Speed Booster block crumbling, or a bombed block's reveal. It waits its
/// reaction's first delay, then shows each image in turn for that image's updates, requesting the image's sound as it
/// shows and restoring the terrain its setup saved where an image asks for it, and leaves the room on the update after its
/// last image. Each image it shows tells the room's block reactions whether its block now stands broken open. The
/// reaction plays on the shared animation player (#2480).
/// </summary>
internal class BlockReactionObject : RoomObjectBehaviour
{
    /// <summary>
    /// A reaction of <paramref name="change"/>'s timing whose setup saved <paramref name="savedTerrain"/>, which its images
    /// restore where they ask for it (#2484). A reaction whose setup saves none is a <see cref="LastingBlockReactionObject"/>.
    /// </summary>
    internal BlockReactionObject(BlockReactionChange change, RoomTerrainBlock savedTerrain)
    {
        Change = Ensure.NotNull(change);
        SavedTerrain = savedTerrain;
        _images = Playing(change);
    }

    /// <summary>The reaction's authored timing.</summary>
    internal BlockReactionChange Change { get; }

    /// <summary>
    /// The terrain the reaction's setup saved at its block, which it restores as the block grows back. Grapple setup saves
    /// the earlier block; collision-bomb setup saves the bomb block's parent instead.
    /// </summary>
    private RoomTerrainBlock SavedTerrain { get; }

    /// <summary>
    /// The reaction's images playing, each for its own updates: its first shows on the update its first delay runs out
    /// on, and it ends on the update after its last image's updates run out.
    /// </summary>
    private readonly AnimationPlayer<RoomObjectFrameRun, BlockReactionFrame, NoAnimationCue> _images;

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
        // An image is a draw at the block or the block's saved terrain restored.
        if (frame.Image is BlockReactionImage.Drawn drawn)
            context.Draw(place, drawn.Draw);
        else
            RestoreBlock(place, context, SavedTerrain);
        // The owner of the room's block reactions learns as each image shows whether the block now stands broken open,
        // which a room object waiting on a block's breakup observes.
        context.RoomObjects.BlockReactions.RecordImageShown(place.Block, frame.Image);
    }

    /// <summary>Restores the terrain the reaction's setup saved where an image asks for it.</summary>
    /// <param name="place">The reaction's place.</param>
    /// <param name="context">The update.</param>
    /// <param name="saved">The terrain the reaction's setup saved.</param>
    private protected virtual void RestoreBlock(RoomObjectPlace place, RoomObjectStepContext context, RoomTerrainBlock saved) =>
        context.DrawSavedTerrain(place, saved);

    /// <summary><paramref name="change"/>'s images, played from its first delay.</summary>
    private static AnimationPlayer<RoomObjectFrameRun, BlockReactionFrame, NoAnimationCue> Playing(BlockReactionChange change)
    {
        AnimationSegments<RoomObjectFrameRun, BlockReactionFrame, NoAnimationCue> images =
            RoomObjectFrameRuns.Of(Ensure.NotNull(change).Frames, image => image.Updates);
        return AnimationPlayer<RoomObjectFrameRun, BlockReactionFrame, NoAnimationCue>.StartingAfter(
            images, RoomObjectFrameRuns.Once(images), change.FirstFrameDelay);
    }
}
