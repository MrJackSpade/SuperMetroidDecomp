using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Checks the 35 Kraid foot OAM frame identities and two-part geometry against cartridge data.</summary>
internal static class KraidFootFrameGeometryVerification
{
    /// <summary>Traverses native presentation operands and compares ordered frame keys and bounds.</summary>
    internal static void VerifyKraidFootFrameGeometry(SuperMetroidAddressSpace rom)
    {
        // The typed foot programs match bank $A7 (--kraid-part-animations).
        var selected = new SortedSet<ushort>(KraidAnimations.Spritemaps(KraidPartProgram.FootInitial, KraidPartProgram.FootNeutral, KraidPartProgram.FootWalkingForward,
                     KraidPartProgram.FootWalkingForwardFinished, KraidPartProgram.FootLungeForward, KraidPartProgram.FootLungeForwardFinished,
                     KraidPartProgram.FootWalkingBackward, KraidPartProgram.FootWalkingBackwardLoop));
        AssertEqual(35, selected.Count, "original selected Kraid foot roots");
        var expected = selected.Select(pointer => new EnemyExtendedFrameDefinition(0xa7, pointer,
            $"kraid_foot_oam_{pointer:X4}")).ToArray();
        AssertEqual(expected.Length, KraidFootVisualDefinitions.Frames.Length, "Kraid foot sequence length");
        for (int index = 0; index < expected.Length; index++)
        {
            AssertEqual(expected[index], KraidFootVisualDefinitions.Frame(index), "native Kraid foot identity and key");
            AssertEqual(expected[index], KraidFootVisualDefinitions.Frames[index], "Kraid foot indexed sequence");
            int address = 0xa70000 | expected[index].Pointer;
            AssertEqual(2, SnesLittleEndian.Word(KraidFootFrameGeometryRomByte(rom, address), KraidFootFrameGeometryRomByte(rom, address + 1)), "native two-component frame size");
        }
        int ordinal = 0;
        foreach (var frame in KraidFootVisualDefinitions.Frames)
            AssertEqual(expected[ordinal++], frame, "Kraid foot enumeration order");
        AssertEqual(35, ordinal, "Kraid foot enumeration count");
        AssertTrue(expected.SequenceEqual(KraidFootVisualDefinitions.Frames.ToArray()), "Kraid foot materialization");
        AssertThrows<IndexOutOfRangeException>(() => KraidFootVisualDefinitions.Frame(-1), "negative foot index");
        AssertThrows<IndexOutOfRangeException>(() => KraidFootVisualDefinitions.Frame(35), "foot index past end");
    }

    /// <summary>Reads a native foot presentation operand or spritemap count byte.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid foot frame geometry")]
    private static byte KraidFootFrameGeometryRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
