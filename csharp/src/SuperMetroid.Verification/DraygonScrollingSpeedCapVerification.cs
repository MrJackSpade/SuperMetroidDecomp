using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Game;

/// <summary>Checks the camera-follow speed cap when Draygon carries Samus.</summary>
internal static class DraygonScrollingSpeedCapVerification
{
    /// <summary>
    /// MoveSamusWithDraygon ($A5:94A9) ends with CapScrollingSpeed ($A0:B7A1): an axis that moved
    /// twelve or more pixels gets a previous position twelve pixels away, on the side of the
    /// movement, so the camera follows a carried Samus at most thirteen pixels a frame. In the
    /// 13% movie Draygon's grab drops Samus 33 pixels and native's camera moves only 13.
    /// </summary>
    internal static void VerifyDraygonScrollingSpeedCap()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var checkpoint = FixedPosition<RoomPixels>.FromWords(0x0190, 0, 0x0170, 0);

        // Down 33 and right 8: Y is capped twelve beyond Samus, X is under the threshold.
        (ushort x, ushort y) = PreviousAfterDraygonMove(bus, checkpoint, body: new(0x0190, 0x0169));
        AssertEqual((ushort)0x0190, x, "an 8-pixel move keeps the previous X");
        AssertEqual((ushort)(0x0191 + 12), y, "a 33-pixel drop puts the previous Y twelve below Samus");

        // Up 32 and left 24: both axes are capped twelve above and left of Samus.
        (x, y) = PreviousAfterDraygonMove(bus, checkpoint, body: new(0x0170, 0x0128));
        AssertEqual((ushort)(0x0178 - 12), x, "a 24-pixel move left puts the previous X twelve left of Samus");
        AssertEqual((ushort)(0x0150 - 12), y, "a 32-pixel rise puts the previous Y twelve above Samus");

        // Down 11: under the threshold, the frame's previous Y stands.
        (_, y) = PreviousAfterDraygonMove(bus, checkpoint, body: new(0x0190, 0x0153));
        AssertEqual((ushort)0x0170, y, "an 11-pixel drop keeps the previous Y");
        Console.WriteLine("  Draygon scrolling-speed cap: carried Samus's previous position stays within twelve pixels.");
    }

    /// <summary>Runs the real Draygon carry path and reads Samus's resulting previous-position words.</summary>
    private static (ushort X, ushort Y) PreviousAfterDraygonMove(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace bus, FixedPosition<RoomPixels> checkpoint, Position16<RoomPixels> body)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(bus);
        enemies.ScrollCap._samusPreviousPositionCheckpoint = checkpoint;
        var samus = new SamusState { Position = new(new(checkpoint.X.Whole, 0), new(checkpoint.Y.Whole, 0))};
        samus.DraygonGrabbed.Begin(bus, samus, draygonFacingRight: true);
        var state = new DraygonEnemyState(enemies.SlotTable.Slots[0]);
        state.Body.Position = state.Body.Position.WithWhole(body);
        state.FacingRight = true;
        enemies.DraygonBehavior.MoveSamusWithDraygon(state, samus);
        return samus.PeekPreviousPositionWords(checkpoint);
    }

    /// <summary>Runs the focused Draygon camera-follow cap verification command.</summary>
    [VerificationCommand("--draygon-scrolling-speed-cap")]
    internal static int DraygonScrollingSpeedCapCommand()
    {
        Suite(nameof(VerifyDraygonScrollingSpeedCap), () => VerifyDraygonScrollingSpeedCap());
        return 0;
    }
}
