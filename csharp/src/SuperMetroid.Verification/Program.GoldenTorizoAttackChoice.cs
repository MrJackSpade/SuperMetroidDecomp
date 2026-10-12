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

/// <summary>Represents golden torizo attack choice verification.</summary>
internal static class GoldenTorizoAttackChoiceVerification
{
    // #1269: Instruction_GoldenTorizo_CallY_OrY2_ForAttack ($AA:D53B) adds NMI_FrameCounter
    // ($05B6), not the 8-bit counter at $05B5. In the 100% movie the two disagree in bit 3
    // when the Torizo lands from a backward jump: native throws sonic booms, not chozo orbs.
    /// <summary>Verifies golden torizo attack choice.</summary>
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
        var samus = runtime.Player.Samus!;
        RoomEnemySlot torizo = runtime.Enemies.SlotTable.Slots.Single(slot =>
            slot.EnemyDefinitionPointer == EnemyDefinitionId.GoldenTorizo);

        // Native inputs at the movie's landing: (X >> 1) + carry + $05B6 has bit 3 clear,
        // while the same sum with $05B5 ($2D) has it set.
        samus.Position = samus.Position.WithWholeX(0x01b8);
        samus.Missiles = 187;
        torizo.CurrentInstruction = landedFacingLeft;
        torizo.InstructionTimer = 1;
        runtime.Enemies.StepFrame(new RoomEnemyFrame { Camera = new(runtime.Room.Camera!.PixelPosition.X, runtime.Room.Camera.PixelPosition.Y), TimeIsFrozen = false, Samus = samus, NewlyPressedControllerInput = 0, Level = runtime.Room.LevelData, ControllerInput = 0, SamusProjectiles = runtime.Projectiles, NmiFrameCounter8 = 0x2d, NmiFrameCounter = 0xefd8 });
        AssertTrue(torizo.CurrentInstruction is > sonicBoomsFacingLeft and
                < 0xcc57,
            $"NMI_FrameCounter selects the sonic-boom list, not chozo orbs (${chozoOrbsFacingLeft:X4}); " +
            $"instruction ${torizo.CurrentInstruction:X4}");
        Console.WriteLine("Golden Torizo attack choice: the landing chooser adds NMI_FrameCounter, not the 8-bit counter.");
    }

    /// <summary>Golden torizo attack choice command.</summary>
    [VerificationCommand("--golden-torizo-attack-choice")]
    internal static int GoldenTorizoAttackChoiceCommand()
    {
        VerifyGoldenTorizoAttackChoice();
        return 0;
    }
}
