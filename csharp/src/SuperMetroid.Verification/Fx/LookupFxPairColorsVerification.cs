using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Fx.LookupFxBlendSelectorsVerification;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;

using SuperMetroid.Core.Fx;

namespace SuperMetroid.Verification.Fx;

/// <summary>Checks which room-FX blend color components are derived and which remain editable.</summary>
internal static class LookupFxPairColorsVerification
{
    /// <summary>Independent reviewed bit mask of native paired-color components calculated by blend rules.</summary>
    internal static int OriginalFxPairCalculatedMask(byte id, int color) => color == 0 && id is (0x42 or 0xe2 or 0xee) ? 0x7fff : id switch
    {
        0x02 => 0x7c1f,
        0x22 or 0x62 => 0x7fe0,
        0x42 => 0x03e0,
        0xe8 => 0x7fe0,
        _ => 0,
    };

    /// <summary>Compares calculated red channels with native, installed, and applied CGRAM colors.</summary>
    internal static void VerifyFxPairRed(ISnesAddressSpace rom, RoomFxPaletteBlendCatalog stock) => VerifyFxPairComponent(rom, stock, 0);
    /// <summary>Compares calculated green channels with native, installed, and applied CGRAM colors.</summary>
    internal static void VerifyFxPairGreen(ISnesAddressSpace rom, RoomFxPaletteBlendCatalog stock) => VerifyFxPairComponent(rom, stock, 5);
    /// <summary>Compares calculated blue channels with native, installed, and applied CGRAM colors.</summary>
    internal static void VerifyFxPairBlue(ISnesAddressSpace rom, RoomFxPaletteBlendCatalog stock) => VerifyFxPairComponent(rom, stock, 10);

    /// <summary>Checks one RGB5 channel wherever the native blend rule derives it.</summary>
    private static void VerifyFxPairComponent(ISnesAddressSpace rom, RoomFxPaletteBlendCatalog stock, int shift)
    {
        foreach (byte id in OriginalFxBlendIds())
        for (int color = 0; color < 2; color++)
        {
            if ((OriginalFxPairCalculatedMask(id, color) & (31 << shift)) == 0) continue;
            ushort native = ReadVerificationWord(rom, 0x89aa02 + id + color * 2);
            int expected = (native >> shift) & 31;
            int? calculated = shift switch
            {
                0 => RoomFxPaletteBlendDefinitions.CalculatedPairRed(FxNativeWords.PaletteBlend(id), color == 0),
                5 => RoomFxPaletteBlendDefinitions.CalculatedPairGreen(FxNativeWords.PaletteBlend(id), native & 31, color == 0),
                _ => RoomFxPaletteBlendDefinitions.CalculatedPairBlue(FxNativeWords.PaletteBlend(id), native & 31, (native >> 5) & 31, color == 0),
            };
            AssertEqual(expected, (calculated ?? throw new InvalidOperationException("Verification requires calculated.")), "Native paired-color component rule");
            AssertEqual(expected, (stock.Resolve(FxNativeWords.PaletteBlend(id))[color].ToWord() >> shift) & 31, "Installed paired-color component");
            var cgram = new GameplayColours(new SpriteColourSets());
            stock.Apply(cgram.BackgroundSets, FxNativeWords.PaletteBlend(id));
            AssertEqual(expected, (cgram.Colors[25 + color].ToWord() >> shift) & 31, "Applied paired-color component");
        }
    }

