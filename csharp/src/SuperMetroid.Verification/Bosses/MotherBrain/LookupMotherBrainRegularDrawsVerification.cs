using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Verification.Areas.Escape;
using static SuperMetroid.Verification.Rooms.RoomObjects.InstalledFramePictureAssertions;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>Checks Mother Brain's fill rows and tube clears and their shape descriptors against the cartridge; their pictures are installed (#1557).</summary>
internal static class LookupMotherBrainRegularDrawsVerification
{
    /// <summary>The two fill rows' and six tube clears' draw-list addresses and published names.</summary>
    private static readonly LookupEscapeGateStockFrame[] Regular = [new(0x966d, "background-row-e-unused"), new(0x968b, "background-row-f-unused"),
        new(0x96a9, "clear-ceiling-block"), new(0x96b1, "clear-ceiling-tube"), new(0x96bf, "clear-bottom-middle-side-tube"),
        new(0x96cb, "clear-bottom-middle-tubes"), new(0x96ef, "clear-bottom-left-tube"), new(0x9703, "clear-bottom-right-tube")];

    /// <summary>Compares all eight draw lists' geometry and collision with native words, including ownership and bounds.</summary>
    internal static void VerifyMotherBrainRegularDrawMapping(SuperMetroidAddressSpace rom)
    {
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            bool owned = Regular.Any(frame => frame.Pointer == value);
            AssertEqual(owned, MotherBrainFakeDeathDraws.TryDescribeRegular(NativeObjectDraws.At<MotherBrainFakeDeathDraw>(value), out var shape),
                $"Mother Brain regular descriptor ownership {value:X4}");
            if (!owned)
            {
                AssertEqual(default(MotherBrainFakeDeathDraws.RegularDraw), shape, "Mother Brain regular missing descriptor");
                continue;
            }
            foreach (int invalid in new[] { int.MinValue, -1, shape.RunCount, int.MaxValue })
            {
                AssertThrows<ArgumentOutOfRangeException>(() => shape.CollisionAt(invalid, 0), "Mother Brain regular run bounds");
                AssertThrows<ArgumentOutOfRangeException>(() => shape.Start(invalid), "Mother Brain regular start bounds");
            }
            for (int run = 0; run < shape.RunCount; run++)
            {
                int selectedRun = run;
                foreach (int invalid in new[] { int.MinValue, -1, shape.Count(run), int.MaxValue })
                    AssertThrows<ArgumentOutOfRangeException>(() => shape.CollisionAt(selectedRun, invalid), "Mother Brain regular cell bounds");
            }
        }
        AssertLayoutsAsNative(rom, RoomObjectMotherBrainFakeDeathVisualFiles.Family, Regular, collision: true);
        int cells = 0;
        foreach (LookupEscapeGateStockFrame frame in Regular)
        {
            _ = NativeTerrain(rom, frame.Pointer, out int written);
            cells += written;
        }
        AssertEqual(63, cells, "Mother Brain regular sixty-three original physical cells");
    }
}
