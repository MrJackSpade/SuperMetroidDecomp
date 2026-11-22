namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// One of Norfair Ridley's health bands (#1934): how hard his flight and his lunge seek their targets, how fast and high
/// his pogo bounces launch him, and the health shade his belly shows. He is in the lowest wounded band whose threshold his
/// health is under (<see cref="NorfairRidleyHealthBandDefinitions.WoundedLowestFirst"/>), and in
/// <see cref="NorfairRidleyHealthBandDefinitions.Unhurt"/> above them all.
/// </summary>
public sealed partial class NorfairRidleyHealthBand
{
    /// <summary>Defines a band.</summary>
    /// <param name="hoverDivisor">The acceleration divisor of his hover, wall sweep and power-bomb dodge.</param>
    /// <param name="grabDivisor">The acceleration divisor of his lunge at Samus.</param>
    /// <param name="pogoLaunches">The launch of each of the four pogo speed patterns a bounce draws among, slowest first.</param>
    /// <param name="healthShade">The health colouring his belly shows, or null for his own colours.</param>
    internal NorfairRidleyHealthBand(int hoverDivisor, int grabDivisor,
        IReadOnlyList<RidleyPogoDefinition> pogoLaunches, RidleyHealthShade? healthShade)
    {
        HoverDivisor = hoverDivisor;
        GrabDivisor = grabDivisor;
        PogoLaunch = WeightedRandomChoice<RidleyPogoDefinition>.InTurn(RidleyRandomDefinitions.PogoChoice, [.. pogoLaunches]);
        HealthShade = healthShade;
    }


    /// <summary>
    /// The acceleration divisor of his hover, wall sweep and power-bomb dodge: the smaller it is, the faster he seeks his
    /// target.
    /// </summary>
    public int HoverDivisor { get; }

    /// <summary>The acceleration divisor of his lunge at Samus.</summary>
    public int GrabDivisor { get; }

    /// <summary>
    /// The launch a pogo bounce takes (#2510): one of the four pogo speed patterns, which the current random number's
    /// <see cref="RidleyRandomDefinitions.PogoChoice"/> takes in turn, slowest first; each faster pattern adds an eighth of a
    /// pixel an update sideways.
    /// </summary>
    public WeightedRandomChoice<RidleyPogoDefinition> PogoLaunch { get; }

    /// <summary>The health colouring his belly shows in this band, or null for his own colours (#2242).</summary>
    public RidleyHealthShade? HealthShade { get; }

    /// <summary>The band Ridley is in at <paramref name="health"/>.</summary>
    internal static NorfairRidleyHealthBand Of(int health)
    {
        foreach ((int healthBelow, NorfairRidleyHealthBand band) in NorfairRidleyHealthBandDefinitions.WoundedLowestFirst)
        {
            if (health < healthBelow)
                return band;
        }
        return NorfairRidleyHealthBandDefinitions.Unhurt;
    }
}