    /// <summary>Checks omitted derived storage, full RGB5 edit round-trips, and invalid selector bounds.</summary>
    internal static void VerifyFxPairStorageAndEdits(ISnesAddressSpace rom)
    {
        foreach (byte id in OriginalFxBlendIds())
        {
            for (int color = 0; color < 2; color++)
            {
                var owner = new RoomFxPairColor(FxNativeWords.PaletteBlend(id), RgbColor.FromWord(checked(ReadVerificationWord(rom, 0x89aa02 + id + color * 2))), (color == 0) ? RoomFxPairMember.Primary : RoomFxPairMember.Secondary);
                foreach (var field in new VerifyFxPairStorageAndEditsCase[]
                         { new VerifyFxPairStorageAndEditsCase(pair => pair.redOverride, 0), new VerifyFxPairStorageAndEditsCase(pair => pair.greenOverride, 5), new VerifyFxPairStorageAndEditsCase(pair => pair.blueOverride, 10) })
                {
                    bool stored = (OriginalFxPairCalculatedMask(id, color) & (31 << field.Shift)) == 0;
                    AssertEqual(stored, field.Read(owner) is not null, "Stock derived component is not stored");
                    _ = owner.CreateColor();
                    AssertEqual(stored, field.Read(owner) is not null, "Output creates no component cache");
                }
            }
            // Exact property: every externally valid RGB5 edit round-trips unchanged,
            // including intensities above the stock tint's range and saturated values.
            foreach (bool isPrimary in new[] { false, true })
            for (int word = 0; word < 0x8000; word++)
                AssertEqual((ushort)word, new RoomFxPairColor(FxNativeWords.PaletteBlend(id), RgbColor.FromWord(checked((ushort)word)), isPrimary ? RoomFxPairMember.Primary : RoomFxPairMember.Secondary).CreateColor(), "All RGB5 edits round-trip");
            foreach (bool isPrimary in new[] { false, true })
            foreach (int invalid in new[] { int.MinValue, -1, 32, int.MaxValue })
            {
                AssertThrows<ArgumentOutOfRangeException>(() => RoomFxPaletteBlendDefinitions.CalculatedPairGreen(FxNativeWords.PaletteBlend(id), invalid, isPrimary), "Green rule RGB5 bounds");
                AssertThrows<ArgumentOutOfRangeException>(() => RoomFxPaletteBlendDefinitions.CalculatedPairBlue(FxNativeWords.PaletteBlend(id), invalid, 0, isPrimary), "Blue rule RGB5 bounds");
                AssertThrows<ArgumentOutOfRangeException>(() => RoomFxPaletteBlendDefinitions.CalculatedPairBlue(FxNativeWords.PaletteBlend(id), 0, invalid, isPrimary), "Blue rule green-input bounds" );
            }
        }
        for (int value = 0; value <= byte.MaxValue; value++)
        {
            byte id = (byte)value;
            if (OriginalFxBlendIds().Contains(id)) continue;
            foreach (bool isPrimary in new[] { false, true })
            {
                AssertThrows<InvalidDataException>(() => RoomFxPaletteBlendDefinitions.CalculatedPairRed(FxNativeWords.PaletteBlend(id), isPrimary), "Unknown red rule selector rejects");
                AssertThrows<InvalidDataException>(() => RoomFxPaletteBlendDefinitions.CalculatedPairGreen(FxNativeWords.PaletteBlend(id), 0, isPrimary), "Unknown green rule selector rejects");
                AssertThrows<InvalidDataException>(() => RoomFxPaletteBlendDefinitions.CalculatedPairBlue(FxNativeWords.PaletteBlend(id), 0, 0, isPrimary), "Unknown blue rule selector rejects");
            }
        }
    }

    /// <summary>Clears derived color bits in the cartridge source to check extractor recomputation.</summary>
    internal sealed class DerivedBlendSourceGuard(ISnesAddressSpace source) : ISnesWritableAddressSpace, IImportCartridgeSource
    {
        /// <summary>Returns a source byte with calculated pair-color bits removed at blend records.</summary>
        public byte ReadByte(int address)
        {
            byte original = CpuBusRegionRead.Read(source, address);
            foreach (byte id in OriginalFxBlendIds())
            {
                int delta = address - (0x89aa02 + id);
                if ((uint)delta < 4)
                    return (byte)(original & ~(OriginalFxPairCalculatedMask(id, delta / 2) >> (8 * (delta & 1))));
                if (delta is 4 or 5 && id is 0x22 or 0x62)
                    return (byte)(original & ~(0x7fe0 >> (8 * (delta & 1))));
            }
            return original;
        }
        /// <summary>Applies the same derived-bit masking to explicit cartridge reads.</summary>
        public byte ReadCartridgeByte(int address) => ReadByte(address);
        /// <summary>Forwards writes to the underlying address space during extractor checks.</summary>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
