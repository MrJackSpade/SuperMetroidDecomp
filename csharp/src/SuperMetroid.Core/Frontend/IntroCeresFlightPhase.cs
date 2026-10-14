namespace SuperMetroid.Core.Frontend;

/// <summary>Native setup, flight, title, and handoff dispatches of the Ceres approach.</summary>
/// <remarks>Debugger states store these numerically, so new members are appended.</remarks>
internal enum IntroCeresFlightPhase
{
    /// <summary>$8B:BDE4: forced blank until the music queue drains.</summary>
    WaitForMusicQueue,
    /// <summary>The front-view gunship zooms toward the camera while stars and background accelerate diagonally.</summary>
    FlyingIntoCamera,
    /// <summary>The rear-view station image grows while its fixed-color flash fades.</summary>
    FlyingTowardCeres,
    /// <summary>The SPACE COLONY caption writes letters, optional katakana, and a final hold.</summary>
    SpaceColonyTitle,
    /// <summary>The caption and surviving actors dim every other frame toward forced blank.</summary>
    FadeOut,
    /// <summary>$8B:C100 has run; game state $1F follows.</summary>
    Finished,
    /// <summary>$8B:BCA0: the PPU setup dispatch that queues the flight's music.</summary>
    Initial,
    /// <summary>$8B:C0C5 reached forced blank and installed $8B:C100 for the next dispatch.</summary>
    StartGameAtCeres,
}
