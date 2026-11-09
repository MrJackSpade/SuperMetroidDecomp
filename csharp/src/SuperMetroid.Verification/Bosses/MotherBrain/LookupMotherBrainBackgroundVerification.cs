using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Verification.Areas.Escape;
using static SuperMetroid.Verification.Rooms.RoomObjects.InstalledFramePictureAssertions;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>Checks Mother Brain fake-death mural-row geometry and collision against the cartridge; their pictures are installed (#1557).</summary>
internal static class LookupMotherBrainBackgroundVerification
{
    /// <summary>The twelve mural rows' draw-list addresses and published names, top to bottom.</summary>
    private static readonly LookupEscapeGateStockFrame[] Rows = [new(0x9505, "background-row-2"), new(0x9523, "background-row-3"),
        new(0x9541, "background-row-4"), new(0x955f, "background-row-5"), new(0x957d, "background-row-6"), new(0x959b, "background-row-7"),
        new(0x95b9, "background-row-8"), new(0x95d7, "background-row-9"), new(0x95f5, "background-row-a"), new(0x9613, "background-row-b"),
        new(0x9631, "background-row-c"), new(0x964f, "background-row-d")];

    /// <summary>Checks the twelve rows' ownership and order, their one thirteen-cell run against the cartridge, and the column bounds.</summary>
    internal static void VerifyMotherBrainBackgroundGeometry(SuperMetroidAddressSpace rom)
    {
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            int row = Array.FindIndex(Rows, frame => frame.Pointer == value);
            AssertEqual(row >= 0, MotherBrainFakeDeathDraws.TryDescribeBackground(NativeObjectDraws.At<MotherBrainFakeDeathDraw>(value), out var shape),
                "Mother Brain background ownership");
            if (row < 0)
            {
                AssertEqual(default(MotherBrainFakeDeathDraws.BackgroundDraw), shape, "Mother Brain missing background descriptor");
                continue;
            }
            AssertEqual(row, shape.Row, "Mother Brain native row order");
            foreach (int bad in new[] { int.MinValue, -1, MotherBrainFakeDeathObjectDrawDefinitions.BackgroundWidth, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => shape.CollisionAt(bad), "Mother Brain background column bounds");
        }
        AssertLayoutsAsNative(rom, RoomObjectMotherBrainFakeDeathVisualFiles.Family, Rows, collision: false);
    }

    /// <summary>Checks that every mural cell collides as the cartridge's does: slopes on the border rows, air between.</summary>
    internal static void VerifyMotherBrainBackgroundCollision(SuperMetroidAddressSpace rom) =>
        AssertLayoutsAsNative(rom, RoomObjectMotherBrainFakeDeathVisualFiles.Family, Rows, collision: true);
}
