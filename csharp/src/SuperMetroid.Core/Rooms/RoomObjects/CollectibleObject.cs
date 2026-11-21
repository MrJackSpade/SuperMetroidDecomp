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
/// <param name="terrain">The terrain the item writes its looks into: a tank's own blocks, or the block pair an item that shows its own picture took.</param>
/// <param name="phase">The phase the item's setup leaves it in.</param>
/// <param name="location">Where the item lies in the game's item record, as its placement named it.</param>
/// <param name="savedTerrain">The block the item's setup left at its place, which a shot block's return restores.</param>
internal sealed partial class CollectibleObject(
    InWorldCollectibleKind kind,
    CollectiblePresentation presentation,
    CollectibleTerrain terrain,
    CollectiblePhase phase,
    ItemLocation location,
    RoomTerrainBlock savedTerrain) : RoomObjectBehaviour
{
    /// <summary>
    /// The block the item's setup left at its place, its collision made the item's own (#2484): a concealing shot block
    /// draws it back as it hides the item again, and a collected item's shot block as it respawns.
    /// </summary>
    internal RoomTerrainBlock SavedTerrain { get; } = savedTerrain;

    /// <summary>Where the item lies in the game's item record, which its collection marks collected.</summary>
    internal ItemLocation Location { get; } = Ensure.NotNull(location);

    /// <summary>Inventory or capacity item granted when this item is collected.</summary>
    public InWorldCollectibleKind Kind { get; } = kind;
    /// <summary>Exposed, Chozo orb, or shot-block reveal behavior.</summary>
    public CollectiblePresentation Presentation { get; } = presentation;
    /// <summary>
    /// The terrain the item writes its looks into (#2558): a tank's own blocks, or the block pair an item that shows its own
    /// picture took (<see cref="CollectibleBlockPairs"/>).
    /// </summary>
    internal CollectibleTerrain Terrain { get; } = Ensure.NotNull(terrain);
    /// <summary>Current reveal, visible, message, empty, or respawn phase.</summary>
    public CollectiblePhase Phase { get; private set; } = phase;
    /// <summary>
    /// What the item shows playing: an exposed item's looks and a Chozo orb's shell from the item's first update, which
    /// shows their first image; the other phases' images from when each starts.
    /// </summary>
    private readonly AnimationPlayer<CollectibleSequence, CollectibleImage, CollectiblePhase> _shown =
        AnimationPlayer<CollectibleSequence, CollectibleImage, CollectiblePhase>.Starting(CollectibleSequenceAnimations.Segments,
            phase == CollectiblePhase.ChozoOrb ? CollectibleSequenceAnimations.OrbPulsing : CollectibleSequenceAnimations.Exposed);
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

            case CollectiblePhase.VisibleTouched:
            case CollectiblePhase.ShotBlockVisibleTouched:
                context.RoomObjects.CollectibleObjects.Acquire(this, place, context.Outcome);
                return;

            case CollectiblePhase.Visible:
            case CollectiblePhase.ShotBlockVisible:
            case CollectiblePhase.ChozoOrb:
                _ = ShowsOn(place, context);
                return;

            case CollectiblePhase.ChozoOrbShot:
                // Breaking the shell persists nothing: an orb broken on an earlier visit is whole again.
                Phase = CollectiblePhase.ChozoOrbBurst;
                Show(CollectibleSequenceAnimations.OrbBurst, place, context);
                return;

            case CollectiblePhase.ChozoOrbBurst:
                // Nothing reaches a bursting orb, so the item it exposes waits for a touch.
                if (!ShowsOn(place, context))
                    return;
                Phase = CollectiblePhase.Visible;
                Show(CollectibleSequenceAnimations.Exposed, place, context);
                return;

            case CollectiblePhase.ShotBlock:
            case CollectiblePhase.CollectedShotBlock:
                return;

            case CollectiblePhase.ShotBlockShot:
            case CollectiblePhase.CollectedShotBlockShot:
                Phase = Phase == CollectiblePhase.CollectedShotBlockShot ? CollectiblePhase.CollectedShotBlockReveal : CollectiblePhase.ShotBlockReveal;
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
                // Nothing reaches a crumbling block, so the item it reveals waits for a touch.
                if (!ShowsOn(place, context))
                    return;
                Phase = CollectiblePhase.ShotBlockVisible;
                Show(CollectibleSequenceAnimations.Revealed, place, context);
                return;

            case CollectiblePhase.ShotBlockReconceal:
                if (!ShowsOn(place, context))
                    return;
                // The shot block shows whole as its saved block returns over it on the same update.
                context.Draw(place, RoomObjectCollectibleDrawDefinitions.ShotBlockIntact);
                context.DrawSavedTerrain(place, SavedTerrain);
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
                context.DrawSavedTerrain(place, SavedTerrain);
                Phase = CollectiblePhase.CollectedShotBlock;
                return;

            default:
                throw new InvalidDataException($"Collectible {Kind} reached unsupported phase {Phase}.");
        }
    }

    /// <summary>
    /// A touch or a shot reached the item in a phase that takes it, which the item reacts to on its next update (#2545):
    /// a touched item is collected, a shot orb bursts and a shot block crumbles. Reaching it again before then changes
    /// nothing.
    /// </summary>
    /// <exception cref="InvalidOperationException">The item's phase takes no touch or shot.</exception>
    internal void Trigger() => Phase = Phase switch
    {
        CollectiblePhase.Visible or CollectiblePhase.VisibleTouched => CollectiblePhase.VisibleTouched,
        CollectiblePhase.ShotBlockVisible or CollectiblePhase.ShotBlockVisibleTouched => CollectiblePhase.ShotBlockVisibleTouched,
        CollectiblePhase.ChozoOrb or CollectiblePhase.ChozoOrbShot => CollectiblePhase.ChozoOrbShot,
        CollectiblePhase.ShotBlock or CollectiblePhase.ShotBlockShot => CollectiblePhase.ShotBlockShot,
        CollectiblePhase.CollectedShotBlock or CollectiblePhase.CollectedShotBlockShot => CollectiblePhase.CollectedShotBlockShot,
        CollectiblePhase.CollectedEmpty or CollectiblePhase.ChozoOrbBurst or CollectiblePhase.ShotBlockReveal or
            CollectiblePhase.CollectedShotBlockReveal or CollectiblePhase.ShotBlockReconceal or
            CollectiblePhase.CollectedShotBlockEmpty or CollectiblePhase.CollectedShotBlockRespawn or
            CollectiblePhase.AwaitingMessage or CollectiblePhase.ResumeAfterMessage or CollectiblePhase.EmptyAwaitingDelete =>
            throw new InvalidOperationException($"Collectible {Kind} in {Phase} takes no touch or shot."),
        _ => throw new InvalidOperationException($"Undefined {nameof(CollectiblePhase)} {Phase}."),
    };

    /// <summary>
    /// The item has been collected: it keeps its block and its place while its pickup message shows, and finishes its
    /// pickup once the message closes.
    /// </summary>
    internal void AwaitMessage() => Phase = CollectiblePhase.AwaitingMessage;

    /// <summary>The item's pickup message has closed: the item finishes its pickup within the same room-object update.</summary>
    internal void ResumeAfterMessage(IRoomObjectDrawing drawing, RoomLevelData level, RoomObjectPlace place)
    {
        Phase = CollectiblePhase.ResumeAfterMessage;
        FinishPickup(drawing, level, place);
    }

    /// <summary>
    /// The pickup message has closed: the item shows its empty block for one update and then leaves, or a shot block's
    /// item leaves its block empty for <see cref="CollectibleTimingDefinitions.CollectedShotBlockEmptyUpdates"/> before it respawns.
    /// </summary>
    private void FinishPickup(IRoomObjectDrawing drawing, RoomLevelData level, RoomObjectPlace place)
    {
        Ensure.NotNull(drawing);
        Ensure.NotNull(place);
        if (Presentation == CollectiblePresentation.ShotBlock)
        {
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
        switch (image.What)
        {
            case CollectibleImage.DrawnImage drawn:
                drawn.Draw.Draw(level, place.Block, drawing);
                return;
            case CollectibleImage.VisibleLook { Look: var look }:
                Terrain.VisibleFrame(Kind, look).Draw(level, place.Block, drawing);
                if (Terrain.ShowsOwnPicture)
                    level.ShowCollectible(place.Block, new CollectiblePicture(Kind, look));
                return;
            default:
                throw new InvalidOperationException($"Undefined collectible image {image.What}.");
        }
    }
}
