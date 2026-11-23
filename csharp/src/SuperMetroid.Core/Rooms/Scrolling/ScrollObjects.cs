using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>The room's scroll triggers and scroll extensions: their setup, and Samus's touch of a trigger.</summary>
internal sealed partial class ScrollObjects
{
    /// <summary>The room's objects, in the update order, which the room load binds to their trigger blocks.</summary>
    private readonly RoomObjectPlaceList _places;

    /// <summary>
    /// Each scroll-trigger block's owner, the first trigger at the block in the update order, bound once when the room's
    /// objects are loaded. A trigger is never deleted or moved, and only a trigger writes a scroll-trigger block (its own),
    /// so every such block a touch can meet is a key.
    /// </summary>
    private readonly Dictionary<Position16<RoomBlocks>, ScrollTriggerObject> _owners = [];

    /// <summary>Binds the scroll triggers to the room's objects.</summary>
    internal ScrollObjects(RoomObjectPlaceList places)
    {
        _places = places;
    }

    /// <summary>
    /// Binds every scroll-trigger block of the loaded room to its trigger, once the room's objects are set up. The game's
    /// touch searches the room objects for the block and never returns when none holds it, so a level whose
    /// scroll-trigger block has no trigger is rejected here, when the room loads, rather than met in play: no level of the
    /// game has such a block, and an edited installation's is invalid data.
    /// </summary>
    /// <param name="level">The loaded room's level, after its objects' setups wrote their blocks.</param>
    /// <exception cref="InvalidDataException">A scroll-trigger block of the level has no scroll trigger.</exception>
    internal void BindTriggerBlocks(RoomLevelData level)
    {
        Ensure.NotNull(level);
        _owners.Clear();
        foreach (RoomObjectPlace place in _places.FromLastPlace)
        {
            if (place.Active && place.Behaviour is ScrollTriggerObject trigger)
                _owners.TryAdd(place.Block, trigger);
        }

        for (int y = 0; y < level.Size.Y; y++)
        {
            for (int x = 0; x < level.Size.X; x++)
            {
                RoomCollisionBlock block = level.GetCollisionBlock(new(x, y));
                if (block.CollisionType == RoomCollisionType.SpecialAir &&
                    block.Behavior.SpecialAir == SpecialAirReaction.ScrollTrigger && !_owners.ContainsKey(block.At))
                {
                    throw new InvalidDataException($"Scroll-trigger block ({x},{y}) of the room's level has no scroll trigger.");
                }
            }
        }
    }

    /// <summary>
    /// Applies the scroll trigger's touch to the already-resolved origin of its special-air trigger block.
    /// </summary>
    /// <param name="block">The room block of the trigger origin, after any scroll-extension traversal, which the room load
    /// bound to its trigger.</param>
    /// <remarks>
    /// The first touch makes the trigger's writes run on the next room-object update; further collision probes before
    /// then change nothing. The first trigger at the block in the update order takes the touch.
    /// </remarks>
    public void NotifyScrollTouch(Position16<RoomBlocks> block) => _owners[block].Touch();

    /// <summary>Runs one scroll trigger's setup at the place the room population took for it.</summary>
    /// <param name="level">The room's level data.</param>
    /// <param name="place">The place the population took.</param>
    /// <param name="change">The scroll change the trigger applies.</param>
    internal static void SetupScrollTrigger(RoomLevelData level, RoomObjectPlace place, RoomScrollChange change)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(place);
        var trigger = new ScrollTriggerObject(change);
        place.Become(trigger);
        trigger.Rest(level, place.Block);
    }

    /// <summary>Runs one scroll extension's setup at the place the room population took for it.</summary>
    /// <param name="level">The room's level data.</param>
    /// <param name="place">The place the population took.</param>
    /// <param name="extension">The way back toward the trigger the extension leads.</param>
    internal static void SetupScrollExtension(RoomLevelData level, RoomObjectPlace place, ScrollExtension extension)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(place);
        // The setup changes how the block collides and reacts, and the block keeps its picture and what it shows
        // (#2502).
        RoomCollisionType collision = extension.Horizontal ? RoomCollisionType.HorizontalExtension : RoomCollisionType.VerticalExtension;
        level.SetCollisionAndBehavior(place.Block, collision, RoomBlockBehavior.Extending(extension.BlocksTowardTrigger));

        // An extension's whole effect is its setup, which removes it at once, freeing its place for the room's next object.
        place.Delete();
    }
}
