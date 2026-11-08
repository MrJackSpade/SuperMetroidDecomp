using static SuperMetroid.Core.Bosses.Draygon.DraygonIntroDanceDefinitions;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's opening Evir dance: how long each Evir waits and the step of the traced path it takes.</summary>
internal static class DraygonIntroDance
{
    /// <summary>How many dance updates the dancing Evir spawned <paramref name="dancer"/>-th waits before its first step.</summary>
    /// <param name="dancer">The Evir's place in the order the dance spawns them, from zero.</param>
    /// <exception cref="ArgumentOutOfRangeException">The dance spawns no Evir in that place.</exception>
    public static int StartDelay(int dancer) =>
        dancer is >= 0 and < EvirCount
            ? StartDelays[dancer]
            : throw new ArgumentOutOfRangeException(nameof(dancer), dancer, "The Draygon dance spawns four Evirs.");

    /// <summary>
    /// Step <paramref name="step"/> of the traced path. The dance's updates and the Evirs' delays reach every step; an
    /// Evir stops at the first delete step it takes, but the steps after the deletes stay part of the path. A step
    /// outside the path is corrupt state.
    /// </summary>
    /// <exception cref="InvalidDataException">The step is outside the traced path.</exception>
    public static DraygonIntroMovement Movement(int step) =>
        (uint)step < (uint)TracedPath.Length
            ? TracedPath[step]
            : throw new InvalidDataException($"Draygon intro movement step {step} is outside the traced path.");
}
