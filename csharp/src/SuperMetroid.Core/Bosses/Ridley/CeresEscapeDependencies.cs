using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The collaborators a <see cref="CeresEscape"/> is constructed with, built once by its owner.</summary>
[RoomLifetime]
internal sealed record CeresEscapeDependencies
{
    /// <summary>The enemy pass's results, which the self-destruct writes.</summary>
    public required EnemyPassEvents PassEvents { get; init; }

    /// <summary>Provides room services and the floor's frames.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>Queues the getaway's and the warning's sounds and music.</summary>
    public required EnemyAudioQueue Audio { get; init; }

    /// <summary>Shows the warning's look, the chamber's walls and the escape's colour effects.</summary>
    public required EnemyPresentation Presentation { get; init; }

    /// <summary>The station's escape progress, which the getaway and the self-destruct advance.</summary>
    public required ICeresEscapeStatus Status { get; init; }

    /// <summary>Says whether the warning types its Japanese text.</summary>
    public required EnemyRoomStatus RoomStatus { get; init; }

    /// <summary>The session's installed boss artwork.</summary>
    public required BossArtworkBinding BossArtwork { get; init; }

    /// <summary>The session's unauthored Speed Booster cadence, whose delays the getaway's wing frames leave behind.</summary>
    public required UnauthoredBoostCadence UnauthoredBoostCadence { get; init; }
}
