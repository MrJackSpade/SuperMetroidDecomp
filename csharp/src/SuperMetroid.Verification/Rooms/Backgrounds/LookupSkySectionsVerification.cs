using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Verification.Rooms.Backgrounds;

/// <summary>
/// Checks the scrolling sky's bands (#1687) against the cartridge's per-line scroll table: each of its entries is a row
/// boundary, a 16.16 speed and the scroll word the speed adds to and the entry's lines show; the word after the
/// last entry is the sky's end row; and the table's lookup compares rows as sixteen-bit words.
/// </summary>
internal static class LookupSkySectionsVerification
{
    /// <summary>The cartridge's first table entry.</summary>
    private const int FirstEntry = 0x88aec1;

    /// <summary>The bytes of one entry: its top row, its speed's fraction and whole words, and its scroll word's address.</summary>
    private const int EntryBytes = 8;

    /// <summary>The loop bound the sky's step compares its entry offset with.</summary>
    private const int LoopBound = 0x88adef;

    /// <summary>The address of the table's first scroll word; each word takes four bytes.</summary>
    private const int FirstScrollWord = 0x9f80;

    /// <summary>The bytes between scroll words.</summary>
    private const int ScrollWordBytes = 4;

    /// <summary>The 16.16 phase whose every drift has come back to where it started: 2^20 moving frames, the slowest speed's.</summary>
    private const int FullPeriodFrames = 1 << 20;

    /// <summary>One entry of the cartridge's table.</summary>
    /// <param name="TopRow">The first row the entry covers, a sixteen-bit row.</param>
    /// <param name="Speed">The 16.16 speed the entry adds to its scroll word each frame.</param>
    /// <param name="ScrollWord">The scroll word the entry adds to and its lines show.</param>
    private readonly record struct CartridgeEntry(int TopRow, uint Speed, int ScrollWord);

    /// <summary>The cartridge's sky table, decoded.</summary>
    /// <param name="Entries">The table's entries from the top down.</param>
    /// <param name="EndRow">The row below the last entry.</param>
    private sealed record CartridgeSky(CartridgeEntry[] Entries, int EndRow)
    {
        /// <summary>The scroll words the entries add to, one per entry.</summary>
        internal int ScrollWords => Entries.Length;

        /// <summary>The scroll word sixteen-bit row <paramref name="row"/> shows, or null for the table's fallback (unshifted).</summary>
        internal int? ScrollWordAt(int row)
        {
            for (int index = 0; index < Entries.Length; index++)
            {
                int next = index + 1 < Entries.Length ? Entries[index + 1].TopRow : EndRow;
                if (row >= Entries[index].TopRow && row < next)
                    return Entries[index].ScrollWord;
            }
            return null;
        }

        /// <summary>One moving frame of the cartridge's step: every entry adds its speed, then the last scroll word is cleared.</summary>
        internal void Advance(uint[] words)
        {
            foreach (CartridgeEntry entry in Entries)
                words[entry.ScrollWord] = unchecked(words[entry.ScrollWord] + entry.Speed);
            words[^1] = 0;
        }
    }

    /// <summary>Decodes the cartridge's sky table.</summary>
    private static CartridgeSky Decode(CartridgeImportAddressSpace rom)
    {
        int count = ReadVerificationWord(rom, LoopBound) / EntryBytes;
        AssertEqual(23, count, "Native sky loop bound");
        var entries = new CartridgeEntry[count];
        for (int index = 0; index < count; index++)
        {
            int source = FirstEntry + index * EntryBytes;
            entries[index] = new(ReadVerificationWord(rom, source),
                SixteenSixteenWords.Compose(ReadVerificationWord(rom, source + 4), ReadVerificationWord(rom, source + 2)),
                (ReadVerificationWord(rom, source + 6) - FirstScrollWord) / ScrollWordBytes);
        }
        return new CartridgeSky(entries, ReadVerificationWord(rom, FirstEntry + count * EntryBytes));
    }

