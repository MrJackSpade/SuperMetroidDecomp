using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Verification.Areas.Escape;
using static SuperMetroid.Verification.Rooms.RoomObjects.InstalledFramePictureAssertions;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>Checks Mother Brain glass draw geometry and collision against the cartridge's draw lists; their pictures are installed (#1557).</summary>
internal static class LookupGlassLayoutsVerification
{
    /// <summary>The eleven glass frames' draw-list addresses and published names.</summary>
    private static readonly LookupEscapeGateStockFrame[] Frames = [new(0x9717, "initial"), new(0x971d, "pane-damage-1"),
        new(0x9731, "pane-damage-2"), new(0x9745, "pane-transition"), new(0x974f, "shifted-pane-1"), new(0x9769, "shifted-pane-2"),
        new(0x9781, "shifted-pane-3"), new(0x978f, "shatter-1"), new(0x97b7, "shatter-2"), new(0x97e7, "shatter-3"), new(0x9817, "cleared")];

    /// <summary>Checks which draws are the glass's, each frame's runs and continuations against the cartridge, and the shape bounds.</summary>
    internal static void VerifyGlassLayoutGeometry(SuperMetroidAddressSpace rom)
    {
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            bool owned = Frames.Any(frame => frame.Pointer == raw);
            AssertEqual(owned, MotherBrainGlassDraws.TryDescribe(NativeObjectDraws.At<MotherBrainGlassDraw>(raw), out var shape), "Glass layout shape domain");
            if (!owned) AssertEqual(default(MotherBrainGlassDraws.Draw), shape, "Glass layout missing shape");
        }
        AssertLayoutsAsNative(rom, RoomObjectMotherBrainGlassVisualFiles.Family, Frames, collision: false);
        foreach (LookupEscapeGateStockFrame frame in Frames)
        {
            MotherBrainGlassDraws.TryDescribe(NativeObjectDraws.At<MotherBrainGlassDraw>(frame.Pointer), out var shape);
            foreach (int invalid in new[] { int.MinValue, -1, shape.RunCount, int.MaxValue })
            {
                AssertThrows<ArgumentOutOfRangeException>(() => shape.BlockCount(invalid), "Glass layout run bounds");
                AssertThrows<ArgumentOutOfRangeException>(() => shape.Start(invalid), "Glass layout start bounds");
                AssertThrows<ArgumentOutOfRangeException>(() => shape.Vertical(invalid), "Glass layout direction bounds");
                AssertThrows<ArgumentOutOfRangeException>(() => shape.CollisionAt(invalid, 0), "Glass layout collision run bounds");
            }
            for (int run = 0; run < shape.RunCount; run++)
            {
                int selectedRun = run;
                foreach (int invalid in new[] { int.MinValue, -1, shape.BlockCount(run), int.MaxValue })
                    AssertThrows<ArgumentOutOfRangeException>(() => shape.CollisionAt(selectedRun, invalid), "Glass layout cell bounds");
            }
        }
    }

    /// <summary>Compares how every glass cell collides with the cartridge.</summary>
    internal static void VerifyGlassLayoutCollision(SuperMetroidAddressSpace rom) =>
        AssertLayoutsAsNative(rom, RoomObjectMotherBrainGlassVisualFiles.Family, Frames, collision: true);
}
