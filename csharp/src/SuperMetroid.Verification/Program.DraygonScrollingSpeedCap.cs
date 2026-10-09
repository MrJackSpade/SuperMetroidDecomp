using System.Reflection;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    /// <summary>
    /// MoveSamusWithDraygon ($A5:94A9) ends with CapScrollingSpeed ($A0:B7A1): an axis that moved
    /// twelve or more pixels gets a previous position twelve pixels away, on the side of the
    /// movement, so the camera follows a carried Samus at most thirteen pixels a frame. In the
    /// 13% movie Draygon's grab drops Samus 33 pixels and native's camera moves only 13.
    /// </summary>
    private static void VerifyDraygonScrollingSpeedCap()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var checkpoint = new SamusCameraPoint(XPosition: 0x0190, XSubposition: 0, YPosition: 0x0170, YSubposition: 0);

        // Down 33 and right 8: Y is capped twelve beyond Samus, X is under the threshold.
        (ushort x, ushort y) = PreviousAfterDraygonMove(bus, checkpoint, bodyX: 0x0190, bodyY: 0x0169);
        AssertEqual((ushort)0x0190, x, "an 8-pixel move keeps the previous X");
        AssertEqual((ushort)(0x0191 + 12), y, "a 33-pixel drop puts the previous Y twelve below Samus");

        // Up 32 and left 24: both axes are capped twelve above and left of Samus.
        (x, y) = PreviousAfterDraygonMove(bus, checkpoint, bodyX: 0x0170, bodyY: 0x0128);
        AssertEqual((ushort)(0x0178 - 12), x, "a 24-pixel move left puts the previous X twelve left of Samus");
        AssertEqual((ushort)(0x0150 - 12), y, "a 32-pixel rise puts the previous Y twelve above Samus");

        // Down 11: under the threshold, the frame's previous Y stands.
        (_, y) = PreviousAfterDraygonMove(bus, checkpoint, bodyX: 0x0190, bodyY: 0x0153);
        AssertEqual((ushort)0x0170, y, "an 11-pixel drop keeps the previous Y");
        Console.WriteLine("  Draygon scrolling-speed cap: carried Samus's previous position stays within twelve pixels.");
    }

    private static (ushort X, ushort Y) PreviousAfterDraygonMove(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace bus, SamusCameraPoint checkpoint, ushort bodyX, ushort bodyY)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        typeof(RoomEnemySystem).GetField("_samusPreviousPositionCheckpoint", flags)!.SetValue(enemies, checkpoint);
        var samus = new SamusState { XPosition = checkpoint.XPosition, YPosition = checkpoint.YPosition };
        samus.DraygonGrabbed.Begin(bus, samus, draygonFacingRight: true);
        var state = new DraygonEnemyState(enemies.Slots[0]);
        state.Body.XPosition = bodyX;
        state.Body.YPosition = bodyY;
        state.FacingRight = true;
        typeof(RoomEnemySystem).GetMethod("MoveSamusWithDraygon", flags)!.Invoke(enemies, [state, samus]);
        return samus.PeekPreviousPositionWords(checkpoint);
    }
}
