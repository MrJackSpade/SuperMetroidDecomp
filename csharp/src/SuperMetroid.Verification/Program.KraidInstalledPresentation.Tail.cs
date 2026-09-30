using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    /// <summary>Constructed authored pages expose priority loss even when stock tail words lack that bit.</summary>
    private static void VerifyKraidWorkingMapTail()
    {
        RoomBackgroundTilemapAtlas Map(int seed)
        {
            var pages = Enumerable.Range(0, 2).Select(page => new RoomBackgroundTilemapPage
            {
                Cells = Enumerable.Range(0, RoomBackgroundTilemapFormat.CellsPerPage)
                    .Select(cell => TilemapContractCell(unchecked((ushort)(seed + page * 1024 + cell)))).ToArray(),
            }).ToArray();
            using var json = new MemoryStream();
            RoomBackgroundTilemapAtlas.Write(json, new RoomBackgroundTilemapDocument
            { Version = RoomBackgroundTilemapFormat.Version, Pages = pages }, KraidBackgroundRomData.DecompressedTilemapBytes);
            json.Position = 0;
            return RoomBackgroundTilemapAtlas.Load(json, KraidBackgroundRomData.DecompressedTilemapBytes);
        }
        RoomBackgroundTilemapAtlas upper = Map(0), lower = Map(KraidBackgroundRomData.PriorityBit);
        var art = new KraidBackgroundArtwork(upper, lower, new Dictionary<ushort, KraidHeadTilemapAtlas>(),
            new EnemyIdentityFixture().Build().KraidBackground!.RoomBackgroundTiles);
        var catalog = new EnemyTileArtworkCatalog(new Dictionary<ushort, RoomCharacterAtlas>(),
            new Dictionary<ushort, EnemyPaletteSheet>(), kraidBackground: art);
        KraidEnemyState working = BuildKraidWorkingMap(catalog);
        for (int word = 0; word < KraidBackgroundRomData.WorkingTilemapWords; word++)
        {
            ReadOnlySpan<byte> source = word < KraidBackgroundRomData.VisiblePageWords ? upper.Transfer.Span : lower.Transfer.Span;
            int sourceWord = word < KraidBackgroundRomData.VisiblePageWords || word >= KraidBackgroundRomData.PreservedLowerTailFirstWord
                ? word : word - KraidBackgroundRomData.WorkingLowerHalfWord;
            ushort expected = BinaryPrimitives.ReadUInt16LittleEndian(source[(sourceWord * 2)..]);
            if (word < KraidBackgroundRomData.PreservedLowerTailFirstWord) expected &= unchecked((ushort)~KraidBackgroundRomData.PriorityBit);
            if (word >= KraidBackgroundRomData.BlankRowWorkingWord) expected = KraidBackgroundRomData.BlankTile;
            AssertEqual(expected, working.BackgroundTilemapWords[word], "Kraid native copied/untouched/blank boundary at word " + word);
        }
        Console.WriteLine("PASS Kraid working map: all 2,048 words, copied priority clear, 224 untouched tail words and 32-word blank row.");
    }
}
