using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
namespace SuperMetroid.Verification.Bosses.Torizo;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Bosses.Torizo;

/// <summary>Checks Golden Torizo's landing attack selection with the native 16-bit NMI counter.</summary>
internal static class GoldenTorizoAttackChoiceVerification
{
    /// <summary>Confirms $AA:D53B adds $05B6 to the landing chooser, selecting sonic booms at the 100% movie's backward-jump landing.</summary>
    internal static void VerifyGoldenTorizoAttackChoice()
    {
        const ushort landedFacingLeft = 0xcdaf;
        const ushort sonicBoomsFacingLeft = 0xcbed;
        const ushort chozoOrbsFacingLeft = 0xcb41;
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.GoldenTorizo);
        var samus = runtime.Player.IntroducedSamus;
        RoomEnemySlot torizo = runtime.Enemies.SlotTable.Slots.Single(slot =>
            slot.DefinitionId == EnemyDefinitionId.GoldenTorizo);

        samus.Position = samus.Position.WithWholeX(0x01b8);
        samus.Missiles = 187;
        TorizoEnemyState state = runtime.Enemies.BombTorizoBehavior.Access.GoldenTorizo
            ?? throw new InvalidOperationException("Verification requires the Golden Torizo state.");
        state.BodyPlayback = TorizoAnimations.Start(TorizoNativeListAddresses.AnimationAt(landedFacingLeft)
            ?? throw new InvalidOperationException("No Torizo animation starts at the landing list."));
        torizo.AnimationClock.StepOnNextUpdate();
        runtime.Enemies.StepStandaloneFrame(runtime.Enemies.StandaloneFrame(runtime.Room.LoadedLevelData) with { Camera = new(runtime.Room.LoadedCamera.PixelPosition.X, runtime.Room.LoadedCamera.PixelPosition.Y), TimeIsFrozen = false, Samus = samus, NewlyPressedControllerInput = 0, ControllerInput = 0, SamusProjectiles = runtime.Projectiles, ShortCounter = 0x2d, FrameCounter = 0xefd8 });
        TorizoAnimation runningAnimation = TorizoAnimations.Playing(state.BodyPlayback);
        ushort running = TorizoNativeListAddresses.Entry(runningAnimation);
        AssertTrue(running is >= sonicBoomsFacingLeft and < 0xcc57,
            $"NMI_FrameCounter selects the sonic-boom list, not chozo orbs (${chozoOrbsFacingLeft:X4}); " +
            $"animation {runningAnimation} at ${running:X4}");
        Console.WriteLine("Golden Torizo attack choice: the landing chooser adds NMI_FrameCounter, not the 8-bit counter.");
    }

    /// <summary>Runs the focused landing attack-choice reproduction from the verification command line.</summary>
    [VerificationCommand("--golden-torizo-attack-choice")]
    internal static int GoldenTorizoAttackChoiceCommand()
    {
        VerifyGoldenTorizoAttackChoice();
        return 0;
    }
}
