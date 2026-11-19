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
    /// Whether an Evir <paramref name="step"/> updates into its dance has made the traced path's last move, so the update
    /// ends its dance. Every Evir reaches it before the dance ends.
    /// </summary>
    public static bool HasDanced(int step) => step >= TracedPath.Length;

    /// <summary>The move of step <paramref name="step"/> of the traced path. A step outside the path is corrupt state.</summary>
    /// <exception cref="InvalidDataException">The step is outside the traced path.</exception>
    public static Offset16<RoomPixels> Move(int step) =>
        step >= 0 && step < TracedPath.Length
            ? TracedPath[step]
            : throw new InvalidDataException($"Draygon intro movement step {step} is outside the traced path.");
}
