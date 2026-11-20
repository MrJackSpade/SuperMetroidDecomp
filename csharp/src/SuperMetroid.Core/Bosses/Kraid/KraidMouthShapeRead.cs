namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>One mouth-collision read at a place of Kraid's head animation: a shape, the closed mouth's missing shape, or no pose at all.</summary>
internal readonly record struct KraidMouthShapeRead
{
    /// <summary>A value of its parts; only its own operations make one.</summary>
    /// <param name="kind">What the read finds.</param>
    /// <param name="shape">The shape found, for <see cref="KraidMouthShapeReadKind.Shape"/>.</param>
    private KraidMouthShapeRead(KraidMouthShapeReadKind kind, KraidMouthHitbox shape)
    {
        Kind = kind;
        Shape = shape;
    }

    /// <summary>What the read finds.</summary>
    public KraidMouthShapeReadKind Kind { get; init; }

    /// <summary>The shape found, for <see cref="KraidMouthShapeReadKind.Shape"/>.</summary>
    public KraidMouthHitbox Shape { get; init; }

    /// <summary>A read that finds no pose (<see cref="Compatibility.KraidMouthTrailsHead"/>).</summary>
    internal static KraidMouthShapeRead NoPose { get; } = new(KraidMouthShapeReadKind.NoPose, default);

    /// <summary>The vulnerable shape of <paramref name="frame"/>.</summary>
    internal static KraidMouthShapeRead VulnerableOf(KraidHeadFrame frame) => new(KraidMouthShapeReadKind.Shape, frame.VulnerableShape);

    /// <summary>The invulnerable shape of <paramref name="frame"/>, or the closed mouth's missing shape.</summary>
    internal static KraidMouthShapeRead InvulnerableOf(KraidHeadFrame frame) => frame.InvulnerableShape is { } shape
        ? new(KraidMouthShapeReadKind.Shape, shape)
        : new(KraidMouthShapeReadKind.NoShape, default);
}
