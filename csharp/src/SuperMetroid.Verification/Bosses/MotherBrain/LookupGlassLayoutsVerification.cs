using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Verification.Areas.Escape;
using static SuperMetroid.Verification.Rooms.RoomObjects.InstalledFramePictureAssertions;
using static SuperMetroid.Verification.Rooms.RoomObjects.RoomBlockShapeAssertions;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>Checks Mother Brain glass draw geometry and collision against the cartridge's draw lists; their pictures are installed (#1557).</summary>
internal static class LookupGlassLayoutsVerification
{
    /// <summary>The eleven glass frames' draw-list addresses and published names.</summary>
    private static readonly LookupEscapeGateStockFrame[] Frames = [new(0x9717, "initial"), new(0x971d, "pane-damage-1"),
        new(0x9731, "pane-damage-2"), new(0x9745, "pane-transition"), new(0x974f, "shifted-pane-1"), new(0x9769, "shifted-pane-2"),
        new(0x9781, "shifted-pane-3"), new(0x978f, "shatter-1"), new(0x97b7, "shatter-2"), new(0x97e7, "shatter-3"), new(0x9817, "cleared")];

    /// <summary>
    /// Checks which draws are the glass's, each frame's runs and continuations against the cartridge, and that each shape
    /// covers exactly the cells its draw list writes, inside the four-by-four square left of and below the glass block.
    /// </summary>
    internal static void VerifyGlassLayoutGeometry(CartridgeImportAddressSpace rom)
    {
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            bool owned = Frames.Any(frame => frame.Pointer == raw);
            AssertEqual(owned, MotherBrainGlassDraws.TryDescribe(NativeObjectDraws.At<MotherBrainGlassDraw>(raw), out RoomBlockShape? shape), "Glass layout shape domain");
            if (!owned) AssertTrue(shape is null, "Glass layout missing shape");
        }
        AssertLayoutsAsNative(rom, RoomObjectMotherBrainGlassVisualFiles.Family, Frames, collision: false);
        foreach (LookupEscapeGateStockFrame frame in Frames)
        {
            if (!MotherBrainGlassDraws.TryDescribe(NativeObjectDraws.At<MotherBrainGlassDraw>(frame.Pointer), out RoomBlockShape? shape))
                throw new InvalidOperationException($"Glass frame {frame.Id} has no shape.");
            RoomBlockShape described = shape;
            AssertShapeAsNative(rom, described, frame.Pointer, frame.Id);
            AssertTrue(described.Cells.All(cell => cell.Cell.X is >= -3 and <= 0 && cell.Cell.Y is >= 0 and <= 3),
                $"Glass shape {frame.Id} lies inside the glass's square");
        }
    }

    /// <summary>Compares how every glass cell collides with the cartridge.</summary>
    internal static void VerifyGlassLayoutCollision(CartridgeImportAddressSpace rom) =>
        AssertLayoutsAsNative(rom, RoomObjectMotherBrainGlassVisualFiles.Family, Frames, collision: true);
}
