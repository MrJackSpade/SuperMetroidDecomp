using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.AssetExtraction.Installation;
namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>
/// Checks that a cartridge-backed Golden Torizo can enter its native walking combat
/// after the compiled awakening sequence when installed artwork is present.
/// </summary>
internal static class GoldenTorizoCartridgeFallbackVerification
{
    /// <summary>
    /// Installed artwork does not imply that every Torizo combat instruction is
    /// compiled yet. This independent, cartridge-backed runtime verifies that
    /// the handoff from compiled awakening to the native walking program stays
    /// playable while the ROM-free migration remains open.
    /// </summary>
    internal static void VerifyGoldenTorizoCartridgeCombatFallback(
        string sourceRom, GameInstallation installation)
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
        var runtime = CreateRetailRuntimeFixture();
        InstalledEnemyArtwork installedArtwork = installation.LoadEnemyTiles();
        runtime.EnemySession.TileArtwork = installedArtwork.Enemies;
        runtime.BossArtwork.Artwork = installedArtwork.Bosses;
        runtime.AreaArtwork.Artwork = installedArtwork.Areas;
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.GoldenTorizo);
        var samus = runtime.Player.IntroducedSamus;
        samus.Position = samus.Position.WithWhole(new(0x0180, 0x0150));
        samus.MaxHealth = 1499;
        samus.Health = 1499;
        RoomEnemySlot boss = runtime.Enemies.SlotTable.Slots.Single(slot =>
            slot.DefinitionId == EnemyDefinitionId.GoldenTorizo);
        TorizoEnemyState state = runtime.Enemies.BombTorizoBehavior.Attacks.GoldenTorizo
            ?? throw new InvalidOperationException("Verification requires the Golden Torizo state.");
        bool enteredCombat = false;
        for (int frame = 0; frame < 850; frame++)
        {
            SnesButton input = frame is >= 500 and < 900 && frame % 20 == 0
                ? SnesButton.X : 0;
            try
            {
                runtime.StepFrame(input);
            }
            catch (Exception error)
            {
                throw new InvalidOperationException(
                    $"Cartridge-backed Golden Torizo failed at frame {frame}, " +
                    $"program={state.BodyProgram}.", error);
            }
            // The awakening hands off to the walking-left lists; every later Golden Torizo list is combat.
            enteredCombat |= state.BodyProgram.Program >= TorizoBodyProgram.GoldenWalkingLeftLeftLegMoving;
        }
        AssertTrue(enteredCombat,
            "cartridge-backed installed-art Golden Torizo reaches walking combat; " +
            $"program={state.BodyProgram}, health={samus.Health}");
        Console.WriteLine("Golden Torizo cartridge fallback: 850 wake/combat frames with installed artwork.");
    }
}
