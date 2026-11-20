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
    /// <summary>A reaction of <paramref name="change"/>'s timing whose setup saved no terrain, so none of its images restores a block.</summary>
    internal BlockReactionObject(BlockReactionChange change)
    {
        Change = Ensure.NotNull(change);
        _images = Playing(change);
    }

    /// <summary>
    /// A reaction of <paramref name="change"/>'s timing whose setup saved <paramref name="savedTerrain"/>, which its images
    /// restore where they ask for it (#2484).
    /// </summary>
    internal BlockReactionObject(BlockReactionChange change, RoomTerrainBlock savedTerrain)
        : this(change) => SavedTerrain = savedTerrain;

    /// <summary>The reaction's authored timing.</summary>
    internal BlockReactionChange Change { get; }

    /// <summary>
    /// The terrain the reaction's setup saved at its block, which it restores as the block grows back; null when its setup
    /// saved none. Grapple setup saves the earlier block; collision-bomb setup saves the bomb block's parent instead.
    /// </summary>
    private RoomTerrainBlock? SavedTerrain { get; }

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

    /// <summary>Restores the terrain the reaction's setup saved where an image asks for it.</summary>
    /// <param name="place">The reaction's place.</param>
    /// <param name="context">The update.</param>
    /// <exception cref="InvalidOperationException">The reaction's setup saved no terrain.</exception>
    private protected virtual void RestoreBlock(RoomObjectPlace place, RoomObjectStepContext context) =>
        context.DrawSavedTerrain(place, SavedTerrain
            ?? throw new InvalidOperationException($"The block reaction at block {place.Block} restores a block its setup never saved."));

    /// <summary><paramref name="change"/>'s images, played from its first delay.</summary>
    private static AnimationPlayer<RoomObjectFrameRun, BlockReactionFrame, NoAnimationCue> Playing(BlockReactionChange change)
    {
        AnimationSegments<RoomObjectFrameRun, BlockReactionFrame, NoAnimationCue> images =
            RoomObjectFrameRuns.Of(Ensure.NotNull(change).Frames, image => image.Updates);
        return AnimationPlayer<RoomObjectFrameRun, BlockReactionFrame, NoAnimationCue>.StartingAfter(
            images, RoomObjectFrameRuns.Once(images), change.FirstFrameDelay);
    }
}
