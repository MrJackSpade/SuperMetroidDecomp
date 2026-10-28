using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Harness;

/// <summary>Checks that invalid bindings, brightness, PLM identities, and unloaded enemy services fail explicitly.</summary>
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
        () => duplicateBindings.RequireRetailPermutation(),
        "duplicate persisted controller binding is rejected");

    AssertThrows<ArgumentOutOfRangeException>(
        () => _ = DisplayBrightness.Require(-1),
        "negative display brightness is rejected");
    AssertThrows<ArgumentOutOfRangeException>(
        () => _ = DisplayBrightness.Require(DisplayBrightness.Full + 1),
        "display brightness above full is rejected");

    AssertTrue(RoomPlmSystem.IsSupportedRoomPopulationHeader(
            PlmHeaderId.YellowDoorFacingLeft),
        "translated yellow-door PLM is classified by the production dispatcher");
    AssertTrue(!RoomPlmSystem.IsSupportedRoomPopulationHeader((PlmHeaderId)(-1)),
        "unknown PLM is absent from the production dispatcher");

    var enemies = new RoomEnemySystem(new EnemySession());
    AssertThrows<InvalidOperationException>(
        () => _ = enemies.Room.Services,
        "enemy AI services are rejected before Load installs them");

    Console.WriteLine(
        "  Strict failures: bindings, brightness, PLM coverage, and enemy services reject bad state loudly.");
}
}
