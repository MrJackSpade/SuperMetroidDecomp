using System.Security.Cryptography;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// Replays issue #267's preserved Warehouse Save exit and compares the production frame
/// with the exact main-screen layer designation written by the cartridge door IRQ.
/// </summary>
internal static class WarehouseSaveExitAudit
{
    private const ushort ReproductionCameraX = 0x037c;

    public static int Run(string recordingPath, string romPath, string outputDirectory)
    {
        ControllerInputRecording recording = ControllerInputRecording.Read(recordingPath);
        string fullRomPath = Path.GetFullPath(romPath);
        using (FileStream rom = File.OpenRead(fullRomPath))
        {
            byte[] digest = SHA256.HashData(rom);
            if (!CryptographicOperations.FixedTimeEquals(digest, recording.RomSha256))
                throw new InvalidDataException("Warehouse Save replay ROM digest does not match.");
        }

        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(fullRomPath);
        recording.InitialSaveRam.CopyTo(bus.SaveRam);
        var game = new SuperMetroidGame(bus, recording.GameOptions);
        var apuPortEchoes = new byte[4];
        bool sawSourceRoom = false;
        bool sawReportedDoor = false;

        for (int frameIndex = 0; frameIndex < recording.ControllerInputs.Length; frameIndex++)
        {
            FrontendFrame frame = game.Step(recording.ControllerInputs[frameIndex]);
            foreach (CartridgeAudioCommand command in frame.AudioCommands)
            {
                if (command.Kind == CartridgeAudioCommandKind.WritePort)
                    apuPortEchoes[command.Port] = command.Value;
            }
            game.SetAudioAcknowledgements(new CartridgeAudioAcknowledgements(
                apuPortEchoes[0], apuPortEchoes[1], apuPortEchoes[2], apuPortEchoes[3]));

            SuperMetroidRuntime? runtime = game.RuntimeForVerification;
            if (runtime?.ActiveRoom?.Pointer == RoomHeaderPointers.WarehouseSave)
            {
                sawSourceRoom = true;
                sawReportedDoor |= runtime.PendingDoorTransition?.Pointer ==
                    DoorPointers.WarehouseKihunterFromSave;
            }

            if (runtime?.ActiveRoom?.Pointer != RoomHeaderPointers.WarehouseKihunter ||
                runtime.Camera?.XPosition != ReproductionCameraX ||
                game.DoorTransitionPhaseForVerification !=
                    DoorTransitionPhase.WaitForDoorOpeningScroll)
            {
                continue;
            }

            if (!sawSourceRoom || !sawReportedDoor)
            {
                throw new InvalidDataException(
                    "Replay reached the destination without traversing Warehouse Save door $83:925E.");
            }

            GameplayPpuRenderSnapshot ppu = runtime.DisplayedGameplayPpu;
            Rgba32[] cartridgeLayerFrame =
                SnesGameplayFrameRenderer.RenderHudOrdinaryBackgroundsAndObjs(
                    runtime.Vram,
                    runtime.Cgram,
                    runtime.DisplayedOam,
                    ppu.Bg1HorizontalScroll,
                    ppu.Bg1VerticalScroll,
                    ppu.Bg2HorizontalScroll,
                    ppu.Bg2VerticalScroll,
                    bg3CharacterBaseWord: runtime.GameplayHudCharacterBaseWord,
                    mainScreenLayers: SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Obj);

            Directory.CreateDirectory(outputDirectory);
            string actualPath = Path.Combine(outputDirectory, "WarehouseSaveExit.actual.png");
            string expectedPath = Path.Combine(outputDirectory, "WarehouseSaveExit.cartridge-layers.png");
            PngWriter.WriteRgba(actualPath, FrontendFrame.Width, FrontendFrame.Height, frame.Pixels);
            PngWriter.WriteRgba(
                expectedPath,
                FrontendFrame.Width,
                FrontendFrame.Height,
                cartridgeLayerFrame);

            int mismatches = frame.Pixels.AsSpan().SequenceEqual(cartridgeLayerFrame.AsSpan())
                ? 0
                : Enumerable.Range(0, frame.Pixels.Length)
                    .Count(pixel => frame.Pixels[pixel] != cartridgeLayerFrame[pixel]);
            if (mismatches != 0)
            {
                throw new InvalidDataException(
                    $"Reproduced issue #267 on recorded frame {frameIndex}: production " +
                    $"composition differs from the cartridge's BG1+OBJ door IRQ on " +
                    $"{mismatches} pixels. Captures: {Path.GetFullPath(outputDirectory)}.");
            }

            Console.WriteLine(
                $"Warehouse Save exit audit passed at recorded frame {frameIndex}: " +
                "$01/$36 -> $01/$2C uses the cartridge BG1+OBJ transition display.");
            return 0;
        }

        throw new InvalidDataException(
            "Recording never reached issue #267's mid-scroll Warehouse Save exit state.");
    }
}
