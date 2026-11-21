using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>
/// Checks the room's room-object places (#1622): spawns take the first free place in update order, a full room loses the
/// spawn, a removed object's place is the next one taken, and a removed object leaves nothing at its place. What a place
/// holds is one value (#2560): free, taken by a spawn awaiting its object, or the object.
/// </summary>
internal static class RoomObjectPlacesVerification
{
    /// <summary>
    /// Fills the room's forty places and checks each spawn takes the next place in update order, a taken place holds no
    /// object until its setup gives it one, a forty-first spawn takes none, an object that leaves frees its place for the
    /// next spawn, which updates where it did, and the place keeps nothing of the object that left.
    /// </summary>
    internal static void VerifyRoomObjectPlaces()
    {
        var table = new RoomObjectPlaceList();
        AssertEqual(RoomObjectPlaces.PlaceCount, table.InUpdateOrder.Count, "the room lists every place, free ones included");
        for (int spawn = 0; spawn < RoomObjectPlaces.PlaceCount; spawn++)
        {
            RoomObjectPlace place = table.Spawn() ?? throw new InvalidOperationException($"Spawn {spawn} found a free place.");
            AssertTrue(ReferenceEquals(table.InUpdateOrder[spawn], place), $"spawn {spawn} takes the next place in update order");
            AssertTrue(place.Active, $"spawn {spawn}'s place holds its object");
            AssertEqual<RoomObjectBehaviour?>(null, place.Behaviour, $"spawn {spawn}'s place awaits the object its setup gives it");
            place.Block = new Position16<RoomBlocks>(spawn, 0);
            var held = new EscapeRoomHeldPlaceObject();
            place.Become(held);
            AssertTrue(ReferenceEquals(held, place.Behaviour), $"spawn {spawn}'s place holds the object its setup gave it");
            AssertThrows<InvalidOperationException>(() => place.Become(new EscapeRoomHeldPlaceObject()),
                $"spawn {spawn}'s place, once it holds its object, takes no other");
        }
        AssertTrue(table.IsFull, "forty spawns fill the room");
        AssertEqual(RoomObjectPlaces.PlaceCount, table.ActiveCount, "every place holds an object");
        AssertEqual<RoomObjectPlace?>(null, table.Spawn(), "a spawn into a full room is lost");

        RoomObjectPlace removed = table.InUpdateOrder[11];
        removed.Delete();
        AssertTrue(!removed.Active && !table.IsFull, "an object that leaves frees its place");
        AssertEqual(default, removed.Block, "a freed place keeps no block");
        AssertEqual<RoomObjectBehaviour?>(null, removed.Behaviour, "a freed place holds no typed object");
        AssertThrows<InvalidOperationException>(() => removed.Become(new EscapeRoomHeldPlaceObject()),
            "a freed place takes no object until a spawn takes it");
        AssertTrue(ReferenceEquals(removed, table.Spawn()), "the next spawn takes the freed place, updating where the removed object did");
        AssertTrue(table.IsFull, "the spawn fills the room again");

        removed.Delete();
        removed.Delete();
        AssertEqual(RoomObjectPlaces.PlaceCount - 1, table.ActiveCount, "removing a removed object frees nothing more");
        AssertEqual<int?>(11, RoomObjectPlaces.FirstFree(place => table.InUpdateOrder[place].Active),
            "the room's places hand out the first free place in update order");
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
