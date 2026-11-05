using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Enemies.Corpses;
using SuperMetroid.AssetExtraction.Rooms.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.TestSupport.VerificationOracles;

/// <summary>
/// Reference oracle of the game's character rot of every rotting corpse (#1531): the corpse's characters staged in work
/// memory as the game's initializer copies them, each rot call's pixel movers run over every character column of them,
/// each column from its own first moved row, and the rows the game uploads into the sprite characters after each call (a
/// dead monster after every call, Mother Brain after every call but the last, the dead Torizo one half each main-AI call,
/// with its sand falling into its strips every fifteenth call). The game instead draws each corpse frame through the rows
/// Core publishes; the corpse rot drawing check compares that draw with the frame composed over <see cref="Characters"/>.
/// Moved from the extraction, which pictured every rot step before #1531; nothing in the game reads it.
/// </summary>
internal sealed class CorpseCharacterRot
{
    /// <summary>The rows the corpse uploads: a dead Torizo's even half's first, then its odd half's.</summary>
    private readonly CorpseRotRow[] rows;

    /// <summary>Each row's bytes after each rot call, by call from none taken, then by row; a row not uploaded then holds none.</summary>
    private readonly List<byte[]?[]> afterCall;

    /// <summary>The rows of the dead Torizo's even half, which come first; none for any other corpse.</summary>
    private readonly int evenRows;

    /// <summary>A corpse's rows and each row's bytes after each rot call.</summary>
    private CorpseCharacterRot(string name, CorpseRotRow[] rows, List<byte[]?[]> afterCall, int evenRows)
    {
        Name = name;
        this.rows = rows;
        this.afterCall = afterCall;
        this.evenRows = evenRows;
    }

    /// <summary>The corpse, named for messages.</summary>
    internal string Name { get; }

    /// <summary>The rot calls the corpse takes until its last row has fallen, the last one included.</summary>
    internal int CallCount => afterCall.Count - 1;

    /// <summary>
    /// The sprite characters <paramref name="loaded"/> with the corpse's rows as the game last uploaded them by rot call
    /// <paramref name="call"/>, counted from one; before the first upload they show the loaded characters.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The corpse takes no such call.</exception>
    internal byte[] Characters(ReadOnlySpan<byte> loaded, int call)
    {
        Require(call);
        byte[] characters = loaded.ToArray();
        for (int row = 0; row < rows.Length; row++)
        {
            if (LastUpload(row, call) is { } bytes)
                bytes.CopyTo(characters.AsSpan(rows[row].FirstByte));
        }
        return characters;
    }

    /// <summary>
    /// The sprite characters <paramref name="loaded"/> with the dead Torizo's even half as its main-AI calls last uploaded
    /// it, after <paramref name="evenCall"/> rot calls, and its odd half after <paramref name="oddCall"/>; a half not yet
    /// uploaded shows the loaded characters.
    /// </summary>
    /// <exception cref="InvalidOperationException">The corpse is not uploaded by halves.</exception>
    internal byte[] Characters(ReadOnlySpan<byte> loaded, int? evenCall, int? oddCall)
    {
        if (evenRows == 0)
            throw new InvalidOperationException($"{Name} is not uploaded by halves.");
        byte[] characters = loaded.ToArray();
        for (int row = 0; row < rows.Length; row++)
        {
            if ((row < evenRows ? evenCall : oddCall) is not int call)
                continue;
            Require(call);
            byte[] bytes = afterCall[call][row] ?? throw new InvalidOperationException($"{Name} uploads every row of a half at every call.");
            bytes.CopyTo(characters.AsSpan(rows[row].FirstByte));
        }
        return characters;
    }

    /// <summary>The bytes row <paramref name="row"/> was last uploaded with by call <paramref name="call"/>, or none.</summary>
    private byte[]? LastUpload(int row, int call)
    {
        for (int earlier = call; earlier >= 0; earlier--)
        {
            if (afterCall[earlier][row] is { } bytes)
                return bytes;
        }
        return null;
    }

