using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus;

/// <summary>
/// The compatibility services that reproduce what Samus's glitch paths read past the end of their own data
/// (#1474), as the import pipeline decoded them from the player's game data. The session's
/// gameplay runtime receives them with the installed content and hands each to the owner that consults it when it builds
/// that owner: her animation and the projectile system, which takes its own group whole and the terrain a Wave shot's
/// scan past the room's ceiling finds straddling two blocks (#1612).
/// Lifetime: the installation's, shared by every runtime of the session.
/// </summary>
public sealed class SamusOverreadServices
{
    /// <summary>Keeps every decoded service.</summary>
    /// <param name="framesBeyondAnimation">What a Samus animation frame past its pose's animation reads.</param>
    /// <param name="projectiles">The services Samus's projectiles consult where they read past their data.</param>
    /// <param name="straddledTerrain">The terrain a Wave shot's scan past the room's ceiling finds straddling two blocks.</param>
    internal SamusOverreadServices(
        FramesBeyondAnimation<SamusAnimationName, SamusAnimationFrame, SamusAnimationPlaceName, SamusPoseAnimationStep> framesBeyondAnimation,
        SamusProjectileOverreads projectiles,
        StraddledTerrain<RoomTerrainBlock> straddledTerrain)
    {
        FramesBeyondAnimation = Ensure.NotNull(framesBeyondAnimation);
        Projectiles = Ensure.NotNull(projectiles);
        StraddledTerrain = Ensure.NotNull(straddledTerrain);
    }

    /// <summary>
    /// What a Samus animation frame that names no step of its pose's animation reads, and where a named place lands in an
    /// animation it does not belong to, by a pose that animates by each animation.
    /// </summary>
    internal FramesBeyondAnimation<SamusAnimationName, SamusAnimationFrame, SamusAnimationPlaceName, SamusPoseAnimationStep> FramesBeyondAnimation { get; }

    /// <summary>The services Samus's projectiles consult where their glitch paths read past their data.</summary>
    internal SamusProjectileOverreads Projectiles { get; }

    /// <summary>The terrain a Wave shot's scan past the room's ceiling finds straddling two blocks.</summary>
    internal StraddledTerrain<RoomTerrainBlock> StraddledTerrain { get; }
}
