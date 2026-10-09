using System.Reflection;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    /// <summary>
    /// MoveSamusWithDraygon ($A5:94A9) places Samus before it tests the escape bit, so the
    /// first carry call after an escape drags the released Samus once more and then sends
    /// Draygon flying straight up. In the 13% movie this last drag moves Samus two pixels left.
    /// </summary>
    private static void VerifyDraygonEscapeDrag()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        var samus = new SamusState { XPosition = 0x0135, YPosition = 0x019d };
        samus.DraygonGrabbed.Begin(bus, samus, draygonFacingRight: false);
        samus.DraygonGrabbed.Release(bus, samus);

        var state = new DraygonEnemyState(enemies.Slots[0]) { Function = DraygonAiFunction.CarrySamus };
        state.Body.XPosition = 0x013b;
        state.Body.YPosition = 0x0176;
        typeof(RoomEnemySystem).GetMethod("MoveSamusWithDraygon", flags)!.Invoke(enemies, [state, samus]);

        AssertEqual((ushort)(0x013b - 8), samus.XPosition, "the escaped Samus is placed eight pixels left of Draygon");
        AssertEqual((ushort)(0x0176 + 0x28), samus.YPosition, "the escaped Samus is placed 40 pixels below Draygon");
        AssertEqual(DraygonAiFunction.FlyStraightUp, state.Function, "Draygon then sees the escape and flies up");
        AssertTrue(!samus.DraygonGrabbed.ConsumeOwnerReleaseSignal(), "the placement consumed the escape bit");
        Console.WriteLine("  Draygon escape drag: the first carry call after an escape still places Samus.");
    }
}
