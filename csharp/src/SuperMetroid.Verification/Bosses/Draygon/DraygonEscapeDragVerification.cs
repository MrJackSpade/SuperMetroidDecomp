using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Bosses.Draygon;
using SuperMetroid.Verification.Harness;

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
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(0x0135, 0x019d)) };
        samus.DraygonGrabbed.Begin(samus, draygonFacingRight: false);
        samus.DraygonGrabbed.Release(samus);

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
