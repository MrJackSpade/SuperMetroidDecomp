using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus;

/// <summary>
/// The compatibility services that reproduce what Samus's glitch paths read past the end of their own data
/// (#1474), as the import pipeline decoded them from the player's game data (owner question 3). The session's
/// gameplay runtime receives them with the installed content and hands each to the owner that consults it when it builds
/// that owner: her animation and the projectile system, which takes its own group whole and the terrain a Wave shot's
/// scan past the room's ceiling finds straddling two blocks (#1612). The colours Gravity Speed Booster
/// and the Chainsaw beam read past their tables are no longer here (#1512): only the import reads them, to pre-colour her
/// sheets in those shades.
/// Lifetime: the installation's, shared by every runtime of the session.
/// </summary>
public sealed class SamusOverreadServices
{
    /// <summary>Keeps every decoded service.</summary>
    /// <param name="animationContinuation">What a Samus animation frame past its pose's animation reads.</param>

    /// <param name="projectiles">The services Samus's projectiles consult where they read past their data.</param>
    /// <param name="straddledTerrain">The terrain a Wave shot's scan past the room's ceiling finds straddling two blocks.</param>
    internal SamusOverreadServices(
        LaidOutAnimationContinuation<SamusPoseId, SamusPoseAnimationStep> animationContinuation,
        SamusProjectileOverreads projectiles,
        StraddledTerrain<RoomTerrainBlock> straddledTerrain)
    {
        AnimationContinuation = Ensure.NotNull(animationContinuation);
        Projectiles = Ensure.NotNull(projectiles);
        StraddledTerrain = Ensure.NotNull(straddledTerrain);
    }

    /// <summary>What a Samus animation frame past its pose's animation reads, by a pose that animates by each animation.</summary>
    internal LaidOutAnimationContinuation<SamusPoseId, SamusPoseAnimationStep> AnimationContinuation { get; }


    /// <summary>The services Samus's projectiles consult where their glitch paths read past their data.</summary>
    internal SamusProjectileOverreads Projectiles { get; }

    /// <summary>The terrain a Wave shot's scan past the room's ceiling finds straddling two blocks.</summary>
    internal StraddledTerrain<RoomTerrainBlock> StraddledTerrain { get; }
}
