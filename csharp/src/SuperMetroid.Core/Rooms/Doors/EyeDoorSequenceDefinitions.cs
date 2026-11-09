namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The eye doors' authored data (#1554): how near Samus must come, the hits the eye takes, and the sequences of the eye
/// and of the body and bottom it watches over. Both facings play the same sequences, mirrored; the right-facing body
/// and bottom pulse faster.
/// </summary>
internal static class EyeDoorSequenceDefinitions
{
    /// <summary>The sound each hit on the eye makes.</summary>
    internal static readonly SoundEffectId HitSound = EnemySound.EnemyKilled;

    /// <summary>How crowded the sound queue may be for the hit sound to join it.</summary>
    internal static readonly SoundRequestPriority HitSoundPriority = SoundRequestPriority.Normal;

    /// <summary>The hits the eye takes before the door opens.</summary>
    internal const int HitThreshold = 3;

    /// <summary>The hit count a Super Missile leaves: the hit it then counts passes the threshold.</summary>
    internal const int SuperMissileHitCounter = 119;

    /// <summary>How many times the destroyed eye flashes to the blue cap before it turns into it.</summary>
    internal const int OpeningFlashes = 10;

    /// <summary>How near Samus must stand, across and up or down, to open the closed eye or keep it watching.</summary>
    internal static Extent16<RoomBlocks> WakeReach { get; } = new(6, 4);

    /// <summary>How near Samus must stand, across and up or down, for the open eye to blink.</summary>
    internal static Extent16<RoomBlocks> InFrontReach { get; } = new(1, 4);

    /// <summary>How near Samus must stand, across and up or down, to make the body and bottom pulse.</summary>
    internal static Extent16<RoomBlocks> PulseReach { get; } = new(6, 16);

    /// <summary>The closed eye, for four updates before it looks for Samus again.</summary>
    internal static IReadOnlyList<EyeDoorStep> Closed { get; } = [new(EyeDoorObjectFrame.Eye0, 4)];

    /// <summary>The eye opening as Samus nears: half open for eight updates.</summary>
    internal static IReadOnlyList<EyeDoorStep> Opening { get; } = [new(EyeDoorObjectFrame.Eye1, 8)];

    /// <summary>The open eye watching Samus: it fires three shots, then blinks slowly.</summary>
    internal static IReadOnlyList<EyeDoorStep> Watching { get; } =
    [
        new(EyeDoorObjectFrame.Eye2, 64),
        new(EyeDoorObjectFrame.Eye2, 32, EyeDoorAction.Shoot),
        new(EyeDoorObjectFrame.Eye2, 32, EyeDoorAction.Shoot),
        new(EyeDoorObjectFrame.Eye2, 64, EyeDoorAction.Shoot),
        new(EyeDoorObjectFrame.Eye1, 6),
        new(EyeDoorObjectFrame.Eye0, 48),
        new(EyeDoorObjectFrame.Eye0, 48),
        new(EyeDoorObjectFrame.Eye1, 6),
    ];

    /// <summary>The eye blinking while Samus stands right in front of it: closed for four updates.</summary>
    internal static IReadOnlyList<EyeDoorStep> Blinking { get; } = [new(EyeDoorObjectFrame.Eye0, 4)];

    /// <summary>The eye flinching from a hit short of the threshold: it smokes, recovers and sweats.</summary>
    internal static IReadOnlyList<EyeDoorStep> Flinching { get; } =
    [
        new(EyeDoorObjectFrame.Eye3, 2),
        new(EyeDoorObjectFrame.Eye4, 2),
        new(EyeDoorObjectFrame.Eye3, 2, EyeDoorAction.SpawnTwoSmoke),
        new(EyeDoorObjectFrame.Eye4, 2),
        new(EyeDoorObjectFrame.Eye3, 2),
        new(EyeDoorObjectFrame.Eye4, 2, EyeDoorAction.SpawnTwoSmoke),
        new(EyeDoorObjectFrame.Eye1, 4),
        new(EyeDoorObjectFrame.Eye0, 8),
        new(EyeDoorObjectFrame.Eye0, 56, EyeDoorAction.SpawnSweat),
        new(EyeDoorObjectFrame.Eye1, 4),
        new(EyeDoorObjectFrame.Eye4, 4),
    ];

    /// <summary>One flash of the destroyed eye: the cleared opening for three updates, then the blue cap for four.</summary>
    internal static IReadOnlyList<EyeDoorStep> OpeningFlash { get; } =
    [
        new(EyeDoorObjectFrame.OpeningClear, 3),
        new(EyeDoorObjectFrame.BlueCap, 4),
    ];

    /// <summary>The still body or bottom: its first pose for eight updates before it looks for Samus again.</summary>
    internal static IReadOnlyList<EyeDoorStep> Still { get; } = [new(EyeDoorObjectFrame.Pose0, 8)];

    /// <summary>Each facing's body and bottom pulsing while Samus is near: its poses there and back.</summary>
    internal static IReadOnlyDictionary<EyeDoorOrientation, IReadOnlyList<EyeDoorStep>> Pulse { get; } =
        new Dictionary<EyeDoorOrientation, IReadOnlyList<EyeDoorStep>>
    {
        [EyeDoorOrientation.Left] =
        [
            new(EyeDoorObjectFrame.Pose0, 8),
            new(EyeDoorObjectFrame.Pose1, 8),
            new(EyeDoorObjectFrame.Pose2, 8),
            new(EyeDoorObjectFrame.Pose1, 8),
        ],
        [EyeDoorOrientation.Right] =
        [
            new(EyeDoorObjectFrame.Pose0, 6),
            new(EyeDoorObjectFrame.Pose1, 6),
            new(EyeDoorObjectFrame.Pose2, 6),
            new(EyeDoorObjectFrame.Pose1, 6),
        ],
    };
}
