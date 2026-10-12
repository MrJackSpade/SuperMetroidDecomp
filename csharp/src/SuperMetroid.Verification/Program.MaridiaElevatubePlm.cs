using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using static MaridiaElevatubeVisualsVerification;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.AssetExtraction;
using static AssertionsVerification;
using static LookupElevatubeVerification;
using static SuiteTimingVerification;
using static VerificationHelpersVerification;

/// <summary>Represents maridia elevatube plm verification.</summary>
internal static class MaridiaElevatubePlmVerification
{
    /// <summary>Verifies maridia elevatube plm.</summary>
    internal static void VerifyMaridiaElevatubePlm()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SuperMetroid.AssetExtraction.SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Elevatube oracle revision");
        Suite(nameof(VerifyElevatubePhysicalDraw), () => VerifyElevatubePhysicalDraw(rom));
        Suite(nameof(VerifyElevatubeProgramControls), () => VerifyElevatubeProgramControls(rom));
        Suite(nameof(VerifyElevatubeProgramDraw), () => VerifyElevatubeProgramDraw(rom));
        Suite(nameof(VerifyElevatubeProgramSound), () => VerifyElevatubeProgramSound(rom));

        RoomLevelData level = CreateRoom(new(4, 4), new ushort[16], new byte[16],
            blockDefinitions: new byte[0x400 * 8]);
        var plms = new RoomPlmSystem();
        AssertTrue(plms.WreckedShipTreadmill.TrySpawnMaridiaElevatube(level),
            "native door setup allocates the elevatube PLM");
        var guarded = NewMaridiaElevatubeSourceGuard(new TestAddressSpace());
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        int blockIndex = level.GetBlockIndex(
            new(MaridiaElevatubePlmRomData.BlockX, MaridiaElevatubePlmRomData.BlockY));
        int heardAtFrame = -1;
        for (int frame = 0; frame < 24 && plms.SlotTable.ActiveCount != 0; frame++)
        {
            plms.Step(guarded, level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
            if (frame == 0)
                AssertEqual((ushort)0x8180,
                    level.GetCollisionBlockByIndex(blockIndex).LevelWord,
                    "first elevatube handler pass draws its physical block");
            if (plms.Sounds.SoundRequests.Any(request =>
                    request.SoundEffect == SoundEffectId.FromCartridge(
                        SoundEffectLibrary.Library2,
                        MaridiaElevatubePlmDefinitions.SoundId)))
                heardAtFrame = frame;
        }
        AssertEqual(16, heardAtFrame,
            "elevatube sound follows the native sixteen-frame hold");
        AssertEqual(0, plms.SlotTable.ActiveCount,
            "elevatube PLM deletes after the sound instruction");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "elevatube program and draw execute without their source ROM bytes");
        Console.WriteLine(
            "Maridia elevatube PLM: native delay, draw, sound, and delete pass with source reads forbidden.");
    }

    /// <summary>Creates maridia elevatube source guard.</summary>
    internal static CartridgeReadGuard NewMaridiaElevatubeSourceGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address =>
        {
            int pointer = SnesAddressMath.OffsetInBank(address);
            return (SnesAddressMath.BankNumber(address)) == 0x84 &&
                ((pointer >= MaridiaElevatubePlmDefinitions.InstructionList &&
                  pointer < MaridiaElevatubePlmDefinitions.InstructionList + 9) ||
                 (pointer >= MaridiaElevatubePlmDefinitions.DrawPointer &&
                  pointer < MaridiaElevatubePlmDefinitions.DrawPointer + 6));
        }, address => $"Elevatube PLM reread compiled source ${address:X6}.");

    /// <summary>Maridia elevatube plm command.</summary>
    [VerificationCommand("--maridia-elevatube-plm")]
    internal static int MaridiaElevatubePlmCommand()
    {
        Suite(nameof(VerifyMaridiaElevatubePlm), () => VerifyMaridiaElevatubePlm());
        return 0;
    }

    /// <summary>Runs the stream4 elevatube lookup command.</summary>
    [VerificationCommand("--lookup-stream-4-elevatube")]
    internal static int LookupStream4ElevatubeCommand()
    {
        Suite(nameof(VerifyMaridiaElevatubePlm), () => VerifyMaridiaElevatubePlm());
        Suite(nameof(VerifyMaridiaElevatubeVisuals), () => VerifyMaridiaElevatubeVisuals());
        return 0;
    }
}
