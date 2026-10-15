namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>Native state-$22 and state-$25 functions from station destruction through Zebes arrival.</summary>
internal enum CeresDestructionPhase
{
    /// <summary>Holds the first Ceres scene until its queued music has drained.</summary>
    WaitForMusicQueue,
    /// <summary>Drifts the camera and raises master brightness to full.</summary>
    FadeInAndDrift,
    /// <summary>Continues camera drift and zoom until the final station blast is spawned.</summary>
    ApproachExplosion,
    /// <summary>Pans, rotates, and scales the gunship away from the station blast.</summary>
    FlyingAwayFromExplosion,
    /// <summary>Holds the explosion scene for its authored post-flight interval.</summary>
    HoldAfterExplosion,
    /// <summary>Darkens Ceres before Zebes reveal or the state-$25 game-over exit.</summary>
    FadeOutCeres,
    /// <summary>Transfers the planet-reveal Mode-1 art on the following dispatcher call.</summary>
    FlyToZebesInitial,
    /// <summary>Raises planet-scene brightness while reducing its mosaic.</summary>
    FadeInZebes,
    /// <summary>Finishes mosaic reduction before creating the Mode-7 planet actors.</summary>
    RemoveZebesMosaic,
    /// <summary>Waits for the title actor's instruction to start the camera approach.</summary>
    PlanetZebesTitle,
    /// <summary>Moves toward Zebes and increases scale to the first approach limit.</summary>
    FlyingTowardZebesA,
    /// <summary>Rotates and scales the approach until camera X passes the negative threshold.</summary>
    FlyingTowardZebesB,
    /// <summary>Changes pan direction, scales farther, and removes BG1 before the hold.</summary>
    FlyingTowardZebesC,
    /// <summary>Holds the close planet scene while OBJ actors continue.</summary>
    HoldCloseZebes,
    /// <summary>Slides planet and stars upward until the completion star exits.</summary>
    SlideZebesSceneAway,
    /// <summary>Signals the frontend owner to advance after scene completion.</summary>
    Finished,
}
