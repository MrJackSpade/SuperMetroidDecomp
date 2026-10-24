using SuperMetroid.Core.Bosses.Artwork;
using System.Buffers.Binary;
using static SuperMetroid.Verification.Enemies.EnemyContentIdentityFixtureVerification;
using static SuperMetroid.Verification.Rooms.TilemapJsonContractsVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>Checks how installed Kraid background pages populate the working tilemap.</summary>
internal static class KraidInstalledPresentationTailVerification
{
    /// <summary>Constructed authored pages expose priority loss even when stock tail words lack that bit.</summary>
    internal static void VerifyKraidWorkingMapTail()
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
        var art = new KraidBackgroundArtwork(upper, lower, new Dictionary<int, KraidHeadTilemapAtlas>(),
            (new EnemyIdentityFixture().Build().Bosses.KraidBackground ?? throw new InvalidOperationException("Verification requires new EnemyIdentityFixture().Build().KraidBackground.")).RoomBackgroundTiles);
        ushort[] working = KraidBodyFixtures.WorkingMap(new BossArtworkCatalog { KraidBackground = art });
        for (int word = 0; word < KraidBackgroundRomData.WorkingTilemapWords; word++)
        {
            ReadOnlySpan<byte> source = word < KraidBackgroundRomData.VisiblePageWords ? upper.Bytes.Span : lower.Bytes.Span;
            int sourceWord = word < KraidBackgroundRomData.VisiblePageWords || word >= KraidBackgroundRomData.PreservedLowerTailFirstWord
                ? word : word - KraidBackgroundRomData.WorkingLowerHalfWord;
            ushort expected = BinaryPrimitives.ReadUInt16LittleEndian(source[(sourceWord * 2)..]);
            if (word < KraidBackgroundRomData.PreservedLowerTailFirstWord) expected &= unchecked((ushort)~KraidBackgroundRomData.PriorityBit);
            if (word >= KraidBackgroundRomData.BlankRowWorkingWord) expected = KraidBackgroundRomData.BlankTile;
            AssertEqual(expected, working[word], "Kraid native copied/untouched/blank boundary at word " + word);
        }
        Console.WriteLine("PASS Kraid working map: all 2,048 words, copied priority clear, 224 untouched tail words and 32-word blank row.");
    }
}
