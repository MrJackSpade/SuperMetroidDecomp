using SuperMetroid.Core.Input;
namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// Read-only port of <c>DetermineProspectivePoseFromTransitionTable</c>.
/// </summary>
/// <remarks>
/// This matcher deliberately stops before applying a prospective pose. The runtime owns the
/// transition side effects. Authored conditions are compiled; non-authored pose indexes fail
/// explicitly instead of parsing adjacent code as a transition list. The returned
/// record states which transition entry won for a precise
/// held/newly-pressed input chord.
/// </remarks>
public static class SamusPoseTransitionTable
{
    /// <summary>Held Start and Select bits excluded from the pose-table input match.</summary>
    private const ControllerButton StartAndSelectMask = ControllerButton.Start | ControllerButton.Select;

    /// <summary>
    /// Runs the complete lookup and preserves whether failure enters
    /// <c>Samus_Pose_Func2</c>'s pose-definition fallback.
    /// </summary>
    /// <remarks>
    /// There are two observably different ways to return without a prospective pose.
    /// Zero input or exhausting a nonempty table calls the pose-definition fallback. A table
    /// beginning with a terminator returns directly while any input is held;
    /// matching a record whose target is the current pose returns directly. A nullable
    /// transition alone cannot represent that distinction. In particular, running-gun
    /// pose <c>$0B</c> matches itself while Right+Shoot is held, but Shoot alone reaches
    /// the terminator and must fall back to standing pose <c>$01</c>.
    /// </remarks>
    public static SamusPoseTransitionLookup Lookup(
        SamusPoseId currentPose,
        ControllerButton canonicalHeldInput,
        ControllerButton canonicalNewInput)
    {

        // Branches directly to lookup-failure handling when the raw controller
        // word is zero, before it reads this table at all.
        if (canonicalHeldInput == ControllerButton.None)
            return new SamusPoseTransitionLookup(null, UsesPoseDefinitionFallback: true);

        // Start and Select never participate in pose chords. The lookup initializes its two
        // complement masks from only the direction bits, then adds canonical action bits explicitly.
        ControllerButton held = canonicalHeldInput & ~StartAndSelectMask;
        ControllerButton newlyPressed = canonicalNewInput & ~StartAndSelectMask;

        if (!SamusPoseInputs.TryGetInputList(currentPose, out SamusPoseInputList inputList))
        {
            throw new InvalidDataException(
                $"Pose {currentPose} has no authored input-transition graph; " +
                "adjacent game code is not mechanics metadata.");
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
