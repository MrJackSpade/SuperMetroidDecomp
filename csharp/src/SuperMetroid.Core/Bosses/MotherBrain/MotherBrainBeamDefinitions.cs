namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The definition data of Mother Brain's rainbow beam: where it leaves her mouth and how long it holds Samus's shots
/// off. Its colours are installed (#1557).
/// </summary>
public static class MotherBrainBeamDefinitions
{
    /// <summary>How far right of the head's X the beam's apex is.</summary>
    public const int MouthXOffset = 14;

    /// <summary>How far below the head's Y the beam's apex is.</summary>
    public const int MouthYOffset = 5;

    /// <summary>
    /// The projectile cooldown, in updates, Mother Brain gives Samus as she lowers her neck to fire the beam: Samus,
    /// still free, cannot shoot again until it runs out, however short or long her own cooldown was.
    /// </summary>
    public const int NeckLoweredSamusCooldown = 8;

    /// <summary>The projectile cooldown, in updates, the beam leaves Samus as it starts and captures her: none.</summary>
    public const int BeamStartedSamusCooldown = 0;

    /// <summary>The projectile cooldown, in updates, the beam leaves Samus as it ends and releases her.</summary>
    public const int BeamEndedSamusCooldown = 8;
}
