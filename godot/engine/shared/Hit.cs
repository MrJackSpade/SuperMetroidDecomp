using System.Collections.Immutable;

namespace Engine.Shared;

/// <summary>
/// One hit being delivered. It carries every kind that is true of it, general and particular together, so one shot
/// can be several things at once. Whatever it lands on decides what it does.
/// </summary>
public sealed record Hit
{
    /// <summary>A hit of <paramref name="damage"/> carrying <paramref name="kinds"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The damage is negative.</exception>
    /// <exception cref="ArgumentException">The hit carries no kind.</exception>
    public Hit(int damage, HitSide side, params HitKind[] kinds)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(damage);
        if (kinds.Length == 0)
            throw new ArgumentException("A hit carries at least one kind.", nameof(kinds));
        Damage = damage;
        Side = side;
        Kinds = [.. kinds];
    }

    /// <summary>How much damage it does before its target's reaction.</summary>
    public int Damage { get; init; }

    /// <summary>Whose hit it is.</summary>
    public HitSide Side { get; init; }

    /// <summary>Every kind the hit carries.</summary>
    public ImmutableHashSet<HitKind> Kinds { get; init; }

    /// <summary>Which way the hit was travelling, for knocking its target back; zero when it has no direction.</summary>
    public PixelOffset Direction { get; init; }

    /// <summary>Whether what it harms makes no cry of its own, as under a blast or a blow that has its own sound.</summary>
    public bool Quiet { get; init; }

    /// <summary>How many updates fewer than for any other hit what it harms shows that it was hurt: a blow that shows for less.</summary>
    public int FlashFewer { get; init; }

    /// <summary>
    /// Where what delivers the hit is as it lands, when that is something with a place of its own, as a shot is: for
    /// whatever shows where it was struck or reckons from there. Null for a hit that comes of nothing with a place.
    /// </summary>
    public PixelPosition? At { get; init; }

    /// <summary>Whoever caused the hit, when its target needs to know: the launcher of a shot, not the shot.</summary>
    public object? Source { get; init; }

    /// <summary>Whether the hit carries <paramref name="kind"/>.</summary>
    public bool Carries(HitKind kind) => Kinds.Contains(kind);

    /// <summary>Whether the hit carries any of <paramref name="kinds"/>.</summary>
    public bool CarriesAny(IEnumerable<HitKind> kinds) => kinds.Any(Kinds.Contains);
}
