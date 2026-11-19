using System.Collections.Frozen;
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
public static class SamusPoseInputTransitions
{
    /// <summary>The gameplay actions in the order a transition lists those it reads (#2567).</summary>
    private static readonly IReadOnlyList<GameplayAction> ActionOrder = Ensure.EveryMemberOnce<GameplayAction>(
    [
        GameplayAction.None,
        GameplayAction.Up,
        GameplayAction.Down,
        GameplayAction.Left,
        GameplayAction.Right,
        GameplayAction.Start,
        GameplayAction.Shoot,
        GameplayAction.Jump,
        GameplayAction.Dash,
        GameplayAction.ItemSelect,
        GameplayAction.ItemCancel,
        GameplayAction.AimUp,
        GameplayAction.AimDown,
    ]);

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
        SelectableTargets[SamusPoseTraits.Of(currentPose).Inputs].Value.Contains(target);

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

        SamusPoseInputMatch match = SamusPoseInputRules.Match(SamusPoseTraits.Of(currentPose).Inputs, held, newlyPressed);
        if (match.Transition is not { } transition)
            return new SamusPoseTransitionLookup(null, UsesPoseDefinitionFallback: match.HasConditions);

        // The game returns directly when the winning condition keeps this pose.
        return transition.ProspectivePose == currentPose
            ? new SamusPoseTransitionLookup(null, UsesPoseDefinitionFallback: false)
            : new SamusPoseTransitionLookup(transition, UsesPoseDefinitionFallback: false);
    }

    /// <summary>
    /// The poses <paramref name="inputList"/> selects under some held chord its transitions read, with any part of it newly
    /// pressed, as <see cref="Lookup"/> reads it: with an action held and Start and item selection left out.
    /// </summary>
    private static FrozenSet<SamusPoseId> Selectable(SamusPoseInputList inputList)
    {
        GameplayAction[] read = [.. ActionOrder.Where(action =>
            action != GameplayAction.None && (action & StartAndItemSelect) == GameplayAction.None)];
        var targets = new HashSet<SamusPoseId>();
        foreach (GameplayAction[] heldActions in Subsets(read).Where(actions => actions.Length > 0))
        {
            GameplayAction held = heldActions.Aggregate(GameplayAction.None, (chord, action) => chord | action);
            foreach (GameplayAction[] pressedActions in Subsets(heldActions))
            {
                GameplayAction newlyPressed = pressedActions.Aggregate(GameplayAction.None, (chord, action) => chord | action);
                if (SamusPoseInputRules.Match(inputList, held, newlyPressed).Transition is { } transition)
                    targets.Add(transition.ProspectivePose);
            }
        }
        return targets.ToFrozenSet();
    }

    /// <summary>Every subset of <paramref name="actions"/>, the empty one included.</summary>
    private static IEnumerable<GameplayAction[]> Subsets(GameplayAction[] actions) =>
        actions.Aggregate<GameplayAction, IEnumerable<GameplayAction[]>>([[]],
            (subsets, action) => subsets.SelectMany(subset => new[] { subset, [.. subset, action] }));
}
