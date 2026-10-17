namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>One mouth-collision read at a head-program cursor: a shape, the closed mouth's missing shape, or no pose at all.</summary>
/// <param name="Kind">What the read finds.</param>
/// <param name="Shape">The shape found, for <see cref="KraidMouthShapeReadKind.Shape"/>.</param>
internal readonly record struct KraidMouthShapeRead(KraidMouthShapeReadKind Kind, KraidMouthHitbox Shape)
{
    /// <summary>A read landing outside every head pose.</summary>
    internal static KraidMouthShapeRead OutsideHeadFrames { get; } = new(KraidMouthShapeReadKind.OutsideHeadFrames, default);

    /// <summary>The vulnerable shape of <paramref name="frame"/>.</summary>
    internal static KraidMouthShapeRead VulnerableOf(KraidHeadFrame frame) => new(KraidMouthShapeReadKind.Shape, frame.VulnerableShape);

    /// <summary>The invulnerable shape of <paramref name="frame"/>, or the closed mouth's missing shape.</summary>
    internal static KraidMouthShapeRead InvulnerableOf(KraidHeadFrame frame) => frame.InvulnerableShape is { } shape
        ? new(KraidMouthShapeReadKind.Shape, shape)
        : new(KraidMouthShapeReadKind.NoShape, default);
}
