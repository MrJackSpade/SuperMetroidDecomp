using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The room's in-world collectibles' service (#1554): item and tank setup, the touches and shots that trigger them, the
/// pickup with its message, fanfare and inventory, and each collectible's picture where it lies. Each item is a typed
/// room object (<see cref="CollectibleObject"/>) that holds and advances its own state.
/// </summary>
[RoomLifetime]
internal sealed partial class CollectibleObjects
{

    /// <summary>The room's system and event state.</summary>
    private readonly SharedGameState _system;
    /// <summary>Samus as the room reads and changes her.</summary>
    private readonly IRoomSamusAccess _samus;
    /// <summary>The room's effect overlay FX.</summary>
    private readonly IRoomLiquid _roomFx;
    /// <summary>Updates level blocks and queues their draws.</summary>
    private readonly IRoomObjectDrawing _drawing;
    /// <summary>The room's places, searched for the item a touch or a shot reaches and for the items the room holds.</summary>
    private readonly RoomObjectPlaceList _places;
    /// <summary>
    /// Connects the collectibles to room state and drawing, and to the room's block pairs, which its collectibles that
    /// show their own picture take in turn.
    /// </summary>
    internal CollectibleObjects(SharedGameState system, IRoomSamusAccess samus, IRoomLiquid roomFx, IRoomObjectDrawing drawing, RoomObjectPlaceList places,
        CollectibleBlockPairs blockPairs, RoomObjectSoundQueue sounds)
    {
        _sounds = Ensure.NotNull(sounds);
        _system = system;
        _samus = samus;
        _roomFx = roomFx;
        _drawing = drawing;
        _places = places;
        _blockPairs = Ensure.NotNull(blockPairs);
    }

    /// <summary>The terrain block pairs the room's collectibles that show their own picture take in turn.</summary>
    private readonly CollectibleBlockPairs _blockPairs;
    /// <summary>The room objects' audio requests, which a pickup's fanfare joins.</summary>
    private readonly RoomObjectSoundQueue _sounds;
    /// <summary>
    /// The items collected whose pickup message is still open, each at its place, in the order they were collected: the
    /// room-object update's order, the order of their places.
    /// </summary>
    private readonly List<AwaitingPickup> _awaitingMessage = [];

    /// <summary>An item collected whose pickup message is still open, at its place, and what its message's close does to the room.</summary>
    /// <param name="Place">The item's place.</param>
    /// <param name="Item">The item.</param>
    /// <param name="RaisesLava">
    /// Whether its message's close starts the room's lava rising (#2562): the Speed Booster in its Chozo orb, whose pickup
    /// leaves the lava's motion to the message's close. The last item collected before the close decides.
    /// </param>
    private readonly partial record struct AwaitingPickup(RoomObjectPlace Place, CollectibleObject Item, bool RaisesLava);

    /// <summary>Whether the room objects' step is waiting for its synchronous pickup message.</summary>
    internal bool HasPendingCollectibleMessage => _awaitingMessage.Count != 0;

    /// <summary>
    /// A message box has closed: the first item awaiting its message, whose box the step opened first (#2553), finishes its
    /// pickup within the same room-object update, an item showing its empty block before it leaves, a shot block's item
    /// leaving its block empty. A later item's box opens next and its item finishes when that box closes, as each item's
    /// box returns into its own room object's update. A box no item opened finds none awaiting and finishes none.
    /// </summary>
    internal void CompleteCollectibleMessage(RoomLevelData level)
    {
        if (_awaitingMessage.Count == 0)
            return;
        AwaitingPickup awaiting = _awaitingMessage[0];
        _awaitingMessage.RemoveAt(0);
        awaiting.Item.ResumeAfterMessage(_drawing, level, awaiting.Place);
        // The Speed Booster's pickup starts the lava rising once its message has closed.
        if (awaiting.RaisesLava)
            _roomFx.SetLiquidSpeed(SpeedBoosterPickupDefinitions.LavaRiseVelocity);
    }

    /// <summary>Every permanent-item room object the room holds, from the last place in update order back to the first.</summary>
    public IReadOnlyList<CollectibleObjectSnapshot> Collectibles => _places.FromLastPlace
        .Where(place => place.Active)
        .SelectMany(place => place.Behaviour is CollectibleObject { } item
            ? [new CollectibleObjectSnapshot(place.Block, item.Location, item.Kind)]
            : Array.Empty<CollectibleObjectSnapshot>())
        .ToArray();

    /// <summary>
    /// Publishes Samus contact with a visible item block. The item's touch setup
    /// finds the item room object sharing this block and moves it to its touched phase,
    /// which the following room-object step collects.
    /// </summary>
    public bool TryNotifyCollectibleTouch(Position16<RoomBlocks> block)
    {
        foreach (RoomObjectPlace place in _places.FromLastPlace)
        {
            if (!place.Active || place.Block != block || place.Behaviour is not CollectibleObject item)
                continue;
            // The pickup message keeps both the visible block and the item that lies on it. As the message closes,
            // Samus's movement can touch that block before the room objects' update empties and removes the item. A touch
            // finds the item by its block whatever it is doing, so it is acknowledged here without being collected again
            // or disturbing what it does as the message closes.
            if (item.Phase is CollectiblePhase.AwaitingMessage or CollectiblePhase.ResumeAfterMessage or
                CollectiblePhase.EmptyAwaitingDelete)
                return true;
            if (item.Phase is not (
                    CollectiblePhase.Visible or CollectiblePhase.VisibleTouched or
                    CollectiblePhase.ShotBlockVisible or CollectiblePhase.ShotBlockVisibleTouched))
                return false;
            item.Trigger();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Triggers the Chozo orb or concealed item block at <paramref name="block"/> that a shot, bomb or grapple reached:
    /// whatever reached it, the item responds with the same trigger.
    /// </summary>
    public void NotifyCollectibleProjectileHit(
        Position16<RoomBlocks> block)
    {
        foreach (RoomObjectPlace place in _places.FromLastPlace)
        {
            if (!place.Active || place.Block != block || place.Behaviour is not CollectibleObject item)
                continue;
            if (item.Phase is not (
                    CollectiblePhase.ChozoOrb or CollectiblePhase.ChozoOrbShot or
                    CollectiblePhase.ShotBlock or CollectiblePhase.ShotBlockShot or
                    CollectiblePhase.CollectedShotBlock or CollectiblePhase.CollectedShotBlockShot))
            {
                return;
            }
            item.Trigger();
            return;
        }
    }

    /// <summary>
    /// Runs one permanent-item setup on the place selected by the shared room loader, for <paramref name="item"/>, whose
    /// location its placement (<paramref name="placed"/>) names, as the collectible's definition and its pictures describe it.
    /// </summary>
    internal void SetupCollectible(
        RoomObjectRoom room,
        RoomObjectPlace place,
        PermanentCollectibleObject item,
        CollectibleIdentity placed)
    {
        Ensure.NotNull(placed);
        RoomLevelData level = room.Level;
        SharedGameState system = room.System;
        (InWorldCollectibleKind kind, CollectiblePresentation presentation) = item;
        RoomCollisionBlock original = level.GetCollisionBlock(place.Block);
        bool collected = system.HasCollectedItem(placed.Location);
        CollectibleTerrain terrain = CollectibleDefinitions.Items[kind].Picture == CollectiblePictureSource.OwnPicture
            ? CollectibleTerrain.OwnPicture(_blockPairs.Take())
            : CollectibleTerrain.TankBlocks;

        RoomTerrainBlock setup = original.Terrain.WithCollision(
            presentation == CollectiblePresentation.ShotBlock ? RoomCollisionType.ShootableBlock : RoomCollisionType.Air);
        // The setup changes how the block collides and reacts, and the block keeps its picture and what it shows
        // (#2502).
        level.SetCollision(place.Block, setup.Collision);
        level.SetBehavior(place.Block, RoomBlockBehavior.Reacting(ShootableReaction.CollectibleTrigger));

        CollectiblePhase phase = collected
            ? presentation == CollectiblePresentation.ShotBlock
                ? CollectiblePhase.CollectedShotBlock
                : CollectiblePhase.CollectedEmpty
            : presentation switch
            {
                CollectiblePresentation.Exposed => CollectiblePhase.Visible,
                // An item orb tests only whether its item was collected. A shell
                // broken on an earlier visit is rebuilt until the item is picked up.
                CollectiblePresentation.ChozoOrb => CollectiblePhase.ChozoOrb,
                CollectiblePresentation.ShotBlock => CollectiblePhase.ShotBlock,
                _ => throw new ArgumentOutOfRangeException(nameof(presentation)),
            };
        place.Become(new CollectibleObject(kind, presentation, terrain, phase, placed.Location, setup));
    }

    /// <summary>
    /// Collects <paramref name="item"/>, placed at <paramref name="place"/>: sets persistence and inventory before the
    /// pickup joins the step's <paramref name="outcome"/>, for the update to open its message, and the fanfare is requested.
    /// </summary>
    internal void Acquire(CollectibleObject item, RoomObjectPlace place, RoomObjectStepOutcome outcome)
    {
        Ensure.NotNull(item);
        Ensure.NotNull(place);
        Ensure.NotNull(outcome);
        IRoomSamus samus = _samus.Current;
        SharedGameState system = _system;

        system.CollectItem(item.Location);
        CollectibleDefinition definition = CollectibleDefinitions.Items[item.Kind];
        definition.Grant.Apply(samus);

        outcome.Collected(new CollectiblePickupEvent(definition.Message, definition.Consequence));
        // The item's cue starts the fanfare as it runs, before its message box opens.
        _sounds.QueuePermanentItemFanfare();

        // The item is granted before its message opens; it keeps its block and its place while the message shows, and
        // finishes its pickup once the message closes.
        item.AwaitMessage();
        _awaitingMessage.Add(new AwaitingPickup(place, item,
            RaisesLava: definition.LavaRisingPlacements.Contains(item.Presentation)));
    }
}
