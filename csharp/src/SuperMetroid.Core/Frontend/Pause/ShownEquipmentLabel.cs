using SuperMetroid.Core.Frontend.Pause.Artwork;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// How the equipment page shows one ordinary item label (#1467). A label's first five cells (its head) and the rest of a
/// nine-cell label can show different states: an A press dispatched from the beam column rewrites only a beam label's five
/// cells of the label the cursor moves onto, leaving the rest as it was. A five-cell beam label has no rest, and its rest
/// state always equals its head's.
/// </summary>
/// <param name="Label">The label.</param>
/// <param name="Head">How its first five cells show.</param>
/// <param name="Rest">How its remaining cells show.</param>
internal readonly record struct ShownEquipmentLabel(PauseEquipmentLabelId Label, PauseEquipmentLabelState Head, PauseEquipmentLabelState Rest)
{
    /// <summary><paramref name="label"/> showing <paramref name="state"/> in every cell.</summary>
    internal static ShownEquipmentLabel Whole(PauseEquipmentLabelId label, PauseEquipmentLabelState state) => new(label, state, state);
}
