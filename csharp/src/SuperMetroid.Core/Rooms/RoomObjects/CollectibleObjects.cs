using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The room's in-world collectibles' service (#1554): item and tank setup, the touches and shots that trigger them, the
/// pickup with its message, fanfare and inventory, and each collectible's picture where it lies. Each item is a typed
/// room object (<see cref="CollectibleObject"/>) that holds and advances its own state.
/// </summary>
[RoomLifetime]
internal sealed class CollectibleObjects
{

    /// <summary>The room's system and event state.</summary>
    private readonly SharedGameState _system;
    /// <summary>Samus as the room reads and changes her.</summary>
    private readonly IRoomSamusAccess _samus;
    /// <summary>The room's effect overlay FX.</summary>
    private readonly IRoomFxMotion _roomFx;
    /// <summary>Updates level blocks and queues their draws.</summary>
    private readonly IRoomObjectDrawing _drawing;
    /// <summary>The room's places, searched for the item a touch or a shot reaches and for the items the room holds.</summary>
    private readonly RoomObjectPlaceList _places;
    /// <summary>
    /// Connects the collectibles to room state and drawing, and to the room's block pairs, which its collectibles that
    /// show their own picture take in turn.
    /// </summary>
    internal CollectibleObjects(SharedGameState system, IRoomSamusAccess samus, IRoomFxMotion roomFx, IRoomObjectDrawing drawing, RoomObjectPlaceList places,
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

    /// <summary>Pickup events published by the current room-object step.</summary>
    private readonly List<CollectiblePickupEvent> _collectiblePickupEvents = new();
    /// <summary>The terrain block pairs the room's collectibles that show their own picture take in turn.</summary>
    private readonly CollectibleBlockPairs _blockPairs;
    /// <summary>The room objects' audio requests, which a pickup's fanfare joins.</summary>
    private readonly RoomObjectSoundQueue _sounds;
    /// <summary>Delays the Speed Booster Chozo lava motion write until message return.</summary>
    private bool _pendingSpeedBoosterPickupContinuation;
    /// <summary>
    /// The items collected whose pickup message is still open, each at its place, in the order they were collected: the
    /// room-object update's order, the order of their places.
    /// </summary>
    private readonly List<AwaitingPickup> _awaitingMessage = [];

    /// <summary>An item collected whose pickup message is still open, at its place.</summary>
    /// <param name="Place">The item's place.</param>
    /// <param name="Item">The item.</param>
    private readonly record struct AwaitingPickup(RoomObjectPlace Place, CollectibleObject Item);

    /// <summary>Whether the room objects' step is waiting for its synchronous pickup message.</summary>
    internal bool HasPendingCollectibleMessage => _awaitingMessage.Count != 0;

    /// <summary>
    /// The pickup message has closed: each item awaiting it finishes its pickup within the same room-object update, an item
    /// showing its empty block before it leaves, a shot block's item leaving its block empty. They finish from the last
    /// place back, the order the room's searches for a collectible run in.
    /// </summary>
    internal void CompleteCollectibleMessage(RoomLevelData level)
    {
        for (int index = _awaitingMessage.Count - 1; index >= 0; index--)
        {
            AwaitingPickup awaiting = _awaitingMessage[index];
            awaiting.Item.ResumeAfterMessage(_drawing, level, awaiting.Place);
        }
        _awaitingMessage.Clear();
        CompleteSpeedBoosterPickupContinuation();
    }

    /// <summary>Applies the delayed lava-rise velocity after the Speed Booster message returns.</summary>
    private void CompleteSpeedBoosterPickupContinuation()
    {
        if (!_pendingSpeedBoosterPickupContinuation)
            return;
        _roomFx.SetLiquidSpeed(SpeedBoosterPickupDefinitions.LavaRiseVelocity);
        _pendingSpeedBoosterPickupContinuation = false;
    }

    /// <summary>Pickup publications produced during the most recent room-object step.</summary>
    public IReadOnlyList<CollectiblePickupEvent> CollectiblePickupEvents =>
        _collectiblePickupEvents;

    /// <summary>Every permanent-item room object the room holds, from the last place in update order back to the first.</summary>
    public IReadOnlyList<CollectibleObjectSnapshot> Collectibles => _places.FromLastPlace
        .Where(place => place.Active)
        .SelectMany(place => place.Behaviour is CollectibleObject { } item
            ? [new CollectibleObjectSnapshot(place.Block, item.Location, item.Kind)]
            : Array.Empty<CollectibleObjectSnapshot>())
        .ToArray();

    /// <summary>
    /// Publishes Samus contact with a visible item block. The item's touch setup
    /// finds the item room object sharing this block and arms its trigger timer;
    /// acquisition remains owned by the following room-object step.
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
                    CollectiblePhase.Visible or CollectiblePhase.ShotBlockVisible))
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
    public bool TryNotifyCollectibleProjectileHit(
        Position16<RoomBlocks> block)
    {
        foreach (RoomObjectPlace place in _places.FromLastPlace)
        {
            if (!place.Active || place.Block != block || place.Behaviour is not CollectibleObject item)
                continue;
            if (item.Phase is not (
                    CollectiblePhase.ChozoOrb or
                    CollectiblePhase.ShotBlock or
                    CollectiblePhase.CollectedShotBlock))
            {
                return false;
            }
            item.Trigger();
            return true;
        }
        return false;
    }

    /// <summary>Clears pickup publications at the start of a room-object step.</summary>
    internal void BeginCollectibleFrame() => _collectiblePickupEvents.Clear();

    /// <summary>
    /// Runs one permanent-item setup on the place selected by the shared room loader, for <paramref name="item"/>, whose
    /// location its placement (<paramref name="placed"/>) names, as the collectible's definition and its pictures describe it.
    /// </summary>
    internal void SetupCollectible(
        RoomObjectRoom room,
        RoomObjectPlace place,
        PermanentCollectibleObject item,
        CollectedItemArgument placed)
    {
        Ensure.NotNull(placed);
        RoomLevelData level = room.Level;
        SharedGameState system = room.System;
        (InWorldCollectibleKind kind, CollectiblePresentation presentation) = item;
        RoomCollisionBlock original = level.GetCollisionBlock(place.Block);
        bool collected = system.HasCollectedItem(placed.Location);
        CollectibleBlockPairs.Pair? blockPair = CollectibleDefinitions.Items[kind].Picture == CollectiblePictureSource.OwnPicture ? _blockPairs.Take() : null;

        RoomTerrainBlock setup = original.Terrain.WithCollision(
            presentation == CollectiblePresentation.ShotBlock ? RoomCollisionType.ShootableBlock : RoomCollisionType.Air);
        level.SetTerrain(place.Block, setup);
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
        place.Become(new CollectibleObject(kind, presentation, blockPair, phase, placed.Location, setup));
    }

    /// <summary>
    /// Collects <paramref name="item"/>, placed at <paramref name="place"/>: sets persistence and inventory before publishing
    /// the synchronous message and fanfare request.
    /// </summary>
    internal void Acquire(CollectibleObject item, RoomObjectPlace place)
    {
        Ensure.NotNull(item);
        Ensure.NotNull(place);
        IRoomSamus samus = _samus.Current;
        SharedGameState system = _system;

        system.CollectItem(item.Location);
        CollectibleDefinition definition = CollectibleDefinitions.Items[item.Kind];
        definition.Grant.Apply(samus);

        var pickup = new CollectiblePickupEvent(item.Kind, definition.Message, definition.Consequence);
        _collectiblePickupEvents.Add(pickup);
        // The item's cue starts the fanfare as it runs, before its message box opens.
        _sounds.QueuePermanentItemFanfare();
        _pendingSpeedBoosterPickupContinuation =
            item.Kind == InWorldCollectibleKind.SpeedBooster &&
            item.Presentation == CollectiblePresentation.ChozoOrb;

        // The item is granted before its message opens; it keeps its block and its place while the message shows, and
        // finishes its pickup once the message closes.
        item.AwaitMessage();
        _awaitingMessage.Add(new AwaitingPickup(place, item));
    }
}
