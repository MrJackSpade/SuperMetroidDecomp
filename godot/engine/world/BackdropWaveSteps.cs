using Engine.Content;

namespace Engine.World;

/// <summary>
/// The wave a <see cref="BackdropWaveEffect"/> plays on the rows of what lies behind its room: a table of whole
/// pixels gone round a row at a time, shown a step at a time. Each step shows for a number of updates and the next
/// draws the table a row further up, or further down.
/// </summary>
/// <remarks>
/// It is a data file:
/// <code>
/// { "offsets": [1, 1, 0, 0, -1, -1, 0, 0], "updates": 6, "down": false,
///   "below_surface": true, "moves_down": false, "shown_at_start": 0 }
/// </code>
/// </remarks>
/// <param name="Offsets">How many pixels further on a row reads its picture, one entry a row.</param>
/// <param name="Updates">How many updates each step shows for.</param>
/// <param name="Down">Whether rows read further down their picture; otherwise further across.</param>
/// <param name="BelowSurface">Whether only the rows below the surface of the room's liquid are waved, the table counted from the first of them; otherwise every row in view, counted from the view's top.</param>
/// <param name="MovesDown">Whether each step draws the table a row further down; otherwise a row further up.</param>
/// <param name="ShownAtStart">How many updates its first step counts as shown as the room is set up.</param>
public sealed record BackdropWaveSteps(IReadOnlyList<int> Offsets, int Updates, bool Down, bool BelowSurface, bool MovesDown, int ShownAtStart)
{
    /// <summary>Reads a wave from its data file.</summary>
    public static BackdropWaveSteps Read(DataNode data)
    {
        ArgumentNullException.ThrowIfNull(data);
        int[] offsets = [.. data.List("offsets").Select(offset => offset.AsInt())];
        if (offsets.Length < 1 || offsets.Length > Ui.Recolouring.MostWaveEntries)
            throw data.Get("offsets").Problem($"lists {offsets.Length} entries; a wave's table has 1 to {Ui.Recolouring.MostWaveEntries}");
        int updates = data.Int("updates");
        if (updates < 1)
            throw data.Get("updates").Problem("must be at least 1");
        return new BackdropWaveSteps(offsets, updates, data.Flag("down"), data.Flag("below_surface"), data.Flag("moves_down"), data.Int("shown_at_start"));
    }
}
