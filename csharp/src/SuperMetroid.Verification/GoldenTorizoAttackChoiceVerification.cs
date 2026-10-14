using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static RetailRuntimeFixtureVerification;

/// <summary>Checks Golden Torizo's landing attack selection with the native 16-bit NMI counter.</summary>
internal static class GoldenTorizoAttackChoiceVerification
{
    /// <summary>Confirms $AA:D53B adds $05B6 to the landing chooser, selecting sonic booms at the 100% movie's backward-jump landing.</summary>
    internal static void VerifyGoldenTorizoAttackChoice()
    {
        const ushort landedFacingLeft = 0xcdaf;
        const ushort sonicBoomsFacingLeft = 0xcbed;
        const ushort chozoOrbsFacingLeft = 0xcb41;
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.GoldenTorizo);
        var samus = runtime.Player.IntroducedSamus;
        RoomEnemySlot torizo = runtime.Enemies.SlotTable.Slots.Single(slot =>
            slot.EnemyDefinitionPointer == EnemyDefinitionId.GoldenTorizo);

        samus.Position = samus.Position.WithWholeX(0x01b8);
        samus.Missiles = 187;
        torizo.CurrentInstruction = landedFacingLeft;
        torizo.InstructionTimer = NativeCountdown.Start(1);
        runtime.Enemies.StepFrame(new RoomEnemyFrame { Camera = new(runtime.Room.LoadedCamera.PixelPosition.X, runtime.Room.LoadedCamera.PixelPosition.Y), TimeIsFrozen = false, Samus = samus, NewlyPressedControllerInput = 0, Level = runtime.Room.LevelData, ControllerInput = 0, SamusProjectiles = runtime.Projectiles, NmiFrameCounter8 = 0x2d, NmiFrameCounter = 0xefd8 });
        AssertTrue(torizo.CurrentInstruction is > sonicBoomsFacingLeft and
                < 0xcc57,
            $"NMI_FrameCounter selects the sonic-boom list, not chozo orbs (${chozoOrbsFacingLeft:X4}); " +
            $"instruction ${torizo.CurrentInstruction:X4}");
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
