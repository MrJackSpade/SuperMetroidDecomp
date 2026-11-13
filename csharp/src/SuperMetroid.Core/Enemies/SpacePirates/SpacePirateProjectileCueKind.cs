namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>What a laser or claw animation cue does.</summary>
internal enum SpacePirateProjectileCueKind
{
    /// <summary>Installs the movement behaviour.</summary>
    StartMoving,

    /// <summary>
    /// Installs the movement behaviour and runs it at once:
    /// the laser moves on the update it starts.
    /// </summary>
    StartMovingNow,
}
