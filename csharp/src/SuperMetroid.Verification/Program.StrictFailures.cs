using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
static void VerifyStrictFailureBoundaries()
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
            RoomPlmHeaders.YellowDoorFacingLeft),
        "translated yellow-door PLM is classified by the production dispatcher");
    AssertTrue(!RoomPlmSystem.IsSupportedRoomPopulationHeader(0xdead),
        "unknown PLM is absent from the production dispatcher");

    var enemies = new RoomEnemySystem();
    AssertThrows<InvalidOperationException>(
        () => enemies.RequireRandomNumber(),
        "missing enemy RNG reader is rejected");
    AssertThrows<InvalidOperationException>(
        () => enemies.RequireAreaBossDefeated(),
        "missing enemy boss-state reader is rejected");
    AssertThrows<InvalidOperationException>(
        () => enemies.RequireEvent(EventNumber.MotherBrainGlassDestroyed),
        "missing enemy event-state reader is rejected");
    AssertThrows<InvalidOperationException>(
        () => enemies.RequireSetAreaBossDefeated(),
        "missing enemy boss-state writer is rejected");

    Console.WriteLine(
        "  Strict failures: bindings, brightness, PLM coverage, and enemy services reject bad state loudly.");
}
}
