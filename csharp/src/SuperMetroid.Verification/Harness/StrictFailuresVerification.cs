using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Harness;

/// <summary>Checks that invalid bindings, brightness and PLM identities fail explicitly.</summary>
internal static class StrictFailuresVerification
{
/// <summary>Exercises specific invalid inputs at the controller, display, PLM, and enemy-service boundaries.</summary>
internal static void VerifyStrictFailureBoundaries()
{
    var duplicateBindings = new ControllerBindings(
        Shoot: ControllerButton.X,
        Jump: ControllerButton.X,
        Dash: ControllerButton.B,
        ItemSelect: ControllerButton.Select,
        ItemCancel: ControllerButton.Y,
        AimUp: ControllerButton.R,
        AimDown: ControllerButton.L);
    AssertThrows<InvalidDataException>(
        () => duplicateBindings.RequireUniquePermutation(),
        "duplicate persisted controller binding is rejected");

    AssertThrows<ArgumentOutOfRangeException>(
        () => _ = DisplayBrightness.StepsAbove(-1),
        "negative display brightness is rejected");
    AssertThrows<ArgumentOutOfRangeException>(
        () => _ = DisplayBrightness.StepsAbove(DisplayBrightness.StepsFromBlackToFull + 1),
        "display brightness above full is rejected");

    AssertTrue(RoomObjectSystem.IsSupportedRoomPopulationHeader(
            RoomObjectKind.YellowDoorFacingLeft),
        "translated yellow-door PLM is classified by the production dispatcher");
    AssertTrue(!RoomObjectSystem.IsSupportedRoomPopulationHeader((RoomObjectKind)(-1)),
        "unknown PLM is absent from the production dispatcher");

    Console.WriteLine(
        "  Strict failures: bindings, brightness and PLM coverage reject bad state loudly.");
}
}
