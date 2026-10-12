using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The per-slot platform tables shared with the enemy system (#633).</summary>
internal sealed record PlatformSharedTables
{
    /// <summary>Movement functions.</summary>
    public required PlatformMovementFunctions[] MovementFunctions { get; init; }

    /// <summary>Previous positions.</summary>
    public required ushort[] PreviousPositions { get; init; }

    /// <summary>Vertically moving flags.</summary>
    public required ushort[] VerticallyMovingFlags { get; init; }

    /// <summary>Vertically still flags.</summary>
    public required ushort[] VerticallyStillFlags { get; init; }

    /// <summary>Maximum y speed table indexes.</summary>
    public required ushort[] MaximumYSpeedTableIndexes { get; init; }

    /// <summary>Previous y movement functions.</summary>
    public required ushort[] PreviousYMovementFunctions { get; init; }

    /// <summary>Suspensor platform flags.</summary>
    public required ushort[] SuspensorPlatformFlags { get; init; }
}
