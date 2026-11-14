using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.Core.World;

/// <summary>
/// The room state's setup (<see cref="RoomSetupBehavior"/>) at the point the game runs it while a room loads: once the
/// room's own room objects are placed and the door's setup has run, before the door closes behind Samus and before the
/// room's enemies load. The setups' earthquakes and the projectile one spawns belong to the room's enemies, so they start
/// once the enemies are loaded (<see cref="EscapeRoomEffects"/>); the scrolling skies, the statues' unlocking and the
/// Ceres setups act where the room builds those owners.
/// </summary>
internal static class RoomStateSetup
{
    /// <summary>Runs the part of <paramref name="room"/>'s setup that acts while the room's objects are being placed.</summary>
    /// <param name="room">The room and the state loading.</param>
    /// <param name="level">The room's level data.</param>
    /// <param name="roomObjects">The room's room objects, which the setups that spawn one join.</param>
    /// <param name="scrolls">The room's scroll grid, which Shaktool's controller reads.</param>
    /// <param name="system">The session's progress.</param>
    internal static void Run(SelectedRoom room, RoomLevelData level, RoomObjectSystem roomObjects, RoomScrollGrid scrolls,
        SharedGameState system)
    {
        Ensure.NotNull(room);
        Ensure.NotNull(level);
        Ensure.NotNull(roomObjects);
        Ensure.NotNull(system);
        switch (room.State.SetupBehavior)
        {
            case RoomSetupBehavior.SetupShaktoolRoomObject:
                roomObjects.Shaktool.TrySpawnShaktoolRoomController(scrolls);
                return;
            case RoomSetupBehavior.TurnWallIntoShotBlocksDuringEscape:
                roomObjects.EscapeAnimals.SetupCrittersEscapeBlock(level,
                    level.GridBlock(new(EscapeAnimalWallDefinitions.WallX, EscapeAnimalWallDefinitions.WallY)));
                return;
            // The Climb's escape setup spawns the fake-wall room object; the spawn silently drops it when all forty
            // places are taken.
            case RoomSetupBehavior.AutoDestroyWallDuringEscape:
                roomObjects.Tourian.TrySpawnOldTourianEscapeShaftWall(level);
                return;
            // The mainstreet's escape setup spawns this room object before it starts the quake EscapeRoomEffects owns.
            case RoomSetupBehavior.ClearBlocksAfterSavingAnimalsAndShakeScreen:
                roomObjects.EscapeAnimals.TrySpawnCrateriaMainstreetEscapePassage(level);
                return;
            // Tourian's first room hands Samus its area's map, as a map station would (#1799).
            case RoomSetupBehavior.SetCollectedMap:
                system.SetAreaMapAcquired(room.Area);
                return;
            case RoomSetupBehavior.None:
            case RoomSetupBehavior.LandSkyWithLandingSiteQuake:
            case RoomSetupBehavior.ScrollingSkyLand:
            case RoomSetupBehavior.ScrollingSkyOcean:
            case RoomSetupBehavior.RunStatueUnlockingAnimations:
            case RoomSetupBehavior.SpawnPrePhantoonRoomEnemyProjectile:
            case RoomSetupBehavior.KeepDraygonRoomDisplayAcrossPause:
            case RoomSetupBehavior.SetZebesTimebombEventAndLightHorizontalShaking:
            case RoomSetupBehavior.SetLightHorizontalRoomShaking:
            case RoomSetupBehavior.SetMediumHorizontalRoomShaking:
            case RoomSetupBehavior.MediumHorizontalShakingWithRisingLava:
            case RoomSetupBehavior.TurnCeresDoorToSolidBlocksAndSpawnHaze:
            case RoomSetupBehavior.SpawnCeresHaze:
            case RoomSetupBehavior.ShowSpritesOnCeresRidleyBackgroundsAndSpawnHaze:
                return;
            default:
                throw new InvalidOperationException($"Undefined room setup {room.State.SetupBehavior}.");
        }
    }
}
