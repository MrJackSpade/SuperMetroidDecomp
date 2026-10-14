using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Crocomire;

namespace SuperMetroid.Verification.Bosses.Crocomire;

/// <summary>Checks Crocomire BG2 frame pointers and pose-relative vertical scroll against native data.</summary>
internal static class CrocomireBg2ScrollVerification
{
    /// <summary>Runs the pointer-order and body-position correction checks for all seventeen BG2 entries.</summary>
    internal static void VerifyCrocomireBg2ScrollDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyCrocomireBg2FramePositions), () => VerifyCrocomireBg2FramePositions(rom));
        Suite(nameof(VerifyCrocomireBg2PoseCorrections), () => VerifyCrocomireBg2PoseCorrections(rom));
    }

    /// <summary>Confirms all seventeen native BG2 spritemap pointers are unique and remain in authored order.</summary>
    private static void VerifyCrocomireBg2FramePositions(SuperMetroidAddressSpace rom)
    {
        AssertEqual(17, CrocomireBg2ScrollDefinitions.EntryCount, "all native BG2 frame pointers");
        var seen = new HashSet<ushort>();
        for (int index = 0; index < 17; index++)
        {
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank((0xa48b79 + 2 * index));
            ushort actual = CrocomireBg2ScrollDefinitions.Entry(index).SpritemapPointer;
            AssertEqual(expected, actual, "Crocomire BG2 native pointer order");
            AssertTrue(seen.Add(actual), "Crocomire BG2 pointers are unique");
        }
        AssertThrows<IndexOutOfRangeException>(() => CrocomireBg2ScrollDefinitions.Entry(-1), "negative BG2 entry");
        AssertThrows<IndexOutOfRangeException>(() => CrocomireBg2ScrollDefinitions.Entry(17), "BG2 entry past end");
    }

    /// <summary>Compares per-pose offsets and wrapped body-Y scroll, including all unselected or misaligned pointers in the 16-bit domain.</summary>
    private static void VerifyCrocomireBg2PoseCorrections(SuperMetroidAddressSpace rom)
    {
        var originalOffsets = new Dictionary<ushort, ushort>();
        for (int index = 0; index < 17; index++)
        {
            ushort pointer = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank((0xa48b79 + 2 * index));
            ushort offset = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank((0xa40000 | unchecked((ushort)(pointer + 0x1c))));
            originalOffsets[pointer] = offset;
            AssertEqual(offset, CrocomireBg2ScrollDefinitions.Entry(index).Offset, "Crocomire BG2 native pose correction");
            foreach (ushort bodyY in new ushort[] { 0, 0x43, 0x90, 0xfffc })
            {
                ushort expected = unchecked((ushort)(0x43 - bodyY + offset));
                AssertEqual(expected, CrocomireBg2ScrollDefinitions.VerticalScroll(bodyY, pointer),
                    "Crocomire BG2 mapped pose and wrapped body position");
            }
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
        {
            ushort offset = originalOffsets.GetValueOrDefault((ushort)pointer);
            AssertEqual(unchecked((ushort)(0x43 - 0x90 + offset)),
                CrocomireBg2ScrollDefinitions.VerticalScroll(0x90, (ushort)pointer),
                "Crocomire BG2 full pointer selection domain");
        }
    }
}
