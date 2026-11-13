using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Where Phantoon's door changes are spawned, and the terrain change each makes.</summary>
internal static class PhantoonDoorChangeDefinitions
{
    /// <summary>The room block both door changes are spawned at: the top block of the door's transition column.</summary>
    internal static readonly Position16<RoomBlocks> Block = new(0, 6);

    /// <summary>
    /// The change each door change makes: its frame drawn once the door's top block has been held solid
    /// (<see cref="PhantoonDoorBlockHeldSolid"/>), on the object's first update; the object leaves the room on its second.
    /// </summary>
    internal static readonly IReadOnlyDictionary<PhantoonDoorChange, TerrainChange> Changes =
        new Dictionary<PhantoonDoorChange, TerrainChange>
        {
            [PhantoonDoorChange.FightDoor] = new(PhantoonDoorBlockHeldSolid.HeldUpdates,
                [new TerrainChangeFrame(RoomObjectDraw.Of(PhantoonDoorDraw.FightDoor), 1)]),
            [PhantoonDoorChange.RestoredDoor] = new(PhantoonDoorBlockHeldSolid.HeldUpdates,
                [new TerrainChangeFrame(RoomObjectDraw.Of(PhantoonDoorDraw.RestoredDoor), 1)]),
        };
}
