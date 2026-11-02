using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// One PLM family's installed visual-only overrides: the authored blocks of each compiled draw that differs from its
/// stock appearance, keyed by the draw, flattened in run then block order. Collision, programs and run geometry stay
/// compiled; the import validates the overrides against the compiled draws before they reach the game.
/// </summary>
internal sealed class PlmVisualOverrides
{
    /// <summary>The changed draws' appearances; a draw absent here shows its stock appearance.</summary>
    private readonly IReadOnlyDictionary<PlmDraw, RoomBlockAppearance[]> changed;

    /// <summary>Holds the validated appearances of the draws that differ from stock.</summary>
    /// <param name="changed">The changed draws' flattened appearances, owned by these overrides.</param>
    internal PlmVisualOverrides(IReadOnlyDictionary<PlmDraw, RoomBlockAppearance[]> changed) =>
        this.changed = Ensure.NotNull(changed);

    /// <summary>The authored blocks of a draw that differs from stock; false for a stock draw.</summary>
    internal bool TryGet(PlmDraw draw, [NotNullWhen(true)] out RoomBlockAppearance[]? blocks) =>
        changed.TryGetValue(draw, out blocks);
}
