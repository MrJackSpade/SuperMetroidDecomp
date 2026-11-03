using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The sprite colours a room graphics set's load leaves, and the palette row each enemy drawn under it draws in (#1512):
/// the import's knowledge of the rows, which the set's sheets are coloured in. Each member's sixteen colours are loaded
/// into the row its record names, record after record, so a later record loading a row overwrites an earlier one, as on
/// the cartridge; a member loaded twice draws in the row its first record names.
/// </summary>
internal sealed class LoadedSetColours
{
    /// <summary>The colours in one sprite palette row.</summary>
    internal const int RowColours = 16;

    /// <summary>The row each member's first record loads its colours into.</summary>
    private readonly Dictionary<EnemyDefinitionId, int> memberRows;

    /// <summary>Keeps the loaded colours and the members' rows.</summary>
    private LoadedSetColours(RgbColor[] spriteColours, Dictionary<EnemyDefinitionId, int> memberRows)
    {
        SpriteColours = spriteColours;
        this.memberRows = memberRows;
    }

    /// <summary>The eight sprite rows' colours, sixteen each, as the set's load leaves them.</summary>
    internal RgbColor[] SpriteColours { get; }

    /// <summary>The colours set <paramref name="set"/>'s load leaves, from the installed palettes in <paramref name="inputs"/>.</summary>
    /// <exception cref="InvalidDataException">The set's rows are not catalogued for each of its records, or a member has no palette.</exception>
    internal static LoadedSetColours Of(EnemySheetInputs inputs, RoomEnemyGraphicsSetDefinition set)
    {
        Ensure.NotNull(inputs);
        Ensure.NotNull(set);
        ReadOnlySpan<RoomEnemyGraphicsSetHeader> records = set.Records.Span;
        int[] rows = records.Length == 0 ? []
            : EnemyGraphicsSetPaletteRowDefinitions.Rows.TryGetValue(set.Name, out int[]? catalogued) ? catalogued
            : throw new InvalidDataException($"Enemy graphics set {set.Name} has no catalogued palette rows.");
        if (rows.Length != records.Length)
            throw new InvalidDataException($"Enemy graphics set {set.Name} catalogues {rows.Length} palette rows for {records.Length} records.");
        RgbColor[] colours = RoomLoadPalettes.SpriteColours(inputs.Palettes);
        var memberRows = new Dictionary<EnemyDefinitionId, int>();
        for (int record = 0; record < records.Length; record++)
        {
            EnemyDefinitionId enemy = records[record].DefinitionId;
            inputs.Enemies.PaletteOf(enemy).Colours.CopyTo(colours.AsSpan(rows[record] * RowColours, RowColours));
            memberRows.TryAdd(enemy, rows[record]);
        }
        return new LoadedSetColours(colours, memberRows);
    }

    /// <summary>
    /// The row <paramref name="enemy"/> draws in under the set: the shared row its initialization fixes, the row of the
    /// enemy whose row it copies, its own first record's, or, for an enemy the set does not load, the enemy projectiles'
    /// row, as the game's fallback selects.
    /// </summary>
    internal int RowOf(EnemyDefinitionId enemy)
    {
        if (EnemySheetRowDefinitions.FixedRows.TryGetValue(enemy, out int fixedRow))
            return fixedRow;
        EnemyDefinitionId owner = EnemySheetRowDefinitions.DrawsInRowOf.GetValueOrDefault(enemy, enemy);
        return memberRows.TryGetValue(owner, out int row) ? row : EnemySheetRowDefinitions.EnemyProjectiles;
    }

    /// <summary>The set's colours with row <paramref name="row"/> holding <paramref name="replaced"/> from its colour <paramref name="first"/> on.</summary>
    internal RgbColor[] With(int row, int first, ReadOnlySpan<RgbColor> replaced)
    {
        RgbColor[] colours = [.. SpriteColours];
        replaced.CopyTo(colours.AsSpan(Ensure.BetweenInclusive(0, 7, row) * RowColours + first, replaced.Length));
        return colours;
    }
}
