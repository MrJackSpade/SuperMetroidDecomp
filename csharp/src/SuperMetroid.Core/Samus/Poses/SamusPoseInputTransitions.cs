using SuperMetroid.Core.Input;
namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// Read-only selection of the prospective pose from the input transitions.
/// </summary>
/// <remarks>
/// This matcher deliberately stops before applying a prospective pose. The runtime owns the
/// transition side effects. Authored conditions are compiled; a pose with no authored input
/// transitions fails loudly. The returned lookup states which input transition won for a precise
/// held/newly-pressed input chord.
/// </remarks>
public static class SamusPoseInputTransitions
{
    /// <summary>Held Start and item selection, excluded from the pose-table input match.</summary>
    private const GameplayAction StartAndItemSelect = GameplayAction.Start | GameplayAction.ItemSelect;

    /// <summary>
    /// Runs the complete lookup and preserves whether failure enters
    /// the pose handling's pose-definition fallback.
    /// </summary>
    /// <remarks>
    /// There are two observably different ways to return without a prospective pose.
    /// Zero input or exhausting a nonempty list of transitions calls the pose-definition fallback. A pose
    /// with no transitions returns directly while any input is held;
    /// matching a transition whose target is the current pose returns directly. A nullable
    /// transition alone cannot represent that distinction. In particular, running-gun
    /// pose <see cref="SamusPoseId.MovingRightGunExtendedPose"/> matches itself while Right+Shoot is held, but Shoot alone reaches
    /// the end of its transitions and must fall back to standing pose <see cref="SamusPoseId.FacingRightNormalPose"/>.
    /// </remarks>
    public static SamusPoseTransitionLookup Lookup(
        SamusPoseId currentPose,
        GameplayAction heldActions,
        GameplayAction newlyPressedActions)
    {
        // Branches directly to lookup-failure handling when no action is held,
        // before it reads this table at all.
        if (heldActions == GameplayAction.None)
            return new SamusPoseTransitionLookup(null, UsesPoseDefinitionFallback: true);

        // Start and item selection never participate in pose chords: the lookup reads only the directions and the
        // actions its chords name.
        GameplayAction held = heldActions & ~StartAndItemSelect;
        GameplayAction newlyPressed = newlyPressedActions & ~StartAndItemSelect;

        if (!SamusPoseInputs.TryGetInputList(currentPose, out SamusPoseInputList inputList))
        {
            throw new InvalidDataException(
                $"Pose {currentPose} has no authored input transitions.");
        }
        SamusPoseInputMatch match = SamusPoseInputs.Match(inputList, held, newlyPressed);
        if (match.Rule is not { } rule)
            return new SamusPoseTransitionLookup(null, UsesPoseDefinitionFallback: match.HasConditions);

        // The game returns directly when the winning condition keeps this pose.
        if (rule.TargetPose == currentPose)
            return new SamusPoseTransitionLookup(null, UsesPoseDefinitionFallback: false);

        return new SamusPoseTransitionLookup(
            new SamusPoseTransition(
                ProspectivePose: rule.TargetPose),
            UsesPoseDefinitionFallback: false);
    }

}
