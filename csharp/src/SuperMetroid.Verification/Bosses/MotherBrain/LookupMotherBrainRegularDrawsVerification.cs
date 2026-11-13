using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Verification.Areas.Escape;
using static SuperMetroid.Verification.Rooms.RoomObjects.InstalledFramePictureAssertions;
using static SuperMetroid.Verification.Rooms.RoomObjects.RoomBlockShapeAssertions;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>Checks Mother Brain's tube clears and their shape descriptors against the cartridge; their pictures are installed (#1557).</summary>
internal static class LookupMotherBrainRegularDrawsVerification
{
    /// <summary>
    /// The six tube clears' draw-list addresses and published names. The two solid rows the cartridge stores before them
    /// ($84:966D and $84:968B) are drawn by nothing in the game (#1686), so they are no frames of it.
    /// </summary>
    internal static readonly LookupEscapeGateStockFrame[] Regular = [new(0x96a9, "clear-ceiling-block"), new(0x96b1, "clear-ceiling-tube"), new(0x96bf, "clear-bottom-middle-side-tube"),
        new(0x96cb, "clear-bottom-middle-tubes"), new(0x96ef, "clear-bottom-left-tube"), new(0x9703, "clear-bottom-right-tube")];

    /// <summary>
    /// Compares all six draw lists' geometry and collision with native words, and checks that each shape covers exactly
    /// the cells its draw list writes. Which other draws are the room's is checked with the mural rows
    /// (<see cref="LookupMotherBrainBackgroundVerification.VerifyMotherBrainBackgroundGeometry"/>).
    /// </summary>
    internal static void VerifyMotherBrainRegularDrawMapping(CartridgeImportAddressSpace rom)
    {
        foreach (LookupEscapeGateStockFrame frame in Regular)
        {
            if (!MotherBrainFakeDeathDraws.TryDescribe(NativeObjectDraws.At<MotherBrainFakeDeathDraw>(frame.Pointer), out RoomBlockShape? shape))
                throw new InvalidOperationException($"Mother Brain tube clear {frame.Id} has no shape.");
            AssertShapeAsNative(rom, shape, frame.Pointer, frame.Id);
        }
        AssertLayoutsAsNative(rom, RoomObjectMotherBrainFakeDeathVisualFiles.Family, Regular, collision: true);
        int cells = 0;
        foreach (LookupEscapeGateStockFrame frame in Regular)
        {
            _ = NativeTerrain(rom, frame.Pointer, out int written);
            cells += written;
        }
        AssertEqual(37, cells, "Mother Brain regular thirty-seven original physical cells");
    }
}
