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
    internal static readonly LookupEscapeGateStockFrame[] Rows = [new(0x9505, "background-row-2"), new(0x9523, "background-row-3"),
        new(0x9541, "background-row-4"), new(0x955f, "background-row-5"), new(0x957d, "background-row-6"), new(0x959b, "background-row-7"),
        new(0x95b9, "background-row-8"), new(0x95d7, "background-row-9"), new(0x95f5, "background-row-a"), new(0x9613, "background-row-b"),
        new(0x9631, "background-row-c"), new(0x964f, "background-row-d")];

    /// <summary>The mural rows in the order the mural shows them, top to bottom.</summary>
    private static readonly MotherBrainFakeDeathDraw[] MuralOrder =
    [
        MotherBrainFakeDeathDraw.MuralRow1, MotherBrainFakeDeathDraw.MuralRow2, MotherBrainFakeDeathDraw.MuralRow3,
        MotherBrainFakeDeathDraw.MuralRow4, MotherBrainFakeDeathDraw.MuralRow5, MotherBrainFakeDeathDraw.MuralRow6,
        MotherBrainFakeDeathDraw.MuralRow7, MotherBrainFakeDeathDraw.MuralRow8, MotherBrainFakeDeathDraw.MuralRow9,
        MotherBrainFakeDeathDraw.MuralRow10, MotherBrainFakeDeathDraw.MuralRow11, MotherBrainFakeDeathDraw.MuralRow12,
    ];

    /// <summary>
    /// Checks which draws are the fake-death room's (the mural rows, the wall and door, and the fill rows and tube clears
    /// together), the twelve rows' order, their one thirteen-cell run against the cartridge, and that each row's shape
    /// covers exactly the cells its draw list writes.
    /// </summary>
    internal static void VerifyMotherBrainBackgroundGeometry(CartridgeImportAddressSpace rom)
    {
        LookupEscapeGateStockFrame[] family = [.. Rows, .. LookupMotherBrainBoundaryVerification.Boundaries,
            .. LookupMotherBrainRegularDrawsVerification.Regular];
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            bool owned = family.Any(frame => frame.Pointer == value);
            AssertEqual(owned, MotherBrainFakeDeathDraws.TryDescribe(NativeObjectDraws.At<MotherBrainFakeDeathDraw>(value), out RoomBlockShape? shape),
                "Mother Brain fake-death room ownership");
            if (!owned) AssertTrue(shape is null, "Mother Brain missing fake-death room shape");
        }
        for (int row = 0; row < Rows.Length; row++)
        {
            AssertEqual(MuralOrder[row], RoomObjectDraw.FrameOf<MotherBrainFakeDeathDraw>(NativeObjectDraws.At<MotherBrainFakeDeathDraw>(Rows[row].Pointer),
                "Mother Brain mural row"), "Mother Brain native row order");
            if (!MotherBrainFakeDeathDraws.TryDescribe(NativeObjectDraws.At<MotherBrainFakeDeathDraw>(Rows[row].Pointer), out RoomBlockShape? shape))
                throw new InvalidOperationException($"Mother Brain mural row {Rows[row].Id} has no shape.");
            AssertShapeAsNative(rom, shape, Rows[row].Pointer, Rows[row].Id);
        }
        AssertLayoutsAsNative(rom, RoomObjectMotherBrainFakeDeathVisualFiles.Family, Rows, collision: false);
    }

    /// <summary>Checks that every mural cell collides as the cartridge's does: slopes on the border rows, air between.</summary>
    internal static void VerifyMotherBrainBackgroundCollision(CartridgeImportAddressSpace rom) =>
        AssertLayoutsAsNative(rom, RoomObjectMotherBrainFakeDeathVisualFiles.Family, Rows, collision: true);
}
