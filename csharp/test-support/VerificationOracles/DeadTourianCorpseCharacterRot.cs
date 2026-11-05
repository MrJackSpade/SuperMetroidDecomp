using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Enemies.Corpses;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.TestSupport.VerificationOracles;

/// <summary>
/// Reference oracle of the game's character rot of a dead Zoomer, Ripper or Skree (#1531): the corpse's characters staged
/// in work memory as the game's initializer copies them from the corpse sheet, each rot call's pixel movers run over every
/// character column of them, and the rows the game uploads into the sprite characters after each call. The game draws the
/// corpse's frame through the rows Core publishes (<see cref="SuperMetroid.Core.Assets.CorpseRotRows"/>); a check compares
/// that draw with the frame composed over <see cref="Characters"/> at every rot call. The layouts are the extraction's,
/// moved here when the extraction stopped picturing these corpses' rot; nothing in the game reads them.
/// </summary>
internal sealed class DeadTourianCorpseCharacterRot
{
    /// <summary>Bytes in one character.</summary>
    private const int TileBytes = SpriteArtwork.CharacterByteCount;

    /// <summary>The bytes one row of the corpse sheet's characters spans.</summary>
    private const int SheetRowBytes = 0x200;

    /// <summary>The bytes a row's fall out of a character's last two rows carries back into the next character row.</summary>
    private const int WrapCarryBack = 12;

    /// <summary>The rows the corpse uploads.</summary>
    private readonly CorpseRotRow[] rows;

    /// <summary>Each row's bytes after each rot call, by call from the first, then by row.</summary>
    private readonly List<byte[][]> afterCall;

    /// <summary>The corpse's rows and each row's bytes after each rot call.</summary>
    private DeadTourianCorpseCharacterRot(CorpseRotRow[] rows, List<byte[][]> afterCall)
    {
        this.rows = rows;
        this.afterCall = afterCall;
    }

    /// <summary>The rot calls the corpse takes until its last row has fallen, the last one included.</summary>
    internal int CallCount => afterCall.Count;

    /// <summary>
    /// The rot of variant <paramref name="variant"/> of a dead <paramref name="species"/>, staged from the installed
    /// <paramref name="enemies"/>' corpse sheet and rotted to its end.
    /// </summary>
    /// <exception cref="InvalidDataException">The installed corpse sheet is not the size the corpses stage from.</exception>
    internal static DeadTourianCorpseCharacterRot Of(EnemyTileSourceArtwork enemies, DeadTourianCorpseSpecies species, int variant)
    {
        ReadOnlyMemory<byte> sheet = Ensure.NotNull(enemies).TilesOf(CorpseSheetDefinitions.DeadMonsterSheet);
        if (sheet.Length != CorpseSheetDefinitions.DeadMonsterSheetByteCount)
            throw new InvalidDataException($"Installed corpse sheet is {sheet.Length} bytes, not {CorpseSheetDefinitions.DeadMonsterSheetByteCount}.");
        Layout layout = LayoutOf(species);
        if ((uint)variant >= (uint)layout.SheetSources.Length)
            throw new ArgumentOutOfRangeException(nameof(variant), variant, $"A dead {species} has {layout.SheetSources.Length} variants.");
        int block = layout.WorkBufferOffset + variant * layout.Columns * layout.CharacterRows * TileBytes;
        var staged = new CorpseStaging();
        for (int row = 0; row < layout.CharacterRows; row++)
            staged.Copy(sheet.Span, new CorpseGraphicsCopy(layout.SheetSources[variant] + SheetRowBytes * row, block + layout.Columns * TileBytes * row,
                layout.Columns * TileBytes));
        var geometry = new CorpseRowGeometry(InHeap: false, layout.StagedCharacterRows, layout.Columns,
            [.. Enumerable.Range(0, layout.Columns).Select(column => block / 2 + TileBytes / 2 * column)], new int[layout.Columns],
            CopyLimit: 8 * layout.CharacterRows - 2, Wrap: layout.Columns * TileBytes - WrapCarryBack);
        CorpseRotRow[] rows = [.. UploadsOf(species, variant).Select((upload, index) =>
            new CorpseRotRow(index, upload.SourceAddress, upload.SizeInBytes, upload.FirstTableByte))];
        var schedule = new CorpseRotSchedule(DeadTourianCorpses.For(species, variant).RowCount);
        var afterCall = new List<byte[][]>();
        bool rotting = true;
        while (rotting)
        {
            rotting = schedule.Step((y, operation) => staged.MoveRow(geometry, y, operation), _ => { });
            afterCall.Add([.. rows.Select(staged.Upload)]);
        }
        return new DeadTourianCorpseCharacterRot(rows, afterCall);
    }

