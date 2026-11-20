namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Verification access to the arrived Baby of Mother Brain's late fight.</summary>
internal static class BabyMetroidArrivalAccess
{
    /// <summary>Reads the Baby a check has had arrive.</summary>
    extension(BabyMetroidArrival self)
    {
        /// <summary>The arrived Baby; a check reads it only once the final beam's spawn has built it.</summary>
        /// <exception cref="InvalidOperationException">The Baby has not arrived.</exception>
        internal BabyMetroidCutsceneState Arrived() => self switch
        {
            BabyMetroidArrival.Arrived arrived => arrived.Baby,
            _ => throw new InvalidOperationException("The check reads the late fight's Baby before it has arrived."),
        };
    }
}
