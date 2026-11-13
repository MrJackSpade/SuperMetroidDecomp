namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The charge-flare components a frame shows, as the flare draw draws them after advancing them.</summary>
/// <param name="Count">Components shown: none, the main flare alone, or the flare and both sparks.</param>
/// <param name="Descending">
/// True for Hyper Beam's descending animation, which shows the fast sparks first, then the slow sparks, then the main flare.
/// </param>
[IndependentFlag("whether the authored flare components run in descending order, a fact of the data")]
public readonly record struct ChargeFlareComponents(int Count, bool Descending)
{
    /// <summary>The shown components in drawing order.</summary>
    public IEnumerable<ChargeFlareComponent> DrawOrder
    {
        get
        {
            IEnumerable<ChargeFlareComponent> shown = Enum.GetValues<ChargeFlareComponent>().Take(Count);
            return Descending ? shown.Reverse() : shown;
        }
    }
}
