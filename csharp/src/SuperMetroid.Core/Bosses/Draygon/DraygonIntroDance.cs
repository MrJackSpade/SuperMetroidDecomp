using static SuperMetroid.Core.Bosses.Draygon.DraygonIntroDanceDefinitions;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's opening Evir dance: the step of the traced path each Evir takes.</summary>
internal static partial class DraygonIntroDance
{

    /// <summary>
    /// Whether an Evir <paramref name="step"/> updates into its dance has made the traced path's last move, so the update
    /// ends its dance: the step just after the path's last move. Every Evir reaches it before the dance ends, and none
    /// dances on past it (#2558).
    /// </summary>
    public static bool HasDanced(int step) => step == TracedPath.Length;

    /// <summary>The move of step <paramref name="step"/> of the traced path. A step outside the path is corrupt state.</summary>
    /// <exception cref="InvalidDataException">The step is outside the traced path.</exception>
    public static Offset16<RoomPixels> Move(int step) =>
        step >= 0 && step < TracedPath.Length
            ? TracedPath[step]
            : throw new InvalidDataException($"Draygon intro movement step {step} is outside the traced path.");
}
