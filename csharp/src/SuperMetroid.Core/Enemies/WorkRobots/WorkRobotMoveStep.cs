namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>One Work Robot walk step: distance, laser velocities, wall/ledge/fall continuations (#633).</summary>
internal sealed record WorkRobotMoveStep
{
    /// <summary>Horizontal pixels.</summary>
    public required int HorizontalPixels { get; init; }

    /// <summary>Gets or sets variable A, the signed 8.8 horizontal velocity assigned to newly fired robot lasers.</summary>
    public required ushort FacingLaserVelocity { get; init; }

    /// <summary>The animation a wall turns the walk into.</summary>
    public required WorkRobotProgram WallProgram { get; init; }

    /// <summary>Check ledge.</summary>
    public required bool CheckLedge { get; init; }

    /// <summary>Ledge probe direction.</summary>
    public required int LedgeProbeDirection { get; init; }

    /// <summary>The animation a ledge turns the walk into, when the step watches for ledges.</summary>
    public required WorkRobotProgram? FallProgram { get; init; }

    /// <summary>Gets or sets variable A, the signed 8.8 horizontal velocity assigned to newly fired robot lasers.</summary>
    public required ushort FallLaserVelocity { get; init; }
}
