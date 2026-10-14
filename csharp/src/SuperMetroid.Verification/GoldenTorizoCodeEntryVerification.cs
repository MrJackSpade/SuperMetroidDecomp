using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static FrontendRomFreeVerification;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Samus.Projectiles;

/// <summary>Checks the Golden Torizo room-entry code and its queued beam artwork.</summary>
internal static class GoldenTorizoCodeEntryVerification
{
    /// <summary>Confirms the native item grant while the entering Wave artwork survives the next NMI.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static void VerifyGoldenTorizoCodeEntry()
    {
        // Isolated SRAM; installed artwork is read-only and no player save is loaded.
        var installation = RepositoryInstallation.Installation;
        var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        var game = new SuperMetroidGame(memory, new SuperMetroidGameOptions { Invincibility = true }, renderGameplayFrames: false);
        PrepareRomFreeBindings(installation)(game, false);
        game.InitializeDirectRoomVerification();
        var runtime = game.GameplayRuntimeForVerification;
        runtime.Player.IntroducedSamus.EquippedBeams = (ushort)SamusBeamFlags.Wave;
        runtime.Player.IntroducedSamus.CollectedBeams = (ushort)SamusBeamFlags.Wave;
        runtime.Controller1.Latch(GoldenTorizoCodeDefinitions.ControllerChord);
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.GoldenTorizo);
        AssertEqual(GoldenTorizoCodeDefinitions.Beams, runtime.Player.IntroducedSamus.EquippedBeams, "GT code retains exact native beam grant");
        AssertEqual(GoldenTorizoCodeDefinitions.Items, runtime.Player.IntroducedSamus.EquippedItems, "GT code retains exact native equipment grant");
        AssertTrue(runtime.VramWrites.Entries.Any(entry => entry.AssetId == VramAssetId.BeamWaveTiles),
            "room transition queued the entering Wave artwork before code changed the beam word");
        for (int frame = 0; frame < 3; frame++) game.Step(0);
        AssertTrue(runtime.Vram.Bytes.Slice(SuperMetroid.Core.Samus.Artwork.BeamTileAtlasDefinitions.DestinationWord * 2,
            SuperMetroid.Core.Samus.Artwork.BeamTileAtlasDefinitions.ByteCount).SequenceEqual(
                (runtime.Artwork.BeamArtwork ?? throw new InvalidOperationException("Verification requires runtime.Artwork.BeamArtwork.")).Resolve(VramAssetId.BeamWaveTiles).Span),
            "accepted NMI retains the entering beam artwork after enemy graphics uploads");
        AssertEqual(SuperMetroidGameState.MainGameplay, game.GameState, "GT room entry resumes gameplay after code");
        Console.WriteLine("Golden Torizo code: real room initialization and three continuation frames succeed with the native grant.");
    }

    /// <summary>Runs the focused room-entry and artwork-continuation check.</summary>
    [VerificationCommand("--golden-torizo-code-entry")]
    internal static int GoldenTorizoCodeEntryCommand()
    {
     VerifyGoldenTorizoCodeEntry(); return 0; 
    }
}
