using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.RoomObjects;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Rooms.Doors;

using SuperMetroid.Verification.Harness;

/// <summary>
/// Checks the door caps' draws that show no sliding cap (#1627, #1994): a blue cap's closed cap drawn solid and a grey cap's
/// cleared opening are named looks, not a missing frame, the descriptors accept them and the cap's closed and sliding
/// looks and nothing else, and the import still finds each at the facing's place in the cartridge and gives it its
/// published name.
/// </summary>
internal static class DoorCapAbsentFramesVerification
{
    /// <summary>Each facing's solid blue cap and grey cleared opening, their cartridge places and published names.</summary>
    private static readonly (ColoredDoorOrientation Facing, ushort Solid, string SolidId, ushort Cleared, string ClearedId)[] Facings =
    [
        (ColoredDoorOrientation.Left, 0xa9a7, "left-closed", 0xa677, "clear-left"),
        (ColoredDoorOrientation.Right, 0xa9e3, "right-closed", 0xa683, "clear-right"),
        (ColoredDoorOrientation.Up, 0xaa1f, "up-closed", 0xa68f, "clear-up"),
        (ColoredDoorOrientation.Down, 0xaa5b, "down-closed", 0xa69b, "clear-down"),
    ];

    /// <summary>The cap closed and sliding aside, the frames each grey sequence names, and their published words.</summary>
    private static readonly (DoorCapLook Look, DoorCapShown Frame, string Name)[] Openings =
    [
        (DoorCapLook.Closed, new DoorCapShown(DoorCapLook.Closed, DoorCapShownPaint.Own), "closed"),
        (DoorCapLook.Ajar, new DoorCapShown(DoorCapLook.Ajar, DoorCapShownPaint.Own), "ajar"),
        (DoorCapLook.HalfOpen, new DoorCapShown(DoorCapLook.HalfOpen, DoorCapShownPaint.Own), "half-open"),
        (DoorCapLook.NearlyOpen, new DoorCapShown(DoorCapLook.NearlyOpen, DoorCapShownPaint.Own), "nearly-open"),
    ];

    /// <summary>Checks the solid cap and the cleared opening against the sliding looks and the cartridge's places.</summary>
    internal static void VerifyDoorCapAbsentFrames()
    {
        foreach ((ColoredDoorOrientation facing, ushort solid, string solidId, ushort cleared, string clearedId) in Facings)
        {
            RoomObjectDraw solidDraw = BlueDoorCaps.ClosedSolid(facing);
            AssertTrue(RoomObjectDrawIdentity.TryFrame<BlueDoorCaps.Draw>(solidDraw, out BlueDoorCaps.Draw blue) && blue.Look == DoorCapLook.ClosedSolid,
                $"{facing} solid blue cap is its own look");
            AssertEqual(solid, RoomObjectDrawAddressCodec.Of(solidDraw), $"{facing} solid blue cap is the facing's first cartridge record");
            AssertEqual(solidId, RoomObjectBlueDoorVisualFiles.VisualId(solidDraw), $"{facing} solid blue cap shares the closed cap's name");
            AssertEqual(BlueDoorCaps.Family.Of(new BlueDoorCaps.Draw(facing, DoorCapLook.Closed)), BlueDoorCaps.VisualSource(solidDraw),
                $"{facing} solid blue cap shows the closed cap's picture");
            for (int step = 0; step < Openings.Length; step++)
            {
                RoomObjectDraw opening = BlueDoorCaps.Family.Of(new BlueDoorCaps.Draw(facing, Openings[step].Look));
                AssertTrue(RoomObjectDrawIdentity.TryFrame<BlueDoorCaps.Draw>(opening, out BlueDoorCaps.Draw shown) && shown.Look == Openings[step].Look,
                    $"{facing} blue {Openings[step].Look} is a blue cap draw");
                AssertEqual(solid + (1 + step) * 12, (int)RoomObjectDrawAddressCodec.Of(opening),
                    $"{facing} blue {Openings[step].Look} follows the solid cap in the cartridge");
                AssertEqual(opening, BlueDoorCaps.VisualSource(opening), $"{facing} blue {Openings[step].Look} shows its own picture");
            }
            AssertTrue(!BlueDoorObjectDrawDefinitions.Collisions.ContainsKey(new BlueDoorCaps.Draw(facing, DoorCapLook.Cleared)),
                $"{facing} blue cap draws no cleared opening of its own");

            RoomObjectDraw clearedDraw = DoorCaps.Image(DoorCapPaint.Grey, facing, new DoorCapShown(DoorCapLook.Cleared, DoorCapShownPaint.Grey));
            AssertEqual(GreyDoorCaps.Family.Of(GreyDoorCaps.Draw.ClearedOf(facing)), clearedDraw, $"{facing} cleared image is the grey cleared opening");
            AssertTrue(RoomObjectDrawIdentity.TryFrame<GreyDoorCaps.Draw>(clearedDraw, out GreyDoorCaps.Draw grey) && grey.IsCleared,
                $"{facing} cleared opening is its own look");
            AssertEqual(cleared, RoomObjectDrawAddressCodec.Of(clearedDraw), $"{facing} cleared opening's cartridge record");
            AssertEqual(clearedId, RoomObjectGreyDoorVisualFiles.VisualId(clearedDraw), $"{facing} cleared opening's published name");
            foreach ((DoorCapLook look, DoorCapShown frame, string name) in Openings)
            {
                RoomObjectDraw closing = DoorCaps.Image(DoorCapPaint.Grey, facing, frame);
                AssertTrue(RoomObjectDrawIdentity.TryFrame<GreyDoorCaps.Draw>(closing, out GreyDoorCaps.Draw shown) && !shown.IsCleared && shown.Look == look,
                    $"{facing} grey {frame} shows the grey cap {look}");
                AssertEqual($"grey-{DoorCapOrientations.VisualName(facing)}-{name}", RoomObjectGreyDoorVisualFiles.VisualId(closing),
                    $"{facing} grey {look}'s published name");
            }
            AssertTrue(!GreyDoorObjectDrawDefinitions.Collisions.ContainsKey(new GreyDoorCaps.Draw(facing, DoorCapLook.ClosedSolid)),
                $"{facing} grey cap draws no solid closed cap");
        }
        Console.WriteLine("Door cap absent frames: each facing's solid blue cap and grey cleared opening are named looks and keep their cartridge places and names.");
    }

    /// <summary>Dispatches the door caps' absent-frame checks.</summary>
    [VerificationCommand("--door-cap-absent-frames")]
    internal static int DoorCapAbsentFramesCommand()
    {
        Suite(nameof(VerifyDoorCapAbsentFrames), () => VerifyDoorCapAbsentFrames());
        return 0;
    }
}
