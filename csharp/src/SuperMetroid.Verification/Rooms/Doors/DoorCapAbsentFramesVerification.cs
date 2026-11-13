using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.RoomObjects;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Rooms.Doors;

using SuperMetroid.Verification.Harness;

/// <summary>
/// Checks the door caps' draws that show no animation frame (#1627): a blue cap's physical alias and a grey cap's cleared
/// opening hold no frame instead of a minus-one marker, the descriptors accept them and the four frames and nothing else,
/// and the import still finds each at the facing's place in the cartridge and gives it its published name.
/// </summary>
internal static class DoorCapAbsentFramesVerification
{
    /// <summary>Each facing's blue alias and grey cleared opening, their cartridge places and published names.</summary>
    private static readonly (ColoredDoorOrientation Facing, ushort Alias, string AliasId, ushort Cleared, string ClearedId)[] Facings =
    [
        (ColoredDoorOrientation.Left, 0xa9a7, "left-frame-0", 0xa677, "clear-left"),
        (ColoredDoorOrientation.Right, 0xa9e3, "right-frame-0", 0xa683, "clear-right"),
        (ColoredDoorOrientation.Up, 0xaa1f, "up-frame-0", 0xa68f, "clear-up"),
        (ColoredDoorOrientation.Down, 0xaa5b, "down-frame-0", 0xa69b, "clear-down"),
    ];

    /// <summary>Checks the alias and the cleared opening against the four frames and the cartridge's places.</summary>
    internal static void VerifyDoorCapAbsentFrames()
    {
        foreach ((ColoredDoorOrientation facing, ushort alias, string aliasId, ushort cleared, string clearedId) in Facings)
        {
            RoomObjectDraw aliasDraw = BlueDoorCaps.Alias(facing);
            AssertTrue(BlueDoorCaps.TryDescribe(aliasDraw, out BlueDoorCaps.Draw blue) && blue.IsAlias && blue.Frame is null,
                $"{facing} blue alias holds no opening frame");
            AssertEqual(alias, RoomObjectDrawAddressCodec.Of(aliasDraw), $"{facing} blue alias is the facing's first cartridge record");
            AssertEqual(aliasId, RoomObjectBlueDoorVisualFiles.VisualId(aliasDraw), $"{facing} blue alias shares frame zero's name");
            AssertEqual(RoomObjectDraw.Of(new BlueDoorCaps.Draw(facing, 0)), BlueDoorCaps.VisualSource(aliasDraw),
                $"{facing} blue alias shows frame zero's picture");
            for (int frame = 0; frame < 4; frame++)
            {
                RoomObjectDraw opening = RoomObjectDraw.Of(new BlueDoorCaps.Draw(facing, frame));
                AssertTrue(BlueDoorCaps.TryDescribe(opening, out BlueDoorCaps.Draw shown) && !shown.IsAlias,
                    $"{facing} blue frame {frame} is an opening frame");
                AssertEqual(alias + (1 + frame) * 12, (int)RoomObjectDrawAddressCodec.Of(opening),
                    $"{facing} blue frame {frame} follows the alias in the cartridge");
                AssertEqual(opening, BlueDoorCaps.VisualSource(opening), $"{facing} blue frame {frame} shows its own picture");
            }
            AssertTrue(!BlueDoorCaps.TryDescribe(RoomObjectDraw.Of(new BlueDoorCaps.Draw(facing, -1)), out _),
                $"{facing} blue cap has no frame minus one");
            AssertTrue(!BlueDoorCaps.TryDescribe(RoomObjectDraw.Of(new BlueDoorCaps.Draw(facing, 4)), out _),
                $"{facing} blue cap has no fifth opening frame");

            RoomObjectDraw clearedDraw = DoorCaps.Image(DoorCapPaint.Grey, facing, DoorCapFrame.Clear);
            AssertEqual(RoomObjectDraw.Of(GreyDoorCaps.Draw.ClearedOf(facing)), clearedDraw, $"{facing} cleared image is the grey cleared opening");
            AssertTrue(GreyDoorCaps.TryDescribe(clearedDraw, out GreyDoorCaps.Draw grey) && grey.IsCleared && grey.Frame is null,
                $"{facing} cleared opening holds no cap frame");
            AssertEqual(cleared, RoomObjectDrawAddressCodec.Of(clearedDraw), $"{facing} cleared opening's cartridge record");
            AssertEqual(clearedId, RoomObjectGreyDoorVisualFiles.VisualId(clearedDraw), $"{facing} cleared opening's published name");
            DoorCapFrame[] closingImages = [DoorCapFrame.Closed0, DoorCapFrame.Closed1, DoorCapFrame.Closed2, DoorCapFrame.Closed3];
            for (int frame = 0; frame < closingImages.Length; frame++)
            {
                RoomObjectDraw closing = DoorCaps.Image(DoorCapPaint.Grey, facing, closingImages[frame]);
                AssertTrue(GreyDoorCaps.TryDescribe(closing, out GreyDoorCaps.Draw shown) && !shown.IsCleared && shown.Frame == frame,
                    $"{facing} grey frame {frame} is a cap frame");
                AssertEqual($"grey-{DoorCapOrientations.VisualName(facing)}-frame-{frame}", RoomObjectGreyDoorVisualFiles.VisualId(closing),
                    $"{facing} grey frame {frame}'s published name");
            }
            AssertTrue(!GreyDoorCaps.TryDescribe(RoomObjectDraw.Of(new GreyDoorCaps.Draw(facing, -1)), out _),
                $"{facing} grey cap has no frame minus one");
            AssertTrue(!GreyDoorCaps.TryDescribe(RoomObjectDraw.Of(new GreyDoorCaps.Draw(facing, 4)), out _),
                $"{facing} grey cap has no fifth frame");
        }
        Console.WriteLine("Door cap absent frames: each facing's blue alias and grey cleared opening hold no frame and keep their cartridge places and names.");
    }

    /// <summary>Dispatches the door caps' absent-frame checks.</summary>
    [VerificationCommand("--door-cap-absent-frames")]
    internal static int DoorCapAbsentFramesCommand()
    {
        Suite(nameof(VerifyDoorCapAbsentFrames), () => VerifyDoorCapAbsentFrames());
        return 0;
    }
}
