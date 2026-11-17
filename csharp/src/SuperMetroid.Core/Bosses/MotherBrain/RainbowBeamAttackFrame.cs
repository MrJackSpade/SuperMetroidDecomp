namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// What one step of Mother Brain's late fight reads of the update it runs in (#633, #2261): the counters, the shared random
/// sequence, the Power Bomb, the escape's text and the room's earthquake. Every member is the update's own; none has a
/// default.
/// </summary>
public sealed record RainbowBeamAttackFrame
{
    /// <summary>The enemy frame counter's current count; its place in a short repeating cycle paces the beam's colour cadence.</summary>
    public required int EnemyFrameCounter { get; init; }

    /// <summary>The main enemy pass counter's current count; its place in a short repeating cycle paces ammunition drain and corpse dust sounds.</summary>
    public required int EnemyPassCount { get; init; }

    /// <summary>Whether a Power Bomb's explosion is active, which stops the countdown to the rainbow beam's firing.</summary>
    public required bool PowerBombActive { get; init; }

    /// <summary>The room's random number as the step reads it, without advancing the generator.</summary>
    public required MotherBrainRandomSample RandomNumberSeed { get; init; }

    /// <summary>
    /// Advances the room's shared random sequence and returns its next number: every death explosion and escape-door burst
    /// draws one.
    /// </summary>
    public required Func<int> NextRandomNumber { get; init; }

    /// <summary>Whether the game shows the escape's alternate text, the time-bomb subtitle the Japanese text adds.</summary>
    public required bool AlternateEscapeText { get; init; }

    /// <summary>Whether the escape's typewriter finished its text on this update.</summary>
    public required bool TypewriterFinished { get; init; }

    /// <summary>Whether the room's earthquake has stopped as this step observes it.</summary>
    public required bool RoomEarthquakeStopped { get; init; }

    /// <summary>The session's record of what the beam's push hides from the camera, which every push writes.</summary>
    public required RainbowBeamCameraCheckpoint CameraCheckpoint { get; init; }
}
