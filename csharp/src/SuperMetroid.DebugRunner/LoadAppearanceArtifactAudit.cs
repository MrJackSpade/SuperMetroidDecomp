using System.Security.Cryptography;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;

/// <summary>Exact recorded-session capture used to diagnose issue #256.</summary>
internal static class LoadAppearanceArtifactAudit
{
    /// <summary>
    /// Builds the minimal persistent state which caused the artifact: an existing save
    /// whose contiguous SRAM payload contains a nonzero HUD selection. The production
    /// frontend and saved-game loader must clear that transient word before any appearance
    /// frame can ask the independent arm-cannon renderer for an OBJ.
    /// </summary>
    public static int RunRegression(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var savedSamus = new SamusState
        {
            Health = 99,
            MaxHealth = 99,
            Missiles = 5,
            MaxMissiles = 5,
            SuperMissiles = 1,
            MaxSuperMissiles = 1,
            SelectedHudItem = 2,
        };
        var saveRam = new SuperMetroidSaveRam(bus);
        saveRam.SaveSlot(
            0,
            SuperMetroidSaveSnapshot.Capture(
                savedSamus,
                new Bank80SystemState(),
                area: 0,
                saveStation: 0,
                gameTime: new GameTimeState()));
        saveRam.SelectSlot(0);
        if (saveRam.ReadSlot(0)?.HudItem != 2)
            throw new InvalidDataException("Regression setup did not persist HUD item two.");

        var game = new SuperMetroidGame(
            bus,
            new SuperMetroidGameOptions { SkipOpeningCinematic = true });
        FrontendFrame frame = FrontendAuditDriver.EnterSelectedSlot(game);
        var apuPortEchoes = new byte[4];
        bool sawFormerArtifactFrame = false;
        for (int step = 0; step < RoomLoadingRomData.SavedGameAppearanceFrameCount + 8; step++)
        {
            SuperMetroidRuntime runtime = game.RuntimeForVerification
                ?? throw new InvalidOperationException("Regression load lost its runtime.");
            if (!runtime.SamusLoadAppearanceActive)
                break;

            AssertNoPersistedSelectionArtifact(runtime);
            ushort elapsed = unchecked((ushort)(
                RoomLoadingRomData.SavedGameAppearanceFrameCount -
                runtime.SamusLoadAppearanceFramesRemaining));
            if (elapsed == 64)
            {
                AssertFormerArtifactFrame(runtime);
                sawFormerArtifactFrame = true;
            }

            frame = game.Step(0);
            EchoAudio(frame.AudioCommands, apuPortEchoes);
            game.SetAudioAcknowledgements(new CartridgeAudioAcknowledgements(
                apuPortEchoes[0], apuPortEchoes[1], apuPortEchoes[2], apuPortEchoes[3]));
        }

        if (!sawFormerArtifactFrame)
            throw new InvalidDataException("Synthetic load ended before artifact frame 64.");
        Console.WriteLine(
            "Issue #256 regression passed: a persisted Super-Missile selection was cleared " +
            "and emitted no load-appearance cannon OBJ.");
        return 0;
    }

