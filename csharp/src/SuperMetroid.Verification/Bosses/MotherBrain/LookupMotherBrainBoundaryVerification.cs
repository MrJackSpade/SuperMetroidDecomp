using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Verification.Areas.Escape;
using static SuperMetroid.Verification.Rooms.RoomObjects.InstalledFramePictureAssertions;
using static SuperMetroid.Verification.Rooms.RoomObjects.RoomBlockShapeAssertions;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>Cross-checks Mother Brain fake-death wall and door geometry and collision against the cartridge; their pictures are installed (#1557).</summary>
internal static class LookupMotherBrainBoundaryVerification
{
    /// <summary>The wall-fill and escape-door draw-list addresses and published names.</summary>
    internal static readonly LookupEscapeGateStockFrame[] Boundaries = [new(0x94a3, "fill-wall"), new(0x94b1, "escape-door")];

    /// <summary>
    /// Checks that the wall and door are the fake-death room's draws, their two vertical runs against the cartridge, and
    /// that each shape covers exactly the cells its draw list writes. Which other draws are the room's is checked with the
    /// mural rows (<see cref="LookupMotherBrainBackgroundVerification.VerifyMotherBrainBackgroundGeometry"/>).
    /// </summary>
    internal static void VerifyMotherBrainBoundaryGeometry(CartridgeImportAddressSpace rom)
    {
        foreach (LookupEscapeGateStockFrame frame in Boundaries)
        {
            if (!MotherBrainFakeDeathDraws.TryDescribe(NativeObjectDraws.At<MotherBrainFakeDeathDraw>(frame.Pointer), out RoomBlockShape? shape))
                throw new InvalidOperationException($"Mother Brain boundary {frame.Id} has no shape.");
            AssertShapeAsNative(rom, shape, frame.Pointer, frame.Id);
        }
        AssertLayoutsAsNative(rom, RoomObjectMotherBrainFakeDeathVisualFiles.Family, Boundaries, collision: false);
    }

    /// <summary>Compares how every cell of both boundary records collides with the decoded native blocks.</summary>
    internal static void VerifyMotherBrainBoundaryCollision(CartridgeImportAddressSpace rom) =>
        AssertLayoutsAsNative(rom, RoomObjectMotherBrainFakeDeathVisualFiles.Family, Boundaries, collision: true);
}
