using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;

namespace SuperMetroid.Verification.Areas.Tourian;

/// <summary>
/// Checks #1799: loading Tourian's first room grants Samus the Tourian map, as the room's setup does in the original
/// (setup $8F:C90A sets the current area's map-station bit and the current area's map flag).
/// </summary>
internal static class TourianFirstRoomMapVerification
{
    /// <summary>
    /// Another Tourian room leaves the area's map uncollected; Tourian's first room grants it through the room loader's
    /// production setup, and grants no other area's map.
    /// </summary>
    internal static void VerifyTourianFirstRoomGrantsAreaMap()
    {
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        AssertTrue(!runtime.System.HasAreaMap(AreaId.Tourian), "a new session has no Tourian map");

        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.Metroids1);
        AssertTrue(!runtime.System.HasAreaMap(AreaId.Tourian), "a Tourian room whose setup does nothing grants no map");

        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.TourianFirst);
        AssertTrue(runtime.System.HasAreaMap(AreaId.Tourian), "entering Tourian's first room grants the Tourian map");
        foreach (AreaId area in new[] { AreaId.Crateria, AreaId.Brinstar, AreaId.Norfair, AreaId.WreckedShip, AreaId.Maridia })
            AssertTrue(!runtime.System.HasAreaMap(area), $"entering Tourian's first room grants no {area} map");
        Console.WriteLine("Tourian first room: entering it grants the Tourian map and no other.");
    }
}
