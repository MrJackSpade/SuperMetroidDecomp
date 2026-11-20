namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Verification's way to put the ending straight into one of its cast's phases (#2571).</summary>
internal static class EndingCreditsStateCastAccess
{
    /// <summary>The cast views of the ending.</summary>
    extension(EndingCreditsState self)
    {
        /// <summary>Puts the ending in the cast's <paramref name="phase"/>, showing the cast in that phase.</summary>
        /// <param name="phase">The cast's phase.</param>
        internal void ShowCastPhase(EndingCastPhase phase)
        {
            PrivateState.Set(self, nameof(EndingCreditsState.Scene), new EndingCastScene(phase));
        }
    }
}
