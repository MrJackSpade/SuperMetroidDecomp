namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>What a laser or claw program cue does.</summary>
internal enum SpacePirateProjectileCueKind
{
    /// <summary>Installs the movement routine.</summary>
    StartMoving,

    /// <summary>
    /// Installs the movement routine and runs it at once:
    /// the laser moves on the update it starts.
    /// </summary>
    StartMovingNow,
}
