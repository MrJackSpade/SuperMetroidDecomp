namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Verification access to <see cref="IntroCeresFlightState"/> members production does not use.</summary>
internal static class IntroCeresFlightStateAccess
{
    /// <summary>Advances the flight and its queued music in the production update order.</summary>
    extension(IntroCeresFlightState self)
    {
        /// <summary>
        /// One standalone game frame: the flight's dispatch, then the music queue handler the
        /// game runs after it, so the flight's own music wait drains as in production.
        /// </summary>
        internal void StepFrame()
        {
            self.Step();
            self.audio
                .AdvanceFrame(default);
        }
    }
}
