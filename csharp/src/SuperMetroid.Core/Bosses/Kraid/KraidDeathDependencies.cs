using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The collaborators a <see cref="KraidDeath"/> is constructed with, built once by its owner.</summary>
[RoomLifetime]
internal sealed record KraidDeathDependencies
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    public required BossFrameResultBuilder FrameResults { get; init; }

    /// <summary>Room scratch memory and services used throughout the Kraid fight.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>Publishes arena shake during growth, death, and heavy movement.</summary>
    public required RoomEarthquake Earthquake { get; init; }

    /// <summary>Provides the native body and seven part slots.</summary>
    public required EnemySlotTable SlotTable { get; init; }

    /// <summary>Applies Kraid palettes, background tiles, and OAM presentation.</summary>
    public required EnemyPresentation Presentation { get; init; }

    /// <summary>Applies contact damage, death state, and resulting drops.</summary>
    public required EnemyDeathsAndDrops DeathsAndDrops { get; init; }

    /// <summary>Coordinates Zebetite behavior registered alongside Kraid's room actors.</summary>
    public required ZebetiteBehavior ZebetiteBehavior { get; init; }

    /// <summary>Kraid's state and part lookups, part programs, function timer, random numbers and the sound he last requested.</summary>
    public required KraidEncounterAccess Access { get; init; }

    /// <summary>Kraid's room background: its fade, color transitions and the ceiling rocks.</summary>
    public required KraidBackground Background { get; init; }

    /// <summary>Kraid's head: the mouth-open attack and reaction, the eye glow and the health palettes.</summary>
    public required KraidHead Head { get; init; }
}
