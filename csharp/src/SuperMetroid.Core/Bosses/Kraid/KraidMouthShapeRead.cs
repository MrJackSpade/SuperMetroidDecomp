namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>One mouth-collision read at a place of Kraid's head animation: a shape, the closed mouth's missing shape, or no pose at all.</summary>
/// <param name="Kind">What the read finds.</param>
internal abstract record KraidMouthShapeRead(KraidMouthShapeReadKind Kind)
{
    /// <summary>A read that finds no pose (<see cref="Compatibility.KraidMouthTrailsHead"/>).</summary>
    internal static KraidMouthShapeRead NoPose { get; } = new NoPoseFound();

    /// <summary>A read that finds the closed mouth's missing shape.</summary>
    private static KraidMouthShapeRead NoShape { get; } = new MissingShape();

    /// <summary>The vulnerable shape of <paramref name="frame"/>.</summary>
    internal static KraidMouthShapeRead VulnerableOf(KraidHeadFrame frame) => new Found(frame.VulnerableShape);

    /// <summary>The invulnerable shape of <paramref name="frame"/>, or the closed mouth's missing shape.</summary>
    internal static KraidMouthShapeRead InvulnerableOf(KraidHeadFrame frame) => frame.InvulnerableShape is { } shape
        ? new Found(shape)
        : NoShape;

    /// <summary>A read that finds a shape.</summary>
    /// <param name="Shape">The shape found.</param>
    internal sealed record Found(KraidMouthHitbox Shape) : KraidMouthShapeRead(KraidMouthShapeReadKind.Shape);

    /// <summary>A read that finds the closed mouth's missing shape.</summary>
    private sealed record MissingShape() : KraidMouthShapeRead(KraidMouthShapeReadKind.NoShape);

    /// <summary>A read that finds no pose.</summary>
    private sealed record NoPoseFound() : KraidMouthShapeRead(KraidMouthShapeReadKind.NoPose);
}
