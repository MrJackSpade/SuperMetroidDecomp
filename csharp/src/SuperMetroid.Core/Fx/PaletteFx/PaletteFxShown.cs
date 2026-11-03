namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// The colour record each palette-FX colour group last showed (#1512), in the order they were shown: a group keeps
/// showing its last record after its object ends, until the room is left, and where two groups recolour the same colours
/// the later one shows. The owner of a group's pictures names the shade of the record its group shows.
/// </summary>
internal sealed class PaletteFxShown
{
    /// <summary>Each group shown and its last record, least recently shown first.</summary>
    private readonly List<(PaletteFxColours Colours, PaletteFxFrame Record)> shown = [];

    /// <summary>Records that <paramref name="colours"/> shows <paramref name="record"/> now, over every group shown before.</summary>
    internal void Show(PaletteFxColours colours, PaletteFxFrame record)
    {
        shown.RemoveAll(entry => entry.Colours == colours);
        shown.Add((colours, record));
    }

    /// <summary>The record <paramref name="colours"/> last showed, or null while it has shown none.</summary>
    internal PaletteFxFrame? RecordOf(PaletteFxColours colours)
    {
        foreach ((PaletteFxColours group, PaletteFxFrame record) in shown)
        {
            if (group == colours)
                return record;
        }
        return null;
    }

    /// <summary>Each group shown and its last record, least recently shown first.</summary>
    internal IReadOnlyList<(PaletteFxColours Colours, PaletteFxFrame Record)> InOrder => shown;
}
