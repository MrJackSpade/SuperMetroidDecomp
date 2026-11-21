using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.EffectActors;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>The collaborators a <see cref="DraygonParts"/> is constructed with, built by the Draygon behavior that owns it.</summary>
[RoomLifetime]
internal sealed partial record DraygonPartsDependencies
{
    /// <summary>Accesses the active room, enemy places, and shared random numbers.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>Spawns the parts' projectiles.</summary>
    public required EnemyProjectileEngine Projectiles { get; init; }

    /// <summary>Starts the room's earthquakes.</summary>
    public required RoomEarthquake Earthquake { get; init; }

    /// <summary>Spawns the parts' effect actors.</summary>
    public required RoomEffectActors EffectActors { get; init; }

    /// <summary>The enemy presentation the parts write their colours and layers to.</summary>
    public required EnemyPresentation Presentation { get; init; }

    /// <summary>The enemy update's frame context.</summary>
    public required EnemyFrameContext FrameContext { get; init; }

    /// <summary>The enemy update's sound and music requests.</summary>
    public required EnemyAudioQueue Audio { get; init; }

    /// <summary>The goop and turret shots, whose goop behaviour Draygon's goop is bound to.</summary>
    public required DraygonShots Shots { get; init; }

    /// <summary>The room's enemy places, among which each part finds the body listed before it.</summary>
    public required EnemyPlaces Places { get; init; }
}
