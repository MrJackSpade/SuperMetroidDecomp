namespace SuperMetroid.Core.Samus;

/// <summary>Verification access to <see cref="SamusDraygonGrabbedState"/> members production does not use.</summary>
internal static class SamusDraygonGrabbedStateAccess
{
    /// <summary>Exposes <see cref="SamusDraygonGrabbedState"/> state that only verification reads.</summary>
    extension(SamusDraygonGrabbedState self)
    {
        /// <summary>
        /// Presses of the directions counted toward escaping, 0 through 65535, on the count Crystal Flash's ammunition drain and the suit
        /// pickup's stage share (<see cref="SharedEscapeAndDrainCount{TStage}"/>), so it holds any value those leave, including a
        /// wrapped countdown. A press continues from zero past 65535, and the release test is the shared count's own
        /// (<see cref="SharedEscapeAndDrainCount{TStage}.EscapePressesReach"/>).
        /// </summary>
        public int EscapeButtonCounter =>
            PrivateState.Field<SamusDraygonGrabbedState, SharedEscapeAndDrainCount<Abilities.SamusSuitPickupStage>>(self, "escapePresses").EscapePresses;
    }
}
