using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>Verifies that Golden Torizo code entry grants its native equipment and preserves the entering Wave artwork through room initialization and NMI continuation.</summary>
    private static void VerifyGoldenTorizoCodeEntry()
    {
        // Isolated SRAM; installed artwork is read-only and no player save is loaded.
        var installation = RepositoryInstallation.Installation;
        var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        var game = new SuperMetroidGame(memory, new SuperMetroidGameOptions { Invincibility = true }, renderGameplayFrames: false);
        PrepareRomFreeBindings(installation)(game, false);
        game.InitializeDirectRoomVerification();
        var runtime = game.RuntimeForVerification!;
        runtime.Samus!.EquippedBeams = (ushort)SamusBeamFlags.Wave;
        runtime.Samus.CollectedBeams = (ushort)SamusBeamFlags.Wave;
        runtime.Controller1.Latch(GoldenTorizoCodeDefinitions.ControllerChord);
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.GoldenTorizo);
        AssertEqual(GoldenTorizoCodeDefinitions.Beams, runtime.Samus.EquippedBeams, "GT code retains exact native beam grant");
        AssertEqual(GoldenTorizoCodeDefinitions.Items, runtime.Samus.EquippedItems, "GT code retains exact native equipment grant");
        AssertTrue(runtime.VramWrites.Entries.Any(entry => entry.AssetId == VramAssetId.BeamWaveTiles),
            "room transition queued the entering Wave artwork before code changed the beam word");
        for (int frame = 0; frame < 3; frame++) game.Step(0);
        AssertTrue(runtime.Vram.Bytes.Slice(SuperMetroid.Core.Assets.BeamTileAtlasDefinitions.DestinationWord * 2,
            SuperMetroid.Core.Assets.BeamTileAtlasDefinitions.ByteCount).SequenceEqual(
                runtime.BeamArtwork!.Resolve(VramAssetId.BeamWaveTiles).Span),
            "accepted NMI retains the entering beam artwork after enemy graphics uploads");
        AssertEqual(SuperMetroidGameState.MainGameplay, game.GameState, "GT room entry resumes gameplay after code");
        Console.WriteLine("Golden Torizo code: real room initialization and three continuation frames succeed with the native grant.");
    }
}
