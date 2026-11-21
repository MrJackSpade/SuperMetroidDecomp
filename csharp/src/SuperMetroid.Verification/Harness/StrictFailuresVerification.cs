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
    var duplicateBindings = ControllerBindings.Default.With(ControllerAction.Jump, ControllerButton.X);
    AssertThrows<InvalidDataException>(
        () => duplicateBindings.RequireUniquePermutation(),
        "duplicate persisted controller binding is rejected");

    AssertThrows<ArgumentOutOfRangeException>(
        () => _ = DisplayBrightness.Black.Brighter(-1),
        "a negative brightening step is rejected");
    AssertThrows<ArgumentOutOfRangeException>(
        () => _ = DisplayBrightness.Full.Darker(-1),
        "a negative darkening step is rejected");
    AssertTrue(DisplayBrightness.Full.Brighter(1) == DisplayBrightness.Full && DisplayBrightness.Black.Darker(1) == DisplayBrightness.Black,
        "display brightness never steps past full or black");

    AssertTrue(RoomObjectSystem.IsSupportedRoomPopulationHeader(
            RoomObjectKind.YellowDoorFacingLeft),
        "translated yellow-door PLM is classified by the production dispatcher");
    AssertTrue(!RoomObjectSystem.IsSupportedRoomPopulationHeader((RoomObjectKind)(-1)),
        "unknown PLM is absent from the production dispatcher");

    Console.WriteLine(
        "  Strict failures: bindings, brightness and PLM coverage reject bad state loudly.");
}
}
