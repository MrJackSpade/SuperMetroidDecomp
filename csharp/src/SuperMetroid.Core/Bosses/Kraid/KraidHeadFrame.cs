namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// One pose of Kraid's head: the head image it shows, and the two mouth shapes projectiles are tested against while the
/// pose is shown.
/// </summary>
/// <param name="FrameName">The installed name of the head image the pose shows in his body sheet (<see cref="KraidBodyFrameNames"/>).</param>
/// <param name="VulnerableShape">The shape a shot hurts Kraid through while this pose is shown.</param>
/// <param name="InvulnerableShape">The inner shape that deflects shots, or null for the closed mouth, which has none.</param>
internal readonly record struct KraidHeadFrame(string FrameName, KraidMouthHitbox VulnerableShape, KraidMouthHitbox? InvulnerableShape)
{
    /// <summary>The closed mouth.</summary>
    internal static KraidHeadFrame Closed { get; } = new(KraidBodyFrameNames.HeadMouthClosed, KraidMouthHitbox.ClosedVulnerable, null);

    /// <summary>The first opening stage.</summary>
    internal static KraidHeadFrame Opening { get; } = new(KraidBodyFrameNames.HeadMouthOpening, KraidMouthHitbox.OpeningVulnerable, KraidMouthHitbox.OpeningInvulnerable);

    /// <summary>The second, wider opening stage.</summary>
    internal static KraidHeadFrame Wider { get; } = new(KraidBodyFrameNames.HeadMouthWider, KraidMouthHitbox.WiderVulnerable, KraidMouthHitbox.WiderInvulnerable);

    /// <summary>The fully open mouth, the pose whose timer paces the spat rocks.</summary>
    internal static KraidHeadFrame Open { get; } = new(KraidBodyFrameNames.HeadMouthOpen, KraidMouthHitbox.OpenVulnerable, KraidMouthHitbox.OpenInvulnerable);
}
