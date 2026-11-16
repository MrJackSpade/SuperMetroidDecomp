namespace SuperMetroid.Core.Samus;

/// <summary>Transitions of <see cref="HudItem"/>.</summary>
public static class HudItems
{
    /// <summary>The selection ring, in the order the select button steps through it; X-ray wraps back to the beam.</summary>
    private static readonly HudItem[] SelectionRing =
    [
        HudItem.Beam,
        HudItem.Missile,
        HudItem.SuperMissile,
        HudItem.PowerBomb,
        HudItem.Grapple,
        HudItem.XRay,
    ];

    /// <summary>Every HUD item, in the order the select button steps through them.</summary>
    public static IReadOnlyList<HudItem> All => SelectionRing;

    /// <summary>The next entry of the selection ring, wrapping after X-ray.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="item"/> is not on the ring.</exception>
    public static HudItem Next(HudItem item)
    {
        int place = Array.IndexOf(SelectionRing, item);
        if (place < 0)
            throw new InvalidOperationException($"{nameof(HudItem)} {item} is not on the selection ring.");
        return SelectionRing[(place + 1) % SelectionRing.Length];
    }
}
