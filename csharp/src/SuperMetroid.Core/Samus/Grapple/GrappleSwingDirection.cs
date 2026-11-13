namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>
/// One of the <see cref="Count"/> directions a swinging rope is drawn and carried in: the direction nearest the rope's
/// mirrored angle chooses both where Samus's body hangs from the rope and which swing picture shows her. Directions are
/// numbered from zero at the angle zero, each an equal share of the turn on from the last; an angle halfway between two
/// takes the later one, and the last half share before a full turn belongs to direction zero.
/// </summary>
public readonly record struct GrappleSwingDirection
{
    /// <summary>The directions one full turn of the rope holds.</summary>
    public const int Count = 32;

    /// <summary>Keeps an already range-checked direction number.</summary>
    private GrappleSwingDirection(int number) => Number = number;

    /// <summary>The direction's place in the turn, 0 through <see cref="Count"/> less one, counted on from the angle zero.</summary>
    public int Number { get; }

    /// <summary>The direction nearest <paramref name="angle"/>.</summary>
    public static GrappleSwingDirection Nearest(TrigonometryAngle angle) => new(angle.NearestDirection(Count));

    /// <summary>The direction at place <paramref name="number"/>, 0 through <see cref="Count"/> less one.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="number"/> is outside the turn's directions.</exception>
    public static GrappleSwingDirection FromNumber(int number) => new(Ensure.BetweenInclusive(0, Count - 1, number));

    /// <summary>Every direction, in order around the turn from the angle zero.</summary>
    public static IReadOnlyList<GrappleSwingDirection> All { get; } = [.. Enumerable.Range(0, Count).Select(FromNumber)];
}
