using SuperMetroid.Core.Frontend;

/// <summary>Verification access to <see cref="SuperMetroidGame"/> members production does not use.</summary>
internal static class SuperMetroidGameAccess
{
    /// <summary>Exposes selected frontend identity and direct-room verification setup.</summary>
    extension(SuperMetroidGame self)
    {
        /// <summary>Identity of the installed area-map presentation, if present.</summary>
        internal string? MapPresentationIdentity => self.Artwork.MapPresentation?.ContentIdentity;

        /// <summary>Random word retained by the frontend dispatcher.</summary>
        internal ushort DispatcherRandomNumber => self.FrontendRandomOwner.RandomNumber;

        /// <summary>
        /// Initialize the ordinary gameplay runtime for a friend verifier that
        /// directly loads a retail room. This uses the same runtime setup as a new
        /// game, then omits only the title/intro/fade dispatcher frames. It is not
        /// an alternate playable entry point or a substitute for the separate
        /// full-startup integration test.
        /// </summary>
        internal void InitializeDirectRoomVerification()
        {
            if (self.runtime is not null || self.GameState != SuperMetroidGameState.Reset)
                throw new InvalidOperationException(
                    "Direct-room verification requires a fresh frontend.");
            if (!((bool)(self.SetupSelectedGame())))
                throw new InvalidOperationException(
                    "New-game runtime setup did not select the Ceres arrival path.");
            self.GameState = SuperMetroidGameState.MainGameplay;
        }

        /// <summary>
        /// Exposes the cartridge coroutine phase only to the friend verification/debug hosts.
        /// Production UI code continues to consume the coarser public game state.
        /// </summary>
        internal DoorTransitionPhase DoorTransitionPhaseForVerification => self.doorTransition.Phase;
    }
}
