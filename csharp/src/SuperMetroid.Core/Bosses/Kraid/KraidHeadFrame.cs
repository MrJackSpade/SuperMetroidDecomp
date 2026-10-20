namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// One pose of Kraid's head: how far the mouth is open, and the two mouth shapes projectiles are tested against while
/// the pose is shown. The pose also selects the head image.
/// </summary>
/// <param name="MouthStage">How far the mouth is open, from zero (closed) to <see cref="FullyOpenMouthStage"/>.</param>
/// <param name="VulnerableShape">The shape a shot hurts Kraid through while this pose is shown.</param>
/// <param name="InvulnerableShape">The inner shape that deflects shots, or null for the closed mouth, which has none.</param>
internal readonly record struct KraidHeadFrame([property: IntegerQuantity("how far open the mouth is, counted from closed")] int MouthStage, KraidMouthHitbox VulnerableShape, KraidMouthHitbox? InvulnerableShape)
{
    /// <summary>The mouth stage of a fully open head, the stage whose timer paces the spat rocks.</summary>
    internal const int FullyOpenMouthStage = 3;

    /// <summary>The closed mouth.</summary>
    internal static KraidHeadFrame Closed { get; } = new(0, KraidMouthHitbox.ClosedVulnerable, null);

    /// <summary>The first opening stage.</summary>
    internal static KraidHeadFrame Opening { get; } = new(1, KraidMouthHitbox.OpeningVulnerable, KraidMouthHitbox.OpeningInvulnerable);

    /// <summary>The second, wider opening stage.</summary>
    internal static KraidHeadFrame Wider { get; } = new(2, KraidMouthHitbox.WiderVulnerable, KraidMouthHitbox.WiderInvulnerable);

    /// <summary>The fully open mouth.</summary>
    internal static KraidHeadFrame Open { get; } = new(FullyOpenMouthStage, KraidMouthHitbox.OpenVulnerable, KraidMouthHitbox.OpenInvulnerable);

    /// <summary>The four poses, from closed to fully open, indexed by mouth stage.</summary>
    internal static IReadOnlyList<KraidHeadFrame> ByMouthStage { get; } = [Closed, Opening, Wider, Open];
}
