using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>Represents strict failures verification.</summary>
internal static class StrictFailuresVerification
{
/// <summary>verify strict failure boundaries.</summary>
internal static void VerifyStrictFailureBoundaries()
{
    var duplicateBindings = new ControllerBindings(
        Shoot: (ushort)SnesButton.X,
        Jump: (ushort)SnesButton.X,
        Dash: (ushort)SnesButton.B,
        ItemSelect: (ushort)SnesButton.Select,
        ItemCancel: (ushort)SnesButton.Y,
        AimUp: (ushort)SnesButton.R,
        AimDown: (ushort)SnesButton.L);
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