    /// <summary>Requires a call the corpse takes, or none taken.</summary>
    private void Require(int call)
    {
        if ((uint)call > (uint)CallCount)
            throw new ArgumentOutOfRangeException(nameof(call), call, $"{Name} takes {CallCount} rot calls.");
    }

    /// <summary>The dead Sidehopper of graphics variant <paramref name="graphicsVariant"/> (population parameter 0 or 2).</summary>
    internal static CorpseCharacterRot Sidehopper(EnemyTileSourceArtwork enemies, int graphicsVariant)
    {
        DeadSidehopperCorpseDefinition definition = graphicsVariant == 0
            ? DeadSidehopperCorpseDefinitions.InitiallyAlive : DeadSidehopperCorpseDefinitions.InitiallyDead;
        CorpseStaging staged = CorpseStaging.Sidehopper(DeadMonsterSheet(enemies), graphicsVariant);
        return Batch(definition.Kind, staged, CorpseRowGeometry.Sidehopper(graphicsVariant), definition.RowCount);
    }

    /// <summary>Variant <paramref name="variant"/> of a dead Zoomer, Ripper or Skree.</summary>
    internal static CorpseCharacterRot Tourian(EnemyTileSourceArtwork enemies, DeadTourianCorpseSpecies species, int variant)
    {
        DeadTourianCorpseVariant layout = DeadTourianCorpseStagingDefinitions.Variants(species)[variant];
        DeadTourianCorpseDefinition definition = DeadTourianCorpses.For(species, variant);
        CorpseStaging staged = CorpseStaging.Tourian(DeadMonsterSheet(enemies), layout);
        return Batch(definition.Kind, staged, CorpseRowGeometry.Tourian(species, layout), definition.RowCount);
    }

    /// <summary>
    /// Mother Brain's corpse, which uploads its six rows after each rot call but the last, whose finished schedule returns
    /// before its rows are shown again.
    /// </summary>
    internal static CorpseCharacterRot MotherBrain(BossSourceArtwork bosses)
    {
        RoomCharacterAtlas atlas = Ensure.NotNull(bosses).MotherBrainCorpse
            ?? throw new InvalidDataException($"Installed boss art holds no Mother Brain corpse ({MotherBrainCorpseSheet.FileName}).");
        if (atlas.Characters.Length != MotherBrainCorpseSheet.ByteCount)
            throw new InvalidDataException($"Installed {MotherBrainCorpseSheet.FileName} holds {atlas.Characters.Length} bytes, not {MotherBrainCorpseSheet.ByteCount}.");
        CorpseRotRow[] rows = [.. MotherBrainCorpseSheet.RotRows.Select(row =>
            new CorpseRotRow(row.Row, CorpseWorkMemory.CorpseRottingHeap + MotherBrainCorpseStaging.RottingOffset(row), MotherBrainCorpseStaging.ByteCount(row),
                MotherBrainCorpseSheet.RotRowFirstCharacter(row) * SpriteArtwork.CharacterByteCount))];
        return Batch("MotherBrain", rows, new CorpseRotSchedule(MotherBrainCorpseArtworkDefinitions.PixelRowCount),
            CorpseStaging.MotherBrain(atlas.Characters.Span), CorpseRowGeometry.MotherBrain, uploadsLastCall: false);
    }

