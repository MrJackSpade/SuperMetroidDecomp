using System.Collections.Frozen;
using SuperMetroid.Core.Input;
namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// Read-only selection of the prospective pose from the input transitions.
/// </summary>
/// <remarks>
/// The selection stops before applying a prospective pose. The runtime owns the
/// transition side effects. Authored conditions are compiled; an undefined pose fails loudly. The
/// returned lookup states which input transition won for a precise
/// held/newly-pressed input chord.
/// </remarks>
public static class SamusPoseInputTransitions
{
    /// <summary>Held Start and item selection, which the pose's input transitions do not read.</summary>
    private const GameplayAction StartAndItemSelect = GameplayAction.Start | GameplayAction.ItemSelect;

    /// <summary>
    /// The poses each input-transition list can select, found once per list the first time it is asked about by reading
    /// the list under every held chord its transitions read and every part of that chord newly pressed.
    /// </summary>
    private static readonly FrozenDictionary<SamusPoseInputList, Lazy<FrozenSet<SamusPoseId>>> SelectableTargets =
        Enum.GetValues<SamusPoseInputList>().ToFrozenDictionary(list => list, list => new Lazy<FrozenSet<SamusPoseId>>(() => Selectable(list)));

    /// <summary>
    /// Whether the input transitions of <paramref name="currentPose"/> select <paramref name="target"/> for some held and
    /// newly pressed actions (#2397).
    /// </summary>
    /// <exception cref="InvalidOperationException">The pose is undefined.</exception>
    internal static bool CanSelect(SamusPoseId currentPose, SamusPoseId target) =>
        SelectableTargets[SamusPoseInputs.InputList(currentPose)].Value.Contains(target);

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
        // With no action held, no input transition matches and the pose's definition decides, without reading the
        // pose's input transitions at all.
        if (heldActions == GameplayAction.None)
            return new SamusPoseTransitionLookup(null, UsesPoseDefinitionFallback: true);

        // Start and item selection never participate in pose chords: the lookup reads only the directions and the
        // actions its chords name.
        GameplayAction held = heldActions & ~StartAndItemSelect;
        GameplayAction newlyPressed = newlyPressedActions & ~StartAndItemSelect;

        SamusPoseInputMatch match = SamusPoseInputs.Match(SamusPoseInputs.InputList(currentPose), held, newlyPressed);
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

    /// <summary>
    /// The poses <paramref name="inputList"/> selects under some held chord its transitions read, with any part of it newly
    /// pressed, as <see cref="Lookup"/> reads it: with an action held and Start and item selection left out.
    /// </summary>
    private static FrozenSet<SamusPoseId> Selectable(SamusPoseInputList inputList)
    {
        GameplayAction[] read = [.. Enum.GetValues<GameplayAction>().Where(action =>
            action != GameplayAction.None && (action & StartAndItemSelect) == GameplayAction.None)];
        var targets = new HashSet<SamusPoseId>();
        foreach (GameplayAction[] heldActions in Subsets(read).Where(actions => actions.Length > 0))
        {
            GameplayAction held = heldActions.Aggregate(GameplayAction.None, (chord, action) => chord | action);
            foreach (GameplayAction[] pressedActions in Subsets(heldActions))
            {
                GameplayAction newlyPressed = pressedActions.Aggregate(GameplayAction.None, (chord, action) => chord | action);
                if (SamusPoseInputs.Match(inputList, held, newlyPressed).Rule is { } rule)
                    targets.Add(rule.TargetPose);
            }
        }
        return targets.ToFrozenSet();
    }

    /// <summary>Every subset of <paramref name="actions"/>, the empty one included.</summary>
    private static IEnumerable<GameplayAction[]> Subsets(GameplayAction[] actions) =>
        actions.Aggregate<GameplayAction, IEnumerable<GameplayAction[]>>([[]],
            (subsets, action) => subsets.SelectMany(subset => new[] { subset, [.. subset, action] }));
}
