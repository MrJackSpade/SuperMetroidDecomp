using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Verification.Areas.Escape;
using static SuperMetroid.Verification.Rooms.RoomObjects.InstalledFramePictureAssertions;
using static SuperMetroid.Verification.Rooms.RoomObjects.RoomBlockShapeAssertions;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>Checks Mother Brain fake-death mural-row geometry and collision against the cartridge; their pictures are installed (#1557).</summary>
internal static class LookupMotherBrainBackgroundVerification
{
    /// <summary>The twelve mural rows' draw-list addresses and published names, top to bottom.</summary>
    internal static readonly LookupEscapeGateStockFrame[] Rows = [new(0x9505, "mural-row-1"), new(0x9523, "mural-row-2"),
        new(0x9541, "mural-row-3"), new(0x955f, "mural-row-4"), new(0x957d, "mural-row-5"), new(0x959b, "mural-row-6"),
        new(0x95b9, "mural-row-7"), new(0x95d7, "mural-row-8"), new(0x95f5, "mural-row-9"), new(0x9613, "mural-row-10"),
        new(0x9631, "mural-row-11"), new(0x964f, "mural-row-12")];

    /// <summary>
    /// Checks which draws are the fake-death room's (the wall and door and the tube clears together) and which are the
    /// mural's rows (#2258), that the mural's one row draw is stored row by row from the top, their one thirteen-cell run
    /// against the cartridge, and that each row's shape covers exactly the cells its draw list writes.
    /// </summary>
    internal static void VerifyMotherBrainBackgroundGeometry(CartridgeImportAddressSpace rom)
    {
        LookupEscapeGateStockFrame[] family = [.. LookupMotherBrainBoundaryVerification.Boundaries, .. LookupMotherBrainRegularDrawsVerification.Regular];
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            bool owned = family.Any(frame => frame.Pointer == value);
            AssertEqual(owned, RoomObjectDrawIdentity.TryShape(MotherBrainFakeDeathDraws.Family, NativeObjectDraws.At<MotherBrainFakeDeathDraw>(value), out RoomBlockShape? shape),
                "Mother Brain fake-death room ownership");
            if (!owned) AssertTrue(shape is null, "Mother Brain missing fake-death room shape");
            bool mural = Rows.Any(frame => frame.Pointer == value);
            AssertEqual(mural, RoomObjectDrawIdentity.TryShape(MotherBrainFakeDeathDraws.MuralRowFamily,
                NativeObjectDraws.At<MotherBrainFakeDeathDraws.MuralRowDraw>(value), out RoomBlockShape? rowShape), "Mother Brain mural row ownership");
            if (!mural) AssertTrue(rowShape is null, "Mother Brain missing mural row shape");
        }
        for (int row = 0; row < Rows.Length; row++)
        {
            RoomObjectDraw native = NativeObjectDraws.At<MotherBrainFakeDeathDraws.MuralRowDraw>(Rows[row].Pointer);
            AssertEqual<RoomObjectDraw>(MotherBrainFakeDeathDraws.MuralRow(row), native, "Mother Brain native row order");
            if (!RoomObjectDrawIdentity.TryShape(MotherBrainFakeDeathDraws.MuralRowFamily, native, out RoomBlockShape? shape))
                throw new InvalidOperationException($"Mother Brain mural row {Rows[row].Id} has no shape.");
            AssertShapeAsNative(rom, shape, Rows[row].Pointer, Rows[row].Id);
        }
        AssertLayoutsAsNative(rom, RoomObjectMotherBrainFakeDeathVisualFiles.Family, Rows, collision: false);
    }

    /// <summary>Checks that every mural cell collides as the cartridge's does: slopes on the border rows, air between.</summary>
    internal static void VerifyMotherBrainBackgroundCollision(CartridgeImportAddressSpace rom) =>
        AssertLayoutsAsNative(rom, RoomObjectMotherBrainFakeDeathVisualFiles.Family, Rows, collision: true);
}