    /// <summary>
    /// The sprite characters <paramref name="loaded"/> with the corpse's rows as the game uploads them after rot call
    /// <paramref name="call"/>, counted from one; before the first call, <paramref name="call"/> zero, they show the
    /// loaded characters.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The corpse takes no such call.</exception>
    internal byte[] Characters(ReadOnlySpan<byte> loaded, int call)
    {
        if ((uint)call > (uint)afterCall.Count)
            throw new ArgumentOutOfRangeException(nameof(call), call, $"The corpse takes {afterCall.Count} rot calls.");
        byte[] characters = loaded.ToArray();
        if (call == 0)
            return characters;
        byte[][] uploaded = afterCall[call - 1];
        for (int row = 0; row < rows.Length; row++)
            uploaded[row].CopyTo(characters.AsSpan(rows[row].FirstByte));
        return characters;
    }

    /// <summary>
    /// The staged layout of <paramref name="species"/>'s corpses: each variant's block of characters, staged one after
    /// another in the dead-monster work buffer, copied a character row at a time from the variant's place in the corpse sheet.
    /// </summary>
    private static Layout LayoutOf(DeadTourianCorpseSpecies species) => species switch
    {
        DeadTourianCorpseSpecies.Zoomer => new Layout(Columns: 3, CharacterRows: 2, StagedCharacterRows: 3, WorkBufferOffset: 0x0940,
            SheetSources: [0x0a60, 0x0ac0, 0x0b20]),
        DeadTourianCorpseSpecies.Ripper => new Layout(Columns: 3, CharacterRows: 2, StagedCharacterRows: 3, WorkBufferOffset: 0x0b80,
            SheetSources: [0x0a00, 0x0b80]),
        DeadTourianCorpseSpecies.Skree => new Layout(Columns: 2, CharacterRows: 4, StagedCharacterRows: 5, WorkBufferOffset: 0x0640,
            SheetSources: [0x02a0, 0x00e0, 0x01c0]),
        _ => throw new ArgumentOutOfRangeException(nameof(species), species, null),
    };

    /// <summary>The rows variant <paramref name="variant"/> of a dead <paramref name="species"/> uploads after each rot call, in the game's order.</summary>
    private static IEnumerable<CorpseRowUpload> UploadsOf(DeadTourianCorpseSpecies species, int variant) => species switch
    {
        DeadTourianCorpseSpecies.Zoomer => Uploads(2, 0x60, 0x2940 + variant * 0xc0, 0x7530 + variant * 0x30),
        DeadTourianCorpseSpecies.Ripper => Uploads(2, 0x60, 0x2b80 + variant * 0xc0, 0x7500 + variant * 0xc0),
        // The first Skree's rows begin on the next character row of the table, the others' on its first.
        DeadTourianCorpseSpecies.Skree => Uploads(4, 0x40, 0x2640 + variant * 0x100, variant == 0 ? 0x7150 : 0x7000 + variant * 0x70),
        _ => throw new ArgumentOutOfRangeException(nameof(species), species, null),
    };

    /// <summary>
    /// <paramref name="count"/> consecutive staged rows of <paramref name="stride"/> bytes from work-memory offset
    /// <paramref name="source"/>, each uploaded to the next 32-character row of the table from <paramref name="destination"/>.
    /// </summary>
    private static IEnumerable<CorpseRowUpload> Uploads(int count, int stride, int source, int destination) =>
        Enumerable.Range(0, count).Select(index => new CorpseRowUpload(stride, 0x7e00, source + stride * index, destination + 0x100 * index));

    /// <summary>A species' staged corpse layout.</summary>
    /// <param name="Columns">The character columns of each variant's block.</param>
    /// <param name="CharacterRows">The character rows of each variant's block.</param>
    /// <param name="StagedCharacterRows">The character rows the movers reach, one more than the block's.</param>
    /// <param name="WorkBufferOffset">Where in the dead-monster work buffer the first variant's block begins.</param>
    /// <param name="SheetSources">Each variant's first byte in the corpse sheet.</param>
    private sealed record Layout(int Columns, int CharacterRows, int StagedCharacterRows, int WorkBufferOffset, int[] SheetSources);
}
