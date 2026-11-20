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

    /// <summary>
    /// The times the beam's continuing steps queue its sound: one on each of its first seven steps, none after.
    /// </summary>
    public const int SoundAttempts = 7;

    /// <summary>The rainbow beam's starting and narrowest width.</summary>
    public static readonly RotationAngle RainbowNarrowestWidth = RotationAngle.FromSteps(2);

    /// <summary>The rainbow beam's widest angular width: twelve steps.</summary>
    public static readonly RotationAngle RainbowWidestWidth = RotationAngle.FromSteps(12);

    /// <summary>How far one widening or narrowing of the rainbow beam changes its width: one and a half steps.</summary>
    public static readonly RotationAngle RainbowWidthStep = RotationAngle.FromStepFraction(3, 2);

    /// <summary>
    /// Where the beam's explosions burst from Samus, whom each then follows, in the order they burst: the first beam explosion takes the
    /// first, and the last is followed by the first again (#2258).
    /// </summary>
    public static IReadOnlyList<Offset16<RoomPixels>> ExplosionOffsets { get; } =
        [new(6, 2), new(-4, 5), new(2, -4), new(3, 6), new(-6, -2), new(8, -6), new(0, 7), new(-8, -7)];
}