    /// <summary>
    /// The sky's bands cover the cartridge's rows, and every gameplay line of every camera position shows the scroll the
    /// cartridge's table gives it: its sixteen-bit row's entry's scroll word, or none at the table's fallback rows. A
    /// camera above the room's top, whose rows the table reads past row 65535, shows what the port's negative rows show.
    /// </summary>
    internal static void VerifySkySectionTopPositions(CartridgeImportAddressSpace rom)
    {
        CartridgeSky cartridge = Decode(rom);
        AssertEqual(cartridge.EndRow, ScrollingSkyBandDefinitions.EndRow, "Sky end row");
        AssertEqual(cartridge.Entries[0].TopRow, ScrollingSkyBandDefinitions.Bands[0].TopRow, "Sky first row");
        foreach (SkyBand band in ScrollingSkyBandDefinitions.Bands)
            AssertTrue(cartridge.Entries.Any(entry => entry.TopRow == band.TopRow), $"Sky band at row {band.TopRow} begins at a cartridge entry");

        var sky = new ScrollingSkyState(ScrollingSkyChunkTable.Land);
        var words = new uint[cartridge.ScrollWords];
        for (int frame = 0; frame < 16; frame++)
        {
            sky.ProcessFrame(0x0300, false);
            cartridge.Advance(words);
        }
        var expected = new ushort[ushort.MaxValue + 1];
        for (int row = 0; row <= ushort.MaxValue; row++)
            expected[row] = cartridge.ScrollWordAt(row) is { } word ? FixedCoordinate.WholeOf(unchecked((int)words[word])) : (ushort)0;
        for (int cameraY = 0; cameraY <= ushort.MaxValue; cameraY++)
        {
            ScrollingSkyLayer layer = sky.Publish(cameraY)
                ?? throw new InvalidOperationException("The configured sky publishes its layer.");
            ScrollBands bands = SkyBandScrolls.Of(layer, 0);
            for (int line = ScreenGeometry.GameplayHudHeightPixels; line < ScreenGeometry.ScreenHeightPixels; line++)
            {
                ushort actual = ScrollShapeLines.BandScroll(bands, line);
                ushort want = expected[CoordinateAxis.Wrap(cameraY + line)];
                if (actual != want)
                    AssertEqual(want, actual, $"Sky camera {cameraY} line {line}: band selection, camera above the room and fallback rows");
            }
        }
    }

    /// <summary>
    /// Over a full period of the drifts, with frozen frames among the moving ones, every band shows the offset the
    /// cartridge's scroll word for its rows reaches, each frozen frame shows every band unshifted and moves nothing, and
    /// the period ends with every band back where it started and not before.
    /// </summary>
    internal static void VerifySkyBandDriftsOverFullPeriod(CartridgeImportAddressSpace rom)
    {
        CartridgeSky cartridge = Decode(rom);
        var sky = new ScrollingSkyState(ScrollingSkyChunkTable.Land);
        var words = new uint[cartridge.ScrollWords];
        int moving = 0, frozenFrames = 0, firstReturn = 0;
        for (int frame = 0; moving < FullPeriodFrames; frame++)
        {
            bool frozen = frame % 4099 is >= 4096;
            sky.ProcessFrame(0x0300, frozen);
            if (frozen)
                frozenFrames++;
            else
            {
                cartridge.Advance(words);
                moving++;
            }
            ScrollingSkyLayer layer = sky.Publish(0x0300)
                ?? throw new InvalidOperationException("The configured sky publishes its layer.");
            if (layer.ShowsDrift == frozen)
                AssertEqual(!frozen, layer.ShowsDrift, $"frame {frame} shows the drift only when time runs");
            bool atStart = true;
            foreach (CartridgeEntry entry in cartridge.Entries)
            {
                int want = FixedCoordinate.WholeOf(unchecked((int)words[entry.ScrollWord]));
                int actual = layer.BandAt(entry.TopRow)?.Offset
                    ?? throw new InvalidOperationException($"No sky band covers row {entry.TopRow}.");
                if (actual != want)
                    AssertEqual(want, actual, $"frame {frame}: the band at row {entry.TopRow} shows its cartridge scroll word");
                if (frozen && layer.OffsetAt(entry.TopRow) != 0)
                    AssertEqual(0, layer.OffsetAt(entry.TopRow), $"frozen frame {frame} shows row {entry.TopRow} unshifted");
                atStart &= actual == 0;
            }
            if (!frozen && atStart && firstReturn == 0)
                firstReturn = moving;
        }
        AssertTrue(words.All(word => word == 0), "the cartridge's scroll words end the period where they started");
        AssertEqual(FullPeriodFrames, firstReturn, "every band first shows its starting offset again at the period's end");
        AssertTrue(frozenFrames > 0, "the period includes frozen frames");
    }
}
