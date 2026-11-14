using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The sprite colours a room graphics set's load leaves, and the palette row each enemy drawn under it draws in (#1512),
/// which the set's sheets are coloured in. Each member's sixteen colours are loaded into the row its record names, record
/// after record, so a later record loading a row overwrites an earlier one, as on the cartridge.
/// </summary>
internal sealed class LoadedSetColours
{
    /// <summary>The colours in one sprite palette row.</summary>
    internal const int RowColours = 16;

    /// <summary>The set's rows.</summary>
    private readonly GraphicsSetRows rows;

    /// <summary>Keeps the loaded colours and the set's rows.</summary>
    private LoadedSetColours(RgbColor[] spriteColours, GraphicsSetRows rows)
    {
        SpriteColours = spriteColours;
        this.rows = rows;
    }

    /// <summary>The eight sprite rows' colours, sixteen each, as the set's load leaves them.</summary>
    internal RgbColor[] SpriteColours { get; }

    /// <summary>The colours set <paramref name="set"/>'s load leaves, from the installed palettes in <paramref name="inputs"/>.</summary>
    /// <exception cref="InvalidDataException">The set's rows are not catalogued for each of its records, or a member has no palette.</exception>
    internal static LoadedSetColours Of(EnemySheetInputs inputs, RoomEnemyGraphicsSetMembers set)
    {
        Ensure.NotNull(inputs);
        Ensure.NotNull(set);
        var rows = new GraphicsSetRows(set, inputs.SetPaletteRows);
        RgbColor[] colours = RoomLoadPalettes.SpriteColours(inputs.Palettes);
        ReadOnlySpan<RoomEnemyGraphicsSetMember> records = set.Records.Span;
        for (int record = 0; record < records.Length; record++)
            records[record].PaletteIn(inputs.Enemies).Colours.CopyTo(colours.AsSpan(rows.RecordRow(record) * RowColours, RowColours));
        return new LoadedSetColours(colours, rows);
    }

    /// <summary>The row <paramref name="enemy"/> draws in under the set (<see cref="GraphicsSetRows.RowOf"/>).</summary>
    internal int RowOf(EnemyDefinitionId enemy) => rows.RowOf(enemy);

    /// <summary>The set's colours with row <paramref name="row"/> holding <paramref name="replaced"/> from its colour <paramref name="first"/> on.</summary>
    internal RgbColor[] With(int row, int first, ReadOnlySpan<RgbColor> replaced)
    {
        RgbColor[] colours = [.. SpriteColours];
        replaced.CopyTo(colours.AsSpan(Ensure.BetweenInclusive(0, 7, row) * RowColours + first, replaced.Length));
        return colours;
    }
}
