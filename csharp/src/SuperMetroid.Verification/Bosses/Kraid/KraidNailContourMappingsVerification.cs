using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>Compares Kraid's nail contour ledges and the installed record past them with the original cartridge words.</summary>
internal static class KraidNailContourMappingsVerification
{
    /// <summary>The cartridge's nail contour: a left-offset word then a bound word per record, the ledges first.</summary>
    private const int ContourRecords = 0xa7bf1d;

    /// <summary>The bytes of one contour record.</summary>
    private const int RecordBytes = 4;

    /// <summary>Follows the original contour walk over the cartridge: the first record whose bound lies above the nail.</summary>
    private static int NativeRecord(Func<int, ushort> word, int relativeY)
    {
        int record = 0;
        while (unchecked((short)(word(ContourRecords + 2 + RecordBytes * record) - relativeY)) >= 0)
        {
            record++;
            if (record >= 256) throw new InvalidOperationException("Native nail contour walk did not terminate.");
        }
        return record;
    }

    /// <summary>Checks the four ledges' left offsets and every relative Y the ledges select against the native contour walk.</summary>
    internal static void VerifyKraidNailLeftOffsets(CartridgeImportAddressSpace rom)
    {
        ushort Word(int address) => LittleEndianBytes.Word(KraidNailLeftOffsetsRomByte(rom, address), KraidNailLeftOffsetsRomByte(rom, address + 1));
        AssertEqual(4, KraidNailContourDefinitions.LedgeCount, "Nail contour ledge count");
        for (int ledge = 0; ledge < KraidNailContourDefinitions.LedgeCount; ledge++)
            AssertEqual(Word(ContourRecords + RecordBytes * ledge), KraidNailContour.LeftAt(ledge), "Original nail left word");
        AssertThrows<ArgumentOutOfRangeException>(() => KraidNailContour.LeftAt(-1), "Negative nail ledge");
        AssertThrows<ArgumentOutOfRangeException>(() => KraidNailContour.LeftAt(KraidNailContourDefinitions.LedgeCount), "Past nail ledge");
        // Independent original-word walk: a relative Y the walk settles on a ledge for selects that ledge's word, and
        // one it walks past every ledge for selects no ledge.
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            int record = NativeRecord(Word, raw);
            int? ledgeLeft = KraidNailContour.LedgeLeftOffset(raw);
            if (record < KraidNailContourDefinitions.LedgeCount)
                AssertTrue(ledgeLeft == Word(ContourRecords + RecordBytes * record), "Every relative Y on a ledge selects the native left offset");
            else
                AssertTrue(ledgeLeft is null, "Every relative Y past the ledges selects no ledge");
        }
    }

    /// <summary>Reads a source byte for the independent Kraid nail left-offset oracle.</summary>
    private static byte KraidNailLeftOffsetsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks the four ledges' top boundaries and rejects out-of-range ledges.</summary>
    internal static void VerifyKraidNailTopBoundaries(CartridgeImportAddressSpace rom)
    {
        for (int ledge = 0; ledge < KraidNailContourDefinitions.LedgeCount; ledge++)
        {
            int address = ContourRecords + 2 + RecordBytes * ledge;
            ushort expected = LittleEndianBytes.Word(KraidNailTopBoundariesRomByte(rom, address), KraidNailTopBoundariesRomByte(rom, address + 1));
            AssertEqual(expected, KraidNailContour.TopAt(ledge), "Original nail top boundary");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => KraidNailContour.TopAt(-1), "Negative nail ledge top");
        AssertThrows<ArgumentOutOfRangeException>(() => KraidNailContour.TopAt(KraidNailContourDefinitions.LedgeCount), "Past nail ledge top");
    }

    /// <summary>Reads a source byte for the independent Kraid nail top-boundary oracle.</summary>
    private static byte KraidNailTopBoundariesRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>
    /// Decodes the contour record the walk reads past Kraid's four ledges from the cartridge and compares it with the
    /// installed <see cref="KraidNailPastLedges"/> (#1500): every relative Y the original walk settles on that record for
    /// is pushed back at its left word, and every relative Y the walk carries further is rejected.
    /// </summary>
    internal static void VerifyKraidNailPastLedges(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        ushort Word(int address) => cartridge.ReadCartridgeWord(address);
        KraidNailPastLedges installed = InstalledFixtures.BossAndIntroOverreads.KraidNailPastLedges;
        int pastLedges = KraidNailContourDefinitions.LedgeCount;
        ushort nativeLeft = Word(ContourRecords + RecordBytes * pastLedges);
        int pushed = 0;
        int rejected = 0;
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            int record = NativeRecord(Word, raw);
            if (record < pastLedges)
                continue;
            if (record == pastLedges)
            {
                AssertEqual(nativeLeft, installed.LeftOffset(raw), "A nail past the ledges is pushed back at the native record's left word");
                pushed++;
            }
            else
            {
                AssertThrows<ArgumentOutOfRangeException>(() => installed.LeftOffset(raw), "A nail beyond the record past the ledges is rejected");
                rejected++;
            }
        }
        AssertTrue(pushed > 0 && rejected > 0, "The walk both settles on the record past the ledges and carries beyond it");
        Console.WriteLine($"Kraid nail past ledges: {pushed} relative-Y words push at the native record, {rejected} beyond it are rejected.");
    }

    /// <summary>Runs the installed record past Kraid's nail contour ledges against the cartridge.</summary>
    [VerificationCommand("--kraid-nail-past-ledges")]
    internal static int KraidNailPastLedgesCommand()
    {
        Suite(nameof(VerifyKraidNailPastLedges), () => VerifyKraidNailPastLedges(LoadRepositoryRom()));
        return 0;
    }
}
