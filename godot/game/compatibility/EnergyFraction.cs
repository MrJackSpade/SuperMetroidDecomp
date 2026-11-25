namespace Game.Compatibility;

/// <summary>
/// The part of one energy Samus holds beyond her whole energy, which ongoing damage is taken from first, as the
/// cartridge takes it: heat, lava, acid, spikes and a chewing plant take her energy a part at a time, and a part that
/// is more than the one she holds borrows a whole energy at once, the rest of that energy becoming the part she
/// holds. So the first update of such damage takes an energy, where a count that waited for a whole to build up
/// would take it some updates later. The part is counted in 65536ths of an energy and is kept from one damage to the
/// next.
/// </summary>
/// <remarks>
/// A suit divides the update's damage counted in 256ths of an energy, the finer part dropped, before it is taken.
/// Damage that would leave her below no energy leaves none, and no part held. Read by her ongoing damage on every
/// update she takes any; hers for as long as she is.
/// </remarks>
public sealed class EnergyFraction
{
    private const int PartsPerEnergy = 65536, CoarseParts = 256;

    /// <summary>How many hundredths of an energy the parts are given in, and the share of them that is a whole number of parts.</summary>
    private const int Hundredths = 100;

    private int _held;

    /// <summary>
    /// Takes one update's ongoing damage, given in hundredths of an energy and divided by <paramref name="suitDivisor"/>,
    /// from <paramref name="energy"/>. Returns how many whole energy it takes.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The damage is no whole number of the cartridge's parts, or the divisor is not positive.</exception>
    public int Take(int hundredths, int suitDivisor, int energy)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(hundredths);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(suitDivisor);
        long scaled = (long)hundredths * PartsPerEnergy;
        if (scaled % Hundredths != 0)
            throw new ArgumentOutOfRangeException(nameof(hundredths), hundredths, "Ongoing damage is a whole number of 65536ths of an energy.");
        long parts = scaled / Hundredths;
        if (suitDivisor > 1)
            parts = parts / CoarseParts / suitDivisor * CoarseParts;
        int whole = (int)(parts / PartsPerEnergy), part = (int)(parts % PartsPerEnergy);
        int taken = whole + (part > _held ? 1 : 0);
        if (taken > energy)
        {
            _held = 0;
            return energy;
        }

        _held = (_held - part + PartsPerEnergy) % PartsPerEnergy;
        return taken;
    }
}
