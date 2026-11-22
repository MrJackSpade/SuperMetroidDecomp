using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>The collaborators a <see cref="CrocomireBehavior"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed partial record CrocomireBehaviorDependencies
{

    /// <summary>The room Crocomire fights in: its services, its random numbers and whether he is already defeated.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>The room's terrain, which Crocomire's steps move him against.</summary>
    public required EnemyTerrain Terrain { get; init; }

    /// <summary>The enemy projectiles his attacks, the bridge's fragments, his dust and his smoke are.</summary>
    public required EnemyProjectileEngine Projectiles { get; init; }

    /// <summary>The room's earthquake, which the bridge's collapse and the wall's break start.</summary>
    public required RoomEarthquake Earthquake { get; init; }

    /// <summary>What the boss draws with: the room's presentation surfaces and the installed boss artwork.</summary>
    public required BossPresentation Presentation { get; init; }

    /// <summary>The enemy update's clocks, whose random enemy counter his combat reads.</summary>
    public required EnemyFrameContext FrameContext { get; init; }

    /// <summary>The enemy touch, which his body and tongue deal Samus.</summary>
    public required EnemyCollision Collision { get; init; }

    /// <summary>The enemies' drops, which his death scatters.</summary>
    public required EnemyDeathsAndDrops DeathsAndDrops { get; init; }

    /// <summary>The room camera's distance mode, which the fight requests.</summary>
    public required RoomCameraDistance CameraDistance { get; init; }

    /// <summary>The room's boss state, which his defeat completes.</summary>
    public required EnemyRoomStatus RoomStatus { get; init; }

    /// <summary>Crocomire's frame results and sound and music requests.</summary>
    public required CrocomireEncounterAccess Access { get; init; }

    /// <summary>The room's enemy places, among which his tongue finds his body.</summary>
    public required EnemyPlaces Places { get; init; }

    /// <summary>Where his body offers his tongue, listed right after it, the creation that builds it joining his encounter (#2554).</summary>
    public required EnemyPartOffers PartOffers { get; init; }

    /// <summary>His body's sprite layer and the acid smoke that rises from his room's acid surface, built with <see cref="Access"/>.</summary>
    public required CrocomireBodyLayer BodyLayer { get; init; }

    /// <summary>The encounters of the room's bosses, from which his room's shaking and his melt's display read his encounter (#2194).</summary>
    public required RoomBossEncounters Encounters { get; init; }
}
