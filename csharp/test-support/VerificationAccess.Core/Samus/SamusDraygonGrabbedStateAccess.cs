namespace SuperMetroid.Core.Samus;

/// <summary>Verification access to <see cref="SamusDraygonGrabbedState"/> members production does not use.</summary>
internal static class SamusDraygonGrabbedStateAccess
{
    /// <summary>The counts the shared count can hold.</summary>
    private const int CountsHeld = ushort.MaxValue + 1;

    /// <summary>Exposes <see cref="SamusDraygonGrabbedState"/> state that only verification reads.</summary>
    extension(SamusDraygonGrabbedState self)
    {
        /// <summary>
        /// Presses of the directions counted toward escaping, 0 through 65535, on the count Crystal Flash's ammunition drain and the suit
        /// pickup's stage share (<see cref="SharedEscapeAndDrainCount{TStage}"/>), so it holds any value those leave, including a
        /// wrapped countdown. A press continues from zero past 65535. The count is read as Draygon's escape check reads it
        /// (<see cref="SharedEscapeAndDrainCount{TStage}.EscapePressesReach"/>, #2604): the one target it reaches whose next
        /// it does not.
        /// </summary>
        public int EscapeButtonCounter => PressesReached(self.escapePresses);
    }

    /// <summary>
    /// The presses <paramref name="presses"/> holds: the target its escape check reaches whose next target it does not. A
    /// count reaches the targets from 32767 below it up to itself, so exactly one target is the last it reaches.
    /// </summary>
    /// <exception cref="InvalidOperationException">No target is the last the count reaches.</exception>
    private static int PressesReached<TStage>(SharedEscapeAndDrainCount<TStage> presses)
        where TStage : notnull
    {
        for (int target = 0; target < CountsHeld; target++)
        {
            if (presses.EscapePressesReach(target) && !presses.EscapePressesReach((target + 1) % CountsHeld))
                return target;
        }
        throw new InvalidOperationException("The escape presses reach every target or none.");
    }
}
