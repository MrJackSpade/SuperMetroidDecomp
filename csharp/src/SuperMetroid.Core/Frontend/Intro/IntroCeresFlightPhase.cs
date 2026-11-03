namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Setup, flight, title, and handoff dispatches of the Ceres approach.</summary>
internal enum IntroCeresFlightPhase
{
    /// <summary>The screen stays black until the music queue drains.</summary>
    WaitForMusicQueue,
    /// <summary>The front-view gunship zooms toward the camera while stars and background accelerate diagonally.</summary>
    FlyingIntoCamera,
    /// <summary>The rear-view station image grows while its fixed-color flash fades.</summary>
    FlyingTowardCeres,
    /// <summary>The SPACE COLONY caption writes letters, optional katakana, and a final hold.</summary>
    SpaceColonyTitle,
    /// <summary>The caption and surviving actors dim every other frame toward black.</summary>
    FadeOut,
    /// <summary>The approach has ended; the Ceres game state follows.</summary>
    Finished,
    /// <summary>The display setup dispatch that queues the flight's music.</summary>
    Initial,
    /// <summary>The fade reached black and installed the handoff for the next update.</summary>
    StartGameAtCeres,
}
