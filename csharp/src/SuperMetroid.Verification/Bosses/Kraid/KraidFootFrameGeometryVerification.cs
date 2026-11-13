using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Bosses.Kraid.Artwork;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>Checks the 35 Kraid foot OAM frame identities and two-part geometry against cartridge data.</summary>
internal static class KraidFootFrameGeometryVerification
{
    /// <summary>Traverses native presentation operands and compares ordered frame keys and bounds.</summary>
    internal static void VerifyKraidFootFrameGeometry(CartridgeImportAddressSpace rom)
    {
        // The typed foot programs match bank $A7 (--kraid-part-animations).
        var selected = new SortedSet<ushort>(KraidAnimationsTooling.Spritemaps(KraidPartSegment.FootInitial, KraidPartSegment.FootNeutral, KraidPartSegment.FootWalkingForward,
                     KraidPartSegment.FootWalkingForwardFinished, KraidPartSegment.FootLungeForward, KraidPartSegment.FootLungeForwardFinished,
                     KraidPartSegment.FootWalkingBackward, KraidPartSegment.FootWalkingBackwardLoop));
        AssertEqual(35, selected.Count, "original selected Kraid foot roots");
        var expected = selected.Select(pointer => new NativeEnemySpritemapDefinition(0xa7, pointer,
            $"kraid_foot_oam_{pointer:X4}")).ToArray();
        AssertEqual(expected.Length, KraidFootVisualDefinitions.Frames.Length, "Kraid foot sequence length");
        for (int index = 0; index < expected.Length; index++)
        {
            AssertEqual(expected[index], KraidFootVisualDefinitionsConstants.Frame(index), "native Kraid foot identity and key");
            AssertEqual(expected[index], KraidFootVisualDefinitions.Frames[index], "Kraid foot indexed sequence");
            int address = 0xa70000 | expected[index].Pointer;
            AssertEqual(2, LittleEndianBytes.Word(KraidFootFrameGeometryRomByte(rom, address), KraidFootFrameGeometryRomByte(rom, address + 1)), "native two-component frame size");
        }
        int ordinal = 0;
        foreach (var frame in KraidFootVisualDefinitions.Frames)
            AssertEqual(expected[ordinal++], frame, "Kraid foot enumeration order");
        AssertEqual(35, ordinal, "Kraid foot enumeration count");
        AssertTrue(expected.SequenceEqual(KraidFootVisualDefinitions.Frames.ToArray()), "Kraid foot materialization");
        AssertThrows<ArgumentOutOfRangeException>(() => KraidFootVisualDefinitionsConstants.Frame(-1), "negative foot index");
        AssertThrows<ArgumentOutOfRangeException>(() => KraidFootVisualDefinitionsConstants.Frame(35), "foot index past end");
    }

    /// <summary>Reads a native foot presentation operand or spritemap count byte.</summary>
    private static byte KraidFootFrameGeometryRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
