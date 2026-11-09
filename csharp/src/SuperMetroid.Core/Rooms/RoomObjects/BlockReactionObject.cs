namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A block reacting to Samus or her weapons (#1554): a crumble, a shot, Super Missile, Power Bomb, bomb or Grapple block
/// breaking up and perhaps growing back, a Speed Booster block crumbling, or a bombed block's reveal. It waits its
/// reaction's first delay, then shows each image in turn for that image's updates, requesting the image's sound as it
/// shows and restoring the block's saved terrain where an image asks for it, and leaves the room on the update after its
/// last image. Each image it shows tells the room's block reactions whether its block now stands broken open.
/// </summary>
/// <param name="change">The reaction's authored timing.</param>
internal class BlockReactionObject(BlockReactionChange change) : RoomObjectBehaviour
{
    /// <summary>The reaction's authored timing.</summary>
    internal BlockReactionChange Change { get; } = Ensure.NotNull(change);

    /// <summary>How many of the reaction's images have shown.</summary>
    private int _framesShown;

    /// <summary>The updates left before the next image, or before the reaction ends after its last image.</summary>
    private int _updatesBeforeNextStep = Ensure.BetweenInclusive(1, int.MaxValue, change.FirstFrameDelay);

    /// <inheritdoc/>
    internal sealed override void Update(RoomObjectSlot place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        _updatesBeforeNextStep--;
        if (_updatesBeforeNextStep != 0)
            return;
        if (_framesShown == Change.Frames.Count)
        {
            context.RoomObjects.DeleteObject(place);
            return;
        }
        BlockReactionFrame frame = Change.Frames[_framesShown];
        _framesShown++;
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
        context.RoomObjects.BlockReactions.RecordImageShown(place.BlockIndex, frame.Draw);
        _updatesBeforeNextStep = frame.Updates;
    }

    /// <summary>Restores the block's saved terrain where an image asks for it.</summary>
    /// <param name="place">The reaction's place.</param>
    /// <param name="context">The update.</param>
    private protected virtual void RestoreBlock(RoomObjectSlot place, RoomObjectStepContext context) => context.DrawRestoredBlock(place);
}
