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

    /// <summary>Checks which draws are the cannon's, each frame's runs and continuations against the cartridge, and that each shape covers its frame's four cells only.</summary>
    internal static void VerifyCannonLayoutGeometry(CartridgeImportAddressSpace rom)
    {
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            bool owned = Frames.Any(frame => frame.Pointer == raw);
            AssertEqual(owned, DraygonCannonDraws.TryDescribe(NativeObjectDraws.At<DraygonCannonDraw>(raw), out RoomBlockShape? shape), "Cannon layout shape domain");
            if (!owned) AssertTrue(shape is null, "Cannon layout missing shape");
        }
        AssertLayoutsAsNative(rom, RoomObjectDraygonCannonVisualFiles.Family, Frames, collision: false);
        foreach (LookupEscapeGateStockFrame frame in Frames)
        {
            if (!DraygonCannonDraws.TryDescribe(NativeObjectDraws.At<DraygonCannonDraw>(frame.Pointer), out RoomBlockShape? shape))
                throw new InvalidOperationException($"Cannon frame {frame.Id} has no shape.");
            RoomBlockShape described = shape;
            AssertEqual(4, described.Cells.Count, "Cannon shape covers its 2x2 frame");
            foreach (Offset16<RoomBlocks> outside in new Offset16<RoomBlocks>[] { new(0, -1), new(2, 0), new(-2, 0), new(0, 2) })
                AssertThrows<ArgumentOutOfRangeException>(() => described.CollisionAt(outside), "Cannon shape covers only its cells");
        }
    }

    /// <summary>Compares how every shield and damaged-frame cell collides with the cartridge.</summary>
    internal static void VerifyCannonLayoutCollision(CartridgeImportAddressSpace rom) =>
        AssertLayoutsAsNative(rom, RoomObjectDraygonCannonVisualFiles.Family, Frames, collision: true);
}