    public static int Run(string recordingPath, string romPath, string outputDirectory)
    {
        ControllerInputRecording recording = ControllerInputRecording.Read(recordingPath);
        byte[] digest;
        using (FileStream rom = File.OpenRead(romPath))
            digest = SHA256.HashData(rom);
        if (!CryptographicOperations.FixedTimeEquals(digest, recording.RomSha256))
            throw new InvalidDataException("Load-appearance replay ROM SHA-256 does not match.");

        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        recording.InitialSaveRam.CopyTo(bus.SaveRam);
        var saveRam = new SuperMetroidSaveRam(bus);
        int selectedSlot = saveRam.ReadSelectedSlot();
        SuperMetroidSaveSlot sourceSlot = saveRam.ReadSlot(selectedSlot)
            ?? throw new InvalidDataException(
                $"Recorded selected slot {selectedSlot} is empty or corrupt.");
        if (sourceSlot.HudItem == 0)
        {
            throw new InvalidDataException(
                "Issue #256 requires a recording whose SRAM snapshot persisted a nonzero " +
                "HUD selection; this recording cannot reproduce the stray cannon OBJ.");
        }

        var game = new SuperMetroidGame(bus, recording.GameOptions);
        var apuPortEchoes = new byte[4];
        Directory.CreateDirectory(outputDirectory);

        bool sawAppearance = false;
        bool sawFormerArtifactFrame = false;
        for (int frameIndex = 0; frameIndex < recording.ControllerInputs.Length; frameIndex++)
        {
            FrontendFrame frame = game.Step(recording.ControllerInputs[frameIndex]);
            EchoAudio(frame.AudioCommands, apuPortEchoes);
            game.SetAudioAcknowledgements(new CartridgeAudioAcknowledgements(
                apuPortEchoes[0], apuPortEchoes[1], apuPortEchoes[2], apuPortEchoes[3]));

            if (game.RuntimeForVerification is not { SamusLoadAppearanceActive: true } runtime)
            {
                if (sawAppearance)
                    break;
                continue;
            }

            sawAppearance = true;
            AssertNoPersistedSelectionArtifact(runtime);

            ushort elapsed = unchecked((ushort)(
                RoomLoadingRomData.SavedGameAppearanceFrameCount -
                runtime.SamusLoadAppearanceFramesRemaining));
            if (elapsed == 64)
            {
                sawFormerArtifactFrame = true;
                AssertFormerArtifactFrame(runtime);
            }
            if (elapsed % 8 != 0)
                continue;

            string output = Path.Combine(outputDirectory, $"load-{elapsed:D3}.png");
            PngWriter.WriteRgba(output, FrontendFrame.Width, FrontendFrame.Height, frame.Pixels);
            Console.WriteLine(
                $"frame={frameIndex}, elapsed={elapsed}, pose=${runtime.Samus!.Pose:X2}, " +
                $"animation={runtime.Samus.AnimationFrame}, " +
                $"cannonMode={runtime.Samus.ArmCannon.EffectiveDrawingMode}, " +
                $"cannonDraw={runtime.LastArmCannonDraw}, " +
                $"output={Path.GetFullPath(output)}");
        }

        if (!sawAppearance)
            throw new InvalidDataException("Recording never entered saved-game appearance.");
        if (!sawFormerArtifactFrame)
            throw new InvalidDataException("Recording ended before exact artifact frame 64.");
        Console.WriteLine(
            $"Issue #256 exact replay passed: persisted HUD item {sourceSlot.HudItem} was " +
            "cleared at load, and no independent cannon OBJ contaminated the appearance.");
        return 0;
    }

    private static void AssertNoPersistedSelectionArtifact(SuperMetroidRuntime runtime)
    {
        SamusState samus = runtime.Samus
            ?? throw new InvalidOperationException("Load appearance lost Samus.");
        if (samus.SelectedHudItem != 0 || samus.AutoCancelHudItemIndex != 0)
        {
            throw new InvalidDataException(
                $"Load retained HUD selection {samus.SelectedHudItem}/" +
                $"{samus.AutoCancelHudItemIndex} after cartridge initialization.");
        }
        if (runtime.LastArmCannonDraw.SpriteWritten)
        {
            throw new InvalidDataException(
                "Load appearance emitted the independent arm-cannon OBJ that previously " +
                "appeared as a detached lightning artifact.");
        }
    }

    private static void AssertFormerArtifactFrame(SuperMetroidRuntime runtime)
    {
        SamusState samus = runtime.Samus
            ?? throw new InvalidOperationException("Load appearance lost Samus.");
        if (samus.Pose != SamusPoseIds.ForwardFacingPowerSuitPose ||
            samus.AnimationFrame != 23)
        {
            throw new InvalidDataException(
                $"Exact artifact frame changed pose/animation timing: " +
                $"pose=${samus.Pose:X2}, animation={samus.AnimationFrame}.");
        }

        bool foundStrayCannon = Enumerable.Range(
                0,
                runtime.DisplayedOam.LastFinalizedSpriteCount)
            .Select(runtime.DisplayedOam.GetEntry)
            .Any(entry =>
                entry.X == 94 && entry.Y == 113 &&
                entry.TileNumber == 0x01f && !entry.IsLarge);
        if (foundStrayCannon)
        {
            throw new InvalidDataException(
                "Exact artifact frame still contains the detached tile-$01F cannon OBJ " +
                "at screen coordinate (94,113).");
        }
    }

    private static void EchoAudio(
        IReadOnlyList<CartridgeAudioCommand> commands,
        Span<byte> ports)
    {
        foreach (CartridgeAudioCommand command in commands)
        {
            if (command.Kind == CartridgeAudioCommandKind.WritePort)
                ports[command.Port] = command.Value;
        }
    }
}
