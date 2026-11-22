using SuperMetroid.Core.Input;
namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// Read-only selection of the prospective pose from the input transitions.
/// </summary>
/// <remarks>
/// The selection stops before applying a prospective pose. The runtime owns the
/// transition side effects. An undefined pose fails loudly. The
/// returned lookup states which input transition won for a precise
/// held/newly-pressed input chord.
/// </remarks>
public static partial class SamusPoseInputTransitions
{

    /// <summary>Held Start and item selection, which the pose's input transitions do not read.</summary>
    private const GameplayAction StartAndItemSelect = GameplayAction.Start | GameplayAction.ItemSelect;

    /// <summary>
    /// Looks the pose's input transitions up: a new pose, the pose's no-input fallback, or keeping the pose.
    /// </summary>
    /// <remarks>
    /// No input, or input no transition of the pose matches, takes the pose's no-input fallback. A pose with
    /// no transitions keeps its pose while any input is held, and so does a match whose target is the current pose.
    /// Running-gun pose <see cref="SamusPoseId.MovingRightGunExtendedPose"/> keeps itself while Right+Shoot is held,
    /// but Shoot alone matches none of its transitions and falls back to standing pose
    /// <see cref="SamusPoseId.FacingRightNormalPose"/>.
    /// </remarks>
    public static SamusPoseTransitionLookup Lookup(
        SamusPoseId currentPose,
        GameplayAction heldActions,
        GameplayAction newlyPressedActions)
    {
        // With no action held, no input transition matches and the pose's definition decides, without reading the
        // pose's input transitions at all.
        if (heldActions == GameplayAction.None)
            return SamusPoseTransitionLookup.PoseDefinitionFallback;

        // Start and item selection never participate in pose chords: the lookup reads only the directions and the
        // actions its chords name.
        GameplayAction held = heldActions & ~StartAndItemSelect;
        GameplayAction newlyPressed = newlyPressedActions & ~StartAndItemSelect;

        SamusPoseInputMatch match = SamusPoseInputRules.Match(SamusPoseTraits.Of(currentPose).Inputs, held, newlyPressed);
        if (match is not SamusPoseInputMatch.Matched { Choice: var transition })
            return match.HasConditions ? SamusPoseTransitionLookup.PoseDefinitionFallback : SamusPoseTransitionLookup.KeepsPose;

        // A match that keeps this pose keeps it, and its fallback is not consulted.
        return transition.ProspectivePose == currentPose
            ? SamusPoseTransitionLookup.KeepsPose
            : SamusPoseTransitionLookup.Choosing(transition);
    }
}
