using SuperMetroid.Core.Rooms.Scrolling;
﻿using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The collaborators a <see cref="KraidRise"/> is constructed with, built once by its owner.</summary>
[RoomLifetime]
internal sealed record KraidRiseDependencies
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    public required BossFrameResultBuilder FrameResults { get; init; }

    /// <summary>Room services used throughout the Kraid fight.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>Owns rocks, lint, and other fight projectiles.</summary>
    public required EnemyProjectileEngine Projectiles { get; init; }

    /// <summary>Publishes arena shake during growth, death, and heavy movement.</summary>
    public required RoomEarthquake Earthquake { get; init; }

    /// <summary>Kraid's eight parts in his room, in the order they run.</summary>
    public required KraidParts Parts { get; init; }

    /// <summary>Applies Kraid palettes, background pictures, and sprite presentation.</summary>
    public required EnemyPresentation Presentation { get; init; }

    /// <summary>The room camera's distance mode, which the fight requests.</summary>
    public required RoomCameraDistance CameraDistance { get; init; }


    /// <summary>Kraid's state and part lookups, part animations, phase timer, random numbers and the sound he last requested.</summary>
    public required KraidEncounterAccess Access { get; init; }

    /// <summary>Kraid's room background: its fade, color transitions and the ceiling rocks.</summary>
    public required KraidBackground Background { get; init; }

    /// <summary>Kraid's head: the mouth-open attack and reaction, the eye glow and the health palettes.</summary>
    public required KraidHead Head { get; init; }
}
