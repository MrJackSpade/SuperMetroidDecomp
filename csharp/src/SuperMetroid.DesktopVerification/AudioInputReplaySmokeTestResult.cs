namespace SuperMetroid.DesktopVerification;

/// <summary>Observable milestones from replaying a desktop journal through the real SPC.</summary>
/// <param name="FramesExecuted">Journal and neutral-tail updates consumed before the first door completed.</param>
/// <param name="ProjectileFired">Whether replayed input fired a Samus projectile.</param>
/// <param name="PowerBeamCommandObserved">Whether the audio engine received the Power Beam SFX request.</param>
/// <param name="PowerBeamAcknowledged">Whether the SPC acknowledged that request.</param>
/// <param name="FirstDoorBegan">Whether the recorded door transition started.</param>
/// <param name="FirstDoorCompleted">Whether that transition completed during replay or its bounded neutral tail.</param>
/// <param name="SourceRoom">Room pointer before the first door handoff.</param>
/// <param name="DestinationRoom">Room pointer after the first door handoff.</param>
public readonly record struct AudioInputReplaySmokeTestResult(
    int FramesExecuted,
    bool ProjectileFired,
    bool PowerBeamCommandObserved,
    bool PowerBeamAcknowledged,
    bool FirstDoorBegan,
    bool FirstDoorCompleted,
    ushort SourceRoom,
    ushort DestinationRoom);
