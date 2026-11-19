namespace SuperMetroid.Core.Progression;

/// <summary>Verification access to <see cref="GameTimeState"/> members production does not use.</summary>
internal static class GameTimeStateAccess
{
    /// <summary>Exposes <see cref="GameTimeState"/> state that only verification reads.</summary>
    extension(GameTimeState self)
    {
        /// <summary>The seconds component: 0 through 59.</summary>
        public int Seconds => self.Reading.Seconds;
    }
}
