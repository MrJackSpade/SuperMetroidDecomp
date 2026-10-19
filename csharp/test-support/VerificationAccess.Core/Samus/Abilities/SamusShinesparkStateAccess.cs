using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Verification access to <see cref="SamusShinesparkState"/> members production does not use.</summary>
internal static class SamusShinesparkStateAccess
{
    /// <summary>Counts released crash echoes and advances their projectile motion after the shinespark collision.</summary>
    extension(SamusShinesparkState self)
    {
        /// <summary>Number of enabled departing echo drawings, independent of slot ownership.</summary>
        internal int ReleasedCrashEchoCount =>
            (self.FirstReleasedCrashEchoSlot.Active ? 1 : 0) +
            (self.SecondReleasedCrashEchoSlot.Active ? 1 : 0);

        /// <summary>
        /// Runs <c>ProjPreInstr_SpeedEcho</c> at <c>$90:D4D2</c> for the two fixed crash-echo
        /// projectile slots. Call this during alpha projectile processing, before Samus moves.
        /// </summary>
        internal void StepReleasedCrashEchoProjectiles(
            ISnesAddressSpace bus,
            SamusState samus,
            Position16<RoomPixels> layer1)
        {
            Ensure.NotNull(bus);
            Ensure.NotNull(samus);
            SamusShinesparkState.StepReleasedCrashEcho(samus, layer1, self.FirstReleasedCrashEchoSlot);
            SamusShinesparkState.StepReleasedCrashEcho(samus, layer1, self.SecondReleasedCrashEchoSlot);
        }
    }
}
