using SuperMetroid.Core.World;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Samus's door-block contacts in a runtime's loaded room, for checks that put her at a door (#2545).</summary>
internal static class DoorContactAccess
{
    /// <summary>The loaded room's door contacts.</summary>
    extension(SuperMetroidRuntime runtime)
    {
        /// <summary>The loaded room's door contacts, which Samus's collision with its door blocks meets.</summary>
        internal RoomDoorContacts DoorContacts => runtime.Room.LoadedScope.RoomObjects.DoorContacts;

        /// <summary>The door Samus's collision entered in the loaded room, or null while she has entered none.</summary>
        internal DoorDefinition? EnteredDoor => runtime.DoorContacts.EnteredDoor;

        /// <summary>
        /// Samus's collision meeting door block <paramref name="block"/> of the loaded room in <paramref name="pose"/>, as her
        /// movement meets one: the room's door contacts act on the contact, and the block's exit is returned.
        /// </summary>
        /// <param name="block">A door block of the loaded room.</param>
        /// <param name="pose">Samus's pose as room collision reads it.</param>
        internal DoorBlockExit MeetDoorBlock(RoomCollisionBlock block, SamusCollisionPose pose)
        {
            DoorBlockExit exit = runtime.Room.LoadedLevelData.DoorExitAt(block);
            runtime.DoorContacts.Meet(DoorContact.Of(exit, pose));
            return exit;
        }
    }
}
