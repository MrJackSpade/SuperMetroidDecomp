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
        RoomCollisionType collision = extension.TowardTrigger switch
        {
            ScrollExtensionDirection.Left or ScrollExtensionDirection.Right => RoomCollisionType.HorizontalExtension,
            ScrollExtensionDirection.Up or ScrollExtensionDirection.Down => RoomCollisionType.VerticalExtension,
            _ => throw new ArgumentOutOfRangeException(nameof(extension), extension, "Undefined scroll extension direction."),
        };
        int offset = Ensure.Listed(BlocksTowardTrigger, extension.TowardTrigger);
        level.SetCollisionAndBehavior(place.Block, collision, RoomBlockBehavior.Extending(offset));

        // An extension's whole effect is its setup, which removes it at once, freeing its place for the room's next object.
        place.Delete();
    }

    /// <summary>The blocks an extension steps along its axis toward its trigger: back for left and up, forward for right and down.</summary>
    private static readonly IReadOnlyDictionary<ScrollExtensionDirection, int> BlocksTowardTrigger = new Dictionary<ScrollExtensionDirection, int>
    {
        [ScrollExtensionDirection.Left] = -1,
        [ScrollExtensionDirection.Right] = 1,
        [ScrollExtensionDirection.Up] = -1,
        [ScrollExtensionDirection.Down] = 1,
    };
}
