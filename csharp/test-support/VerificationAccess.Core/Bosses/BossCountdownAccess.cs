namespace SuperMetroid.Core.Bosses;

/// <summary>Verification access to the boss countdowns' waits, which production starts only with a duration.</summary>
internal static class BossCountdownAccess
{
    /// <summary>Places a boss countdown at a recorded number of updates left.</summary>
    extension(BossCountdown self)
    {
        /// <summary>Gives the countdown <paramref name="updatesLeft"/> updates left: none for a countdown that has run out.</summary>
        internal void SetUpdatesLeft(int updatesLeft) => PrivateState.SetField(self, "wait", UpdateCountdown.For(updatesLeft));

        /// <summary>
        /// The count the cartridge holds for the countdown: one less than its updates left, -1 once it has run out, as a
        /// recorded word read as signed compares with it.
        /// </summary>
        internal int CartridgeCount => self.UpdatesLeft - 1;

        /// <summary>
        /// Places the countdown at the count the cartridge holds for it, one less than its updates left: a count below zero
        /// has run out.
        /// </summary>
        internal void SetCartridgeCount(int count) => self.SetUpdatesLeft(Math.Max(0, count + 1));
    }

    /// <summary>Places a held boss countdown at a recorded number of updates left.</summary>
    extension(HeldBossCountdown self)
    {
        /// <summary>Gives the countdown <paramref name="updatesLeft"/> updates left: none for a countdown that holds run out.</summary>
        internal void SetUpdatesLeft(int updatesLeft) => PrivateState.SetField(self, "wait", UpdateCountdown.For(updatesLeft));
    }
}
