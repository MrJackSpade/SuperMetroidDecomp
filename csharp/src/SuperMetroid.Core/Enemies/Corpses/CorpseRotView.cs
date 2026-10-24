using System.Collections.Immutable;
using System.Globalization;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// The rot a rotting corpse's frames show (#1406): the corpse's kind and the rot step each group of its rows shows, none
/// while a group still shows the graphics set's own characters. A dead monster's rows show one step together; the dead
/// Torizo's two halves, even then odd, each show the step they were last shown at. The corpse selects its rot at the
/// update it rots, and each of its frames drawn over its rows names its sheet's picture of that rot.
/// </summary>
/// <param name="Corpse">The kind of corpse whose rows rot.</param>
/// <param name="Steps">The rot step each group of the corpse's rows shows, or none while the group shows the set's characters.</param>
internal sealed record CorpseRotView(RottingCorpseKind Corpse, ImmutableArray<int?> Steps)
{
    /// <summary>
    /// The name of the picture of sheet frame <paramref name="frame"/> showing this rot: the frame itself while every group
    /// shows the set's characters, else the frame with each group's step.
    /// </summary>
    public string PictureName(string frame)
    {
        ArgumentException.ThrowIfNullOrEmpty(frame);
        if (Steps.All(step => step is null))
            return frame;
        return frame + "@rot" + string.Join('.', Steps.Select(step => step?.ToString(CultureInfo.InvariantCulture) ?? "set"));
    }
}
