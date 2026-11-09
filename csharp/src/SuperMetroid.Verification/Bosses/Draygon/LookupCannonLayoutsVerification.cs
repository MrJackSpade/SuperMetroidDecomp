using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Verification.Areas.Escape;
using static SuperMetroid.Verification.Rooms.RoomObjects.InstalledFramePictureAssertions;

namespace SuperMetroid.Verification.Bosses.Draygon;

/// <summary>Checks Draygon cannon draw geometry and collision against the cartridge's draw lists; their pictures are installed (#1557).</summary>
internal static class LookupCannonLayoutsVerification
{
    /// <summary>The twelve reachable cannon frames' draw-list addresses and published names.</summary>
    private static readonly LookupEscapeGateStockFrame[] Frames = [new(0x9fcd, "right-shield-a"), new(0x9fdd, "right-shield-b"),
        new(0xa02d, "right-damaged-a"), new(0xa03d, "right-damaged-b"), new(0xa04d, "right-damaged-c"), new(0xa05d, "right-damaged-d"),
        new(0xa0ed, "left-shield-a"), new(0xa101, "left-shield-b"), new(0xa165, "left-damaged-a"), new(0xa179, "left-damaged-b"),
        new(0xa18d, "left-damaged-c"), new(0xa1a1, "left-damaged-d")];

    /// <summary>Checks which draws are the cannon's, each frame's runs and continuations against the cartridge, and the shape bounds.</summary>
    internal static void VerifyCannonLayoutGeometry(SuperMetroidAddressSpace rom)
    {
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            bool owned = Frames.Any(frame => frame.Pointer == raw);
            AssertEqual(owned, DraygonCannonDraws.TryDescribe(NativeObjectDraws.At<DraygonCannonDraw>(raw), out var shape), "Cannon layout shape domain");
            if (!owned) AssertEqual(default(DraygonCannonDraws.Draw), shape, "Cannon layout missing shape");
        }
        AssertLayoutsAsNative(rom, RoomObjectDraygonCannonVisualFiles.Family, Frames, collision: false);
        foreach (LookupEscapeGateStockFrame frame in Frames)
        {
            DraygonCannonDraws.TryDescribe(NativeObjectDraws.At<DraygonCannonDraw>(frame.Pointer), out var shape);
            foreach (int invalid in new[] { int.MinValue, -1, shape.RunCount, int.MaxValue })
            {
                AssertThrows<ArgumentOutOfRangeException>(() => shape.BlockCount(invalid), "Cannon layout run bounds");
                AssertThrows<ArgumentOutOfRangeException>(() => shape.Start(invalid), "Cannon layout start bounds");
                AssertThrows<ArgumentOutOfRangeException>(() => shape.CollisionAt(invalid, 0), "Cannon layout collision run bounds");
            }
            for (int run = 0; run < shape.RunCount; run++)
            {
                int selectedRun = run;
                foreach (int invalid in new[] { int.MinValue, -1, shape.BlockCount(run), int.MaxValue })
                    AssertThrows<ArgumentOutOfRangeException>(() => shape.CollisionAt(selectedRun, invalid), "Cannon layout cell bounds");
            }
        }
    }

    /// <summary>Compares how every shield and damaged-frame cell collides with the cartridge.</summary>
    internal static void VerifyCannonLayoutCollision(SuperMetroidAddressSpace rom) =>
        AssertLayoutsAsNative(rom, RoomObjectDraygonCannonVisualFiles.Family, Frames, collision: true);
}
