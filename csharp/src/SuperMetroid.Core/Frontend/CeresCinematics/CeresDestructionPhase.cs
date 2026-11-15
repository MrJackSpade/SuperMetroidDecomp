namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>The destruction cinematic's phases from station destruction through Zebes arrival.</summary>
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
    /// <summary>Darkens Ceres before Zebes reveal or the game-over exit with Samus aboard.</summary>
    FadeOutCeres,
    /// <summary>Shows the planet-reveal art on the following update.</summary>
    FlyToZebesInitial,
    /// <summary>Raises planet-scene brightness while reducing its pixelation.</summary>
    FadeInZebes,
    /// <summary>Finishes reducing the planet's pixelation before creating the turning-floor planet actors.</summary>
    RemoveZebesPixelation,
    /// <summary>Waits for the title actor's cue to start the camera approach.</summary>
    PlanetZebesTitle,
    /// <summary>The gunship flies toward Zebes, the planet growing to the first approach limit (#1981).</summary>
    FlyingTowardZebes,
    /// <summary>The gunship turns toward Zebes, the view rotating and growing until it has panned past 128 pixels left.</summary>
    TurningTowardZebes,
    /// <summary>The gunship closes in on Zebes, the view panning back and growing until the turning floor hides before the hold.</summary>
    ClosingInOnZebes,
    /// <summary>Holds the close planet scene while sprite actors continue.</summary>
    HoldCloseZebes,
    /// <summary>Slides planet and stars upward until the completion star exits.</summary>
    SlideZebesSceneAway,
    /// <summary>Signals the frontend owner to advance after scene completion.</summary>
    Finished,
}
