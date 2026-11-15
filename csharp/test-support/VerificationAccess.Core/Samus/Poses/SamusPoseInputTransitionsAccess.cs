using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Verification access to <see cref="SamusPoseInputTransitions"/> members production does not use.</summary>
internal static class SamusPoseInputTransitionsAccess
{
    /// <summary>Finds the first pose transition for held and newly pressed gameplay actions.</summary>
    extension(SamusPoseInputTransitions)
    {
        /// <summary>
        /// Finds the first matching transition for held and newly pressed gameplay actions.
        /// </summary>
        /// <param name="currentPose">Current pose whose input list is selected.</param>
        /// <param name="heldActions">The held actions.</param>
        /// <param name="newlyPressedActions">The newly pressed actions.</param>
        internal static SamusPoseTransition? Find(
            SamusPoseId currentPose,
            GameplayAction heldActions,
            GameplayAction newlyPressedActions) =>
            SamusPoseInputTransitions.Lookup(currentPose, heldActions, newlyPressedActions).Transition;
    }
}
