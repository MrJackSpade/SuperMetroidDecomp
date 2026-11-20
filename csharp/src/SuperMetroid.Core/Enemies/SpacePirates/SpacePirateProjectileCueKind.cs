namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>What a laser or claw animation cue does.</summary>
internal enum SpacePirateProjectileCueKind
{
    /// <summary>Throws the claw: it flies out and back from the next pass.</summary>
    ThrowClaw,

    /// <summary>Fires the laser and runs its flight at once: the laser moves on the update it fires.</summary>
    FireLaser,
}
