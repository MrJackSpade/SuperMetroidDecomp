using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>The room's scroll triggers and scroll extensions: their setup, and Samus's touch of a trigger.</summary>
[RoomLifetime]
internal sealed class ScrollObjects
{
    /// <summary>The room's objects, in the update order, searched for the trigger at a block.</summary>
    private readonly RoomObjectPlaceList _places;

    /// <summary>Binds the scroll triggers to the room's objects.</summary>
    internal ScrollObjects(RoomObjectPlaceList places)
    {
        _places = places;
    }

    /// <summary>The block of every live scroll trigger, in place order, which a touch with no trigger names as it fails.</summary>
    internal IReadOnlyList<Position16<RoomBlocks>> TriggerBlocks => _places.FromLastPlace
        .Where(place => place.Active && place.Behaviour is ScrollTriggerObject)
        .Select(place => place.Block)
        .ToArray();

    /// <summary>
    /// Applies the scroll trigger's touch to the already-resolved origin of its special-air trigger block.
    /// </summary>
    /// <param name="block">The room block of the trigger origin, after any scroll-extension traversal.</param>
    /// <returns>True when a live scroll trigger owns the block, including an already-touched one; false when none does.</returns>
    /// <remarks>
    /// The first touch makes the trigger's writes run on the next room-object update; further collision probes before
    /// then change nothing. The first trigger at the block in the update order takes the touch.
    /// </remarks>
    public bool TryNotifyScrollTouch(Position16<RoomBlocks> block)
    {
        foreach (RoomObjectPlace place in _places.FromLastPlace)
        {
            if (!place.Active || place.Block != block || place.Behaviour is not ScrollTriggerObject trigger)
                continue;
            trigger.Touch();
            return true;
        }
        return false;
    }

    /// <summary>Runs one scroll trigger's or extension's setup at the place the room population took for it.</summary>
    /// <param name="level">The room's level data.</param>
    /// <param name="place">The place the population took.</param>
    /// <param name="kind">A scroll trigger or one of the scroll extensions.</param>
    /// <param name="change">The scroll change a trigger applies; null for an extension.</param>
    /// <exception cref="InvalidOperationException">The kind is neither, or a trigger has no scroll change.</exception>
    internal static void SetupScroll(RoomLevelData level, RoomObjectPlace place, RoomObjectKind kind, RoomScrollChange? change)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(place);
        if (kind != RoomObjectKind.ScrollTrigger)
        {
            (RoomCollisionType collisionType, int offset) = ScrollExtensionDefinitions.Extensions.TryGetValue(kind, out ScrollExtension extension)
                ? extension
                : throw new InvalidOperationException($"Room object {kind} is neither a scroll trigger nor a scroll extension.");
            level.SetTerrain(
                place.Block,
                level.GetCollisionBlock(place.Block).Terrain.WithCollision(collisionType));
            level.SetBehavior(place.Block, RoomBlockBehavior.Extending(offset));

            // An extension's whole effect is its setup, which removes it at once: the following authored record takes
            // its place.
            place.Delete();
            return;
        }

        var trigger = new ScrollTriggerObject(change ?? throw new InvalidOperationException("A scroll trigger was set up without its scroll change."));
        place.Become(trigger);
        trigger.Rest(level, place.Block);
    }
}
