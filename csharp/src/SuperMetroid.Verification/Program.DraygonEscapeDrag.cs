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

/// <summary>Checks Samus's final carry placement before Draygon consumes the escape signal.</summary>
internal static class DraygonEscapeDragVerification
{
    /// <summary>
    /// MoveSamusWithDraygon ($A5:94A9) places Samus before it tests the escape bit, so the
    /// first carry call after an escape drags the released Samus once more and then sends
    /// Draygon flying straight up. In the 13% movie this last drag moves Samus two pixels left.
    /// </summary>
    internal static void VerifyDraygonEscapeDrag()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(bus);
        var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(0x0135, 0x019d)) };
        samus.DraygonGrabbed.Begin(bus, samus, draygonFacingRight: false);
        samus.DraygonGrabbed.Release(bus, samus);

        var state = new DraygonEnemyState(enemies.SlotTable.Slots[0]) { Function = DraygonAiFunction.CarrySamus };
        state.Body.Position = state.Body.Position.WithWhole(new(0x013b, 0x0176));
        enemies.DraygonBehavior.MoveSamusWithDraygon(state, samus);

        AssertEqual((ushort)(0x013b - 8), samus.Position.X.Whole, "the escaped Samus is placed eight pixels left of Draygon");
        AssertEqual((ushort)(0x0176 + 0x28), samus.Position.Y.Whole, "the escaped Samus is placed 40 pixels below Draygon");
        AssertEqual(DraygonAiFunction.FlyStraightUp, state.Function, "Draygon then sees the escape and flies up");
        AssertTrue(!samus.DraygonGrabbed.ConsumeOwnerReleaseSignal(), "the placement consumed the escape bit");
        Console.WriteLine("  Draygon escape drag: the first carry call after an escape still places Samus.");
    }

    /// <summary>Runs the focused 13% movie Draygon release-placement check from the verification command line.</summary>
    [VerificationCommand("--draygon-escape-drag")]
    internal static int DraygonEscapeDragCommand()
    {
        Suite(nameof(VerifyDraygonEscapeDrag), () => VerifyDraygonEscapeDrag());
        return 0;
    }
}
