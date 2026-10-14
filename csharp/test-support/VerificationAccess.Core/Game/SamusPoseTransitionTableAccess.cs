using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Verification access to <see cref="SamusPoseTransitionTable"/> members production does not use.</summary>
internal static class SamusPoseTransitionTableAccess
{
    /// <summary>Finds the first pose transition after held and newly pressed buttons are mapped to canonical bits.</summary>
    extension(SamusPoseTransitionTable)
    {
        /// <summary>
        /// Finds the first matching transition for canonical Super Metroid input bits.
        /// </summary>
        /// <param name="bus">CPU address space containing bank-$91 transition data.</param>
        /// <param name="currentPose">Current pose whose pointer-table entry is selected.</param>
        /// <param name="canonicalHeldInput">
        /// Held input after custom bindings have been translated to the table's canonical bits.
        /// Directional bits already equal their SNES hardware values; canonical action bits are
        /// jump=$0080, shoot=$0040, aim-down=$0020, aim-up=$0010.
        /// </param>
        /// <param name="canonicalNewInput">Rising-edge form of the same canonical input.</param>
        internal static SamusPoseTransition? Find(
            ISnesAddressSpace bus,
            SamusPoseId currentPose,
            ushort canonicalHeldInput,
            ushort canonicalNewInput) =>
            SamusPoseTransitionTable.Lookup(bus, currentPose, canonicalHeldInput, canonicalNewInput).Transition;
    }
}
