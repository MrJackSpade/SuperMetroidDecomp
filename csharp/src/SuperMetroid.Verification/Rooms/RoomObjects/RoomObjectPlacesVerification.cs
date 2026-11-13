using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>
/// Checks the room's room-object places (#1622): spawns take the first free place in update order, a full room loses the
/// spawn, a removed object's place is the next one taken, and a removed object leaves nothing at its place.
/// </summary>
internal static class RoomObjectPlacesVerification
{
    /// <summary>
    /// Fills the room's forty places and checks each spawn takes the next place in update order, a forty-first spawn takes
    /// none, an object that leaves frees its place for the next spawn, which updates where it did, and the place keeps
    /// nothing of the object that left.
    /// </summary>
    internal static void VerifyRoomObjectPlaces()
    {
        var places = new RoomObjectPlaces();
        var table = new RoomObjectSlotTable(places);
        AssertEqual(RoomObjectPlaces.PlaceCount, table.InProcessingOrder.Count, "the room lists every place, free ones included");
        for (int spawn = 0; spawn < RoomObjectPlaces.PlaceCount; spawn++)
        {
            RoomObjectSlot slot = table.Spawn() ?? throw new InvalidOperationException($"Spawn {spawn} found a free place.");
            AssertTrue(ReferenceEquals(table.InProcessingOrder[spawn], slot), $"spawn {spawn} takes the next place in update order");
            AssertTrue(slot.Active, $"spawn {spawn}'s place holds its object");
            slot.Block = new Position16<RoomBlocks>(spawn, 0);
            slot.Argument = new MetroidRoomArgument(spawn);
        }
        AssertTrue(table.IsFull && places.IsFull, "forty spawns fill the room");
        AssertEqual(RoomObjectPlaces.PlaceCount, table.ActiveCount, "every place holds an object");
        AssertEqual<RoomObjectSlot?>(null, table.Spawn(), "a spawn into a full room is lost");

        RoomObjectSlot removed = table.InProcessingOrder[11];
        removed.Delete();
        AssertTrue(!removed.Active && !table.IsFull, "an object that leaves frees its place");
        AssertEqual(default, removed.Block, "a freed place keeps no block");
        AssertEqual<RoomObjectRoomArgument?>(null, removed.Argument, "a freed place keeps nothing its object was placed with");
        AssertEqual<RoomObjectBehaviour?>(null, removed.Behaviour, "a freed place holds no typed object");
        AssertTrue(ReferenceEquals(removed, table.Spawn()), "the next spawn takes the freed place, updating where the removed object did");
        AssertTrue(table.IsFull, "the spawn fills the room again");

        removed.Delete();
        removed.Delete();
        AssertEqual(RoomObjectPlaces.PlaceCount - 1, places.HeldCount, "removing a removed object frees nothing more");
        AssertThrows<InvalidOperationException>(() => places.Free(11), "a free place cannot be freed again");
        AssertEqual<int?>(11, places.Take(), "the room's places hand out the first free place in update order");
        Console.WriteLine("Room-object places: forty places taken in update order, a full room loses the spawn, and a freed place is reused empty.");
    }

    /// <summary>Runs the focused room-object places check.</summary>
    [VerificationCommand("--room-object-places")]
    internal static int RoomObjectPlacesCommand()
    {
        VerifyRoomObjectPlaces();
        return 0;
    }
}
