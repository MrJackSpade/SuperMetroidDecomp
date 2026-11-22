using SuperMetroid.Core.Areas.Maridia;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.World;

/// <summary>
/// The destination room a door's setup changes (#2382, #2485): the parts its load has built when the setup runs, after
/// the room's objects and before its enemies, and Samus, whom the elevatube's setups place and release. It presents them to
/// the door's room event as <see cref="IDoorEventRoom"/>.
/// </summary>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
internal sealed partial class DoorSetupRoom : IDoorEventRoom
{
    /// <summary>The destination's level, which the setup's room objects edit.</summary>
    internal required RoomLevelData Level { get; init; }

    /// <summary>The destination's area, whose boss the Wrecked Ship treadmill reads.</summary>
    internal required AreaId Area { get; init; }

    /// <summary>The destination's room objects, which the setup spawns among.</summary>
    internal required RoomObjectSystem RoomObjects { get; init; }

    /// <summary>The destination's block animations, which the treadmill setups start.</summary>
    internal required RoomBlockAnimations BlockAnimations { get; init; }

    /// <summary>The room being loaded, whose elevatube ride, where its main composed one, the tube's entry doors start.</summary>
    internal required RoomScopeConstruction Scope { get; init; }

    /// <summary>Samus, whom the elevatube's setups place and release.</summary>
    internal required SamusState Samus { get; init; }

    /// <summary>The gameplay display, whose turning floor the shaft's exit door ends.</summary>
    internal required GameplayDisplayState Display { get; init; }

    /// <summary>The session's progress, whose defeated bosses the Wrecked Ship treadmill reads.</summary>
    internal required SharedGameState System { get; init; }

    /// <inheritdoc/>
    /// <remarks>
    /// The ride is the elevatube room's main's, which the room's definition step composed before the door's setup runs. The
    /// tube's entry doors lead only into the elevatube's room, which the load checks here as the door's setup runs.
    /// </remarks>
    /// <exception cref="InvalidDataException">The door leads into a room whose main is no elevatube.</exception>
    public void StartElevatubeRide(ElevatubeEntry entry)
    {
        switch (Scope.Main)
        {
            case MaridiaElevatubeMain tube:
                tube.StartRide(entry, Samus);
                return;
            default:
                throw new InvalidDataException(
                    $"An elevatube entry door leads into {Scope.Room.Definition.Name}, whose main is no elevatube.");
        }
    }

    /// <inheritdoc/>
    public void ShowOrdinaryDisplay() => Display.ActiveTurningFloor = null;

    /// <inheritdoc/>
    public void StartTreadmillAnimation(WreckedShipTreadmillDirection direction) => BlockAnimations.StartDoorTreadmill(direction);

    /// <inheritdoc/>
    public bool TrySpawnEntranceTreadmill(WreckedShipTreadmillDirection direction) =>
        RoomObjects.WreckedShipTreadmill.TrySpawnWreckedShipEntranceTreadmill(
            Level, direction, System.HasDefeatedAnyOf(Area, DefeatedBosses.AreaBoss));

    /// <inheritdoc/>
    public bool TrySpawnElevatube() => RoomObjects.MaridiaElevatube.TrySpawnMaridiaElevatube(Level);

    /// <inheritdoc/>
    public void ReleaseSamus() => Samus.Control.Unlock();
}
