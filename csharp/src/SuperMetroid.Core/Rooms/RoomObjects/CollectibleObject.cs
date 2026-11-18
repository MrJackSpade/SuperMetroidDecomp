using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// An item or tank lying in the room (#1554). It holds its kind,
/// its presentation (exposed, in a Chozo orb or in a shot block) and its phase as domain values, and shows what its phase
/// shows on the shared animation player (<see cref="CollectibleSequenceAnimations"/>, #2480): an exposed item alternates
/// its two looks until Samus touches it; a Chozo orb cycles its shell until a shot breaks it; a shot block reveals its
/// item for a while and then conceals it again. A touch is collected on the next update, ahead of the image the item
/// shows; the item then waits for its pickup message and, once the message closes, shows its empty block and leaves, or
/// leaves its shot block empty and respawns it.
/// </summary>
/// <param name="kind">The item or tank.</param>
/// <param name="presentation">How the item lies: exposed, in a Chozo orb or in a shot block.</param>
/// <param name="blockPair">The terrain block pair an item that shows its own picture draws into; null for a tank.</param>
/// <param name="phase">The phase the item's setup leaves it in.</param>
/// <param name="location">Where the item lies in the game's item record, as its placement named it.</param>
internal sealed class CollectibleObject(
    InWorldCollectibleKind kind,
    CollectiblePresentation presentation,
    CollectibleBlockPairs.Pair? blockPair,
    CollectiblePhase phase,
    ItemLocation location) : RoomObjectBehaviour
{
    /// <summary>Where the item lies in the game's item record, which its collection marks collected.</summary>
    internal ItemLocation Location { get; } = Ensure.NotNull(location);

    /// <summary>Inventory or capacity item granted when this item is collected.</summary>
    public InWorldCollectibleKind Kind { get; } = kind;
    /// <summary>Exposed, Chozo orb, or shot-block reveal behavior.</summary>
    public CollectiblePresentation Presentation { get; } = presentation;
    /// <summary>
    /// The terrain block pair an item that shows its own picture writes its frames into (<see cref="CollectibleBlockPairs"/>);
    /// null for a tank, whose frames are its kind's own blocks.
    /// </summary>
    public CollectibleBlockPairs.Pair? BlockPair { get; } = blockPair;
    /// <summary>Current reveal, visible, message, empty, or respawn phase.</summary>
    public CollectiblePhase Phase { get; private set; } = phase;
    /// <summary>
    /// What the item shows playing: an exposed item's looks and a Chozo orb's shell from the item's first update, which
    /// shows their first image; the other phases' images from when each starts.
    /// </summary>
    private readonly AnimationPlayer<CollectibleSequence, CollectibleImage, CollectiblePhase> _shown =
        AnimationPlayer<CollectibleSequence, CollectibleImage, CollectiblePhase>.Starting(CollectibleSequenceAnimations.Segments,
            phase == CollectiblePhase.ChozoOrb ? CollectibleSequenceAnimations.OrbPulsing : CollectibleSequenceAnimations.Exposed);
    /// <summary>
    /// Contact or projectile reaction pending for the item's next update: a touch or a shot that reached it, which the item
    /// takes on that update whatever it is doing, so it is a fact of its own beside its phase.
    /// </summary>
    public bool Triggered { get; private set; }

    /// <inheritdoc/>
    internal override void Update(RoomObjectPlace place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        // Collision runs before the room-object update. The item trigger its setup wrote, for touch and shot, therefore
        // pre-empts the image the item shows on this same update; otherwise touching an item could leave Samus embedded
        // in its solid block for up to four extra updates.
        switch (Phase)
        {
            case CollectiblePhase.AwaitingMessage:
                return;

            case CollectiblePhase.ResumeAfterMessage:
                FinishPickup(context.RoomObjects.Drawing, context.Level, place);
                return;

            case CollectiblePhase.EmptyAwaitingDelete:
                // The empty item ends by leaving the room.
                if (ShowsOn(place, context))
                    place.Delete();
                return;

            case CollectiblePhase.CollectedEmpty:
                context.Draw(place, RoomObjectCollectibleDrawDefinitions.Empty);
                place.Delete();
                return;

            case CollectiblePhase.Visible:
            case CollectiblePhase.ShotBlockVisible:
                if (Triggered)
                {
                    context.RoomObjects.CollectibleObjects.Acquire(this, place);
                    return;
                }
                _ = ShowsOn(place, context);
                return;

            case CollectiblePhase.ChozoOrb:
                if (Triggered)
                {
                    // Breaking the shell persists nothing: an orb broken on an earlier visit is whole again.
                    Triggered = false;
                    Phase = CollectiblePhase.ChozoOrbBurst;
                    Show(CollectibleSequenceAnimations.OrbBurst, place, context);
                    return;
                }
                _ = ShowsOn(place, context);
                return;

            case CollectiblePhase.ChozoOrbBurst:
                if (!ShowsOn(place, context))
                    return;
                Phase = CollectiblePhase.Visible;
                if (Triggered)
                {
                    context.RoomObjects.CollectibleObjects.Acquire(this, place);
                    return;
                }
                Show(CollectibleSequenceAnimations.Exposed, place, context);
                return;

            case CollectiblePhase.ShotBlock:
            case CollectiblePhase.CollectedShotBlock:
                if (!Triggered)
                    return;
                Triggered = false;
                Phase = Phase == CollectiblePhase.CollectedShotBlock ? CollectiblePhase.CollectedShotBlockReveal : CollectiblePhase.ShotBlockReveal;
                Show(CollectibleSequenceAnimations.ShotBlockReveal, place, context);
                context.QueueSound(EnemySound.BlockCrumble, SoundRequestPriority.Normal);
                return;

            case CollectiblePhase.CollectedShotBlockReveal:
                if (!ShowsOn(place, context))
                    return;
                Phase = CollectiblePhase.CollectedShotBlockEmpty;
                Show(CollectibleSequenceAnimations.CollectedShotBlockEmpty, place, context);
                return;

            case CollectiblePhase.ShotBlockReveal:
                if (!ShowsOn(place, context))
                    return;
                Phase = CollectiblePhase.ShotBlockVisible;
                if (Triggered)
                {
                    context.RoomObjects.CollectibleObjects.Acquire(this, place);
                    return;
                }
                Show(CollectibleSequenceAnimations.Revealed, place, context);
                return;

            case CollectiblePhase.ShotBlockReconceal:
                if (!ShowsOn(place, context))
                    return;
                // The shot block shows whole as its saved block returns over it on the same update.
                context.Draw(place, RoomObjectCollectibleDrawDefinitions.ShotBlockIntact);
                context.DrawRestoredBlock(place);
                Phase = CollectiblePhase.ShotBlock;
                return;

            case CollectiblePhase.CollectedShotBlockEmpty:
                if (!ShowsOn(place, context))
                    return;
                Phase = CollectiblePhase.CollectedShotBlockRespawn;
                Show(CollectibleSequenceAnimations.ShotBlockRespawn, place, context);
                return;

            case CollectiblePhase.CollectedShotBlockRespawn:
                if (!ShowsOn(place, context))
                    return;
                context.DrawRestoredBlock(place);
                Phase = CollectiblePhase.CollectedShotBlock;
                return;

            default:
                throw new InvalidDataException($"Collectible {Kind} reached unsupported phase {Phase}.");
        }
    }

    /// <summary>A touch or a shot reached the item, which reacts on its next update.</summary>
    internal void Trigger() => Triggered = true;

    /// <summary>
    /// The item has been collected: it keeps its block and its place while its pickup message shows, and finishes its
    /// pickup once the message closes.
    /// </summary>
    internal void AwaitMessage()
    {
        Triggered = false;
        Phase = CollectiblePhase.AwaitingMessage;
    }

    /// <summary>The item's pickup message has closed: the item finishes its pickup within the same room-object update.</summary>
    internal void ResumeAfterMessage(IRoomObjectDrawing drawing, RoomLevelData level, RoomObjectPlace place)
    {
        Phase = CollectiblePhase.ResumeAfterMessage;
        FinishPickup(drawing, level, place);
    }

    /// <summary>
    /// The pickup message has closed: the item shows its empty block for one update and then leaves, or a shot block's
    /// item leaves its block empty for twenty-two eight-update intervals before it respawns.
    /// </summary>
    private void FinishPickup(IRoomObjectDrawing drawing, RoomLevelData level, RoomObjectPlace place)
    {
        Ensure.NotNull(drawing);
        Ensure.NotNull(place);
        if (Presentation == CollectiblePresentation.ShotBlock)
        {
            Triggered = false;
            Phase = CollectiblePhase.CollectedShotBlockEmpty;
            Show(CollectibleSequenceAnimations.CollectedShotBlockEmpty, drawing, level, place);
            return;
        }

        // The empty item draws for one update; it leaves on the following update.
        Phase = CollectiblePhase.EmptyAwaitingDelete;
        Show(CollectibleSequenceAnimations.PickedUpEmpty, drawing, level, place);
    }

    /// <summary>
    /// Plays what the item shows one update on, drawing the image the update shows anew; a revealed item's last look marks
    /// the shot block starting to form again over it.
    /// </summary>
    /// <returns>Whether what the item shows is over: it played its last image's time out on this update.</returns>
    private bool ShowsOn(RoomObjectPlace place, RoomObjectStepContext context)
    {
        if (_shown.StepToNewFrame(reached => Phase = reached) is { } image)
        {
            Draw(image, context.RoomObjects.Drawing, context.Level, place);
            return false;
        }
        return _shown.HasStopped;
    }

    /// <summary>Starts <paramref name="animation"/> and draws its first image, which shows on this update.</summary>
    private void Show(Animation<CollectibleSequence> animation, RoomObjectPlace place, RoomObjectStepContext context) =>
        Show(animation, context.RoomObjects.Drawing, context.Level, place);

    /// <summary>Starts <paramref name="animation"/> and draws its first image, which shows on this update.</summary>
    private void Show(Animation<CollectibleSequence> animation, IRoomObjectDrawing drawing, RoomLevelData level, RoomObjectPlace place)
    {
        _shown.Show(animation);
        Draw(_shown.Showing, drawing, level, place);
    }

    /// <summary>
    /// Draws <paramref name="image"/>: its draw, or the item's visible look, the terrain its kind and block pair write and,
    /// over it, for an item that shows its own picture, its kind's picture of that look (#1538).
    /// </summary>
    private void Draw(CollectibleImage image, IRoomObjectDrawing drawing, RoomLevelData level, RoomObjectPlace place)
    {
        if (image.Look is not { } look)
        {
            (image.Draw ?? throw new InvalidOperationException("A collectible image draws nothing.")).Draw(level, place.Block, drawing);
            return;
        }
        CollectibleObjectDraws.VisibleFrame(Kind, look, BlockPair).Draw(level, place.Block, drawing);
        if (BlockPair is not null)
            level.ShowCollectible(place.Block, new CollectiblePicture(Kind, look));
    }
}
