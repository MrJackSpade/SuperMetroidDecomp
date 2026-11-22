using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Which pose each pose's input list selects for the held and newly pressed actions.</summary>
internal static partial class SamusPoseInputs
{
    /// <summary>
    /// Exchanges Left and Right in <paramref name="input"/>. A left-facing list is its right-facing list read with the
    /// directions exchanged, its selected pose mirrored back.
    /// </summary>
    internal static GameplayAction MirrorButtons(GameplayAction input) =>
        (input & ~(GameplayAction.Left | GameplayAction.Right)) |
        ((input & GameplayAction.Left) != GameplayAction.None ? GameplayAction.Right : GameplayAction.None) |
        ((input & GameplayAction.Right) != GameplayAction.None ? GameplayAction.Left : GameplayAction.None);

    /// <summary>Requires every action of an authored input chord to be present.</summary>
    internal static bool Has(GameplayAction input, GameplayAction required) =>
        (input & required) == required;

    /// <summary>
    /// Marks a selected target, installed by <paramref name="setup"/>, as a matched condition, distinct from an empty
    /// transition list.
    /// </summary>
    internal static SamusPoseInputMatch Accept(SamusPoseId target, SamusPoseTransitionSetup setup) =>
        new SamusPoseInputMatch.Matched(new SamusPoseTransition(target, setup));
}
