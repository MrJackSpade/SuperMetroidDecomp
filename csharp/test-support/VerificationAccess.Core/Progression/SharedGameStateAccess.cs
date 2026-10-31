namespace SuperMetroid.Core.Progression;
/// <summary>Verification access to <see cref="SharedGameState"/> members production does not use.</summary>
internal static class SharedGameStateAccess
{
    /// <summary>Reproduces bank-$80's unsigned 16-by-16 multiply with a full 32-bit product.</summary>
    extension(SharedGameState)
    {
        /// <summary>
        /// Unsigned <c>16 x 16 -> 32</c> multiplication from <c>$80:82D6</c>.
        /// The widening casts must happen before multiplication or C# would discard the high
        /// word before returning it.
        /// </summary>
        internal static uint Multiply16By16(ushort left, ushort right) => (uint)left * right;
    }

    /// <summary>Reads and sets the broken Chozo orbs, which production carries only through saves.</summary>
    extension(SharedGameState self)
    {
        /// <summary>Whether the Chozo orb room data numbers <paramref name="orb"/> has been broken.</summary>
        internal bool HasRoomChozoBit(int orb) => self.Progress.ChozoOrbs.Contains(orb);

        /// <summary>Records the Chozo orb room data numbers <paramref name="orb"/> as broken.</summary>
        internal void SetRoomChozoBit(int orb) => self.Progress.ChozoOrbs.Add(orb);
    }
}
