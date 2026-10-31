using static SuperMetroid.Core.Bosses.Draygon.DraygonIntroDanceDefinitions;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's opening Evir dance: how long each Evir waits and the step of the traced path it takes.</summary>
internal static class DraygonIntroDance
{
    /// <summary>How many dance updates the dancing Evir in sprite-object slot <paramref name="slotIndex"/> waits before its first step.</summary>
    /// <param name="slotIndex">The Evir's sprite-object slot, <see cref="DraygonIntroDanceDefinitions.FirstDancerSlot"/> or one of the three after it.</param>
    /// <exception cref="ArgumentOutOfRangeException">The slot does not hold a dancing Evir.</exception>
    public static int StartDelayForSlot(int slotIndex)
    {
        int dancer = slotIndex - FirstDancerSlot;
        if ((uint)dancer >= (uint)StartDelays.Length)
            throw new ArgumentOutOfRangeException(nameof(slotIndex));
        return StartDelays[dancer];
    }

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