    /// <summary>
    /// The dead Torizo, which uploads one half of its rows each main-AI call from its first, even then odd alternating,
    /// while its sand falls into its two strips every fifteenth rot call.
    /// </summary>
    internal static CorpseCharacterRot Torizo(EnemyTileSourceArtwork enemies)
    {
        ReadOnlyMemory<byte> tiles = Ensure.NotNull(enemies).TilesOf(CorpseSheets.DeadTorizoSheet);
        if (tiles.Length != CorpseSheets.DeadTorizoSheetByteCount)
            throw new InvalidDataException($"Installed {CorpseSheets.DeadTorizoSheet} tiles are {tiles.Length} bytes, not {CorpseSheets.DeadTorizoSheetByteCount}.");
        ReadOnlySpan<byte> atlas = tiles.Span;
        CorpseStaging staged = CorpseStaging.Torizo(atlas);
        CorpseRowGeometry geometry = CorpseRowGeometry.Torizo;
        // The even half's rows first, then the odd half's.
        CorpseRotRow[][] halves = [.. new[] { false, true }.Select(odd =>
            DeadTorizoRowUploadDefinitions.ForHalf(odd).Select((upload, index) => Row(index, CorpseRowSources.OfTorizoHalf(odd, index), upload)).ToArray())];
        CorpseRotRow[] rows = [.. halves[0], .. halves[1]];
        var schedule = new CorpseRotSchedule(DeadTorizoCorpseDefinitions.RowCount);
        var sand = new DeadTorizoSandCadence();
        List<byte[]?[]> afterCall = [[.. rows.Select(staged.Upload)]];
        bool rotting = true;
        while (rotting)
        {
            if (sand.Advance() is int line)
                staged.CopySandLine(atlas, line);
            rotting = schedule.Step((y, operation) => staged.MoveRow(geometry, y, operation), _ => { });
            afterCall.Add([.. rows.Select(staged.Upload)]);
        }
        return new CorpseCharacterRot("Torizo", rows, afterCall, halves[0].Length);
    }

    /// <summary>A dead monster of <paramref name="kind"/>, which uploads all its rows after each rot call, and none before the first.</summary>
    private static CorpseCharacterRot Batch(RottingCorpseKind kind, CorpseStaging staged, CorpseRowGeometry geometry, int rowCount) =>
        Batch(kind.Name, [.. DeadMonsterRowUploadDefinitions.ForKind(kind).Select((upload, index) => Row(index, CorpseRowSources.Of(kind, index), upload))],
            new CorpseRotSchedule(rowCount), staged, geometry, uploadsLastCall: true);

    /// <summary>
    /// A corpse that uploads all its <paramref name="rows"/> after each rot call it takes (the last one too unless
    /// <paramref name="uploadsLastCall"/> is false), and none before the first.
    /// </summary>
    private static CorpseCharacterRot Batch(string name, CorpseRotRow[] rows, CorpseRotSchedule schedule, CorpseStaging staged,
        CorpseRowGeometry geometry, bool uploadsLastCall)
    {
        var afterCall = new List<byte[]?[]> { new byte[]?[rows.Length] };
        bool rotting = true;
        while (rotting)
        {
            rotting = schedule.Step((y, operation) => staged.MoveRow(geometry, y, operation), _ => { });
            afterCall.Add(rotting || uploadsLastCall ? [.. rows.Select(staged.Upload)] : new byte[]?[rows.Length]);
        }
        return new CorpseCharacterRot(name, rows, afterCall, 0);
    }

    /// <summary>The installed dead-monster sheet every dead Sidehopper, Zoomer, Ripper and Skree stages from.</summary>
    private static ReadOnlySpan<byte> DeadMonsterSheet(EnemyTileSourceArtwork enemies)
    {
        ReadOnlyMemory<byte> tiles = Ensure.NotNull(enemies).TilesOf(CorpseSheets.DeadMonsterSheet);
        return tiles.Length == CorpseSheets.DeadMonsterSheetByteCount ? tiles.Span
            : throw new InvalidDataException($"Installed corpse sheet is {tiles.Length} bytes, not {CorpseSheets.DeadMonsterSheetByteCount}.");
    }

    /// <summary>
    /// The row <paramref name="upload"/> uploads, as the <paramref name="index"/>th of its corpse's rows, read from
    /// <paramref name="source"/> in the source bank's work memory (<see cref="CorpseRowSources"/>).
    /// </summary>
    private static CorpseRotRow Row(int index, int source, CorpseRowUpload upload) =>
        new(index, (int)new SnesAddress(LittleEndianBytes.HighByte(CorpseRowSources.BankWord), checked((ushort)source)),
            upload.SizeInBytes, upload.FirstTableByte);
}
