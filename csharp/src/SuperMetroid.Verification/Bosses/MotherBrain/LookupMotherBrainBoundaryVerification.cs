using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Verification.Areas.Escape;
using static SuperMetroid.Verification.Rooms.RoomObjects.InstalledFramePictureAssertions;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>Cross-checks Mother Brain fake-death wall and door geometry and collision against bank $84; their pictures are installed (#1557).</summary>
internal static class LookupMotherBrainBoundaryVerification
{
    /// <summary>The wall-fill and escape-door draw-list addresses and published names.</summary>
    private static readonly LookupEscapeGateStockFrame[] Boundaries = [new(0x94a3, "fill-wall"), new(0x94b1, "escape-door")];

    /// <summary>Checks pointer ownership, the two vertical runs against the cartridge, and the shape bounds.</summary>
    internal static void VerifyMotherBrainBoundaryGeometry(SuperMetroidAddressSpace rom)
    {
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            bool owned = Boundaries.Any(frame => frame.Pointer == raw);
            AssertEqual(owned, MotherBrainFakeDeathDraws.TryDescribeBoundary(NativeObjectDraws.At<MotherBrainFakeDeathDraw>(raw), out var shape),
                "Mother Brain wall/door ownership");
            if (!owned)
            {
                AssertEqual(default(MotherBrainFakeDeathDraws.BoundaryDraw), shape, "Mother Brain missing wall/door descriptor");
                continue;
            }
            foreach (int bad in new[] { int.MinValue, -1, MotherBrainFakeDeathDraws.BoundaryDraw.RunCount, int.MaxValue })
            {
                AssertThrows<ArgumentOutOfRangeException>(() => shape.CollisionAt(bad, 0), "Mother Brain boundary run bounds");
                AssertThrows<ArgumentOutOfRangeException>(() => shape.Start(bad), "Mother Brain boundary start bounds");
            }
            for (int run = 0; run < MotherBrainFakeDeathDraws.BoundaryDraw.RunCount; run++)
            {
                int selectedRun = run;
                foreach (int bad in new[] { int.MinValue, -1, shape.Count(run), int.MaxValue })
                    AssertThrows<ArgumentOutOfRangeException>(() => shape.CollisionAt(selectedRun, bad), "Mother Brain boundary cell bounds");
            }
        }
        AssertLayoutsAsNative(rom, RoomObjectMotherBrainFakeDeathVisualFiles.Family, Boundaries, collision: false);
    }

    /// <summary>Compares how every cell of both boundary records collides with the decoded native blocks.</summary>
    internal static void VerifyMotherBrainBoundaryCollision(SuperMetroidAddressSpace rom) =>
        AssertLayoutsAsNative(rom, RoomObjectMotherBrainFakeDeathVisualFiles.Family, Boundaries, collision: true);
}
