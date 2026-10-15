using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Verification.Harness;

/// <summary>Checks that invalid bindings, brightness, PLM identities, and unloaded enemy services fail explicitly.</summary>
internal static class StrictFailuresVerification
{
/// <summary>Exercises specific invalid inputs at the controller, display, PLM, and enemy-service boundaries.</summary>
internal static void VerifyStrictFailureBoundaries()
{
    var duplicateBindings = new ControllerBindings(
        Shoot: SnesButton.X,
        Jump: SnesButton.X,
        Dash: SnesButton.B,
        ItemSelect: SnesButton.Select,
        ItemCancel: SnesButton.Y,
        AimUp: SnesButton.R,
        AimDown: SnesButton.L);
    AssertThrows<InvalidDataException>(
        () => duplicateBindings.RequireRetailPermutation(),
        "duplicate persisted controller binding is rejected");

    Rgba32[] pixel = [new Rgba32(255, 255, 255, 255)];
    AssertThrows<ArgumentOutOfRangeException>(
        () => MasterBrightnessFilter.Apply(pixel, -1),
        "negative master brightness is rejected");
    AssertThrows<ArgumentOutOfRangeException>(
        () => MasterBrightnessFilter.Apply(pixel, 16),
        "master brightness above INIDISP range is rejected");

    AssertTrue(RoomPlmSystem.IsSupportedRoomPopulationHeader(
            PlmHeaderId.YellowDoorFacingLeft),
        "translated yellow-door PLM is classified by the production dispatcher");
    AssertTrue(!RoomPlmSystem.IsSupportedRoomPopulationHeader((PlmHeaderId)0xdead),
        "unknown PLM is absent from the production dispatcher");

    var enemies = new RoomEnemySystem();
    AssertThrows<InvalidOperationException>(
        () => _ = enemies.Room.Services,
        "enemy AI services are rejected before Load installs them");

    Console.WriteLine(
        "  Strict failures: bindings, brightness, PLM coverage, and enemy services reject bad state loudly.");
}
}
