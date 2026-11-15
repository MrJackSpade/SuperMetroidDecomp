namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// One of Norfair Ridley's health bands (#1934): how hard his flight and his lunge seek their targets, how fast and high
/// his pogo bounces launch him, and the health shade his belly shows. He is in the lowest band whose
/// <see cref="HealthBelow"/> his health is under, and in <see cref="NorfairRidleyHealthBandDefinitions.Unhurt"/> above
/// them all.
/// </summary>
public sealed class NorfairRidleyHealthBand
{
    /// <summary>Defines a band.</summary>
    /// <param name="healthBelow">The health below which Ridley is in the band, or null for the band he starts in.</param>
    /// <param name="hoverDivisor">The acceleration divisor of his hover, wall sweep and power-bomb dodge.</param>
    /// <param name="grabDivisor">The acceleration divisor of his lunge at Samus.</param>
    /// <param name="pogoLaunches">The launch of each of the four pogo speed patterns a bounce draws among.</param>
    /// <param name="healthShade">The installed health shade his belly shows, or null for his own colours.</param>
    internal NorfairRidleyHealthBand(int? healthBelow, int hoverDivisor, int grabDivisor,
        IReadOnlyList<RidleyPogoDefinition> pogoLaunches, int? healthShade)
    {
        HealthBelow = healthBelow;
        HoverDivisor = hoverDivisor;
        GrabDivisor = grabDivisor;
        PogoLaunches = pogoLaunches;
        HealthShade = healthShade;
    }

    /// <summary>The health below which Ridley is in this band, or null for the band he starts in.</summary>
    public int? HealthBelow { get; }

    /// <summary>
    /// The acceleration divisor of his hover, wall sweep and power-bomb dodge: the smaller it is, the faster he seeks his
    /// target.
    /// </summary>
    public int HoverDivisor { get; }

    /// <summary>The acceleration divisor of his lunge at Samus.</summary>
    public int GrabDivisor { get; }

    /// <summary>
    /// The launch of each of the four pogo speed patterns a bounce draws among, slowest first: each faster pattern adds
    /// an eighth of a pixel an update sideways.
    /// </summary>
    public IReadOnlyList<RidleyPogoDefinition> PogoLaunches { get; }

    /// <summary>The installed health shade his belly shows in this band, or null for his own colours.</summary>
    public int? HealthShade { get; }

    /// <summary>The band Ridley is in at <paramref name="health"/>.</summary>
    internal static NorfairRidleyHealthBand Of(int health)
    {
        foreach (NorfairRidleyHealthBand band in NorfairRidleyHealthBandDefinitions.LowestFirst)
        {
            if (health < band.HealthBelow)
                return band;
        }
        return NorfairRidleyHealthBandDefinitions.Unhurt;
    }
}
