namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Where an oval stage of the Power Bomb's blast stands (#2553): the oval it shows on its next update, one oval an update,
/// from its first to its last. The stage ends on the update that shows its last oval, so no play stands past it.
/// </summary>
internal readonly record struct PowerBombOvalPlay
{
    /// <summary>The stage's ovals, by horizontal radius in pixels, in the order it shows them; at least one.</summary>
    private readonly IReadOnlyList<int> _radii;

    /// <summary>The place in <see cref="_radii"/> of the oval the stage shows next, within the list.</summary>
    private readonly int _next;

    /// <summary>A play standing at <paramref name="next"/> of <paramref name="radii"/>, which holds it.</summary>
    private PowerBombOvalPlay(IReadOnlyList<int> radii, int next)
    {
        _radii = radii;
        _next = next;
    }

    /// <summary>A stage's play from its first oval of <paramref name="radii"/>, checked once to hold one.</summary>
    /// <exception cref="ArgumentException">The stage has no oval.</exception>
    public static PowerBombOvalPlay Starting(IReadOnlyList<int> radii) =>
        Ensure.NotNull(radii).Count != 0
            ? new(radii, 0)
            : throw new ArgumentException("A Power Bomb oval stage shows at least one oval.", nameof(radii));

    /// <summary>The horizontal radius, in pixels, of the oval the stage shows next.</summary>
    public int Radius => _radii[_next];

    /// <summary>The play after the next oval shows; null when that oval is the stage's last, which ends the stage.</summary>
    public PowerBombOvalPlay? AfterShowing => _next + 1 < _radii.Count ? new(_radii, _next + 1) : null;
}
