namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>What one of Phantoon's destroyable flames keeps from its spawn (#2554): its orbit, its rain, or, as a casual flame, nothing until it lands.</summary>
internal abstract record PhantoonFlameKeeps
{
    /// <summary>Closes the hierarchy to the cases declared here.</summary>
    private PhantoonFlameKeeps()
    {
    }

    /// <summary>A rage or spiral flame, which circles the body by <paramref name="State"/>.</summary>
    /// <param name="State">The flame's orbit.</param>
    internal sealed record Orbit(PhantoonOrbitingFlameState State, PhantoonFlameMotion Circling) : PhantoonFlameKeeps;

    /// <summary>A rain flame, which hangs by <paramref name="State"/> before it falls.</summary>
    /// <param name="State">The flame's hang.</param>
    internal sealed record Rain(PhantoonRainFlameState State) : PhantoonFlameKeeps;

    /// <summary>A casual flame, which falls to the floor and keeps its landing from then on.</summary>
    internal sealed record Casual : PhantoonFlameKeeps;
}
